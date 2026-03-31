using Microsoft.AspNetCore.Mvc;
using BankProductsRegistry.Frontend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BankProductsRegistry.Frontend.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly HttpClient _httpClient;

        public TransactionsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:7039/");
        }

        public async Task<IActionResult> Index()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var dashboardData = new TransactionDashboardViewModel();

            try
            {
                var response = await _httpClient.GetAsync("api/transactions");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var transactions = JsonSerializer.Deserialize<List<TransactionItemViewModel>>(jsonString, options)
                                       ?? new List<TransactionItemViewModel>();

                    // 1. Asignar la lista de transacciones
                    dashboardData.Transactions = transactions;

                    // 2. Calcular los totales (usando texto porque TransactionType ahora es string)
                    dashboardData.TotalDeposits = transactions
                        .Where(t => !string.IsNullOrEmpty(t.TransactionType) && t.TransactionType.ToLower().Contains("deposito"))
                        .Sum(t => t.Amount);

                    dashboardData.TotalWithdrawals = transactions
                        .Where(t => string.IsNullOrEmpty(t.TransactionType) || !t.TransactionType.ToLower().Contains("deposito"))
                        .Sum(t => t.Amount);

                    dashboardData.NetBalance = dashboardData.TotalDeposits - dashboardData.TotalWithdrawals;
                }
                else
                {
                    ViewBag.ErrorMessage = "No se pudo cargar el historial de transacciones.";
                }
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Error de conexión: El servidor (API) no está respondiendo.";
            }

            return View(dashboardData);
        }
    }
}