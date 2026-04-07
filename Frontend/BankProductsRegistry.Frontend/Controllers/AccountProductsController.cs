using Microsoft.AspNetCore.Mvc;
using BankProductsRegistry.Frontend.Models;
using BankProductsRegistry.Frontend.Utilities;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BankProductsRegistry.Frontend.Controllers
{
    [Authorize]
    public class AccountProductsController : Controller
    {
        private readonly HttpClient _httpClient;

        public AccountProductsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var dashboardData = new AccountProductDashboardViewModel();

            try
            {
                var response = await _httpClient.GetAsync("api/account-products");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var productsList = JsonSerializer.Deserialize<List<AccountProductItemViewModel>>(jsonString, options)
                                       ?? new List<AccountProductItemViewModel>();

                    dashboardData.Products = productsList;

                    // 1. Contar Contratos Activos
                    dashboardData.TotalActive = productsList
                        .Count(p => !string.IsNullOrEmpty(p.Status) && p.Status.ToLower().Contains("activo"));

                    // 2. Sumar el Dinero Gestionado (Montos de las cuentas)
                    dashboardData.TotalVolume = productsList.Sum(p => p.Amount);

                    // Nota: La API de la lista no devuelve 'MaturityDate', así que el 'Vencen Esta Semana' lo dejamos en 0 por ahora
                    // Para un diseño 100% real, la API debería incluir MaturityDate en AccountProductListItemResponse.
                    dashboardData.TotalExpiringSoon = 0;
                }
                else
                {
                    ViewBag.ErrorMessage = "No se pudo cargar el listado de productos contratados.";
                }
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Error de conexión: El servidor (API) no está respondiendo.";
            }

            return View(dashboardData);
        }
        // --- CREAR CONTRATO (GET) ---
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Cargamos los datos de las otras APIs para llenar los comboboxes
            // Nota: Usamos JsonDocument para extraer datos sin importar cómo sea tu DTO exacto
            await LoadDropdownsAsync();

            return View(new AccountProductCreateViewModel());
        }

        // --- CREAR CONTRATO (POST) ---
        [HttpPost]
        public async Task<IActionResult> Create(AccountProductCreateViewModel model)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(model);
            }

            var jsonContent = new StringContent(JsonSerializer.Serialize(model), System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("api/account-products", jsonContent);
                if (response.IsSuccessStatusCode) return RedirectToAction("Index");

                var errorDetail = await ApiErrorParser.ExtractMessageAsync(response);
                ViewBag.ErrorMessage = $"Error al crear el contrato: {errorDetail}";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error de conexión: {ex.Message}";
            }

            await LoadDropdownsAsync();
            return View(model);
        }

        // --- ELIMINAR CONTRATO (POST) ---
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.DeleteAsync($"api/account-products/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = response.StatusCode == System.Net.HttpStatusCode.Conflict
                    ? "No se puede eliminar porque tiene transacciones asociadas."
                    : "No se pudo eliminar el contrato.";
            }

            return RedirectToAction("Index");
        }

        // Helper para cargar las listas desplegables
        private async Task LoadDropdownsAsync()
        {
            try
            {
                // 1. Clientes
                var clientRes = await _httpClient.GetAsync("api/clients");
                if (clientRes.IsSuccessStatusCode) ViewBag.Clients = await clientRes.Content.ReadAsStringAsync();

                // 2. Productos Financieros
                var prodRes = await _httpClient.GetAsync("api/financial-products");
                if (prodRes.IsSuccessStatusCode) ViewBag.Products = await prodRes.Content.ReadAsStringAsync();

                // 3. Empleados
                var empRes = await _httpClient.GetAsync("api/employees");
                if (empRes.IsSuccessStatusCode) ViewBag.Employees = await empRes.Content.ReadAsStringAsync();
            }
            catch { /* Ignoramos si falla, la vista manejará las listas vacías */ }
        }
        // --- VER DETALLES (GET) ---
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"api/account-products/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var product = JsonSerializer.Deserialize<AccountProductDetailsViewModel>(jsonString, options);
                return View(product);
            }
            return RedirectToAction("Index");
        }

        // --- EDITAR CONTRATO (GET) ---
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"api/account-products/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var detail = JsonSerializer.Deserialize<AccountProductDetailsViewModel>(jsonString, options);

                if (detail == null) return RedirectToAction("Index");

                // Mapeamos lo que leímos de la API al modelo del formulario
                var model = new AccountProductEditViewModel
                {
                    Id = detail.Id,
                    ClientId = detail.ClientId,
                    FinancialProductId = detail.FinancialProductId,
                    EmployeeId = detail.EmployeeId,
                    AccountNumber = detail.AccountNumber,
                    Amount = detail.Amount,
                    OpenDate = detail.OpenDate.DateTime,
                    MaturityDate = detail.MaturityDate?.DateTime,
                    Status = detail.Status
                };

                await LoadDropdownsAsync();
                return View(model);
            }
            return RedirectToAction("Index");
        }

        // --- EDITAR CONTRATO (POST) ---
        [HttpPost]
        public async Task<IActionResult> Edit(int id, AccountProductEditViewModel model)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(model);
            }

            var jsonContent = new StringContent(JsonSerializer.Serialize(model), System.Text.Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PutAsync($"api/account-products/{id}", jsonContent);
                if (response.IsSuccessStatusCode) return RedirectToAction("Index");

                var errorDetail = await ApiErrorParser.ExtractMessageAsync(response);
                ViewBag.ErrorMessage = $"Error al actualizar: {errorDetail}";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error de conexión: {ex.Message}";
            }

            await LoadDropdownsAsync();
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Operador,Consulta")]
        public async Task<IActionResult> Pending()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var page = new AccountProductsPendingPageViewModel();
            page.Employees = await LoadEmployeeOptionsAsync();

            try
            {
                var response = await _httpClient.GetAsync("api/account-products/pending");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    page.Pending = JsonSerializer.Deserialize<List<AccountProductItemViewModel>>(json, options) ?? new List<AccountProductItemViewModel>();
                    return View(page);
                }

                ViewBag.ErrorMessage = "No se pudieron cargar las solicitudes pendientes.";
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Error de conexión con el servidor.";
            }

            return View(page);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Operador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, int employeeId)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new { employeeId };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync($"api/account-products/{id}/approve", content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Solicitud aprobada y producto activado.";
                }
                else
                {
                    var detail = await ApiErrorParser.ExtractMessageAsync(response);
                    TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(detail) ? "No se pudo aprobar la solicitud." : detail;
                }
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Error de conexión con el servidor.";
            }

            return RedirectToAction(nameof(Pending));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Operador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.PostAsync($"api/account-products/{id}/reject", null);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Solicitud rechazada.";
                }
                else
                {
                    var detail = await ApiErrorParser.ExtractMessageAsync(response);
                    TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(detail) ? "No se pudo rechazar la solicitud." : detail;
                }
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Error de conexión con el servidor.";
            }

            return RedirectToAction(nameof(Pending));
        }

        private async Task<List<EmployeeOptionViewModel>> LoadEmployeeOptionsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/employees");
                if (!response.IsSuccessStatusCode)
                {
                    return new List<EmployeeOptionViewModel>();
                }

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var employees = JsonSerializer.Deserialize<List<EmployeePickItem>>(json, options) ?? new List<EmployeePickItem>();
                return employees
                    .Where(e => !string.Equals(e.EmployeeCode, "EMP000", StringComparison.OrdinalIgnoreCase))
                    .Select(e => new EmployeeOptionViewModel
                    {
                        Id = e.Id,
                        DisplayName = $"{e.FirstName} {e.LastName} ({e.EmployeeCode})"
                    })
                    .ToList();
            }
            catch
            {
                return new List<EmployeeOptionViewModel>();
            }
        }

        private sealed class EmployeePickItem
        {
            public int Id { get; set; }
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string EmployeeCode { get; set; } = string.Empty;
        }
    }
}