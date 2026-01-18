using Admin.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Admin.Controllers
{
    public class AtividadesController : Controller
    {
        // GET: /Atividades
        public IActionResult Index()
        {
            // Dados de exemplo — substitua por acesso ao banco/serviço
            var atividades = new List<AtividadeViewModel>
            {
                new() { Id = 1, Nome = "Preparo do Solo", Descricao = "Aragem e gradagem da área designada para o plantio da soja.", ValorPorHa = 150m },
                new() { Id = 2, Nome = "Plantio de Sementes", Descricao = "Operação de plantadeira para semeadura da cultura de milho.", ValorPorHa = 200m },
                new() { Id = 3, Nome = "Pulverização", Descricao = "Aplicação de defensivos agrícolas para controle de pragas e doenças.", ValorPorHa = 80m },
                new() { Id = 4, Nome = "Adubação", Descricao = "Aplicação de fertilizantes conforme recomendação técnica.", ValorPorHa = 120m }
            };

            return View(atividades);
        }

        // GET: /Atividades/Details/5
        public IActionResult Details(int id)
        {
            var atividade = GetSampleData().FirstOrDefault(a => a.Id == id);
            if (atividade == null) return NotFound();
            return View(atividade);
        }

        // GET: /Atividades/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Atividades/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AtividadeViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // TODO: salvar no banco
            return RedirectToAction(nameof(Index));
        }

        // GET: /Atividades/Edit/5
        public IActionResult Edit(int id)
        {
            var atividade = GetSampleData().FirstOrDefault(a => a.Id == id);
            if (atividade == null) return NotFound();
            return View(atividade);
        }

        // POST: /Atividades/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, AtividadeViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            // TODO: atualizar no banco
            return RedirectToAction(nameof(Index));
        }

        // POST: /Atividades/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            // TODO: excluir do banco
            return RedirectToAction(nameof(Index));
        }

        // Dados de exemplo para Details/Edit quando não há DB
        private List<AtividadeViewModel> GetSampleData()
        {
            return new List<AtividadeViewModel>
            {
                new() { Id = 1, Nome = "Preparo do Solo", Descricao = "Aragem e gradagem da área designada para o plantio da soja.", ValorPorHa = 150m },
                new() { Id = 2, Nome = "Plantio de Sementes", Descricao = "Operação de plantadeira para semeadura da cultura de milho.", ValorPorHa = 200m },
                new() { Id = 3, Nome = "Pulverização", Descricao = "Aplicação de defensivos agrícolas para controle de pragas e doenças.", ValorPorHa = 80m },
                new() { Id = 4, Nome = "Adubação", Descricao = "Aplicação de fertilizantes conforme recomendação técnica.", ValorPorHa = 120m }
            };
        }
    }
}