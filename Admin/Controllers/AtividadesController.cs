using Admin.Models;
using Admin.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Admin.Controllers
{
    public class AtividadesController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<AtividadesController> _logger;

        public AtividadesController(ApiService apiService, ILogger<AtividadesController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: /Atividades
        public async Task<IActionResult> Index()
        {
            try
            {
                var atividades = await _apiService.GetAtividadesAsync();
                
                if (atividades == null || !atividades.Any())
                {
                    ViewBag.Mensagem = "Nenhuma atividade cadastrada. Cadastre a primeira atividade para começar a agenda.";
                    return View(new List<AtividadeViewModel>());
                }

                return View(atividades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar atividades");
                ViewBag.Erro = "Não foi possível carregar a lista de atividades. Tente novamente mais tarde.";
                return View(new List<AtividadeViewModel>());
            }
        }

        // GET: /Atividades/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var atividade = await _apiService.GetAtividadeByIdAsync(id);
                if (atividade == null)
                {
                    return NotFound();
                }

                return View(atividade);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes da atividade {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Atividades/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Atividades/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AtividadeViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var sucesso = await _apiService.CreateAtividadeAsync(model);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Atividade cadastrada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Não foi possível cadastrar a atividade. Verifique os dados e tente novamente.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar atividade");
                ModelState.AddModelError("", "Ocorreu um erro ao cadastrar a atividade. Tente novamente.");
                return View(model);
            }
        }

        // GET: /Atividades/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var atividade = await _apiService.GetAtividadeByIdAsync(id);
                if (atividade == null)
                {
                    return NotFound();
                }

                return View(atividade);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar atividade para edição {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Atividades/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AtividadeViewModel model)
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

                var sucesso = await _apiService.UpdateAtividadeAsync(id, model);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Atividade atualizada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Não foi possível atualizar a atividade. Verifique os dados e tente novamente.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar atividade {Id}", id);
                ModelState.AddModelError("", "Ocorreu um erro ao atualizar a atividade. Tente novamente.");
                return View(model);
            }
        }

        // GET: /Atividades/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var atividade = await _apiService.GetAtividadeByIdAsync(id);
                if (atividade == null)
                {
                    return NotFound();
                }

                return View(atividade);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar atividade para exclusão {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Atividades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var sucesso = await _apiService.DeleteAtividadeAsync(id);
                if (sucesso)
                {
                    TempData["Sucesso"] = "Atividade excluída com sucesso!";
                }
                else
                {
                    TempData["Erro"] = "Não foi possível excluir a atividade. Verifique se não existem agendas ou reservas vinculadas.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir atividade {Id}", id);
                TempData["Erro"] = "Ocorreu um erro ao excluir a atividade. Tente novamente.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}