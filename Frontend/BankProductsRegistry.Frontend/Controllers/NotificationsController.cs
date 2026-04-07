using Microsoft.AspNetCore.Mvc;
using BankProductsRegistry.Frontend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BankProductsRegistry.Frontend.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly HttpClient _httpClient;

        public NotificationsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var dashboard = new NotificationDashboardViewModel();

            try
            {
                var response = await _httpClient.GetAsync("api/notifications");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var notifs = JsonSerializer.Deserialize<List<NotificationItemViewModel>>(jsonString, options);

                    if (notifs != null)
                    {
                        dashboard.Notifications = notifs;
                        dashboard.UnreadCount = notifs.Count(n => !n.IsRead);
                        dashboard.RiskCount = notifs.Count(n => n.Type.ToLower() == "riesgo");
                        dashboard.InfoCount = notifs.Count(n => n.Type.ToLower() == "sistema" || n.Type.ToLower() == "informativa");
                        dashboard.ActiveTravelsCount = notifs.Count(n => n.Type.ToLower() == "viaje");
                    }
                }
            }
            catch { ViewBag.ErrorMessage = "Error de conexión con la API."; }

            return View(dashboard);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return Unauthorized();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            await _httpClient.PostAsync($"api/notifications/{id}/read", null);
            return Ok();
        }
    }
}