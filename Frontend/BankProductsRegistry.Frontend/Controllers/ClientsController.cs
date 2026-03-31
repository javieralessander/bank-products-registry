using Microsoft.AspNetCore.Mvc;
using BankProductsRegistry.Frontend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace BankProductsRegistry.Frontend.Controllers
{
    [Authorize]
    public class ClientsController : Controller
    {
        private readonly HttpClient _httpClient;

        public ClientsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Asegúrate de que este puerto sea el mismo de tu API
            _httpClient.BaseAddress = new Uri("https://localhost:7039/");
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
        public IActionResult Create()
        {
            return View();
        }

        // 3. Recibir los datos del formulario y enviarlos a la API
        [HttpPost]
        public async Task<IActionResult> Create(ClientViewModel model)
        {
            // Verificamos que los datos del formulario sean válidos antes de enviarlos
            if (!ModelState.IsValid) return View(model);

            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login", "Auth");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Convertimos el modelo a JSON
                var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

                // Enviamos el POST a la API
                var response = await _httpClient.PostAsync("api/clients", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    // Si se creó con éxito, regresamos a la pantalla de la tabla
                    return RedirectToAction("Index");
                }

                // Si la API rechaza la solicitud (ej. cédula o correo duplicado)
                ViewBag.ErrorMessage = "Hubo un problema al crear el cliente. Verifica que la cédula o correo no estén registrados ya.";
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Error de conexión: El servidor (API) no está respondiendo.";
            }

            // Si falló, regresamos la vista con los datos que el usuario ya había escrito para que no empiece de cero
            return View(model);
        }
    }
}