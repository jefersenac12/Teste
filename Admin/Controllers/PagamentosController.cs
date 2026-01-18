using Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers
{
    public class PagamentosController : Controller
    {
        public IActionResult Index()
        {
            var pagamentos = new List<PagamentoViewModel>
            {
                new() { Id = 1, IdReserva = "#RES-2941", Cliente = "Maria Oliveira", Valor = 450.00m, Status = "Pago", Data = new DateTime(2023, 10, 12), NumeroTransacao = "TXN-001" },
                new() { Id = 2, IdReserva = "#RES-2942", Cliente = "Carlos Souza", Valor = 1200.00m, Status = "Pendente", Data = new DateTime(2023, 10, 12), NumeroTransacao = "TXN-002" },
                new() { Id = 3, IdReserva = "#RES-2938", Cliente = "Ana Paula", Valor = 320.00m, Status = "Pago", Data = new DateTime(2023, 10, 11), NumeroTransacao = "TXN-003" },
                new() { Id = 4, IdReserva = "#RES-2930", Cliente = "João Silva", Valor = 150.00m, Status = "Cancelado", Data = new DateTime(2023, 10, 10), NumeroTransacao = "TXN-004" },
                new() { Id = 5, IdReserva = "#RES-2925", Cliente = "Roberto Costa", Valor = 850.00m, Status = "Pago", Data = new DateTime(2023, 10, 8), NumeroTransacao = "TXN-005" },
                new() { Id = 6, IdReserva = "#RES-2920", Cliente = "Fernanda Lima", Valor = 600.00m, Status = "Pendente", Data = new DateTime(2023, 10, 7), NumeroTransacao = "TXN-006" },
                new() { Id = 7, IdReserva = "#RES-2915", Cliente = "Pedro Santos", Valor = 750.00m, Status = "Pago", Data = new DateTime(2023, 10, 5), NumeroTransacao = "TXN-007" },
                new() { Id = 8, IdReserva = "#RES-2910", Cliente = "Juliana Alves", Valor = 420.00m, Status = "Pago", Data = new DateTime(2023, 10, 4), NumeroTransacao = "TXN-008" },
                new() { Id = 9, IdReserva = "#RES-2905", Cliente = "Marcos Rocha", Valor = 980.00m, Status = "Pendente", Data = new DateTime(2023, 10, 3), NumeroTransacao = "TXN-009" },
                new() { Id = 10, IdReserva = "#RES-2900", Cliente = "Patricia Mendes", Valor = 350.00m, Status = "Cancelado", Data = new DateTime(2023, 10, 2), NumeroTransacao = "TXN-010" }
            };

            return View(pagamentos);
        }
    }
}
