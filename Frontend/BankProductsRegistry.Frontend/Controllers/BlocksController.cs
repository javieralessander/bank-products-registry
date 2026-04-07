using Microsoft.AspNetCore.Mvc;
using BankProductsRegistry.Frontend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace BankProductsRegistry.Frontend.Controllers
{
    [Authorize]
    public class BlocksController : Controller
    {
        private readonly HttpClient _httpClient;

        public BlocksController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:7039/");
        }

        // --- DASHBOARD DE BLOQUEOS (GET) ---
        public async Task<IActionResult> Index()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var dashboardData = new BlockDashboardViewModel();

            try
            {
                // Llamamos a la API global de productos contratados
                var response = await _httpClient.GetAsync("api/account-products");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();

                    // Usamos JsonDocument para extraer exactamente lo que necesitamos sin crear clases enormes
                    using var doc = JsonDocument.Parse(jsonString);
                    var allProducts = doc.RootElement.EnumerateArray();

                    foreach (var p in allProducts)
                    {
                        if (p.GetProperty("isBlocked").GetBoolean() && p.TryGetProperty("activeBlock", out var activeBlock) && activeBlock.ValueKind != JsonValueKind.Null)
                        {
                            dashboardData.Blocks.Add(new BlockItemViewModel
                            {
                                AccountProductId = p.GetProperty("id").GetInt32(),
                                BlockId = activeBlock.GetProperty("id").GetInt32(),
                                AccountNumber = p.GetProperty("accountNumber").GetString() ?? "",
                                ClientName = p.GetProperty("clientName").GetString() ?? "",
                                FinancialProductName = p.GetProperty("financialProductName").GetString() ?? "",
                                BlockType = activeBlock.GetProperty("blockType").GetString() ?? "desconocido",
                                Reason = activeBlock.GetProperty("reason").GetString() ?? "Sin motivo",
                                StartsAt = activeBlock.GetProperty("startsAt").GetDateTimeOffset()
                            });
                        }
                    }

                    // Calculamos los totales
                    dashboardData.TotalActiveBlocks = dashboardData.Blocks.Count;
                    dashboardData.TemporaryBlocks = dashboardData.Blocks.Count(b => b.BlockType.ToLower() == "temporal");
                    dashboardData.PermanentBlocks = dashboardData.Blocks.Count(b => b.BlockType.ToLower() == "permanente");
                    dashboardData.FraudBlocks = dashboardData.Blocks.Count(b => b.BlockType.ToLower() == "fraude");
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al cargar los bloqueos: " + ex.Message;
            }

            return View(dashboardData);
        }

        // --- DESBLOQUEAR CUENTA (POST) ---
        [HttpPost]
        public async Task<IActionResult> Release(int accountProductId, int blockId, string reason)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // El body que requiere tu API: AccountProductBlockReleaseRequest
            var payload = new { reason = string.IsNullOrEmpty(reason) ? "Desbloqueo manual desde el panel" : reason };
            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                // Endpoint exacto de tu API
                var response = await _httpClient.PostAsync($"api/account-products/{accountProductId}/blocks/{blockId}/release", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetail = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"No se pudo desbloquear. {(response.StatusCode == System.Net.HttpStatusCode.Conflict ? "Los bloqueos permanentes no se pueden liberar." : "")}";
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Error de conexión al intentar desbloquear.";
            }

            return RedirectToAction("Index");
        }

        // --- NUEVO BLOQUEO (GET) ---
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Cargamos la lista de productos contratados para el desplegable
            var prodRes = await _httpClient.GetAsync("api/account-products");
            if (prodRes.IsSuccessStatusCode) ViewBag.AccountProducts = await prodRes.Content.ReadAsStringAsync();

            return View(new BlockCreateViewModel());
        }

        // --- NUEVO BLOQUEO (POST) ---
        [HttpPost]
        public async Task<IActionResult> Create(BlockCreateViewModel model)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // El DTO del Backend no pide el AccountProductId en el body, lo pide en la URL
            var payload = new
            {
                BlockType = model.BlockType,
                Reason = model.Reason,
                StartsAt = model.StartsAt,
                EndsAt = model.EndsAt
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                // Disparamos a la URL exacta que armó tu equipo: api/account-products/{id}/blocks
                var response = await _httpClient.PostAsync($"api/account-products/{model.AccountProductId}/blocks", jsonContent);

                if (response.IsSuccessStatusCode) return RedirectToAction("Index");

                var errorDetail = await response.Content.ReadAsStringAsync();
                ViewBag.ErrorMessage = $"La API rechazó el bloqueo: {errorDetail}";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error de conexión: {ex.Message}";
            }

            // Si falla, recargamos la lista para el combobox
            var prodRes = await _httpClient.GetAsync("api/account-products");
            if (prodRes.IsSuccessStatusCode) ViewBag.AccountProducts = await prodRes.Content.ReadAsStringAsync();

            return View(model);
        }






    }
}