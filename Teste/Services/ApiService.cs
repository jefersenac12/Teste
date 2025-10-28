using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Teste.Services;

public class ApiService : IDisposable
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
        // Desenvolvimento local (Windows/macOS) - também sem certificado para testes
        defaultUrl = "http://localhost:7064/"; // HTTP para evitar problemas de certificado
#else
        // Produção - HTTP sem certificado
        defaultUrl = "http://tiijeferson.runasp.net/";
#endif
#endif

        _baseUrl = overrideBaseUrl ?? defaultUrl;

        // Configuração do HttpClientHandler para ignorar certificados SSL
        var handler = new HttpClientHandler();
        
        // Ignora erros de certificado SSL (apenas para desenvolvimento/teste)
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout = TimeSpan.FromSeconds(30) // Timeout de 30 segundos
        };

        // Headers padrão
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "TesteApp/1.0");
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