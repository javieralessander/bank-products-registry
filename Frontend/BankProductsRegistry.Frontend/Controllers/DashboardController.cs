using Microsoft.AspNetCore.Mvc;
using BankProductsRegistry.Frontend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BankProductsRegistry.Frontend.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly HttpClient _httpClient;

        public DashboardController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Asegúrate de que el puerto sea el de tu API
            _httpClient.BaseAddress = new Uri("https://localhost:7039/");
        }

        public async Task<IActionResult> Index()
        {
            // 1. Obtener el token de la sesión actual
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");

            // 2. Adjuntar el token a la petición
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            DashboardViewModel dashboardData = new();

            try
            {
                // 3. Llamar al endpoint que acabamos de crear en la API
                var response = await _httpClient.GetAsync("api/reports/dashboard");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    dashboardData = JsonSerializer.Deserialize<DashboardViewModel>(jsonString, options) ?? new();
                }
                else
                {
                    ViewBag.ErrorMessage = "No se pudieron cargar las métricas. El servidor devolvió un error.";
                }
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Error de conexión: El servidor (API) no está respondiendo.";
            }

            // 4. Mandar los datos reales a la vista
            return View(dashboardData);
        }
    }
}