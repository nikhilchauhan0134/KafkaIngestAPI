using System.Net;
using Xunit;
using System.Net.Http.Json;
using System.Text;
using DataCaptureApi.Data;
using DataCaptureApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataCaptureApi.Tests;

public sealed class ItemsEndpointTests : IClassFixture<DataCaptureApiFactory>
{
    private readonly DataCaptureApiFactory _factory;

    public ItemsEndpointTests(DataCaptureApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_valid_item_returns_created_record_with_generated_id()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/items", new
        {
            name = "Sensor reading",
            description = "Line 4 temperature sample"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var item = await response.Content.ReadFromJsonAsync<Item>();
        Assert.NotNull(item);
        Assert.True(item.Id > 0);
        Assert.Equal("Sensor reading", item.Name);
        Assert.Equal("Line 4 temperature sample", item.Description);
        Assert.Equal($"/api/items/{item.Id}", response.Headers.Location?.OriginalString);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var saved = await db.Items.SingleAsync(row => row.Id == item.Id);
        Assert.Equal(item.Name, saved.Name);
        Assert.Equal(item.Description, saved.Description);
    }

    [Fact]
    public async Task Post_two_items_assigns_sequential_ids()
    {
        var client = _factory.CreateClient();

        var firstResponse = await client.PostAsJsonAsync("/api/items", new
        {
            name = "First",
            description = "First record"
        });
        var secondResponse = await client.PostAsJsonAsync("/api/items", new
        {
            name = "Second",
            description = "Second record"
        });

        var first = await firstResponse.Content.ReadFromJsonAsync<Item>();
        var second = await secondResponse.Content.ReadFromJsonAsync<Item>();

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(first.Id + 1, second.Id);
    }

    [Fact]
    public async Task Post_trims_surrounding_whitespace()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/items", new
        {
            name = "  Widget  ",
            description = "  A stored widget  "
        });

        var item = await response.Content.ReadFromJsonAsync<Item>();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(item);
        Assert.Equal("Widget", item.Name);
        Assert.Equal("A stored widget", item.Description);
    }

    [Theory]
    [InlineData("""{"description":"missing name"}""")]
    [InlineData("""{"name":"missing description"}""")]
    [InlineData("""{"name":"   ","description":"present"}""")]
    [InlineData("""{"name":"present","description":"   "}""")]
    [InlineData("{")]
    [InlineData("")]
    public async Task Post_invalid_payload_returns_bad_request(string json)
    {
        var client = _factory.CreateClient();
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/items", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_name_longer_than_200_characters_returns_bad_request()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/items", new
        {
            name = new string('a', 201),
            description = "ok"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_description_longer_than_4000_characters_returns_bad_request()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/items", new
        {
            name = "ok",
            description = new string('b', 4001)
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
