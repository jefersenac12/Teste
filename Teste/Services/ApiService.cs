using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Teste.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ApiService(string? overrideBaseUrl = null)
    {
        string defaultUrl;

#if ANDROID
        // Android sempre usa o servidor remoto via HTTP (sem certificado)
        defaultUrl = "http://tiijeferson.runasp.net/";
#else
#if DEBUG
        // Desenvolvimento local (Windows/macOS)
        defaultUrl = "https://localhost:7064/"; // ajuste a porta conforme sua API local
#else
        // Produção (runasp.net)
        defaultUrl = "https://tiijeferson.runasp.net/";
#endif
#endif

        _baseUrl = overrideBaseUrl ?? defaultUrl;

        var handler = new HttpClientHandler();
        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(_baseUrl)
        };
    }

    public async Task<HttpResponseMessage> PostAsync(string relativePath, HttpContent content, CancellationToken cancellationToken = default)
    {
        return await _httpClient.PostAsync(relativePath, content, cancellationToken).ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> GetAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync(relativePath, cancellationToken).ConfigureAwait(false);
    }

    public async Task<T?> GetJsonAsync<T>(string relativePath, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(relativePath, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<T?> PostJsonAsync<T>(string relativePath, object data, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(relativePath, content, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<HttpResponseMessage> PutAsync(string relativePath, HttpContent content, CancellationToken cancellationToken = default)
    {
        return await _httpClient.PutAsync(relativePath, content, cancellationToken).ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        return await _httpClient.DeleteAsync(relativePath, cancellationToken).ConfigureAwait(false);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}