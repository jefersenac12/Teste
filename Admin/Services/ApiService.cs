using System.Text;
using System.Text.Json;
using Admin.Models;

namespace Admin.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://tiijeferson.runasp.net/api";
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Admin-ASPNET");
        }

        #region Usuarios
        public async Task<List<UsuarioViewModel>?> GetUsuariosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Usuario");
                if (!response.IsSuccessStatusCode)
                    return new List<UsuarioViewModel>();

                var content = await response.Content.ReadAsStringAsync();
                var usuariosApi = JsonSerializer.Deserialize<List<dynamic>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (usuariosApi == null)
                    return new List<UsuarioViewModel>();

                // Mapear dados da API para nosso ViewModel
                var usuarios = new List<UsuarioViewModel>();
                foreach (var usuarioApi in usuariosApi)
                {
                    usuarios.Add(new UsuarioViewModel
                    {
                        Id = usuarioApi.id,
                        Nome = usuarioApi.nome,
                        Telefone = usuarioApi.telefone,
                        Email = usuarioApi.email,
                        CNPJ = usuarioApi.cnpj,
                        Senha = usuarioApi.senha,
                        Tipo = usuarioApi.tipo == 2 ? "Agencia" : "Familia"
                    });
                }

                return usuarios;
            }
            catch
            {
                return new List<UsuarioViewModel>();
            }
        }

        public async Task<UsuarioViewModel?> GetUsuarioByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Usuario/{id}");
                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();
                var usuarioApi = JsonSerializer.Deserialize<dynamic>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (usuarioApi == null)
                    return null;

                // Mapear dados da API para nosso ViewModel
                return new UsuarioViewModel
                {
                    Id = usuarioApi.id,
                    Nome = usuarioApi.nome,
                    Telefone = usuarioApi.telefone,
                    Email = usuarioApi.email,
                    CNPJ = usuarioApi.cnpj,
                    Senha = usuarioApi.senha,
                    Tipo = usuarioApi.tipo == 2 ? "Agencia" : "Familia"
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> CreateUsuarioAsync(UsuarioViewModel usuario)
        {
            try
            {
                // Converter ViewModel para formato da API
                var usuarioApi = new
                {
                    nome = usuario.Nome,
                    telefone = usuario.Telefone,
                    email = usuario.Email,
                    cnpj = usuario.CNPJ,
                    senha = usuario.Senha,
                    tipo = usuario.Tipo?.ToLower() == "agencia" ? 2 : 1
                };

                var json = JsonSerializer.Serialize(usuarioApi);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var endpoint = usuario.Tipo?.ToLower() == "agencia" 
                    ? $"{_baseUrl}/Usuario/cadastrarAgencia"
                    : $"{_baseUrl}/Usuario/cadastrarFamilia";

                var response = await _httpClient.PostAsync(endpoint, content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateUsuarioAsync(int id, UsuarioViewModel usuario)
        {
            try
            {
                // Converter ViewModel para formato da API
                var usuarioApi = new
                {
                    id = id,
                    nome = usuario.Nome,
                    telefone = usuario.Telefone,
                    email = usuario.Email,
                    cnpj = usuario.CNPJ,
                    senha = usuario.Senha,
                    tipo = usuario.Tipo?.ToLower() == "agencia" ? 2 : 1
                };

                var json = JsonSerializer.Serialize(usuarioApi);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUrl}/Usuario/{id}", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUsuarioAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/Usuario/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Safras
        public async Task<List<SafraViewModel>?> GetSafrasAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Safra");
                if (!response.IsSuccessStatusCode)
                    return new List<SafraViewModel>();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<SafraViewModel>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return new List<SafraViewModel>();
            }
        }

        public async Task<SafraViewModel?> GetSafraByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Safra/{id}");
                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<SafraViewModel>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> CreateSafraAsync(SafraViewModel safra)
        {
            try
            {
                var json = JsonSerializer.Serialize(safra);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/Safra", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateSafraAsync(int id, SafraViewModel safra)
        {
            try
            {
                var json = JsonSerializer.Serialize(safra);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUrl}/Safra/{id}", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteSafraAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/Safra/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Atividades
        public async Task<List<AtividadeViewModel>?> GetAtividadesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Atividade");
                if (!response.IsSuccessStatusCode)
                    return new List<AtividadeViewModel>();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<AtividadeViewModel>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return new List<AtividadeViewModel>();
            }
        }

        public async Task<AtividadeViewModel?> GetAtividadeByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Atividade/{id}");
                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<AtividadeViewModel>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> CreateAtividadeAsync(AtividadeViewModel atividade)
        {
            try
            {
                var json = JsonSerializer.Serialize(atividade);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/Atividade", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAtividadeAsync(int id, AtividadeViewModel atividade)
        {
            try
            {
                var json = JsonSerializer.Serialize(atividade);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUrl}/Atividade/{id}", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAtividadeAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/Atividade/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Agenda
        public async Task<List<AgendamentoViewModel>?> GetAgendaAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/agenda");
                if (!response.IsSuccessStatusCode)
                    return new List<AgendamentoViewModel>();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<AgendamentoViewModel>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return new List<AgendamentoViewModel>();
            }
        }

        public async Task<AgendamentoViewModel?> GetAgendaByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/agenda/{id}");
                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<AgendamentoViewModel>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> CreateAgendaAsync(AgendamentoViewModel agenda)
        {
            try
            {
                var json = JsonSerializer.Serialize(agenda);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/agenda", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAgendaAsync(int id, AgendamentoViewModel agenda)
        {
            try
            {
                var json = JsonSerializer.Serialize(agenda);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUrl}/agenda/{id}", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAgendaAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/agenda/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Reservas
        public async Task<List<ReservaViewModel>?> GetReservasAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Reserva");
                if (!response.IsSuccessStatusCode)
                    return new List<ReservaViewModel>();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ReservaViewModel>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return new List<ReservaViewModel>();
            }
        }

        public async Task<ReservaViewModel?> GetReservaByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Reserva/{id}");
                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ReservaViewModel>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> CreateReservaAsync(ReservaViewModel reserva)
        {
            try
            {
                var json = JsonSerializer.Serialize(reserva);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/Reserva", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateReservaAsync(int id, ReservaViewModel reserva)
        {
            try
            {
                var json = JsonSerializer.Serialize(reserva);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUrl}/Reserva/{id}", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteReservaAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/Reserva/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Pagamentos
        public async Task<List<PagamentoViewModel>?> GetPagamentosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Pagamento");
                if (!response.IsSuccessStatusCode)
                    return new List<PagamentoViewModel>();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<PagamentoViewModel>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return new List<PagamentoViewModel>();
            }
        }

        public async Task<PagamentoViewModel?> GetPagamentoByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Pagamento/{id}");
                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PagamentoViewModel>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> CreatePagamentoAsync(PagamentoViewModel pagamento)
        {
            try
            {
                var json = JsonSerializer.Serialize(pagamento);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/Pagamento", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdatePagamentoAsync(int id, PagamentoViewModel pagamento)
        {
            try
            {
                var json = JsonSerializer.Serialize(pagamento);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_baseUrl}/Pagamento/{id}", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeletePagamentoAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/Pagamento/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}
