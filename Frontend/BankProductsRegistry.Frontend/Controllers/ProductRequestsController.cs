using System.Net;
using System.Text;
using System.Text.Json;
using BankProductsRegistry.Frontend.Models;
using BankProductsRegistry.Frontend.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankProductsRegistry.Frontend.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ProductRequestsController : Controller
    {
        private readonly HttpClient _httpClient;

        public ProductRequestsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");

            if (!User.Claims.Any(c => c.Type == "client_id" && !string.IsNullOrEmpty(c.Value)))
            {
                ViewBag.ErrorMessage = "Tu cuenta no está vinculada a un cliente. No puedes solicitar productos.";
                return View(new ProductRequestFormViewModel());
            }

            _httpClient.SetBearerToken(token);

            try
            {
                var response = await _httpClient.GetAsync("api/financial-products");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var products = JsonSerializer.Deserialize<List<FinancialProductPickItem>>(json, options) ?? new List<FinancialProductPickItem>();
                    ViewBag.FinancialProducts = products
                        .Where(p => string.Equals(p.Status, "activo", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Status, "Active", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
                else
                {
                    ViewBag.ErrorMessage = "No se pudieron cargar los productos disponibles.";
                }
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Error de conexión con el servidor.";
            }

            return View(new ProductRequestFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProductRequestFormViewModel model)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");

            if (!User.Claims.Any(c => c.Type == "client_id" && !string.IsNullOrEmpty(c.Value)))
            {
                ViewBag.ErrorMessage = "Tu cuenta no está vinculada a un cliente.";
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                await ReloadProductsAsync(token);
                return View(model);
            }

            _httpClient.SetBearerToken(token);

            var payload = new { financialProductId = model.FinancialProductId, amount = model.Amount };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("api/account-products/me/request", content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Solicitud enviada. Un ejecutivo revisará y activará tu producto cuando sea aprobado.";
                    return RedirectToAction("Index", "AccountProducts");
                }

                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    var detail = await ApiErrorParser.ExtractMessageAsync(response);
                    ViewBag.ErrorMessage = detail ?? "Ya tienes una solicitud pendiente para este producto.";
                }
                else
                {
                    var detail = await ApiErrorParser.ExtractMessageAsync(response);
                    ViewBag.ErrorMessage = string.IsNullOrWhiteSpace(detail)
                        ? "No se pudo registrar la solicitud."
                        : detail;
                }
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Error de conexión con el servidor.";
            }

            await ReloadProductsAsync(token);
            return View(model);
        }

        private async Task ReloadProductsAsync(string token)
        {
            _httpClient.SetBearerToken(token);
            try
            {
                var response = await _httpClient.GetAsync("api/financial-products");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var products = JsonSerializer.Deserialize<List<FinancialProductPickItem>>(json, options) ?? new List<FinancialProductPickItem>();
                    ViewBag.FinancialProducts = products
                        .Where(p => string.Equals(p.Status, "activo", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Status, "Active", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
            }
            catch
            {
                ViewBag.FinancialProducts = new List<FinancialProductPickItem>();
            }
        }
    }
}
