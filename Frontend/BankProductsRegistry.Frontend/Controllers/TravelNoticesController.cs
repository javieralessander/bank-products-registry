using Microsoft.AspNetCore.Mvc;
using BankProductsRegistry.Frontend.Models;
using BankProductsRegistry.Frontend.Utilities;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace BankProductsRegistry.Frontend.Controllers
{
    [Authorize]
    public class TravelNoticesController : Controller
    {
        private readonly HttpClient _httpClient;

        public TravelNoticesController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // --- DASHBOARD GLOBAL (GET) ---
        public async Task<IActionResult> Index()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var dashboardData = new TravelNoticeDashboardViewModel();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            try
            {
                // 1. Obtenemos todas las cuentas
                var accResponse = await _httpClient.GetAsync("api/account-products");
                if (accResponse.IsSuccessStatusCode)
                {
                    var accJson = await accResponse.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(accJson);
                    var accounts = doc.RootElement.EnumerateArray();

                    // 2. Buscamos los viajes de cada cuenta
                    foreach (var acc in accounts)
                    {
                        var accId = acc.GetProperty("id").GetInt32();
                        var travelRes = await _httpClient.GetAsync($"api/account-products/{accId}/travel-notices");

                        if (travelRes.IsSuccessStatusCode)
                        {
                            var travelJson = await travelRes.Content.ReadAsStringAsync();
                            using var travelDoc = JsonDocument.Parse(travelJson);

                            foreach (var notice in travelDoc.RootElement.EnumerateArray())
                            {
                                dashboardData.Notices.Add(new TravelNoticeCardViewModel
                                {
                                    AccountProductId = accId,
                                    NoticeId = notice.GetProperty("id").GetInt32(),
                                    ClientName = acc.GetProperty("clientName").GetString() ?? "",
                                    AccountNumber = acc.GetProperty("accountNumber").GetString() ?? "",
                                    ProductName = acc.GetProperty("financialProductName").GetString() ?? "",
                                    StartsAt = notice.GetProperty("startsAt").GetDateTimeOffset(),
                                    EndsAt = notice.GetProperty("endsAt").GetDateTimeOffset(),
                                    IsActive = notice.GetProperty("isActive").GetBoolean(),
                                    CancelledAt = notice.GetProperty("cancelledAt").ValueKind != JsonValueKind.Null ? notice.GetProperty("cancelledAt").GetDateTimeOffset() : null,
                                    Countries = notice.GetProperty("countries").EnumerateArray().Select(c => c.GetString()).ToArray()!
                                });
                            }
                        }
                    }
                }

                // 3. Calculamos Totales
                var now = DateTimeOffset.UtcNow;
                dashboardData.ActiveTravels = dashboardData.Notices.Count(n => n.IsActive);
                dashboardData.UpcomingTravels = dashboardData.Notices.Count(n => n.StartsAt > now && n.CancelledAt == null);
                dashboardData.FinishedTravels = dashboardData.Notices.Count(n => n.EndsAt < now && n.CancelledAt == null);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error cargando viajes: " + ex.Message;
            }

            return View(dashboardData);
        }

        // --- REGISTRAR VIAJE (GET) ---
        [HttpGet]
        [Authorize(Roles = "Admin,Operador,Cliente")]
        public async Task<IActionResult> Create()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var model = new TravelNoticeCreateViewModel();

            try
            {
                var prodRes = await _httpClient.GetAsync("api/account-products");
                if (prodRes.IsSuccessStatusCode)
                {
                    var json = await prodRes.Content.ReadAsStringAsync();
                    ViewBag.AccountProducts = json;
                    ClientAccountProductFormHelper.ApplyToTravelNotice(User, json, model);
                }
                else
                {
                    var detail = await ApiErrorParser.ExtractMessageAsync(prodRes);
                    ViewBag.ErrorMessage = string.IsNullOrWhiteSpace(detail)
                        ? "No se pudo cargar el listado de productos contratados."
                        : detail;
                }
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Error de conexión: la API no está disponible.";
            }

            return View(model);
        }

        // --- REGISTRAR VIAJE (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Operador,Cliente")]
        public async Task<IActionResult> Create(TravelNoticeCreateViewModel model)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Armamos el payload exigido por tu backend (AccountProductTravelNoticeCreateRequest)
            var payload = new
            {
                StartsAt = model.StartsAt,
                EndsAt = model.EndsAt,
                Reason = string.IsNullOrEmpty(model.CitiesOrReason) ? "Viaje internacional" : model.CitiesOrReason,
                Countries = new[] { model.CountryCode } // Tu API requiere formato ISO (Ej. "US", "ES")
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"api/account-products/{model.AccountProductId}/travel-notices", jsonContent);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Tu aviso de viaje fue registrado correctamente.";
                return RedirectToAction("Index");
            }

            var errorDetail = await ApiErrorParser.ExtractMessageAsync(response);
            ViewBag.ErrorMessage = response.StatusCode == HttpStatusCode.Forbidden
                ? "No tienes permiso para registrar un aviso sobre este producto. Usa solo tus tarjetas contratadas."
                : (string.IsNullOrWhiteSpace(errorDetail) ? "No se pudo registrar el viaje." : errorDetail);

            var prodRes = await _httpClient.GetAsync("api/account-products");
            if (prodRes.IsSuccessStatusCode)
            {
                var json = await prodRes.Content.ReadAsStringAsync();
                ViewBag.AccountProducts = json;
                ClientAccountProductFormHelper.ApplyToTravelNotice(User, json, model);
            }

            return View(model);
        }

        // --- CANCELAR VIAJE (POST) ---
        [HttpPost]
        [Authorize(Roles = "Admin,Operador,Cliente")]
        public async Task<IActionResult> Cancel(int accountProductId, int noticeId, string reason)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Payload que exige la API para cancelar
            var payload = new { reason = string.IsNullOrEmpty(reason) ? "Cancelado a petición del cliente" : reason };
            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync($"api/account-products/{accountProductId}/travel-notices/{noticeId}/cancel", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "No se pudo cancelar el viaje.";
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Error de conexión al intentar cancelar.";
            }

            return RedirectToAction("Index");
        }




    }
}