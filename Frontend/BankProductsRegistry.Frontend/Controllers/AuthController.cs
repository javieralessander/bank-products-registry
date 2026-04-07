using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using BankProductsRegistry.Frontend.Models;
using BankProductsRegistry.Frontend.Utilities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Net;

namespace BankProductsRegistry.Frontend.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient _httpClient;

        public AuthController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /* ========================
            LOGIN
        ======================== */
        [HttpGet]
        public IActionResult Login()
        {
            // Si el usuario ya está logueado, leemos sus roles y lo mandamos a su pantalla
            if (User.Identity is not null && User.Identity.IsAuthenticated)
            {
                var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

                if (roles.Contains("Admin") || roles.Contains("Operador") || roles.Contains("Consulta"))
                {
                    return RedirectToAction("Index", "Dashboard");
                }

                return RedirectToAction("Index", "ClientPortal");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/auth/login", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var authResult = JsonSerializer.Deserialize<AuthResponse>(jsonString, options);

                    if (authResult != null && !string.IsNullOrEmpty(authResult.AccessToken))
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, authResult.User?.FullName ?? model.UserNameOrEmail),
                            new Claim(ClaimTypes.Email, authResult.User?.Email ?? string.Empty),
                            new Claim("jwt_token", authResult.AccessToken)
                        };

                        if (authResult.User?.Roles != null)
                        {
                            foreach (var role in authResult.User.Roles)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, role));
                            }
                        }

                        if (authResult.User?.ClientId is int clientId)
                        {
                            claims.Add(new Claim("client_id", clientId.ToString()));
                        }

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                        if (authResult.User?.Roles != null &&
                            (authResult.User.Roles.Contains("Admin") ||
                             authResult.User.Roles.Contains("Operador") ||
                             authResult.User.Roles.Contains("Consulta")))
                        {
                            return RedirectToAction("Index", "Dashboard");
                        }

                        return RedirectToAction("Index", "ClientPortal");
                    }
                }

                ViewBag.ErrorMessage = "Credenciales inválidas. Verifica tu usuario y contraseña.";
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Error de conexión: El servidor (API) no está respondiendo.";
            }

            return View(model);
        }

        /* ========================
            REGISTRO
        ======================== */
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

                // Llamamos a la API para crear el usuario
                var response = await _httpClient.PostAsync("api/auth/register", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Tu solicitud de acceso fue registrada. Un administrador u operador debe vincular tu perfil de cliente antes de habilitar el inicio de sesión.";
                    return RedirectToAction("Login");
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var detail = await ApiErrorParser.ExtractMessageAsync(response);
                    ViewBag.ErrorMessage = string.IsNullOrWhiteSpace(detail)
                        ? "No se pudo crear la cuenta. Verifica los datos."
                        : $"No se pudo crear la cuenta. {detail}";
                    return View(model);
                }

                ViewBag.ErrorMessage = "No se pudo crear la cuenta. Verifica que los datos sean correctos o que el correo no esté en uso.";
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Error de conexión: El servidor (API) no está respondiendo.";
            }

            return View(model);
        }

        /* ========================
            LOGOUT
        ======================== */
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
