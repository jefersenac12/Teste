using Admin.Models;
using Admin.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Admin.Controllers
{
    public class ComunicadosController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<ComunicadosController> _logger;

        public ComunicadosController(ApiService apiService, ILogger<ComunicadosController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: /Comunicados
        public async Task<IActionResult> Index(int pagina = 1, int tamanhoPagina = 8)
        {
            try
            {
                var comunicados = await _apiService.GetComunicadosAsync();
                
                if (comunicados == null || !comunicados.Any())
                {
                    ViewBag.Mensagem = "Nenhum comunicado cadastrado. Cadastre o primeiro comunicado para começar.";
                    return View(new List<ComunicadoViewModel>());
                }

                // Aplicar paginação
                var totalItens = comunicados.Count;
                var totalPaginas = (int)Math.Ceiling((double)totalItens / tamanhoPagina);
                
                // Validar página atual
                if (pagina < 1) pagina = 1;
                if (pagina > totalPaginas) pagina = totalPaginas;
                
                var itensPagina = comunicados
                    .OrderByDescending(x => x.DataCriacao)
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
                _logger.LogError(ex, "Erro ao carregar comunicados");
                ViewBag.Erro = "Não foi possível carregar a lista de comunicados. Tente novamente mais tarde.";
                return View(new List<ComunicadoViewModel>());
            }
        }

        // GET: /Comunicados/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var comunicado = await _apiService.GetComunicadoByIdAsync(id);
                if (comunicado == null)
                {
                    return NotFound();
                }

                return View(comunicado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes do comunicado {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Comunicados/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Comunicados/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComunicadoViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var sucesso = await _apiService.CreateComunicadoAsync(model);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Comunicado cadastrado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Não foi possível cadastrar o comunicado. Verifique os dados e tente novamente.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar comunicado");
                ModelState.AddModelError("", "Ocorreu um erro ao cadastrar o comunicado. Tente novamente.");
                return View(model);
            }
        }

        // GET: /Comunicados/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var comunicado = await _apiService.GetComunicadoByIdAsync(id);
                if (comunicado == null)
                {
                    return NotFound();
                }

                return View(comunicado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar comunicado para edição {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Comunicados/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ComunicadoViewModel model)
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

                var sucesso = await _apiService.UpdateComunicadoAsync(id, model);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Comunicado atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Não foi possível atualizar o comunicado. Verifique os dados e tente novamente.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar comunicado {Id}", id);
                ModelState.AddModelError("", "Ocorreu um erro ao atualizar o comunicado. Tente novamente.");
                return View(model);
            }
        }

        // GET: /Comunicados/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var comunicado = await _apiService.GetComunicadoByIdAsync(id);
                if (comunicado == null)
                {
                    return NotFound();
                }

                return View(comunicado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar comunicado para exclusão {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Comunicados/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var sucesso = await _apiService.DeleteComunicadoAsync(id);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Comunicado excluído com sucesso!";
                }
                else
                {
                    TempData["Erro"] = "Não foi possível excluir o comunicado. Tente novamente.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir comunicado {Id}", id);
                TempData["Erro"] = "Ocorreu um erro ao excluir o comunicado. Tente novamente.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
