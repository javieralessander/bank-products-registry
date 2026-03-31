using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using BankProductsRegistry.Frontend.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace BankProductsRegistry.Frontend.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient _httpClient;

        public AuthController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:7039/");
        }

        /* ========================
           LOGIN
        ======================== */
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity is not null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Dashboard");

            return View();
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

                    // AHORA BUSCAMOS EL "AccessToken"
                    if (authResult != null && !string.IsNullOrEmpty(authResult.AccessToken))
                    {
                        var claims = new List<Claim>
                        {
                            // Guardamos el nombre real (ej: "Administrador General")
                            new Claim(ClaimTypes.Name, authResult.User?.FullName ?? model.UserNameOrEmail),
                            new Claim(ClaimTypes.Email, authResult.User?.Email ?? string.Empty),
                            new Claim("jwt_token", authResult.AccessToken) // Guardamos el JWT
                        };

                        // AQUI AGREGAMOS LOS ROLES MAGICAMENTE
                        if (authResult.User?.Roles != null)
                        {
                            foreach (var role in authResult.User.Roles)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, role));
                            }
                        }

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                        return RedirectToAction("Index", "Dashboard");
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
            if (User.Identity is not null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
                // Asegúrate de que esta ruta sea la correcta en tu API
                var response = await _httpClient.PostAsync("api/users/register", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cuenta creada exitosamente. Por favor, inicia sesión.";
                    return RedirectToAction("Login");
                }

                ViewBag.ErrorMessage = "Hubo un problema al crear la cuenta. Verifica los datos ingresados.";
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
            return RedirectToAction("Login", "Auth");
        }
    }
}