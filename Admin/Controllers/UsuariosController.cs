using Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers
{
    public class UsuariosController : Controller
    {
        public IActionResult Index()
        {
            var usuarios = new List<UsuarioViewModel>
    {
        new() { Nome = "João da Silva", CNPJ = "12.345.678/0001-90", Tipo = "Agencia" },
        new() { Nome = "Maria Oliveira", CNPJ = "23.000.000/000.22", Tipo = "Agencia" },
        new() { Nome = "Carlos Pereira", Email = "carlos.pereira@example.com", Tipo = "Familia" },
        new() { Nome = "Ana Souza", Email = "ana.souza@example.com", Tipo = "Familia" },
        new() { Nome = "Pedro Santos", CNPJ = "56.789.012/0001-33", Tipo = "Agencia" },
        new() { Nome = "Lucia Lima", CNPJ = "67.890.123/0001-44", Tipo = "Agencia" },
        new() { Nome = "Marcos Rocha", Email = "marcos.rocha@example.com", Tipo = "Familia" },
        new() { Nome = "Fernanda Costa", Email = "89.012.345/0001-66", Tipo = "Familia" }
    };

            return View(usuarios);
        }

    }
}
