using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BankProductsRegistry.Frontend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankProductsRegistry.Frontend.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly HttpClient _httpClient;

        public UsersController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.GetAsync("api/users");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var users = JsonSerializer.Deserialize<List<UserManagementViewModel>>(jsonString, options);
                    return View(users ?? new List<UserManagementViewModel>());
                }

                ViewBag.ErrorMessage = "No se pudo cargar el listado de usuarios.";
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Error de conexión: El servidor (API) no está respondiendo.";
            }

            return View(new List<UserManagementViewModel>());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new UserCreateViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/users", jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Usuario creado correctamente.";
                    return RedirectToAction("Index");
                }

                var detail = await response.Content.ReadAsStringAsync();
                ViewBag.ErrorMessage = $"No se pudo crear el usuario. Detalle: {detail}";
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Error de conexión: El servidor (API) no está respondiendo.";
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(UserStatusUpdateViewModel model)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new { model.IsActive };
            var request = new HttpRequestMessage(HttpMethod.Patch, $"api/users/{model.Id}/status")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Estado de usuario actualizado.";
            }
            else
            {
                var detail = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"No se pudo actualizar el estado. {detail}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(UserRoleUpdateViewModel model)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new { model.Role };
            var request = new HttpRequestMessage(HttpMethod.Patch, $"api/users/{model.Id}/role")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Rol de usuario actualizado.";
            }
            else
            {
                var detail = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"No se pudo actualizar el rol. {detail}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(UserResetPasswordViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.NewPassword) || model.NewPassword.Length < 8)
            {
                TempData["ErrorMessage"] = "La nueva contraseña debe tener al menos 8 caracteres.";
                return RedirectToAction("Index");
            }

            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new { model.NewPassword };
            var json = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"api/users/{model.Id}/reset-password", json);
            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NoContent)
            {
                TempData["SuccessMessage"] = "Contraseña restablecida correctamente.";
            }
            else
            {
                var detail = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"No se pudo restablecer la contraseña. {detail}";
            }

            return RedirectToAction("Index");
        }
    }
}
