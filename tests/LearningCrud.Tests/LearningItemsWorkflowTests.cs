using System.Net;
using System.Text.RegularExpressions;
using LearningCrud.Web.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LearningCrud.Tests;

public sealed class LearningItemsWorkflowTests : IClassFixture<LearningCrudFactory>
{
    private readonly HttpClient _client;
    private readonly LearningCrudFactory _factory;

    public LearningItemsWorkflowTests(LearningCrudFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<LearningDbContext>()
            .Database.EnsureCreated();
    }

    [Fact]
    public async Task User_can_create_view_edit_and_delete_item()
    {
        await ClearItemsAsync();

        var createForm = await _client.GetStringAsync("/LearningItems/Create");
        var createResponse = await PostFormAsync(
            "/LearningItems/Create",
            GetAntiforgeryToken(createForm),
            new Dictionary<string, string>
            {
                ["Title"] = "  EF Core  ",
                ["Description"] = "  Learn persistence  "
            });

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        var detailsHtml = await createResponse.Content.ReadAsStringAsync();
        Assert.Contains("EF Core", detailsHtml);
        Assert.Contains("Learn persistence", detailsHtml);
        Assert.DoesNotContain("  EF Core  ", detailsHtml);

        var itemId = await GetOnlyItemIdAsync();
        var listHtml = await _client.GetStringAsync("/LearningItems");
        Assert.Contains("EF Core", listHtml);

        var detailsResponse = await _client.GetAsync($"/LearningItems/Details/{itemId}");
        Assert.Equal(HttpStatusCode.OK, detailsResponse.StatusCode);
        Assert.Contains("EF Core", await detailsResponse.Content.ReadAsStringAsync());

        var createdAt = await GetCreatedAtAsync(itemId);
        var editForm = await _client.GetStringAsync($"/LearningItems/Edit/{itemId}");
        var editResponse = await PostFormAsync(
            $"/LearningItems/Edit/{itemId}",
            GetAntiforgeryToken(editForm),
            new Dictionary<string, string>
            {
                ["Title"] = "SQLite practice",
                ["Description"] = "Edited description"
            });
        Assert.Equal(HttpStatusCode.OK, editResponse.StatusCode);
        Assert.Contains("SQLite practice", await editResponse.Content.ReadAsStringAsync());
        Assert.Equal(createdAt, await GetCreatedAtAsync(itemId));

        var deleteForm = await _client.GetStringAsync($"/LearningItems/Delete/{itemId}");
        Assert.Contains("Are you sure", deleteForm);
        var deleteResponse = await PostFormAsync(
            $"/LearningItems/Delete/{itemId}",
            GetAntiforgeryToken(deleteForm),
            new Dictionary<string, string>());
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        Assert.Contains("Nothing here yet", await deleteResponse.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.NotFound,
            (await _client.GetAsync($"/LearningItems/Details/{itemId}")).StatusCode);
    }

    [Fact]
    public async Task Invalid_title_is_redisplayed_and_not_saved()
    {
        await ClearItemsAsync();

        var form = await _client.GetStringAsync("/LearningItems/Create");
        var response = await PostFormAsync(
            "/LearningItems/Create",
            GetAntiforgeryToken(form),
            new Dictionary<string, string>
            {
                ["Title"] = " ",
                ["Description"] = "Should not persist"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Enter a title.", html);
        Assert.DoesNotContain("Should not persist", await GetIndexAsync());
        Assert.Equal(0, await GetItemCountAsync());
    }

    [Fact]
    public async Task Title_and_description_length_limits_are_enforced()
    {
        await ClearItemsAsync();

        var form = await _client.GetStringAsync("/LearningItems/Create");
        var response = await PostFormAsync(
            "/LearningItems/Create",
            GetAntiforgeryToken(form),
            new Dictionary<string, string>
            {
                ["Title"] = new string('t', 121),
                ["Description"] = new string('d', 2_001)
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("The title must be 120 characters or fewer.", html);
        Assert.Contains("The description must be 2,000 characters or fewer.", html);
        Assert.Equal(0, await GetItemCountAsync());
    }

    [Fact]
    public async Task Missing_item_returns_not_found_and_mutations_require_antiforgery()
    {
        await ClearItemsAsync();

        Assert.Equal(HttpStatusCode.NotFound,
            (await _client.GetAsync("/LearningItems/Details/987654")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await _client.GetAsync("/LearningItems/Edit/987654")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await _client.GetAsync("/LearningItems/Delete/987654")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await _client.GetAsync("/LearningItems/Delete")).StatusCode);

        var form = await _client.GetStringAsync("/LearningItems/Create");
        var missingEditResponse = await PostFormAsync(
            "/LearningItems/Edit/987654",
            GetAntiforgeryToken(form),
            new Dictionary<string, string> { ["Title"] = " " });
        Assert.Equal(HttpStatusCode.NotFound, missingEditResponse.StatusCode);

        var response = await _client.PostAsync(
            "/LearningItems/Delete/987654",
            new FormUrlEncodedContent(new Dictionary<string, string>()));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<HttpResponseMessage> PostFormAsync(
        string path,
        string token,
        Dictionary<string, string> values)
    {
        values["__RequestVerificationToken"] = token;
        return await _client.PostAsync(path, new FormUrlEncodedContent(values));
    }

    private async Task<string> GetIndexAsync() =>
        await _client.GetStringAsync("/LearningItems");

    private async Task ClearItemsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LearningDbContext>();
        await dbContext.LearningItems.ExecuteDeleteAsync();
    }

    private async Task<int> GetOnlyItemIdAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LearningDbContext>();
        return await dbContext.LearningItems.Select(item => item.Id).SingleAsync();
    }

    private async Task<DateTime> GetCreatedAtAsync(int id)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LearningDbContext>();
        return await dbContext.LearningItems
            .Where(item => item.Id == id)
            .Select(item => item.CreatedAtUtc)
            .SingleAsync();
    }

    private async Task<int> GetItemCountAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LearningDbContext>();
        return await dbContext.LearningItems.CountAsync();
    }

    private static string GetAntiforgeryToken(string html)
    {
        var match = Regex.Match(
            html,
            @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""",
            RegexOptions.CultureInvariant);
        Assert.True(match.Success, "The form should contain an antiforgery token.");
        return match.Groups[1].Value;
    }
}

public sealed class LearningCrudFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public LearningCrudFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<LearningDbContext>();
            services.RemoveAll<DbContextOptions<LearningDbContext>>();
            services.AddDbContext<LearningDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
