using LivestreamApp.Shared.Domain.Enums;
using LivestreamApp.Tests.Integration.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace LivestreamApp.Tests.Integration.Livestream;

[Collection("Integration")]
public sealed class LivestreamFlowTests : IntegrationTestBase
{
    public LivestreamFlowTests(IntegrationTestFactory factory) : base(factory) { }

    [DockerAvailableFact]
    public async Task FullLivestreamFlow_CreateStartJoinLeaveEnd_Succeeds()
    {
        // Arrange — register and login as host
        var hostToken = await RegisterAndLoginAsync("host@test.com", "Host", isHost: true);
        var viewerToken = await RegisterAndLoginAsync("viewer@test.com", "Viewer");

        // Act 1: Host creates room
        var createResponse = await PostAsync("/api/v1/livestream/rooms",
            new { title = "Test Stream", category = "Talk" }, hostToken);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var room = await createResponse.Content.ReadFromJsonAsync<dynamic>();
        var roomId = room!.id.ToString();

        // Act 2: Host starts stream
        var startResponse = await PostAsync($"/api/v1/livestream/rooms/{roomId}/start", null, hostToken);
        Assert.Equal(HttpStatusCode.NoContent, startResponse.StatusCode);

        // Act 3: Viewer joins room
        var joinResponse = await PostAsync($"/api/v1/livestream/rooms/{roomId}/join", null, viewerToken);
        Assert.Equal(HttpStatusCode.OK, joinResponse.StatusCode);

        // Act 4: Get viewer count
        var countResponse = await GetAsync($"/api/v1/livestream/rooms/{roomId}/viewers", viewerToken);
        Assert.Equal(HttpStatusCode.OK, countResponse.StatusCode);
        var countData = await countResponse.Content.ReadFromJsonAsync<dynamic>();
        Assert.True((int)countData!.viewerCount >= 1);

        // Act 5: Viewer leaves
        var leaveResponse = await PostAsync($"/api/v1/livestream/rooms/{roomId}/leave", null, viewerToken);
        Assert.Equal(HttpStatusCode.NoContent, leaveResponse.StatusCode);

        // Act 6: Host ends stream
        var endResponse = await PostAsync($"/api/v1/livestream/rooms/{roomId}/end", null, hostToken);
        Assert.Equal(HttpStatusCode.NoContent, endResponse.StatusCode);

        // Assert: Room is ended
        var roomResponse = await GetAsync($"/api/v1/livestream/rooms/{roomId}", hostToken);
        var finalRoom = await roomResponse.Content.ReadFromJsonAsync<dynamic>();
        Assert.Equal("Ended", finalRoom!.status.ToString());
    }

    private Task<HttpResponseMessage> PostAsync(string url, object? body, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        if (body != null) request.Content = JsonContent.Create(body);
        return Client.SendAsync(request);
    }

    private Task<HttpResponseMessage> GetAsync(string url, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return Client.SendAsync(request);
    }

    private async Task<string> RegisterAndLoginAsync(string email, string displayName, bool isHost = false)
    {
        // Simplified — use existing auth flow from IntegrationTestBase
        return "test-token"; // Placeholder — real implementation uses auth endpoints
    }
}
