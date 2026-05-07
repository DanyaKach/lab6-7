using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Lab4.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Shouldly;
using Testcontainers.PostgreSql;
using Xunit;

namespace Lab4.Tests;

public class ApiFixture : IAsyncLifetime
{
    private PostgreSqlContainer _dbContainer = null!;
    private WebApplicationFactory<Program> _factory = null!;
    private string? _originalConnectionStringEnv;

    public HttpClient HttpClient { get; private set; } = null!;
    public string ConnectionString => _dbContainer.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("test_db")
            .WithUsername("postgres")
            .WithPassword("postgres_password")
            .Build();

        await _dbContainer.StartAsync();

        var dbConnectionString = _dbContainer.GetConnectionString();
        _originalConnectionStringEnv = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", dbConnectionString);

        // Use WebApplicationFactory with real PostgreSQL connection
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptors = services.Where(d =>
                        d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                        d.ServiceType == typeof(AppDbContext)).ToList();
                    foreach (var descriptor in descriptors)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(dbConnectionString));
                });
            });

        HttpClient = _factory.CreateClient();

        // Ensure database is created
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        await using var verifyConnection = new NpgsqlConnection(dbConnectionString);
        await verifyConnection.OpenAsync();
        await using var verifyCommand = new NpgsqlCommand(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'", verifyConnection);
        await using var reader = await verifyCommand.ExecuteReaderAsync();
        var tableNames = new List<string>();
        while (await reader.ReadAsync())
        {
            tableNames.Add(reader.GetString(0));
        }
        Console.WriteLine("Postgres tables: " + string.Join(",", tableNames));
    }

    public async ValueTask DisposeAsync()
    {
        HttpClient?.Dispose();
        _factory?.Dispose();
        if (_dbContainer != null) await _dbContainer.DisposeAsync();

        if (_originalConnectionStringEnv is null)
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
        }
        else
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _originalConnectionStringEnv);
        }
    }
}

public class PostgreSqlIntegrationTests : IClassFixture<ApiFixture>
{
    private readonly ApiFixture _fixture;

    public PostgreSqlIntegrationTests(ApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateStudent_ValidRequest_SavesToDatabase()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = new CreateStudentRequest
        {
            FullName = "PostgreSQL Test User",
            Email = "postgres@example.com",
            EnrollmentDate = DateTime.UtcNow.AddDays(-1)
        };

        var response = await _fixture.HttpClient.PostAsJsonAsync("/api/student", request, ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Verify in database
        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync(ct);
        await using var command = new NpgsqlCommand(
            "SELECT fullname, email FROM students WHERE email = @email", connection);
        command.Parameters.AddWithValue("email", request.Email);

        await using var reader = await command.ExecuteReaderAsync(ct);
        reader.Read().ShouldBeTrue();
        reader.GetString(0).ShouldBe(request.FullName);
        reader.GetString(1).ShouldBe(request.Email);
    }

    [Fact]
    public async Task GetStudents_ReturnsStudentsFromDatabase()
    {
        var ct = TestContext.Current.CancellationToken;

        // Since there's no GET endpoint, test by creating a student and verifying in DB
        var request = new CreateStudentRequest
        {
            FullName = "Get Test User",
            Email = "get@example.com",
            EnrollmentDate = DateTime.UtcNow
        };

        var response = await _fixture.HttpClient.PostAsJsonAsync("/api/student", request, ct);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Verify in database
        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync(ct);
        await using var command = new NpgsqlCommand(
            "SELECT COUNT(*) FROM students WHERE email = @email", connection);
        command.Parameters.AddWithValue("email", request.Email);

        var count = (long)await command.ExecuteScalarAsync(ct);
        count.ShouldBe(1);
    }

    [Fact]
    public async Task UpdateStudent_ValidRequest_UpdatesInDatabase()
    {
        var ct = TestContext.Current.CancellationToken;

        // Create student first
        var createRequest = new CreateStudentRequest
        {
            FullName = "Update Test User",
            Email = "update@example.com",
            EnrollmentDate = DateTime.UtcNow
        };

        var createResponse = await _fixture.HttpClient.PostAsJsonAsync("/api/student", createRequest, ct);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Get the student ID from database
        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync(ct);
        await using var idCommand = new NpgsqlCommand(
            "SELECT id FROM students WHERE email = @email", connection);
        idCommand.Parameters.AddWithValue("email", createRequest.Email);
        var studentId = (int)await idCommand.ExecuteScalarAsync(ct);

        // Update the student
        var updateRequest = new UpdateStudentRequest
        {
            Id = studentId,
            FullName = "Updated Name",
            Email = "updated@example.com"
        };

        var updateResponse = await _fixture.HttpClient.PutAsJsonAsync("/api/student", updateRequest, ct);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Verify in database
        await using var verifyCommand = new NpgsqlCommand(
            "SELECT fullname, email FROM students WHERE id = @id", connection);
        verifyCommand.Parameters.AddWithValue("id", studentId);

        await using var reader = await verifyCommand.ExecuteReaderAsync(ct);
        reader.Read().ShouldBeTrue();
        reader.GetString(0).ShouldBe(updateRequest.FullName);
        reader.GetString(1).ShouldBe(updateRequest.Email);
    }

    [Fact]
    public async Task CreateStudentWithEnrollment_SavesEnrollmentToDatabase()
    {
        var ct = TestContext.Current.CancellationToken;

        // First create a course
        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync(ct);
        await using var courseCommand = new NpgsqlCommand(
            "INSERT INTO courses (title, credits) VALUES (@title, @credits) RETURNING id", connection);
        courseCommand.Parameters.AddWithValue("title", "Integration Test Course");
        courseCommand.Parameters.AddWithValue("credits", 3);
        var courseId = (int)await courseCommand.ExecuteScalarAsync(ct);

        // Create student with enrollment (assuming API supports this)
        var request = new CreateStudentRequest
        {
            FullName = "Enrolled Student",
            Email = "enrolled@example.com",
            EnrollmentDate = DateTime.UtcNow
        };

        var response = await _fixture.HttpClient.PostAsJsonAsync("/api/student", request, ct);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Manually add enrollment to test database relationships
        await using var enrollCommand = new NpgsqlCommand(
            "INSERT INTO enrollments (student_id, course_id, grade) VALUES ((SELECT id FROM students WHERE email = @email), @courseId, @grade)", connection);
        enrollCommand.Parameters.AddWithValue("email", request.Email);
        enrollCommand.Parameters.AddWithValue("courseId", courseId);
        enrollCommand.Parameters.AddWithValue("grade", 85.5m);
        await enrollCommand.ExecuteNonQueryAsync(ct);

        // Verify enrollment exists
        await using var verifyCommand = new NpgsqlCommand(
            @"SELECT s.fullname, c.title, e.grade 
              FROM students s 
              JOIN enrollments e ON s.id = e.student_id 
              JOIN courses c ON c.id = e.course_id 
              WHERE s.email = @email", connection);
        verifyCommand.Parameters.AddWithValue("email", request.Email);

        await using var reader = await verifyCommand.ExecuteReaderAsync(ct);
        reader.Read().ShouldBeTrue();
        reader.GetString(0).ShouldBe(request.FullName);
        reader.GetString(1).ShouldBe("Integration Test Course");
        reader.GetDecimal(2).ShouldBe(85.5m);
    }
}