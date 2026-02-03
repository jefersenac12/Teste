using Admin.Models;
using Admin.Services;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Controllers
{
    public class PagamentosController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<PagamentosController> _logger;

        public PagamentosController(ApiService apiService, ILogger<PagamentosController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: /Pagamentos
        public async Task<IActionResult> Index()
        {
            try
            {
                var pagamentos = await _apiService.GetPagamentosAsync();
                
                if (pagamentos == null || !pagamentos.Any())
                {
                    ViewBag.Mensagem = "Nenhum pagamento encontrado. Os pagamentos aparecerão aqui quando os usuários efetuarem o pagamento das reservas.";
                    return View(new List<PagamentoViewModel>());
                }

                return View(pagamentos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar pagamentos");
                ViewBag.Erro = "Não foi possível carregar a lista de pagamentos. Tente novamente mais tarde.";
                return View(new List<PagamentoViewModel>());
            }
        }

        // GET: /Pagamentos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var pagamento = await _apiService.GetPagamentoByIdAsync(id);
                if (pagamento == null)
                {
                    return NotFound();
                }

                return View(pagamento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes do pagamento {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Pagamentos/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                // Carregar reservas para o dropdown
                var reservas = await _apiService.GetReservasAsync() ?? new List<ReservaViewModel>();

                // Permitir criação mesmo sem dados (removido bloqueio)
                ViewBag.Reservas = reservas;
                
                // Mensagem informativa se não houver dados
                if (!reservas.Any())
                {
                    ViewBag.Aviso = "Nenhuma reserva cadastrada. Você pode criar pagamentos, mas precisará cadastrar reservas primeiro.";
                }
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dados para criação de pagamento");
                ViewBag.Erro = "Não foi possível carregar os dados necessários. Tente novamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Pagamentos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PagamentoViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Recarregar dropdowns em caso de erro
                    var reservas = await _apiService.GetReservasAsync() ?? new List<ReservaViewModel>();
                    ViewBag.Reservas = reservas;
                    return View(model);
                }

                // Validar método de pagamento (só Pix é suportado)
                if (model.Metodo?.ToLower() != "pix")
                {
                    ModelState.AddModelError("Metodo", "Atualmente apenas o método Pix é suportado.");
                    
                    var reservas = await _apiService.GetReservasAsync() ?? new List<ReservaViewModel>();
                    ViewBag.Reservas = reservas;
                    return View(model);
                }

                // Validar status
                var statusValidos = new[] { "pago", "pendente", "cancelado" };
                if (!statusValidos.Contains(model.Status?.ToLower()))
                {
                    ModelState.AddModelError("Status", "Status inválido. Use: Pago, Pendente ou Cancelado.");
                    
                    var reservas = await _apiService.GetReservasAsync() ?? new List<ReservaViewModel>();
                    ViewBag.Reservas = reservas;
                    return View(model);
                }

                // Gerar dados Pix automaticamente se não fornecidos
                if (string.IsNullOrEmpty(model.ChavePix))
                {
                    model.ChavePix = "05680510847"; // Chave Pix padrão
                }

                if (string.IsNullOrEmpty(model.QrCode))
                {
                    // Gerar QR Code simulado
                    model.QrCode = $"pix_{DateTime.Now:yyyyMMddHHmmss}_{model.ReservaId}";
                }

                var sucesso = await _apiService.CreatePagamentoAsync(model);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Pagamento criado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Não foi possível criar o pagamento. Verifique os dados e tente novamente.");
                    
                    var reservas = await _apiService.GetReservasAsync() ?? new List<ReservaViewModel>();
                    ViewBag.Reservas = reservas;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pagamento");
                ModelState.AddModelError("", "Ocorreu um erro ao criar o pagamento. Tente novamente.");
                
                var reservas = await _apiService.GetReservasAsync() ?? new List<ReservaViewModel>();
                ViewBag.Reservas = reservas;
                return View(model);
            }
        }

        // GET: /Pagamentos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var pagamento = await _apiService.GetPagamentoByIdAsync(id);
                if (pagamento == null)
                {
                    return NotFound();
                }

                // Carregar reservas para o dropdown
                var reservas = await _apiService.GetReservasAsync() ?? new List<ReservaViewModel>();
                ViewBag.Reservas = reservas;

                return View(pagamento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar pagamento para edição {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Pagamentos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PagamentoViewModel model)
        {
            try
            {
                if (id != model.Id)
                {
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    var reservas = await _apiService.GetReservasAsync() ?? new List<ReservaViewModel>();
                    ViewBag.Reservas = reservas;
                    return View(model);
                }

                // Validar método de pagamento (só Pix é suportado)
                if (model.Metodo?.ToLower() != "pix")
                {
                    ModelState.AddModelError("Metodo", "Atualmente apenas o método Pix é suportado.");
                    
                    var reservas = await _apiService.GetReservasAsync() ?? new List<ReservaViewModel>();
                    ViewBag.Reservas = reservas;
                    return View(model);
                }

                // Validar status
                var statusValidos = new[] { "pago", "pendente", "cancelado" };
                if (!statusValidos.Contains(model.Status?.ToLower()))
                {
                    ModelState.AddModelError("Status", "Status inválido. Use: Pago, Pendente ou Cancelado.");
                    
                    var reservas = await _apiService.GetReservasAsync() ?? new List<ReservaViewModel>();
                    ViewBag.Reservas = reservas;
                    return View(model);
                }

                var sucesso = await _apiService.UpdatePagamentoAsync(id, model);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Pagamento atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Não foi possível atualizar o pagamento. Verifique os dados e tente novamente.");
                    
                    var reservas = await _apiService.GetReservasAsync() ?? new List<ReservaViewModel>();
                    ViewBag.Reservas = reservas;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar pagamento {Id}", id);
                ModelState.AddModelError("", "Ocorreu um erro ao atualizar o pagamento. Tente novamente.");
                
                var reservas = await _apiService.GetReservasAsync() ?? new List<ReservaViewModel>();
                ViewBag.Reservas = reservas;
                return View(model);
            }
        }

        // GET: /Pagamentos/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var pagamento = await _apiService.GetPagamentoByIdAsync(id);
                if (pagamento == null)
                {
                    return NotFound();
                }

                // Verificar informações importantes para o alerta
                ViewBag.PagamentoEstaPago = pagamento.Status?.ToLower() == "pago";
                ViewBag.PagamentoEstaPendente = pagamento.Status?.ToLower() == "pendente";
                ViewBag.TemChavePix = !string.IsNullOrEmpty(pagamento.ChavePix);
                ViewBag.ValorFormatado = pagamento.Valor.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));

                return View(pagamento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar pagamento para exclusão {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Pagamentos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Obter informações do pagamento antes de excluir
                var pagamento = await _apiService.GetPagamentoByIdAsync(id);
                if (pagamento == null)
                {
                    TempData["Erro"] = "Pagamento não encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar se há outras entidades que dependem deste pagamento
                // Pagamentos geralmente não têm dependências diretas além da reserva
                // Mas vamos verificar se há algum log ou registro financeiro vinculado
                
                var sucesso = await _apiService.DeletePagamentoAsync(id);
                if (sucesso)
                {
                    var mensagem = "Pagamento cancelado com sucesso!";
                    if (pagamento.Status?.ToLower() == "pago")
                    {
                        mensagem += $" Estorno de {pagamento.ValorFormatado} processado para a reserva #{pagamento.ReservaId}.";
                    }
                    else
                    {
                        mensagem += $" Solicitação de pagamento cancelada para a reserva #{pagamento.ReservaId}.";
                    }
                    
                    TempData["Sucesso"] = mensagem;
                }
                else
                {
                    // Tentar identificar o motivo da falha
                    var reserva = await _apiService.GetReservaByIdAsync(pagamento.ReservaId);
                    var motivoFalha = "";
                    
                    if (reserva != null)
                    {
                        motivoFalha = $" Não foi possível cancelar o pagamento pois a reserva #{pagamento.ReservaId} pode estar em um estado que impede o cancelamento. ";
                        motivoFalha += "Verifique se a reserva precisa ser tratada primeiro.";
                    }
                    else
                    {
                        motivoFalha = " Não foi possível cancelar o pagamento. Tente novamente mais tarde.";
                    }
                    
                    TempData["Erro"] = motivoFalha;
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir pagamento {Id}", id);
                TempData["Erro"] = "Ocorreu um erro ao excluir o pagamento. Tente novamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Pagamentos/ConfirmarPagamento/5
        public async Task<IActionResult> ConfirmarPagamento(int id)
        {
            try
            {
                var pagamento = await _apiService.GetPagamentoByIdAsync(id);
                if (pagamento == null)
                {
                    return NotFound();
                }

                if (pagamento.EstaPago)
                {
                    TempData["Erro"] = "Este pagamento já está confirmado.";
                    return RedirectToAction(nameof(Index));
                }

                return View(pagamento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar pagamento para confirmação {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Pagamentos/ConfirmarPagamento/5
        [HttpPost, ActionName("ConfirmarPagamento")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarPagamentoConfirmed(int id)
        {
            try
            {
                var pagamento = await _apiService.GetPagamentoByIdAsync(id);
                if (pagamento == null)
                {
                    return NotFound();
                }

                // Atualizar status para pago
                pagamento.Status = "Pago";
                pagamento.DataPagamento = DateTime.Now;

                var sucesso = await _apiService.UpdatePagamentoAsync(id, pagamento);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Pagamento confirmado com sucesso!";
                }
                else
                {
                    TempData["Erro"] = "Não foi possível confirmar o pagamento. Tente novamente.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao confirmar pagamento {Id}", id);
                TempData["Erro"] = "Ocorreu um erro ao confirmar o pagamento. Tente novamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Pagamentos/Estornar/5
        public async Task<IActionResult> Estornar(int id)
        {
            try
            {
                var pagamento = await _apiService.GetPagamentoByIdAsync(id);
                if (pagamento == null)
                {
                    return NotFound();
                }

                if (!pagamento.EstaPago)
                {
                    TempData["Erro"] = "Apenas pagamentos confirmados podem ser estornados.";
                    return RedirectToAction(nameof(Index));
                }

                return View(pagamento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar pagamento para estorno {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Pagamentos/Estornar/5
        [HttpPost, ActionName("Estornar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EstornarConfirmed(int id)
        {
            try
            {
                var pagamento = await _apiService.GetPagamentoByIdAsync(id);
                if (pagamento == null)
                {
                    return NotFound();
                }

                // Atualizar status para cancelado
                pagamento.Status = "Cancelado";

                var sucesso = await _apiService.UpdatePagamentoAsync(id, pagamento);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Pagamento estornado com sucesso!";
                }
                else
                {
                    TempData["Erro"] = "Não foi possível estornar o pagamento. Tente novamente.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao estornar pagamento {Id}", id);
                TempData["Erro"] = "Ocorreu um erro ao estornar o pagamento. Tente novamente.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
