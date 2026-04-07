using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using BankProductsRegistry.Frontend.Models;
using BankProductsRegistry.Frontend.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankProductsRegistry.Frontend.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ClientPortalController : Controller
    {
        private readonly HttpClient _httpClient;

        public ClientPortalController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = new ClientPortalPageViewModel();

            var clientIdClaim = User.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value;
            if (string.IsNullOrEmpty(clientIdClaim) || !int.TryParse(clientIdClaim, out var clientId))
            {
                model.ErrorMessage = "Tu cuenta aún no está vinculada a un perfil de cliente. Solicita la vinculación en la sucursal o con tu ejecutivo.";
                return View(model);
            }

            model.ClientId = clientId;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.GetAsync($"api/reports/clients/{clientId}/portfolio");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    model.Portfolio = JsonSerializer.Deserialize<ClientPortfolioViewModel>(json, options);
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    model.ErrorMessage = "No se encontró información financiera para tu perfil.";
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    model.ErrorMessage = "No tienes permiso para consultar este resumen.";
                }
                else
                {
                    var detail = await ApiErrorParser.ExtractMessageAsync(response);
                    model.ErrorMessage = string.IsNullOrWhiteSpace(detail)
                        ? "No se pudo cargar tu resumen bancario."
                        : detail;
                }
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage = "Error de conexión: el servidor no está disponible.";
            }

            return View(model);
        }
    }
}
