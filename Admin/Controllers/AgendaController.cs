using Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers
{
    public class AgendaController : Controller
    {
        public IActionResult Index()
        {
            var agendamentos = new List<AgendamentoViewModel>
            {
                new() { Id = 1, Atividade = "Colheita de Uva", Safra = "Janeiro - uva, goiaba, morango, lichia", Data = new DateTime(2024, 1, 15), Hora = new TimeSpan(8, 0, 0), VagasTotais = 20, VagasDisponiveis = 5 },
                new() { Id = 2, Atividade = "Colheita de Goiaba", Safra = "Fevereiro - uva, goiaba, morango", Data = new DateTime(2024, 2, 20), Hora = new TimeSpan(7, 30, 0), VagasTotais = 15, VagasDisponiveis = 10 },
                new() { Id = 3, Atividade = "Colheita de Morango", Safra = "Março - uva, goiaba", Data = new DateTime(2024, 3, 10), Hora = new TimeSpan(10, 0, 0), VagasTotais = 5, VagasDisponiveis = 0 },
                new() { Id = 4, Atividade = "Colheita de Lichia", Safra = "Janeiro - uva, goiaba, morango, lichia", Data = new DateTime(2024, 1, 25), Hora = new TimeSpan(6, 0, 0), VagasTotais = 25, VagasDisponiveis = 12 },
                new() { Id = 5, Atividade = "Plantio de Uva", Safra = "Maio - uva, goiaba, morango", Data = new DateTime(2024, 5, 1), Hora = new TimeSpan(7, 0, 0), VagasTotais = 30, VagasDisponiveis = 18 },
                new() { Id = 6, Atividade = "Aplicação de Fertilizante", Safra = "Abril - goiaba, morango", Data = new DateTime(2024, 4, 15), Hora = new TimeSpan(9, 30, 0), VagasTotais = 12, VagasDisponiveis = 3 },
                new() { Id = 7, Atividade = "Colheita de Goiaba", Safra = "Maio - uva, goiaba, morango", Data = new DateTime(2024, 5, 20), Hora = new TimeSpan(8, 30, 0), VagasTotais = 18, VagasDisponiveis = 7 },
                new() { Id = 8, Atividade = "Irrigação", Safra = "Fevereiro - uva, goiaba, morango", Data = new DateTime(2024, 2, 28), Hora = new TimeSpan(14, 0, 0), VagasTotais = 10, VagasDisponiveis = 2 }
            };

            return View(agendamentos);
        }
        public IActionResult Gerenciar()
        {
            return View();
        }
    }
}
