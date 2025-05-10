#nullable enable

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Unfucked;
using Unfucked.HTTP;
using Unfucked.HTTP.Config;

namespace PortForwardingService.qBittorrent;

public class QbittorrentClient: IDisposable {

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) } };

    private readonly HttpClient httpClient = new UnfuckedHttpClient { Timeout = TimeSpan.FromSeconds(5) };
    private readonly WebTarget  api;

    public QbittorrentClient() {
        api = httpClient
            .Property(PropertyKey.JsonSerializerOptions, JsonOptions)
            .Target(new UrlBuilder("http", "localhost", 8080).Path("/api/v2"));
    }

    /// <summary>
    /// Send an HTTP request to the qBittorrent JSON REST API and receive a response.
    /// </summary>
    /// <param name="verb">HTTP verb to send</param>
    /// <param name="apiMethodSubPath">request URL path after the <c>/api/v2/</c>, such as <c>app/setPreferences</c></param>
    /// <param name="requestBody">optional object to be serialized to JSON and passed in the JSON form field, or <c>null</c> to not send a request body</param>
    /// <returns>the HTTP response</returns>
    /// <exception cref="HttpRequestException">if the response status code is ≥400</exception>
    public async Task<HttpResponseMessage> send(HttpMethod verb, string apiMethodSubPath, object? requestBody = null) =>
        await api.Path(sanitizeSubpath(apiMethodSubPath)).Send(verb, createBody(verb, requestBody));

    /// <inheritdoc cref="send"/>
    /// <returns>deserialized response body</returns>
    /// <typeparam name="T">the type to deserialize from the response JSON body</typeparam>
    public async Task<T?> send<T>(HttpMethod verb, string apiMethodSubPath, object? requestBody = null) =>
        await api.Path(sanitizeSubpath(apiMethodSubPath)).Send<T>(verb, createBody(verb, requestBody));

    private static string sanitizeSubpath(string apiMethodSubPath) => apiMethodSubPath.TrimStart('/');

    private static FormUrlEncodedContent? createBody(HttpMethod verb, object? requestBody) =>
        (verb == HttpMethod.Post || verb == HttpMethod.Put) && requestBody != null ? new FormUrlEncodedContent([
            new KeyValuePair<string, string>("json", JsonSerializer.Serialize(requestBody, JsonOptions)) // that's right, it's JSON inside form URL-encoding
        ]) : null;

    public void Dispose() {
        httpClient.Dispose();
    }

}