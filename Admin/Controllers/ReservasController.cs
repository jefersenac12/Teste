using Admin.Models;
using Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers
{
    public class ReservasController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<ReservasController> _logger;

        public ReservasController(ApiService apiService, ILogger<ReservasController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: /Reservas
        public async Task<IActionResult> Index()
        {
            try
            {
                var reservas = await _apiService.GetReservasAsync();
                
                if (reservas == null || !reservas.Any())
                {
                    ViewBag.Mensagem = "Nenhuma reserva encontrada. As reservas aparecerão aqui quando os usuários fizerem agendamentos.";
                    return View(new List<ReservaViewModel>());
                }

                return View(reservas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar reservas");
                ViewBag.Erro = "Não foi possível carregar a lista de reservas. Tente novamente mais tarde.";
                return View(new List<ReservaViewModel>());
            }
        }

        // GET: /Reservas/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var reserva = await _apiService.GetReservaByIdAsync(id);
                if (reserva == null)
                {
                    return NotFound();
                }

                return View(reserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes da reserva {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Reservas/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                // Carregar agendas e usuários para os dropdowns
                var agendas = await _apiService.GetAgendaAsync() ?? new List<AgendamentoViewModel>();
                var usuarios = await _apiService.GetUsuariosAsync() ?? new List<UsuarioViewModel>();

                if (!agendas.Any())
                {
                    ViewBag.Erro = "É necessário cadastrar agendas antes de criar reservas.";
                    return RedirectToAction("Index", "Agenda");
                }

                if (!usuarios.Any())
                {
                    ViewBag.Erro = "É necessário cadastrar usuários antes de criar reservas.";
                    return RedirectToAction("Index", "Usuarios");
                }

                ViewBag.Agendas = agendas;
                ViewBag.Usuarios = usuarios;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dados para criação de reserva");
                ViewBag.Erro = "Não foi possível carregar os dados necessários. Tente novamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Reservas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservaViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Recarregar dropdowns em caso de erro
                    var agendas = await _apiService.GetAgendaAsync() ?? new List<AgendamentoViewModel>();
                    var usuarios = await _apiService.GetUsuariosAsync() ?? new List<UsuarioViewModel>();
                    ViewBag.Agendas = agendas;
                    ViewBag.Usuarios = usuarios;
                    return View(model);
                }

                // Validar que a quantidade total é a soma das entradas
                var quantidadeCalculada = model.NPEntrada + model.MeiaEntrada + model.InteiraEntrada;
                if (quantidadeCalculada != model.Quantidade)
                {
                    ModelState.AddModelError("Quantidade", "A quantidade total deve ser igual à soma das entradas (NP + Meia + Inteira).");
                    
                    var agendas = await _apiService.GetAgendaAsync() ?? new List<AgendamentoViewModel>();
                    var usuarios = await _apiService.GetUsuariosAsync() ?? new List<UsuarioViewModel>();
                    ViewBag.Agendas = agendas;
                    ViewBag.Usuarios = usuarios;
                    return View(model);
                }

                // Validar data da reserva não é no passado
                if (model.DataReserva < DateTime.Today)
                {
                    ModelState.AddModelError("DataReserva", "A data da reserva não pode ser no passado.");
                    
                    var agendas = await _apiService.GetAgendaAsync() ?? new List<AgendamentoViewModel>();
                    var usuarios = await _apiService.GetUsuariosAsync() ?? new List<UsuarioViewModel>();
                    ViewBag.Agendas = agendas;
                    ViewBag.Usuarios = usuarios;
                    return View(model);
                }

                var sucesso = await _apiService.CreateReservaAsync(model);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Reserva criada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Não foi possível criar a reserva. Verifique os dados e tente novamente.");
                    
                    var agendas = await _apiService.GetAgendaAsync() ?? new List<AgendamentoViewModel>();
                    var usuarios = await _apiService.GetUsuariosAsync() ?? new List<UsuarioViewModel>();
                    ViewBag.Agendas = agendas;
                    ViewBag.Usuarios = usuarios;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar reserva");
                ModelState.AddModelError("", "Ocorreu um erro ao criar a reserva. Tente novamente.");
                
                var agendas = await _apiService.GetAgendaAsync() ?? new List<AgendamentoViewModel>();
                var usuarios = await _apiService.GetUsuariosAsync() ?? new List<UsuarioViewModel>();
                ViewBag.Agendas = agendas;
                ViewBag.Usuarios = usuarios;
                return View(model);
            }
        }

        // GET: /Reservas/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var reserva = await _apiService.GetReservaByIdAsync(id);
                if (reserva == null)
                {
                    return NotFound();
                }

                // Carregar agendas e usuários para os dropdowns
                var agendas = await _apiService.GetAgendaAsync() ?? new List<AgendamentoViewModel>();
                var usuarios = await _apiService.GetUsuariosAsync() ?? new List<UsuarioViewModel>();

                ViewBag.Agendas = agendas;
                ViewBag.Usuarios = usuarios;

                return View(reserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar reserva para edição {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Reservas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReservaViewModel model)
        {
            try
            {
                if (id != model.Id)
                {
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    var agendas = await _apiService.GetAgendaAsync() ?? new List<AgendamentoViewModel>();
                    var usuarios = await _apiService.GetUsuariosAsync() ?? new List<UsuarioViewModel>();
                    ViewBag.Agendas = agendas;
                    ViewBag.Usuarios = usuarios;
                    return View(model);
                }

                // Validar que a quantidade total é a soma das entradas
                var quantidadeCalculada = model.NPEntrada + model.MeiaEntrada + model.InteiraEntrada;
                if (quantidadeCalculada != model.Quantidade)
                {
                    ModelState.AddModelError("Quantidade", "A quantidade total deve ser igual à soma das entradas (NP + Meia + Inteira).");
                    
                    var agendas = await _apiService.GetAgendaAsync() ?? new List<AgendamentoViewModel>();
                    var usuarios = await _apiService.GetUsuariosAsync() ?? new List<UsuarioViewModel>();
                    ViewBag.Agendas = agendas;
                    ViewBag.Usuarios = usuarios;
                    return View(model);
                }

                var sucesso = await _apiService.UpdateReservaAsync(id, model);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Reserva atualizada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Não foi possível atualizar a reserva. Verifique os dados e tente novamente.");
                    
                    var agendas = await _apiService.GetAgendaAsync() ?? new List<AgendamentoViewModel>();
                    var usuarios = await _apiService.GetUsuariosAsync() ?? new List<UsuarioViewModel>();
                    ViewBag.Agendas = agendas;
                    ViewBag.Usuarios = usuarios;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar reserva {Id}", id);
                ModelState.AddModelError("", "Ocorreu um erro ao atualizar a reserva. Tente novamente.");
                
                var agendas = await _apiService.GetAgendaAsync() ?? new List<AgendamentoViewModel>();
                var usuarios = await _apiService.GetUsuariosAsync() ?? new List<UsuarioViewModel>();
                ViewBag.Agendas = agendas;
                ViewBag.Usuarios = usuarios;
                return View(model);
            }
        }

        // GET: /Reservas/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var reserva = await _apiService.GetReservaByIdAsync(id);
                if (reserva == null)
                {
                    return NotFound();
                }

                return View(reserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar reserva para exclusão {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Reservas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var sucesso = await _apiService.DeleteReservaAsync(id);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Reserva excluída com sucesso!";
                }
                else
                {
                    TempData["Erro"] = "Não foi possível excluir a reserva. Verifique se não existem pagamentos vinculados.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir reserva {Id}", id);
                TempData["Erro"] = "Ocorreu um erro ao excluir a reserva. Tente novamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Reservas/Cancelar/5
        public async Task<IActionResult> Cancelar(int id)
        {
            try
            {
                var reserva = await _apiService.GetReservaByIdAsync(id);
                if (reserva == null)
                {
                    return NotFound();
                }

                // Verificar se pode cancelar (regras de negócio)
                if (reserva.DataReserva < DateTime.Today.AddDays(-1))
                {
                    TempData["Erro"] = "Não é possível cancelar reservas com mais de 24 horas de antecedência.";
                    return RedirectToAction(nameof(Index));
                }

                return View(reserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar reserva para cancelamento {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Reservas/Cancelar/5
        [HttpPost, ActionName("Cancelar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarConfirmed(int id)
        {
            try
            {
                var reserva = await _apiService.GetReservaByIdAsync(id);
                if (reserva == null)
                {
                    return NotFound();
                }

                // Lógica de cancelamento (poderia ter um endpoint específico na API)
                var sucesso = await _apiService.DeleteReservaAsync(id);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Reserva cancelada com sucesso!";
                }
                else
                {
                    TempData["Erro"] = "Não foi possível cancelar a reserva. Tente novamente.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar reserva {Id}", id);
                TempData["Erro"] = "Ocorreu um erro ao cancelar a reserva. Tente novamente.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
