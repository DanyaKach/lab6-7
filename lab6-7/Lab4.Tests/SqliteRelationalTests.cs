using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lab4.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Lab4.Tests;

public class SqliteRelationalTests
{
    public readonly AppDbContext Context;
    public readonly SqliteConnection Connection;

    public SqliteRelationalTests()
    {
        Connection = new SqliteConnection("DataSource=:memory:");
        Connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(Connection)
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();
    }

    [Fact]
    public async Task ForeignKey_EnrollingInNonExistingCourse_ThrowsAsync()
    {
        var ct = TestContext.Current.CancellationToken;

        using (Context)
        using (Connection)
        {
            var enrollment = new Enrollment
            {
                StudentId = 999,
                CourseId = 999,
                Grade = 85
            };

            Context.Enrollments.Add(enrollment);
            var exception = await Should.ThrowAsync<DbUpdateException>(
                () => Context.SaveChangesAsync(ct));
            exception.ShouldNotBeNull();
        }
    }

    [Fact]
    public async Task UniqueConstraint_DuplicateEmail_ThrowsAsync()
    {
        var ct = TestContext.Current.CancellationToken;

        using (Connection)
        using (Context)
        {
            var student1 = new Student
            {
                FullName = "Alice", Email = "dup@test.com",
                EnrollmentDate = DateTime.UtcNow
            };
            var student2 = new Student
            {
                FullName = "Bob", Email = "dup@test.com",
                EnrollmentDate = DateTime.UtcNow
            };

            Context.Students.Add(student1);
            await Context.SaveChangesAsync(ct);
            Context.Students.Add(student2);

            await Should.ThrowAsync<DbUpdateException>(
                () => Context.SaveChangesAsync(ct));
        }
    }

    [Fact]
    public async Task CascadeDelete_DeletingStudent_RemovesEnrollmentsAsync()
    {
        var ct = TestContext.Current.CancellationToken;

        using (Connection)
        using (Context)
        {
            var course = new Course { Title = "CS101", Credits = 3 };
            var student = new Student
            {
                FullName = "Charlie", Email = "charlie@test.com",
                EnrollmentDate = DateTime.UtcNow,
                Enrollments = new List<Enrollment>
                {
                    new Enrollment { Course = course, Grade = 88 }
                }
            };
            Context.Students.Add(student);
            await Context.SaveChangesAsync(ct);

            Context.Students.Remove(student);
            await Context.SaveChangesAsync(ct);

            var enrollments = await Context.Enrollments.ToListAsync(ct);
            enrollments.ShouldBeEmpty();
        }
    }

    [Fact]
    public async Task OptimisticConcurrency_HandlingConcurrentUpdates_ThrowsConcurrencyExceptionAsync()
    {
        var ct = TestContext.Current.CancellationToken;

        using (Connection)
        using (Context)
        {
            var student = new Student
            {
                FullName = "Original Name",
                Email = "concurrency@test.com",
                EnrollmentDate = DateTime.UtcNow
            };

            Context.Students.Add(student);
            await Context.SaveChangesAsync(ct);

            var options2 = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(Connection)
                .Options;
            using var context2 = new AppDbContext(options2);

            var studentInContext1 = await Context.Students.FindAsync([student.Id], ct);
            var studentInContext2 = await context2.Students.FindAsync([student.Id], ct);

            // Перший користувач оновлює
            studentInContext1!.FullName = "Updated by User 1";
            await Context.SaveChangesAsync(ct);

            // Другий користувач намагається оновити конкурентно
            studentInContext2!.FullName = "Updated by User 2";

            await Should.ThrowAsync<DbUpdateConcurrencyException>(
                () => context2.SaveChangesAsync(ct));
        }
    }
}
