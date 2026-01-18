using Admin.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Admin.Controllers
{
    public class ReservasController : Controller
    {
        // GET: /Reservas
        public IActionResult Index()
        {
            // Dados de exemplo — substitua pela consulta ao repositório / banco de dados
            var reservas = new List<ReservaViewModel>
            {
                new() { Id = 1, Titulo = "Colheita de Soja", Safra = "Soja 2023/2024", Usuario = "Ana Silva", DataReserva = new DateTime(2024, 3, 10), QtdInteira = 1, QtdMeia = 1, ValorTotal = 150m },
                new() { Id = 2, Titulo = "Plantio de Milho", Safra = "Milho 2024", Usuario = "Carlos Pereira", DataReserva = new DateTime(2024, 4, 18), QtdInteira = 3, QtdMeia = 0, ValorTotal = 300m },
                new() { Id = 3, Titulo = "Inspeção de Pragas", Safra = "Trigo 2024", Usuario = "Mariana Costa", DataReserva = new DateTime(2024, 6, 5), QtdInteira = 1, QtdMeia = 0, ValorTotal = 100m }
            };

            return View(reservas);
        }

        // GET: /Reservas/Details/5
        public IActionResult Details(int id)
        {
            var reserva = GetSampleData().FirstOrDefault(r => r.Id == id);
            if (reserva == null) return NotFound();
            return View(reserva);
        }

        // GET: /Reservas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Reservas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ReservaViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // TODO: salvar no banco
            return RedirectToAction(nameof(Index));
        }

        // GET: /Reservas/Edit/5
        public IActionResult Edit(int id)
        {
            var reserva = GetSampleData().FirstOrDefault(r => r.Id == id);
            if (reserva == null) return NotFound();
            return View(reserva);
        }

        // POST: /Reservas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ReservaViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            // TODO: atualizar no banco
            return RedirectToAction(nameof(Index));
        }

        // POST: /Reservas/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            // TODO: excluir do banco
            return RedirectToAction(nameof(Index));
        }

        // Método interno apenas para popular exemplos nas actions Details/Edit quando não há DB
        private List<ReservaViewModel> GetSampleData()
        {
            return new List<ReservaViewModel>
            {
                new() { Id = 1, Titulo = "Colheita de Soja", Safra = "Soja 2023/2024", Usuario = "Ana Silva", DataReserva = new DateTime(2024, 3, 10), QtdInteira = 1, QtdMeia = 1, ValorTotal = 150m },
                new() { Id = 2, Titulo = "Plantio de Milho", Safra = "Milho 2024", Usuario = "Carlos Pereira", DataReserva = new DateTime(2024, 4, 18), QtdInteira = 3, QtdMeia = 0, ValorTotal = 300m },
                new() { Id = 3, Titulo = "Inspeção de Pragas", Safra = "Trigo 2024", Usuario = "Mariana Costa", DataReserva = new DateTime(2024, 6, 5), QtdInteira = 1, QtdMeia = 0, ValorTotal = 100m }
            };
        }
    }
}