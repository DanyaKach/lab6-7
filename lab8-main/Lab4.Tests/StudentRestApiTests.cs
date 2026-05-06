using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Lab4.Data;
using Shouldly;
using Xunit;
using System.Linq;

namespace Lab4.Tests;

public class StudentRestApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public StudentRestApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateStudent_ValidRequest_ReturnsCreated()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = new CreateStudentRequest
        {
            FullName = "REST Test User",
            Email = "rest@example.com",
            EnrollmentDate = DateTime.UtcNow.AddDays(-1)
        };

        var response = await _client.PostAsJsonAsync("/api/student", request, ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateStudent_EmptyFullName_ReturnsBadRequest()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = new CreateStudentRequest
        {
            FullName = "",
            Email = "valid@example.com",
            EnrollmentDate = DateTime.UtcNow
        };

        var response = await _client.PostAsJsonAsync("/api/student", request, ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync(ct);
        content.ShouldContain("FullName is required and cannot exceed 100 characters.");
    }

    [Fact]
    public async Task UpdateStudent_InvalidEmail_ReturnsBadRequest()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = new UpdateStudentRequest
        {
            Id = 1,
            FullName = "Valid Name",
            Email = "invalid-email"
        };

        var response = await _client.PutAsJsonAsync("/api/student", request, ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync(ct);
        content.ShouldContain("A valid Email is required.");
    }
}
