using Microsoft.AspNetCore.Mvc;
using BankProductsRegistry.Frontend.Models;
using BankProductsRegistry.Frontend.Utilities;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
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
        }

        public async Task<IActionResult> Index()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var dashboardData = new TransactionDashboardViewModel();

            try
            {
                if (User.IsInRole("Cliente"))
                {
                    await LoadClientEligibleAccountsAsync(token, dashboardData);
                }

                var response = await _httpClient.GetAsync("api/transactions");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var transactions = JsonSerializer.Deserialize<List<TransactionItemViewModel>>(jsonString, options)
                                       ?? new List<TransactionItemViewModel>();

                    // 1. Asignar la lista de transacciones
                    dashboardData.Transactions = transactions;

                    // 2. Totales: la API puede enviar tipo como texto ("deposito") o como número de enum (1 = depósito).
                    dashboardData.TotalDeposits = transactions
                        .Where(t => IsDepositTransactionType(t.TransactionType))
                        .Sum(t => t.Amount);

                    dashboardData.TotalWithdrawals = transactions
                        .Where(t => !IsDepositTransactionType(t.TransactionType))
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> RegisterTransfer(ClientTransferFormViewModel model)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Revisa los datos del formulario.";
                return RedirectToAction(nameof(Index));
            }

            var transactionTypeJson = MapOperationKindToApi(model.OperationKind);
            if (transactionTypeJson is null)
            {
                TempData["ErrorMessage"] = "Tipo de operación no válido.";
                return RedirectToAction(nameof(Index));
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new
            {
                accountProductId = model.AccountProductId,
                transactionType = transactionTypeJson,
                transactionChannel = "en_linea",
                amount = model.Amount,
                transactionDate = DateTimeOffset.UtcNow,
                description = string.IsNullOrWhiteSpace(model.Description)
                    ? "Operación desde portal cliente"
                    : model.Description,
                referenceNumber = model.ReferenceNumber,
                countryCode = "DO"
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            try
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/transactions", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Movimiento registrado correctamente. Aparece en tu historial.";
                    return RedirectToAction(nameof(Index));
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    var forbiddenDetail = await ApiErrorParser.ExtractMessageAsync(response);
                    TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(forbiddenDetail)
                        ? "No tienes permiso para completar esta operación. Si el problema continúa, cierra sesión y vuelve a entrar o verifica que el producto sea tuyo."
                        : forbiddenDetail;
                    return RedirectToAction(nameof(Index));
                }

                var apiError = await ApiErrorParser.ExtractMessageAsync(response);
                TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(apiError)
                    ? "No se pudo registrar el movimiento."
                    : apiError;
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Error de conexión con el servidor.";
            }

            return RedirectToAction(nameof(Index));
        }

        private static string? MapOperationKindToApi(string? kind) =>
            kind?.ToLowerInvariant() switch
            {
                "transferencia" => "transferencia",
                "pago" => "pago",
                "deposito" => "deposito",
                "retiro" => "retiro",
                _ => null
            };

        private async Task LoadClientEligibleAccountsAsync(string token, TransactionDashboardViewModel dashboard)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            try
            {
                var response = await _httpClient.GetAsync("api/account-products");
                if (!response.IsSuccessStatusCode)
                {
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    if (!el.TryGetProperty("status", out var statusEl))
                    {
                        continue;
                    }

                    var status = statusEl.GetString();
                    if (status is not ("activo" or "en_mora"))
                    {
                        continue;
                    }

                    var id = el.GetProperty("id").GetInt32();
                    var accountNumber = el.GetProperty("accountNumber").GetString() ?? "";
                    var productName = el.TryGetProperty("financialProductName", out var fp)
                        ? fp.GetString() ?? ""
                        : "";

                    dashboard.ClientAccounts.Add(new ClientAccountPickItem
                    {
                        Id = id,
                        AccountNumber = accountNumber,
                        FinancialProductName = productName,
                        Status = status ?? ""
                    });
                }
            }
            catch
            {
                // El historial de transacciones puede cargar igual; el formulario quedará vacío.
            }
        }

        /// <summary>Depósito si el JSON trae <c>deposito</c> o el valor numérico del enum API (1).</summary>
        private static bool IsDepositTransactionType(string? transactionType)
        {
            if (string.IsNullOrWhiteSpace(transactionType))
            {
                return false;
            }

            var t = transactionType.Trim();
            if (t.Equals("1", StringComparison.Ordinal))
            {
                return true;
            }

            return t.Contains("deposito", StringComparison.OrdinalIgnoreCase);
        }
    }
}