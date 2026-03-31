using Microsoft.AspNetCore.Mvc;
using BankProductsRegistry.Frontend.Models;

namespace BankProductsRegistry.Frontend.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var viewModel = new DashboardViewModel
            {
                TotalClientes = 1248,
                ProductosActivos = 3891,
                TotalTransacciones = 24670,
                VolumenColocado = 4200000m,
                TransaccionesRecientes = new List<TransactionResponse>
                {
                    new TransactionResponse { AccountNumber = "CC-0042341", TransactionType = 0, Amount = 12500m, TransactionDate = new DateTime(2026, 3, 24), Status = "Completada" },
                    new TransactionResponse { AccountNumber = "PR-0078912", TransactionType = 1, Amount = 5000m, TransactionDate = new DateTime(2026, 3, 24), Status = "Completada" },
                    new TransactionResponse { AccountNumber = "IN-0012378", TransactionType = 0, Amount = 80000m, TransactionDate = new DateTime(2026, 3, 23), Status = "Completada" },
                    new TransactionResponse { AccountNumber = "CC-0042888", TransactionType = 0, Amount = 3200m, TransactionDate = new DateTime(2026, 3, 23), Status = "Pendiente" },
                    new TransactionResponse { AccountNumber = "PR-0099001", TransactionType = 1, Amount = 15000m, TransactionDate = new DateTime(2026, 3, 22), Status = "Completada" }
                }
            };
            return View(viewModel);
        }
    }
}