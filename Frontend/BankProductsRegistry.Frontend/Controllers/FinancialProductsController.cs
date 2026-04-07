using Microsoft.AspNetCore.Mvc;
using BankProductsRegistry.Frontend.Models;
using BankProductsRegistry.Frontend.Utilities;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace BankProductsRegistry.Frontend.Controllers
{
    [Authorize]
    public class FinancialProductsController : Controller
    {
        private readonly HttpClient _httpClient;

        public FinancialProductsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.GetAsync("api/financial-products");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var products = JsonSerializer.Deserialize<List<FinancialProductViewModel>>(jsonString, options);
                    return View(products ?? new List<FinancialProductViewModel>());
                }
                ViewBag.ErrorMessage = "Hubo un problema al cargar el catálogo de productos.";
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Error de conexión: El servidor (API) no está respondiendo.";
            }
            return View(new List<FinancialProductViewModel>());
        }

        // --- 1. CREAR PRODUCTO (GET) ---
        [HttpGet]
        [Authorize(Roles = "Admin,Operador")]
        public IActionResult Create()
        {
            return View();
        }

        // --- CREAR PRODUCTO (POST) ---
        [HttpPost]
        [Authorize(Roles = "Admin,Operador")]
        public async Task<IActionResult> Create(FinancialProductViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("api/financial-products", jsonContent);
                if (response.IsSuccessStatusCode) return RedirectToAction("Index");

                // MEJORA: Capturamos el error real de la API
                var errorDetail = await ApiErrorParser.ExtractMessageAsync(response);
                ViewBag.ErrorMessage = $"La API rechazó la creación. Detalle: {errorDetail}";
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Error de conexión al guardar el producto.";
            }
            return View(model);
        }

        // --- 3. EDITAR PRODUCTO (GET) ---
        [HttpGet]
        [Authorize(Roles = "Admin,Operador")]
        public async Task<IActionResult> Edit(int id)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.GetAsync($"api/financial-products/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var product = JsonSerializer.Deserialize<FinancialProductViewModel>(jsonString, options);
                    return View(product);
                }
            }
            catch (HttpRequestException) { }
            return RedirectToAction("Index");
        }

        // --- 4. EDITAR PRODUCTO (POST) ---
        [HttpPost]
        [Authorize(Roles = "Admin,Operador")]
        public async Task<IActionResult> Edit(int id, FinancialProductViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PutAsync($"api/financial-products/{id}", jsonContent);
                if (response.IsSuccessStatusCode) return RedirectToAction("Index");

                // MEJORA: Capturamos el error real de la API
                var errorDetail = await ApiErrorParser.ExtractMessageAsync(response);
                ViewBag.ErrorMessage = $"La API rechazó la actualización. Detalle: {errorDetail}";
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Error de conexión al actualizar.";
            }
            return View(model);
        }

        // --- 5. ELIMINAR PRODUCTO (POST) ---
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.DeleteAsync($"api/financial-products/{id}");
                if (response.IsSuccessStatusCode) return RedirectToAction("Index");

                // Si la API devuelve Conflict (409), el producto está en uso
                TempData["ErrorMessage"] = response.StatusCode == System.Net.HttpStatusCode.Conflict
                    ? "No se puede eliminar el producto porque está siendo usado en cuentas bancarias activas."
                    : "Error al intentar eliminar el producto financiero.";
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Error de conexión al eliminar.";
            }
            return RedirectToAction("Index");
        }
    }
}