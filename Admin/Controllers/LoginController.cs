using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Entrar(string cnpj, string senha)
        {
            // Validações básicas
            if (string.IsNullOrWhiteSpace(cnpj) || string.IsNullOrWhiteSpace(senha))
            {
                TempData["Erro"] = "CNPJ e senha são obrigatórios.";
                return RedirectToAction("Index");
            }

            // Normaliza apenas dígitos do CNPJ para comparação
            var cnpjLimpo = new string(cnpj.Where(char.IsDigit).ToArray());

            // Simulação de login (substituir por verificação no banco/serviço)
            if (cnpjLimpo == "12345678000199" && senha == "123")
            {
                return RedirectToAction("Index", "Home");
            }

            TempData["Erro"] = "CNPJ ou senha inválidos.";
            return RedirectToAction("Index");
        }
    }
}
