using Admin.Models;
using Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers
{
    public class AgendaController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<AgendaController> _logger;

        public AgendaController(ApiService apiService, ILogger<AgendaController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: /Agenda
        public async Task<IActionResult> Index()
        {
            try
            {
                var agenda = await _apiService.GetAgendaAsync();
                
                if (agenda == null || !agenda.Any())
                {
                    ViewBag.Mensagem = "Nenhuma agenda cadastrada. Cadastre safras e atividades primeiro, depois crie a agenda.";
                    return View(new List<AgendamentoViewModel>());
                }

                return View(agenda);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar agenda");
                ViewBag.Erro = "Não foi possível carregar a agenda. Tente novamente mais tarde.";
                return View(new List<AgendamentoViewModel>());
            }
        }

        // GET: /Agenda/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var agendamento = await _apiService.GetAgendaByIdAsync(id);
                if (agendamento == null)
                {
                    return NotFound();
                }

                return View(agendamento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes do agendamento {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Agenda/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                // Carregar safras e atividades para os dropdowns
                var safras = await _apiService.GetSafrasAsync() ?? new List<SafraViewModel>();
                var atividades = await _apiService.GetAtividadesAsync() ?? new List<AtividadeViewModel>();

                if (!safras.Any())
                {
                    ViewBag.Erro = "É necessário cadastrar pelo menos uma safra antes de criar a agenda.";
                    return RedirectToAction("Index", "Safras");
                }

                if (!atividades.Any())
                {
                    ViewBag.Erro = "É necessário cadastrar pelo menos uma atividade antes de criar a agenda.";
                    return RedirectToAction("Index", "Atividades");
                }

                ViewBag.Safras = safras;
                ViewBag.Atividades = atividades;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dados para criação de agenda");
                ViewBag.Erro = "Não foi possível carregar os dados necessários. Tente novamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Agenda/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AgendamentoViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Recarregar dropdowns em caso de erro
                    var safras = await _apiService.GetSafrasAsync() ?? new List<SafraViewModel>();
                    var atividades = await _apiService.GetAtividadesAsync() ?? new List<AtividadeViewModel>();
                    ViewBag.Safras = safras;
                    ViewBag.Atividades = atividades;
                    return View(model);
                }

                // Validar que VagasDisponiveis não é maior que VagasTotais
                if (model.VagasDisponiveis > model.VagasTotais)
                {
                    ModelState.AddModelError("VagasDisponiveis", "Vagas disponíveis não podem ser maiores que vagas totais.");
                    
                    var safras = await _apiService.GetSafrasAsync() ?? new List<SafraViewModel>();
                    var atividades = await _apiService.GetAtividadesAsync() ?? new List<AtividadeViewModel>();
                    ViewBag.Safras = safras;
                    ViewBag.Atividades = atividades;
                    return View(model);
                }

                var sucesso = await _apiService.CreateAgendamentoAsync(model);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Agenda cadastrada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Não foi possível cadastrar a agenda. Verifique os dados e tente novamente.");
                    
                    var safras = await _apiService.GetSafrasAsync() ?? new List<SafraViewModel>();
                    var atividades = await _apiService.GetAtividadesAsync() ?? new List<AtividadeViewModel>();
                    ViewBag.Safras = safras;
                    ViewBag.Atividades = atividades;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar agenda");
                ModelState.AddModelError("", "Ocorreu um erro ao cadastrar a agenda. Tente novamente.");
                
                var safras = await _apiService.GetSafrasAsync() ?? new List<SafraViewModel>();
                var atividades = await _apiService.GetAtividadesAsync() ?? new List<AtividadeViewModel>();
                ViewBag.Safras = safras;
                ViewBag.Atividades = atividades;
                return View(model);
            }
        }

        // GET: /Agenda/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var agendamento = await _apiService.GetAgendaByIdAsync(id);
                if (agendamento == null)
                {
                    return NotFound();
                }

                // Carregar safras e atividades para os dropdowns
                var safras = await _apiService.GetSafrasAsync() ?? new List<SafraViewModel>();
                var atividades = await _apiService.GetAtividadesAsync() ?? new List<AtividadeViewModel>();

                ViewBag.Safras = safras;
                ViewBag.Atividades = atividades;

                return View(agendamento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar agenda para edição {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Agenda/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AgendamentoViewModel model)
        {
            try
            {
                if (id != model.Id)
                {
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    var safras = await _apiService.GetSafrasAsync() ?? new List<SafraViewModel>();
                    var atividades = await _apiService.GetAtividadesAsync() ?? new List<AtividadeViewModel>();
                    ViewBag.Safras = safras;
                    ViewBag.Atividades = atividades;
                    return View(model);
                }

                // Validar que VagasDisponiveis não é maior que VagasTotais
                if (model.VagasDisponiveis > model.VagasTotais)
                {
                    ModelState.AddModelError("VagasDisponiveis", "Vagas disponíveis não podem ser maiores que vagas totais.");
                    
                    var safras = await _apiService.GetSafrasAsync() ?? new List<SafraViewModel>();
                    var atividades = await _apiService.GetAtividadesAsync() ?? new List<AtividadeViewModel>();
                    ViewBag.Safras = safras;
                    ViewBag.Atividades = atividades;
                    return View(model);
                }

                var sucesso = await _apiService.UpdateAgendaAsync(id, model);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Agenda atualizada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Não foi possível atualizar a agenda. Verifique os dados e tente novamente.");
                    
                    var safras = await _apiService.GetSafrasAsync() ?? new List<SafraViewModel>();
                    var atividades = await _apiService.GetAtividadesAsync() ?? new List<AtividadeViewModel>();
                    ViewBag.Safras = safras;
                    ViewBag.Atividades = atividades;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar agenda {Id}", id);
                ModelState.AddModelError("", "Ocorreu um erro ao atualizar a agenda. Tente novamente.");
                
                var safras = await _apiService.GetSafrasAsync() ?? new List<SafraViewModel>();
                var atividades = await _apiService.GetAtividadesAsync() ?? new List<AtividadeViewModel>();
                ViewBag.Safras = safras;
                ViewBag.Atividades = atividades;
                return View(model);
            }
        }

        // GET: /Agenda/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var agendamento = await _apiService.GetAgendaByIdAsync(id);
                if (agendamento == null)
                {
                    return NotFound();
                }

                return View(agendamento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar agenda para exclusão {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Agenda/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var sucesso = await _apiService.DeleteAgendaAsync(id);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Agenda excluída com sucesso!";
                }
                else
                {
                    TempData["Erro"] = "Não foi possível excluir a agenda. Verifique se não existem reservas vinculadas.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir agenda {Id}", id);
                TempData["Erro"] = "Ocorreu um erro ao excluir a agenda. Tente novamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Agenda/Gerenciar - View especial para calendário
        public async Task<IActionResult> Gerenciar()
        {
            try
            {
                var agenda = await _apiService.GetAgendaAsync();
                var safras = await _apiService.GetSafrasAsync();
                var atividades = await _apiService.GetAtividadesAsync();

                ViewBag.Agenda = agenda ?? new List<AgendamentoViewModel>();
                ViewBag.Safras = safras ?? new List<SafraViewModel>();
                ViewBag.Atividades = atividades ?? new List<AtividadeViewModel>();

                if (agenda == null || !agenda.Any())
                {
                    ViewBag.Mensagem = "Nenhuma agenda cadastrada para exibir no calendário.";
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dados do calendário");
                ViewBag.Erro = "Não foi possível carregar os dados do calendário. Tente novamente mais tarde.";
                return View();
            }
        }
    }
}
