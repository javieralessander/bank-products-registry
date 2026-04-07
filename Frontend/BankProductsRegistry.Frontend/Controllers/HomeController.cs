using Microsoft.AspNetCore.Mvc;

namespace BankProductsRegistry.Frontend.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Si el usuario ya está logueado, lo redirigimos automáticamente
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Verifica los roles exactos que usas en tu backend
                if (User.IsInRole("Admin") || User.IsInRole("Employee"))
                {
                    return RedirectToAction("Index", "Dashboard");
                }

                return RedirectToAction("Index", "ClientPortal");
            }

            // Si no está logueado, le mostramos la página del Banco
            return View();
        }
    }
}