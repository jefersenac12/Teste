using System.Text;
using System.Text.Json;
using Admin.Models;
using System.Linq;

namespace Admin.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://tiijeferson.runasp.net/api";

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Usuarios
        public async Task<List<UsuarioViewModel>> GetUsuariosAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Usuario");
            if (!response.IsSuccessStatusCode) return new List<UsuarioViewModel>();
            var json = await response.Content.ReadAsStringAsync();
            List<UsuarioViewModel>? baseLista = null;
            try
            {
                baseLista = JsonSerializer.Deserialize<List<UsuarioViewModel>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (baseLista != null && !baseLista.Any(u => u.Tipo == 1 && string.IsNullOrWhiteSpace(u.CNPJ)))
                {
                    return baseLista;
                }
            }
            catch { }
            var usuarios = new List<UsuarioViewModel>();
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in doc.RootElement.EnumerateArray())
                    {
                        var src = el;
                        if (el.TryGetProperty("usuario", out var nested) && nested.ValueKind == JsonValueKind.Object)
                        {
                            src = nested;
                        }
                        var id = TryGetInt(el, "id", "UsuarioId");
                        var nome = TryGetString(src, "Nome", "nome");
                        var telefone = TryGetString(src, "Telefone", "telefone");
                        var email = TryGetString(src, "Email", "email");
                        var cnpj = TryGetString(src, "CNPJ", "cnpj");
                        var tipo = TryGetByte(src, "Tipo", "tipo");
                        if (tipo == 0) tipo = string.IsNullOrWhiteSpace(cnpj) ? (byte)2 : (byte)1;
                        usuarios.Add(new UsuarioViewModel
                        {
                            Id = id,
                            Nome = nome ?? string.Empty,
                            Telefone = telefone ?? string.Empty,
                            Email = string.IsNullOrWhiteSpace(email) ? null : email,
                            CNPJ = string.IsNullOrWhiteSpace(cnpj) ? null : cnpj,
                            Senha = string.Empty,
                            Tipo = tipo
                        });
                    }
                    return usuarios;
                }
            }
            catch { }
            return baseLista ?? new List<UsuarioViewModel>();
        }

        public async Task<UsuarioViewModel?> GetUsuarioByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Usuario/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UsuarioViewModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<bool> CreateUsuarioAsync(UsuarioViewModel usuario)
        {
            var json = JsonSerializer.Serialize(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/Usuario/cadastrar", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateUsuarioAsync(UsuarioViewModel usuario)
        {
            var json = JsonSerializer.Serialize(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/Usuario/{usuario.Id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateUsuarioAsync(int id, UsuarioViewModel usuario)
        {
            var json = JsonSerializer.Serialize(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_baseUrl}/Usuario/{id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUsuarioAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/Usuario/{id}");
            return response.IsSuccessStatusCode;
        }

        // Safras
        public async Task<List<SafraViewModel>> GetSafrasAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Safra");

            if (!response.IsSuccessStatusCode)
            {
                return new List<SafraViewModel>();
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<SafraViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<SafraViewModel>();
        }

        public async Task<SafraViewModel?> GetSafraByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Safra/{id}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SafraViewModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<bool> CreateSafraAsync(SafraViewModel safra)
        {
            var json = JsonSerializer.Serialize(safra);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/Safra/cadastrar", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateSafraAsync(SafraViewModel safra)
        {
            var json = JsonSerializer.Serialize(safra);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/Safra/{safra.Id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateSafraAsync(int id, SafraViewModel safra)
        {
            var json = JsonSerializer.Serialize(safra);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_baseUrl}/Safra/{id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteSafraAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/Safra/{id}");
            return response.IsSuccessStatusCode;
        }

        // Atividades
        public async Task<List<AtividadeViewModel>> GetAtividadesAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Atividade");

            if (!response.IsSuccessStatusCode)
            {
                return new List<AtividadeViewModel>();
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<AtividadeViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<AtividadeViewModel>();
        }

        public async Task<AtividadeViewModel?> GetAtividadeByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Atividade/{id}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AtividadeViewModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<bool> CreateAtividadeAsync(AtividadeViewModel atividade)
        {
            var json = JsonSerializer.Serialize(atividade);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/Atividade/cadastrar", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAtividadeAsync(AtividadeViewModel atividade)
        {
            var json = JsonSerializer.Serialize(atividade);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/Atividade/{atividade.Id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAtividadeAsync(int id, AtividadeViewModel atividade)
        {
            var json = JsonSerializer.Serialize(atividade);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_baseUrl}/Atividade/{id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAtividadeAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/Atividade/{id}");
            return response.IsSuccessStatusCode;
        }

        // Agenda
        public async Task<List<AgendamentoViewModel>> GetAgendaAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Agenda");

            if (!response.IsSuccessStatusCode)
            {
                return new List<AgendamentoViewModel>();
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<AgendamentoViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<AgendamentoViewModel>();
        }

        public async Task<AgendamentoViewModel?> GetAgendaByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Agenda/{id}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AgendamentoViewModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<bool> CreateAgendamentoAsync(AgendamentoViewModel agendamento)
        {
            var json = JsonSerializer.Serialize(agendamento);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/Agenda/cadastrar", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAgendaAsync(int id, AgendamentoViewModel agendamento)
        {
            var json = JsonSerializer.Serialize(agendamento);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_baseUrl}/Agenda/{id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAgendaAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/Agenda/{id}");
            return response.IsSuccessStatusCode;
        }

        // Reservas
        public async Task<List<ReservaViewModel>> GetReservasAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Reserva");

            if (!response.IsSuccessStatusCode)
            {
                return new List<ReservaViewModel>();
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ReservaViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<ReservaViewModel>();
        }

        public async Task<ReservaViewModel?> GetReservaByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Reserva/{id}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ReservaViewModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<bool> CreateReservaAsync(ReservaViewModel reserva)
        {
            var json = JsonSerializer.Serialize(reserva);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/Reserva/cadastrar", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateReservaAsync(int id, ReservaViewModel reserva)
        {
            var json = JsonSerializer.Serialize(reserva);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_baseUrl}/Reserva/{id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteReservaAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/Reserva/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateReservaStatusAsync(int reservaId, string status)
        {
            var json = JsonSerializer.Serialize(new { Status = status });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/Reserva/{reservaId}/status", content);
            return response.IsSuccessStatusCode;
        }

        // Pagamentos
        public async Task<List<PagamentoViewModel>> GetPagamentosAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Pagamento");

            if (!response.IsSuccessStatusCode)
            {
                return new List<PagamentoViewModel>();
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<PagamentoViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<PagamentoViewModel>();
        }

        public async Task<PagamentoViewModel?> GetPagamentoByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Pagamento/{id}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PagamentoViewModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<bool> CreatePagamentoAsync(PagamentoViewModel pagamento)
        {
            var json = JsonSerializer.Serialize(pagamento);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/Pagamento/cadastrar", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdatePagamentoAsync(int id, PagamentoViewModel pagamento)
        {
            var json = JsonSerializer.Serialize(pagamento);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_baseUrl}/Pagamento/{id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeletePagamentoAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/Pagamento/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdatePagamentoStatusAsync(int pagamentoId, string status)
        {
            var json = JsonSerializer.Serialize(new { Status = status });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/Pagamento/{pagamentoId}/status", content);
            return response.IsSuccessStatusCode;
        }

        static int TryGetInt(JsonElement el, params string[] names)
        {
            foreach (var n in names)
            {
                if (el.TryGetProperty(n, out var p))
                {
                    if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var v)) return v;
                    if (p.ValueKind == JsonValueKind.String && int.TryParse(p.GetString(), out var v2)) return v2;
                }
            }
            return 0;
        }

        static string? TryGetString(JsonElement el, params string[] names)
        {
            foreach (var n in names)
            {
                if (el.TryGetProperty(n, out var p))
                {
                    if (p.ValueKind == JsonValueKind.String) return p.GetString();
                }
            }
            return null;
        }

        static byte TryGetByte(JsonElement el, params string[] names)
        {
            foreach (var n in names)
            {
                if (el.TryGetProperty(n, out var p))
                {
                    if (p.ValueKind == JsonValueKind.Number && p.TryGetByte(out var v)) return v;
                    if (p.ValueKind == JsonValueKind.String && byte.TryParse(p.GetString(), out var v2)) return v2;
                }
            }
            return 0;
        }
    }
}
