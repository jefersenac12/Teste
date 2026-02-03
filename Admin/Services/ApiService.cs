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
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        // Usuarios
        public async Task<List<UsuarioViewModel>> GetUsuariosAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/usuario");
            if (!response.IsSuccessStatusCode) return new List<UsuarioViewModel>();
            var json = await response.Content.ReadAsStringAsync();
            List<UsuarioViewModel>? baseLista = null;
            try
            {
                baseLista = JsonSerializer.Deserialize<List<UsuarioViewModel>>(json, _jsonOptions);
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
            var response = await _httpClient.GetAsync($"{_baseUrl}/usuario/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UsuarioViewModel>(json, _jsonOptions);
        }

        public async Task<bool> CreateUsuarioAsync(UsuarioViewModel usuario)
        {
            var json = JsonSerializer.Serialize(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/usuario", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateUsuarioAsync(UsuarioViewModel usuario)
        {
            var json = JsonSerializer.Serialize(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/usuario/{usuario.Id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateUsuarioAsync(int id, UsuarioViewModel usuario)
        {
            var json = JsonSerializer.Serialize(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_baseUrl}/usuario/{id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUsuarioAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/usuario/{id}");
            return response.IsSuccessStatusCode;
        }

        // Safras
        public async Task<List<SafraViewModel>> GetSafrasAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/safra");

            if (!response.IsSuccessStatusCode)
            {
                return new List<SafraViewModel>();
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<SafraViewModel>>(json, _jsonOptions) ?? new List<SafraViewModel>();
        }

        public async Task<SafraViewModel?> GetSafraByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/safra/{id}");
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

            var response = await _httpClient.PostAsync($"{_baseUrl}/safra", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateSafraAsync(SafraViewModel safra)
        {
            var json = JsonSerializer.Serialize(safra);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/safra/{safra.Id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateSafraAsync(int id, SafraViewModel safra)
        {
            var json = JsonSerializer.Serialize(safra);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_baseUrl}/safra/{id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteSafraAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/safra/{id}");
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
            var json = JsonSerializer.Serialize(atividade, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/Atividade", content);
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
            var agenda = JsonSerializer.Deserialize<List<AgendamentoViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<AgendamentoViewModel>();

            // Se os nomes não vierem preenchidos, buscar separadamente
            if (agenda.Any() && (agenda.Any(a => string.IsNullOrEmpty(a.SafraNome)) || agenda.Any(a => string.IsNullOrEmpty(a.AtividadeNome))))
            {
                await PopulateAgendaNomesAsync(agenda);
            }

            return agenda;
        }

        public async Task<AgendamentoViewModel?> GetAgendaByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Agenda/{id}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            var agendamento = JsonSerializer.Deserialize<AgendamentoViewModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (agendamento != null)
            {
                // Popular nomes se vierem vazios
                if (string.IsNullOrEmpty(agendamento.SafraNome) || string.IsNullOrEmpty(agendamento.AtividadeNome))
                {
                    await PopulateAgendaNomesAsync(new List<AgendamentoViewModel> { agendamento });
                }
            }

            return agendamento;
        }

        public async Task<bool> CreateAgendamentoAsync(AgendamentoViewModel agendamento)
        {
            var json = JsonSerializer.Serialize(agendamento);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/Agenda", content);
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

        private async Task PopulateAgendaNomesAsync(List<AgendamentoViewModel> agenda)
        {
            try
            {
                // Buscar todas as safras e atividades
                var safras = await GetSafrasAsync();
                var atividades = await GetAtividadesAsync();

                // Criar dicionários para lookup rápido
                var safraDict = safras.ToDictionary(s => s.Id, s => s.Nome);
                var atividadeDict = atividades.ToDictionary(a => a.Id, a => a.Nome);

                // Popular os nomes na agenda
                foreach (var agendamento in agenda)
                {
                    if (agendamento.SafraId > 0 && safraDict.TryGetValue(agendamento.SafraId, out var safraNome))
                    {
                        agendamento.SafraNome = safraNome;
                    }

                    if (agendamento.AtividadeId > 0 && atividadeDict.TryGetValue(agendamento.AtividadeId, out var atividadeNome))
                    {
                        agendamento.AtividadeNome = atividadeNome;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log erro mas não interromper o fluxo
                Console.WriteLine($"Erro ao popular nomes da agenda: {ex.Message}");
            }
        }

        private async Task PopulateReservaNomesAsync(List<ReservaViewModel> reservas)
        {
            try
            {
                // Buscar usuários e agenda
                var usuarios = await GetUsuariosAsync();
                var agenda = await GetAgendaAsync();

                // Criar dicionários para lookup rápido
                var usuarioDict = usuarios.ToDictionary(u => u.Id, u => u.Nome);
                var agendaDict = agenda.ToDictionary(a => a.Id, a => $"{a.AtividadeNome} - {a.SafraNome} ({a.DataFormatada})");

                // Popular os nomes nas reservas
                foreach (var reserva in reservas)
                {
                    if (reserva.UsuarioId > 0 && usuarioDict.TryGetValue(reserva.UsuarioId, out var usuarioNome))
                    {
                        reserva.UsuarioNome = usuarioNome;
                    }

                    if (reserva.AgendaId > 0 && agendaDict.TryGetValue(reserva.AgendaId, out var agendaDescricao))
                    {
                        reserva.AgendaDescricao = agendaDescricao;
                    }

                    // Popular tipo do usuário
                    if (reserva.UsuarioId > 0 && usuarioDict.TryGetValue(reserva.UsuarioId, out var _))
                    {
                        var usuario = usuarios.FirstOrDefault(u => u.Id == reserva.UsuarioId);
                        if (usuario != null)
                        {
                            reserva.UsuarioTipo = usuario.Tipo == 1 ? "Agência" : "Família";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log erro mas não interromper o fluxo
                Console.WriteLine($"Erro ao popular nomes das reservas: {ex.Message}");
            }
        }

        private async Task PopulatePagamentoNomesAsync(List<PagamentoViewModel> pagamentos)
        {
            try
            {
                // Buscar reservas
                var reservas = await GetReservasAsync();

                // Criar dicionários para lookup rápido
                var reservaDict = reservas.ToDictionary(r => r.Id, r => new { r.UsuarioNome, r.AgendaDescricao });

                // Popular os nomes nos pagamentos
                foreach (var pagamento in pagamentos)
                {
                    if (pagamento.ReservaId > 0 && reservaDict.TryGetValue(pagamento.ReservaId, out var reservaInfo))
                    {
                        pagamento.ClienteNome = reservaInfo.UsuarioNome;
                        pagamento.ReservaDescricao = reservaInfo.AgendaDescricao;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log erro mas não interromper o fluxo
                Console.WriteLine($"Erro ao popular nomes dos pagamentos: {ex.Message}");
            }
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
            var reservas = JsonSerializer.Deserialize<List<ReservaViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<ReservaViewModel>();

            // Se os nomes não vierem preenchidos, buscar separadamente
            if (reservas.Any() && (reservas.Any(r => string.IsNullOrEmpty(r.UsuarioNome)) || reservas.Any(r => string.IsNullOrEmpty(r.AgendaDescricao))))
            {
                await PopulateReservaNomesAsync(reservas);
            }

            return reservas;
        }

        public async Task<ReservaViewModel?> GetReservaByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Reserva/{id}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            var reserva = JsonSerializer.Deserialize<ReservaViewModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (reserva != null)
            {
                // Popular nomes se vierem vazios
                if (string.IsNullOrEmpty(reserva.UsuarioNome) || string.IsNullOrEmpty(reserva.AgendaDescricao))
                {
                    await PopulateReservaNomesAsync(new List<ReservaViewModel> { reserva });
                }
            }

            return reserva;
        }

        public async Task<bool> CreateReservaAsync(ReservaViewModel reserva)
        {
            var json = JsonSerializer.Serialize(reserva);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/Reserva", content);
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
            var pagamentos = JsonSerializer.Deserialize<List<PagamentoViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<PagamentoViewModel>();

            // Normalizar status para maiúsculas
            foreach (var pagamento in pagamentos)
            {
                if (!string.IsNullOrEmpty(pagamento.Status))
                {
                    pagamento.Status = pagamento.Status.ToUpper();
                }
            }

            // Se os nomes não vierem preenchidos, buscar separadamente
            if (pagamentos.Any() && (pagamentos.Any(p => string.IsNullOrEmpty(p.ClienteNome)) || pagamentos.Any(p => string.IsNullOrEmpty(p.ReservaDescricao))))
            {
                await PopulatePagamentoNomesAsync(pagamentos);
            }

            return pagamentos;
        }

        public async Task<PagamentoViewModel?> GetPagamentoByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Pagamento/{id}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            var pagamento = JsonSerializer.Deserialize<PagamentoViewModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (pagamento != null)
            {
                // Normalizar status para maiúsculas
                if (!string.IsNullOrEmpty(pagamento.Status))
                {
                    pagamento.Status = pagamento.Status.ToUpper();
                }

                // Popular nomes se vierem vazios
                if (string.IsNullOrEmpty(pagamento.ClienteNome) || string.IsNullOrEmpty(pagamento.ReservaDescricao))
                {
                    await PopulatePagamentoNomesAsync(new List<PagamentoViewModel> { pagamento });
                }
            }

            return pagamento;
        }

        public async Task<bool> CreatePagamentoAsync(PagamentoViewModel pagamento)
        {
            var json = JsonSerializer.Serialize(pagamento);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/Pagamento", content);
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
