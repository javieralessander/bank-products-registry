using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankProductsRegistry.Frontend.Controllers
{
    // Solo permitimos el acceso a usuarios autenticados
    [Authorize]
    public class ClientPortalController : Controller
    {
        public IActionResult Index()
        {
            // Nota: Como me indicaste que los roles son 1 (Admin), 2 (Empleado), 3 (Cliente)
            // En un futuro cercano, aquí haremos una petición HTTP a tu API para traer 
            // el balance real y las cuentas de este cliente en específico usando su ID.

            return View();
        }
    }
}