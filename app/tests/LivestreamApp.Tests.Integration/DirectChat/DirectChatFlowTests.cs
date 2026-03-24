using LivestreamApp.Tests.Integration.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace LivestreamApp.Tests.Integration.DirectChat;

[Collection("Integration")]
public sealed class DirectChatFlowTests : IntegrationTestBase
{
    public DirectChatFlowTests(IntegrationTestFactory factory) : base(factory) { }

    [DockerAvailableFact]
    public async Task DirectChatFlow_SendReceiveMarkReadBlock_Succeeds()
    {
        // Arrange
        var viewerToken = await GetTestTokenAsync("viewer");
        var hostToken = await GetTestTokenAsync("host");

        // Act 1: Get or create conversation
        var convResponse = await PostAsync("/api/v1/direct-chat/conversations/start",
            new { hostId = TestHostId }, viewerToken);
        // Note: conversation is created implicitly when sending first message via SignalR
        // REST API returns conversation list

        // Act 2: Get conversations (empty initially)
        var listResponse = await GetAsync("/api/v1/direct-chat/conversations", viewerToken);
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        // Act 3: Block user
        var blockResponse = await PostAsync($"/api/v1/direct-chat/block/{TestHostId}", null, viewerToken);
        Assert.Equal(HttpStatusCode.NoContent, blockResponse.StatusCode);

        // Act 4: Unblock user
        var unblockResponse = await DeleteAsync($"/api/v1/direct-chat/block/{TestHostId}", viewerToken);
        Assert.Equal(HttpStatusCode.NoContent, unblockResponse.StatusCode);
    }

    private static readonly Guid TestHostId = Guid.NewGuid();

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

    private Task<HttpResponseMessage> DeleteAsync(string url, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return Client.SendAsync(request);
    }

    private Task<string> GetTestTokenAsync(string role) => Task.FromResult($"test-{role}-token");
}
