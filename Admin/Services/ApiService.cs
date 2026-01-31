using System.Text;
using System.Text.Json;
using Admin.Models;

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

        // Usuários
        public async Task<List<UsuarioViewModel>> GetUsuariosAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Usuario");

            if (!response.IsSuccessStatusCode)
            {
                return new List<UsuarioViewModel>();
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<UsuarioViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<UsuarioViewModel>();
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

        public async Task<bool> CreateAgendamentoAsync(AgendamentoViewModel agendamento)
        {
            var json = JsonSerializer.Serialize(agendamento);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/Agenda/cadastrar", content);
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

        public async Task<bool> UpdatePagamentoStatusAsync(int pagamentoId, string status)
        {
            var json = JsonSerializer.Serialize(new { Status = status });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}/Pagamento/{pagamentoId}/status", content);
            return response.IsSuccessStatusCode;
        }
    }
}
