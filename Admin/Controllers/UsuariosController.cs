using Admin.Models;
using Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(ApiService apiService, ILogger<UsuariosController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: /Usuarios
        public async Task<IActionResult> Index()
        {
            try
            {
                var usuarios = await _apiService.GetUsuariosAsync();
                
                if (usuarios == null || !usuarios.Any())
                {
                    ViewBag.Mensagem = "Nenhum usuário cadastrado. Cadastre o primeiro usuário para começar.";
                    return View(new List<UsuarioViewModel>());
                }

                return View(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar usuários");
                ViewBag.Erro = "Não foi possível carregar a lista de usuários. Tente novamente mais tarde.";
                return View(new List<UsuarioViewModel>());
            }
        }

        // GET: /Usuarios/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var usuario = await _apiService.GetUsuarioByIdAsync(id);
                if (usuario == null)
                {
                    return NotFound();
                }

                return View(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes do usuário {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Usuarios/Create
        public IActionResult Create(int? tipo)
        {
            if (!tipo.HasValue || (tipo.Value != 1 && tipo.Value != 2))
            {
                return RedirectToAction(nameof(Index));
            }

            var model = new UsuarioViewModel();
            model.Tipo = (byte)tipo.Value;
            ViewBag.TipoPreSelecionado = tipo.Value;
            
            return View(model);
        }

        // POST: /Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioViewModel model)
        {
            try
            {
                // Manter o ViewBag.TipoPreSelecionado para a view
                ViewBag.TipoPreSelecionado = model.Tipo;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var sucesso = await _apiService.CreateUsuarioAsync(model);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Usuário cadastrado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Não foi possível cadastrar o usuário. Verifique os dados e tente novamente.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar usuário");
                ViewBag.TipoPreSelecionado = model.Tipo; // Manter o tipo mesmo em caso de erro
                ModelState.AddModelError("", "Ocorreu um erro ao cadastrar o usuário. Tente novamente.");
                return View(model);
            }
        }

        // GET: /Usuarios/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var usuario = await _apiService.GetUsuarioByIdAsync(id);
                if (usuario == null)
                {
                    return NotFound();
                }

                return View(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar usuário para edição {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UsuarioViewModel model)
        {
            try
            {
                if (id != model.Id)
                {
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var sucesso = await _apiService.UpdateUsuarioAsync(id, model);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Usuário atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Não foi possível atualizar o usuário. Verifique os dados e tente novamente.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar usuário {Id}", id);
                ModelState.AddModelError("", "Ocorreu um erro ao atualizar o usuário. Tente novamente.");
                return View(model);
            }
        }

        // GET: /Usuarios/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var usuario = await _apiService.GetUsuarioByIdAsync(id);
                if (usuario == null)
                {
                    return NotFound();
                }

                return View(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar usuário para exclusão {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var sucesso = await _apiService.DeleteUsuarioAsync(id);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Usuário excluído com sucesso!";
                }
                else
                {
                    TempData["Erro"] = "Não foi possível excluir o usuário. Verifique se não existem registros dependentes.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir usuário {Id}", id);
                TempData["Erro"] = "Ocorreu um erro ao excluir o usuário. Tente novamente.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
