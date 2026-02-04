using System.Text.Json;
using System.Net;

namespace Teste.Services
{
    public class ApiService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _baseUrl;

        public ApiService()
        {
            _baseUrl = "http://tiijeferson.runasp.net/api";
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        // Métodos Genéricos para qualquer entidade
        public async Task<List<T>> GetAllAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}{endpoint}");
                
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return new List<T>();
                
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<T>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Falha na conexão: {ex.Message}");
                return new List<T>();
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Tempo esgotado. Tente novamente.");
                return new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar dados: {ex.Message}");
                return new List<T>();
            }
        }

        public async Task<T?> GetByIdAsync<T>(string endpoint, int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}{endpoint}/{id}");
                
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return default(T);
                
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Falha na conexão: {ex.Message}");
                return default(T);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Tempo esgotado. Tente novamente.");
                return default(T);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar dados: {ex.Message}");
                return default(T);
            }
        }

        public async Task<T?> CreateAsync<T>(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_baseUrl}{endpoint}", content);
                response.EnsureSuccessStatusCode();
                
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Falha na conexão: {ex.Message}");
                return default(T);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Tempo esgotado. Tente novamente.");
                return default(T);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar dados: {ex.Message}");
                return default(T);
            }
        }

        public async Task<T?> UpdateAsync<T>(string endpoint, int id, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"{_baseUrl}{endpoint}/{id}", content);
                response.EnsureSuccessStatusCode();
                
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Falha na conexão: {ex.Message}");
                return default(T);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Tempo esgotado. Tente novamente.");
                return default(T);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar dados: {ex.Message}");
                return default(T);
            }
        }

        public async Task<bool> DeleteAsync(string endpoint, int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}{endpoint}/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Falha na conexão: {ex.Message}");
                return false;
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Tempo esgotado. Tente novamente.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao excluir dados: {ex.Message}");
                return false;
            }
        }

        // Métodos Específicos para casos de uso comuns

        // Login
        public async Task<T?> LoginAsync<T>(string email, string senha, string tipo)
        {
            try
            {
                var loginData = new { Email = email, Senha = senha, Tipo = tipo };
                return await CreateAsync<T>("/Usuario/login", loginData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no login: {ex.Message}");
                return default(T);
            }
        }

        // Buscar por filtro
        public async Task<List<T>> GetByFilterAsync<T>(string endpoint, string filtro)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}{endpoint}/{filtro}");
                
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return new List<T>();
                
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar por filtro: {ex.Message}");
                return new List<T>();
            }
        }

        // Gerar PIX
        public async Task<string?> GerarPixAsync(int reservaId, decimal valor)
        {
            try
            {
                var pixData = new { reservaId = reservaId, valor = valor };
                var response = await CreateAsync<object>("/Pagamento/gerar-pix", pixData);
                
                if (response != null)
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(response);
                    using var document = System.Text.Json.JsonDocument.Parse(json);
                    
                    if (document.RootElement.TryGetProperty("qrCode", out var qrCodeElement))
                        return qrCodeElement.GetString();
                    else if (document.RootElement.TryGetProperty("pixCode", out var pixCodeElement))
                        return pixCodeElement.GetString();
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao gerar PIX: {ex.Message}");
                return null;
            }
        }
    }
}
