using Admin.Models;
using Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers
{
    public class SafrasController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<SafrasController> _logger;

        public SafrasController(ApiService apiService, ILogger<SafrasController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: /Safras
        public async Task<IActionResult> Index(int pagina = 1, int tamanhoPagina = 8)
        {
            try
            {
                var safras = await _apiService.GetSafrasAsync();
                
                if (safras == null || !safras.Any())
                {
                    ViewBag.Mensagem = "Nenhuma safra cadastrada. Cadastre a primeira safra para começar a agenda.";
                    return View(new List<SafraViewModel>());
                }

                // Aplicar paginação
                var totalItens = safras.Count;
                var totalPaginas = (int)Math.Ceiling((double)totalItens / tamanhoPagina);
                
                // Validar página atual
                if (pagina < 1) pagina = 1;
                if (pagina > totalPaginas) pagina = totalPaginas;
                
                var itensPagina = safras
                    .OrderByDescending(x => x.DataInicio)
                    .Skip((pagina - 1) * tamanhoPagina)
                    .Take(tamanhoPagina)
                    .ToList();

                // ViewBag para paginação
                ViewBag.PaginaAtual = pagina;
                ViewBag.TamanhoPagina = tamanhoPagina;
                ViewBag.TotalItens = totalItens;
                ViewBag.TotalPaginas = totalPaginas;

                return View(itensPagina);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar safras");
                ViewBag.Erro = "Não foi possível carregar a lista de safras. Tente novamente mais tarde.";
                return View(new List<SafraViewModel>());
            }
        }

        // GET: /Safras/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var safra = await _apiService.GetSafraByIdAsync(id);
                if (safra == null)
                {
                    return NotFound();
                }

                return View(safra);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes da safra {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Safras/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Safras/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SafraViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Validar que DataFim é posterior a DataInicio
                if (model.DataFim <= model.DataInicio)
                {
                    ModelState.AddModelError("DataFim", "A data final deve ser posterior à data inicial.");
                    return View(model);
                }

                var sucesso = await _apiService.CreateSafraAsync(model);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Safra cadastrada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Não foi possível cadastrar a safra. Verifique os dados e tente novamente.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar safra");
                ModelState.AddModelError("", "Ocorreu um erro ao cadastrar a safra. Tente novamente.");
                return View(model);
            }
        }

        // GET: /Safras/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var safra = await _apiService.GetSafraByIdAsync(id);
                if (safra == null)
                {
                    return NotFound();
                }

                return View(safra);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar safra para edição {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Safras/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SafraViewModel model)
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

                // Validar que DataFim é posterior a DataInicio
                if (model.DataFim <= model.DataInicio)
                {
                    ModelState.AddModelError("DataFim", "A data final deve ser posterior à data inicial.");
                    return View(model);
                }

                var sucesso = await _apiService.UpdateSafraAsync(id, model);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Safra atualizada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Não foi possível atualizar a safra. Verifique os dados e tente novamente.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar safra {Id}", id);
                ModelState.AddModelError("", "Ocorreu um erro ao atualizar a safra. Tente novamente.");
                return View(model);
            }
        }

        // GET: /Safras/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var safra = await _apiService.GetSafraByIdAsync(id);
                if (safra == null)
                {
                    return NotFound();
                }

                // Verificar agendas associadas a esta safra
                var agendas = await _apiService.GetAgendaAsync();
                var agendasAssociadas = agendas?.Where(a => a.SafraId == id).ToList() ?? new List<AgendamentoViewModel>();

                // Verificar reservas e pagamentos associados
                var reservas = await _apiService.GetReservasAsync();
                var pagamentos = await _apiService.GetPagamentosAsync();

                var reservasAssociadas = new List<ReservaViewModel>();
                var pagamentosAssociados = new List<PagamentoViewModel>();

                if (agendasAssociadas.Any())
                {
                    var agendaIds = agendasAssociadas.Select(a => a.Id).ToList();
                    reservasAssociadas = reservas?.Where(r => agendaIds.Contains(r.AgendaId)).ToList() ?? new List<ReservaViewModel>();
                    
                    if (reservasAssociadas.Any())
                    {
                        var reservaIds = reservasAssociadas.Select(r => r.Id).ToList();
                        pagamentosAssociados = pagamentos?.Where(p => reservaIds.Contains(p.ReservaId)).ToList() ?? new List<PagamentoViewModel>();
                    }
                }

                ViewBag.TemAgendas = agendasAssociadas.Any();
                ViewBag.QuantidadeAgendas = agendasAssociadas.Count;
                ViewBag.TemReservas = reservasAssociadas.Any();
                ViewBag.QuantidadeReservas = reservasAssociadas.Count;
                ViewBag.TemPagamentos = pagamentosAssociados.Any();
                ViewBag.QuantidadePagamentos = pagamentosAssociados.Count;
                ViewBag.PagamentosPagos = pagamentosAssociados.Count(p => p.Status?.ToLower() == "pago");
                ViewBag.ValorTotalPagamentos = pagamentosAssociados.Where(p => p.Status?.ToLower() == "pago").Sum(p => p.Valor);
                ViewBag.ValorTotalFormatado = ViewBag.ValorTotalPagamentos?.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));

                // Verificar se a safra está no passado, presente ou futuro
                ViewBag.SafraPassada = safra.DataFim < DateTime.Today;
                ViewBag.SafraEmAndamento = safra.DataInicio <= DateTime.Today && safra.DataFim >= DateTime.Today;
                ViewBag.DiasParaInicio = safra.DataInicio > DateTime.Today ? (safra.DataInicio - DateTime.Today).Days : 0;

                return View(safra);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar safra para exclusão {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Safras/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var sucesso = await _apiService.DeleteSafraAsync(id);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Safra excluída com sucesso!";
                }
                else
                {
                    TempData["Erro"] = "Não foi possível excluir a safra. Verifique se não existem agendas ou reservas vinculadas.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir safra {Id}", id);
                TempData["Erro"] = "Ocorreu um erro ao excluir a safra. Tente novamente.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}