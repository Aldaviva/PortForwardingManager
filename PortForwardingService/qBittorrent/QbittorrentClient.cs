#nullable enable

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PortForwardingService.qBittorrent;

public class QbittorrentClient: IDisposable {

    private readonly HttpClient     httpClient     = new();
    private readonly Uri            baseUri        = new("http://localhost:8080/api/v2/");
    private readonly JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(new JsonSerializerSettings { Converters = { new StringEnumConverter(new SnakeCaseNamingStrategy()) } });

    /// <summary>
    /// Send an HTTP request to the qBittorrent JSON REST API and receive a response.
    /// </summary>
    /// <param name="method">HTTP verb to send</param>
    /// <param name="apiMethodSubPath">request URL path after the <c>/api/v2/</c>, such as <c>app/setPreferences</c></param>
    /// <param name="requestBody">optional object to be serialized to JSON and passed in the JSON form field, or <c>null</c> to not send a request body</param>
    /// <returns>the HTTP response</returns>
    /// <exception cref="HttpRequestException">if the response status code is ≥400</exception>
    public async Task<HttpResponseMessage> send(HttpMethod method, string apiMethodSubPath, object? requestBody = null) {
        FormUrlEncodedContent? formBody = (method == HttpMethod.Post || method == HttpMethod.Put) && requestBody != null ?
            new FormUrlEncodedContent([
                new KeyValuePair<string, string>("json", JsonConvert.SerializeObject(requestBody))
            ]) : null;

        HttpResponseMessage response = await httpClient.SendAsync(new HttpRequestMessage(method, new Uri(baseUri, apiMethodSubPath.TrimStart('/'))) { Content = formBody });
        response.EnsureSuccessStatusCode();
        return response;
    }

    /// <inheritdoc cref="send"/>
    /// <returns>deserialized response body</returns>
    /// <typeparam name="T">the type to deserialize from the response JSON body</typeparam>
    public async Task<T?> send<T>(HttpMethod method, string apiMethodSubPath, object? requestBody = null) {
        HttpResponseMessage  response       = await send(method, apiMethodSubPath, requestBody);
        using StreamReader   streamReader   = new(await response.Content.ReadAsStreamAsync(), Encoding.UTF8);
        using JsonTextReader jsonTextReader = new(streamReader);
        return jsonSerializer.Deserialize<T>(jsonTextReader);
    }

    public void Dispose() {
        httpClient.Dispose();
    }

}