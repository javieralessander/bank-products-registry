using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BankProductsRegistry.Frontend.Utilities;

namespace BankProductsRegistry.Frontend.Controllers
{
    /// <summary>Descarga de PDFs generados por la API (misma sesión JWT que el resto del sitio).</summary>
    [Authorize]
    public sealed class ReportsExportController : Controller
    {
        private readonly HttpClient _httpClient;

        public ReportsExportController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> ClientPortfolioPdf(int clientId)
        {
            if (!EnsureClientScope(clientId))
            {
                return Forbid();
            }

            return await ProxyPdfAsync($"api/reports/clients/{clientId}/portfolio/pdf", $"portafolio-cliente-{clientId}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> ClientCreditHistoryPdf(int clientId)
        {
            if (!EnsureClientScope(clientId))
            {
                return Forbid();
            }

            return await ProxyPdfAsync($"api/reports/clients/{clientId}/credit-history/pdf", $"historial-credito-{clientId}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> ClientCreditScorePdf(int clientId)
        {
            if (!EnsureClientScope(clientId))
            {
                return Forbid();
            }

            return await ProxyPdfAsync($"api/reports/clients/{clientId}/credit-score/pdf", $"score-credito-{clientId}.pdf");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Operador,Consulta")]
        public async Task<IActionResult> DashboardPdf()
        {
            return await ProxyPdfAsync("api/reports/dashboard/pdf", "dashboard-resumen.pdf");
        }

        private bool EnsureClientScope(int clientId)
        {
            if (!User.IsInRole("Cliente"))
            {
                return true;
            }

            var claim = User.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value;
            return int.TryParse(claim, out var cid) && cid == clientId;
        }

        private async Task<IActionResult> ProxyPdfAsync(string relativeUrl, string downloadFileName)
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "jwt_token")?.Value;
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            _httpClient.SetBearerToken(token);
            var response = await _httpClient.GetAsync(relativeUrl);
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            return File(bytes, "application/pdf", downloadFileName);
        }
    }
}
