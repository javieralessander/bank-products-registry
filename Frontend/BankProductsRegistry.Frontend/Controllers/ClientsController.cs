using Microsoft.AspNetCore.Mvc;
using BankProductsRegistry.Frontend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using BankProductsRegistry.Frontend.Utilities;
namespace BankProductsRegistry.Frontend.Controllers
{
    [Authorize]
    public class ClientsController : Controller
    {
        private readonly HttpClient _httpClient;

        public ClientsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // 1. Mostrar la tabla con todos los clientes
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.GetAsync("api/clients");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var clients = JsonSerializer.Deserialize<List<ClientViewModel>>(jsonString, options);

                    return View(clients ?? new List<ClientViewModel>());
                }

                ViewBag.ErrorMessage = "Hubo un problema al cargar los clientes desde el servidor.";
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Error de conexión: El servidor (API) no está respondiendo.";
            }

            return View(new List<ClientViewModel>());
        }

        // 2. Mostrar la pantalla vacía para crear un cliente
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadLinkableUsersAsync();
            return View();
        }

        // 3. Recibir los datos del formulario y enviarlos a la API
        [HttpPost]
        public async Task<IActionResult> Create(ClientViewModel model)
        {
            // Verificamos que los datos del formulario sean válidos antes de enviarlos
            if (!ModelState.IsValid)
            {
                await LoadLinkableUsersAsync();
                return View(model);
            }

            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var payload = new
                {
                    model.FirstName,
                    model.LastName,
                    model.NationalId,
                    model.Email,
                    model.Phone,
                    RegisteredUserId = model.LinkUserId,
                    model.IsActive
                };
                var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/clients", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }

                var apiError = await ApiErrorParser.ExtractMessageAsync(response);
                ViewBag.ErrorMessage = string.IsNullOrWhiteSpace(apiError)
                    ? "Hubo un problema al crear el cliente. Verifica que la cédula o correo no estén registrados ya."
                    : apiError;
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Error de conexión: El servidor (API) no está respondiendo.";
            }

            await LoadLinkableUsersAsync();
            return View(model);
        }

        private async Task LoadLinkableUsersAsync()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.LinkableUsers = new List<PendingClientUserViewModel>();
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.GetAsync("api/clients/pending-users");
                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.LinkableUsers = new List<PendingClientUserViewModel>();
                    return;
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var linkable = JsonSerializer.Deserialize<List<PendingClientUserViewModel>>(jsonString, options)
                    ?? new List<PendingClientUserViewModel>();

                ViewBag.LinkableUsers = linkable;
            }
            catch (HttpRequestException)
            {
                ViewBag.LinkableUsers = new List<PendingClientUserViewModel>();
            }
        }
        // --- 4. VER PORTFOLIO (DETALLES) ---
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 1. Buscamos los datos básicos del cliente
            var clientResponse = await _httpClient.GetAsync($"api/clients/{id}");
            if (!clientResponse.IsSuccessStatusCode) return RedirectToAction("Index");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var clientJson = await clientResponse.Content.ReadAsStringAsync();
            var clientData = JsonSerializer.Deserialize<ClientViewModel>(clientJson, options);

            // 2. Buscamos el reporte financiero (Portfolio)
            var portfolioResponse = await _httpClient.GetAsync($"api/reports/clients/{id}/portfolio");
            ClientPortfolioViewModel portfolioData = new();

            if (portfolioResponse.IsSuccessStatusCode)
            {
                var portfolioJson = await portfolioResponse.Content.ReadAsStringAsync();
                portfolioData = JsonSerializer.Deserialize<ClientPortfolioViewModel>(portfolioJson, options) ?? new();
            }

            // 3. Empaquetamos todo en la bandeja combinada
            var model = new ClientDetailsViewModel
            {
                Client = clientData ?? new(),
                Portfolio = portfolioData
            };

            return View(model);
        }

        // --- 5. ELIMINAR CLIENTE ---
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.DeleteAsync($"api/clients/{id}");

            if (!response.IsSuccessStatusCode)
            {
                // Si la API devuelve 409 (Conflicto), es porque tiene productos registrados
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    TempData["ErrorMessage"] = "No se puede eliminar el cliente porque tiene productos bancarios registrados.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Hubo un error al intentar eliminar el cliente.";
                }
            }

            return RedirectToAction("Index");
        }

        // --- 6. CARGAR VISTA DE EDITAR ---
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"api/clients/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var client = JsonSerializer.Deserialize<ClientViewModel>(jsonString, options);
                return View(client);
            }
            return RedirectToAction("Index");
        }

        // --- 7. GUARDAR EDICIÓN ---
        [HttpPost]
        public async Task<IActionResult> Edit(int id, ClientViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var jsonContent = new StringContent(JsonSerializer.Serialize(model), System.Text.Encoding.UTF8, "application/json");
            // Nota: Tu API usa PUT para actualizar clientes
            var response = await _httpClient.PutAsync($"api/clients/{id}", jsonContent);

            if (response.IsSuccessStatusCode) return RedirectToAction("Index");

            ViewBag.ErrorMessage = "Error al actualizar. Verifica que la cédula/correo no pertenezcan a otro cliente.";
            return View(model);
        }




    }
}
