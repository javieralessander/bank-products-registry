using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BankProductsRegistry.Frontend.Models;
using BankProductsRegistry.Frontend.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankProductsRegistry.Frontend.Controllers
{
    /// <summary>Catálogo de empleados del banco (entidad Employee); no confundir con el rol JWT <c>Operador</c>.</summary>
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly HttpClient _httpClient;

        public EmployeesController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Operador,Consulta")]
        public async Task<IActionResult> Index()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.GetAsync("api/employees");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var employees = JsonSerializer.Deserialize<List<EmployeeViewModel>>(jsonString, options);
                    return View(employees ?? new List<EmployeeViewModel>());
                }

                ViewBag.ErrorMessage = "Hubo un problema al cargar los empleados desde el servidor.";
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Error de conexión: El servidor (API) no está respondiendo.";
            }

            return View(new List<EmployeeViewModel>());
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(EmployeeViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/employees", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    ViewBag.ErrorMessage = "No tienes permisos para crear empleados (solo Admin).";
                    return View(model);
                }

                var errorDetail = await ApiErrorParser.ExtractMessageAsync(response);
                ViewBag.ErrorMessage = $"No se pudo crear el empleado. Detalle: {errorDetail}";
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Error de conexión al crear el empleado.";
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.GetAsync($"api/employees/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var employee = JsonSerializer.Deserialize<EmployeeViewModel>(jsonString, options);
                    return View(employee);
                }
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Error de conexión al cargar el empleado.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, EmployeeViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"api/employees/{id}", jsonContent);

                if (response.IsSuccessStatusCode) return RedirectToAction("Index");

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    ViewBag.ErrorMessage = "No tienes permisos para editar empleados (solo Admin).";
                    return View(model);
                }

                var errorDetail = await ApiErrorParser.ExtractMessageAsync(response);
                ViewBag.ErrorMessage = $"No se pudo actualizar el empleado. Detalle: {errorDetail}";
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Error de conexión al actualizar el empleado.";
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.DeleteAsync($"api/employees/{id}");
                if (response.IsSuccessStatusCode) return RedirectToAction("Index");

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    TempData["ErrorMessage"] = "No tienes permisos para eliminar empleados (solo Admin).";
                }
                else if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    TempData["ErrorMessage"] = "No se puede eliminar el empleado porque tiene productos bancarios asignados.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Hubo un error al intentar eliminar el empleado.";
                }
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Error de conexión al eliminar el empleado.";
            }

            return RedirectToAction("Index");
        }
    }
}
