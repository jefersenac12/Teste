using Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers
{
    public class SafrasController : Controller
    {
        public IActionResult Index()
        {
            var safras = new List<SafraViewModel>
            {
                new() { Id = 1, Nome = "Janeiro - uva, goiaba, morango, lichia", DataInicio = new DateTime(2024, 1, 1), DataFim = new DateTime(2024, 1, 31) },
                new() { Id = 2, Nome = "Fevereiro - uva, goiaba, morango", DataInicio = new DateTime(2024, 2, 1), DataFim = new DateTime(2024, 2, 29) },
                new() { Id = 3, Nome = "Março - uva, goiaba", DataInicio = new DateTime(2024, 3, 1), DataFim = new DateTime(2024, 3, 31) },
                new() { Id = 4, Nome = "Abril - goiaba, morango", DataInicio = new DateTime(2024, 4, 1), DataFim = new DateTime(2024, 4, 30) },
                new() { Id = 5, Nome = "Maio - uva, goiaba, morango", DataInicio = new DateTime(2024, 5, 1), DataFim = new DateTime(2024, 5, 31) }
            };

            return View(safras);
        }
    }
}