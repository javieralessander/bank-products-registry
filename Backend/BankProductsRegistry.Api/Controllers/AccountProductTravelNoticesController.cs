using BankProductsRegistry.Api.Data;
using BankProductsRegistry.Api.Dtos.AccountProducts;
using BankProductsRegistry.Api.Models;
using BankProductsRegistry.Api.Models.Enums;
using BankProductsRegistry.Api.Security;
using BankProductsRegistry.Api.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Controllers;

[Route("api/account-products/{accountProductId:int}/travel-notices")]
[Authorize]
public sealed class AccountProductTravelNoticesController(BankProductsDbContext dbContext) : ApiControllerBase
{
    private const string LocalCountryCode = "DO";

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<AccountProductTravelNoticeResponse>>> GetAllAsync(
        int accountProductId,
        CancellationToken cancellationToken)
    {
        if (!await dbContext.AccountProducts.AnyAsync(accountProduct => accountProduct.Id == accountProductId, cancellationToken))
        {
            return NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Producto contratado no encontrado",
                $"No existe un producto contratado con el id {accountProductId}."));
        }

        var notices = await dbContext.AccountProductTravelNotices
            .AsNoTracking()
            .Include(notice => notice.Countries)
            .Where(notice => notice.AccountProductId == accountProductId)
            .OrderByDescending(notice => notice.StartsAt)
            .ThenByDescending(notice => notice.Id)
            .ToListAsync(cancellationToken);

        return Ok(notices.Select(Map).ToList());
    }

    [HttpPost]
    [Authorize(Policy = AuthPolicies.WriteAccess)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccountProductTravelNoticeResponse>> CreateAsync(
        int accountProductId,
        [FromBody] AccountProductTravelNoticeCreateRequest request,
        CancellationToken cancellationToken)
    {
        var accountProduct = await dbContext.AccountProducts
            .FirstOrDefaultAsync(currentAccountProduct => currentAccountProduct.Id == accountProductId, cancellationToken);

        if (accountProduct is null)
        {
            return NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Producto contratado no encontrado",
                $"No existe un producto contratado con el id {accountProductId}."));
        }

        if (accountProduct.Status == AccountProductStatus.Closed || accountProduct.Status == AccountProductStatus.Cancelled)
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Operacion no permitida",
                "No se puede registrar un aviso de viaje para un producto contratado cerrado o cancelado."));
        }

        var normalizedCountries = NormalizeCountries(request.Countries);
        var validationProblem = ValidateRequest(request, normalizedCountries);
        if (validationProblem is not null)
        {
            return BadRequest(validationProblem);
        }

        var hasOverlap = await dbContext.AccountProductTravelNoticeCountries
            .AnyAsync(country =>
                normalizedCountries.Contains(country.CountryCode) &&
                country.TravelNotice != null &&
                country.TravelNotice.AccountProductId == accountProductId &&
                country.TravelNotice.CancelledAt == null &&
                country.TravelNotice.StartsAt < request.EndsAt &&
                country.TravelNotice.EndsAt > request.StartsAt,
                cancellationToken);

        if (hasOverlap)
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Aviso de viaje solapado",
                "Ya existe un aviso de viaje activo o programado para al menos uno de los paises indicados en ese periodo."));
        }

        var (actorUserId, actorUserName) = GetCurrentActor();
        var notice = new AccountProductTravelNotice
        {
            AccountProductId = accountProductId,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            Reason = NormalizationHelper.NormalizeOptionalText(request.Reason) ?? string.Empty,
            RequestedByUserId = actorUserId,
            RequestedByUserName = actorUserName,
            Countries = normalizedCountries
                .Select(countryCode => new AccountProductTravelNoticeCountry
                {
                    CountryCode = countryCode
                })
                .ToList()
        };

        dbContext.AccountProductTravelNotices.Add(notice);

        // ---> NOTIFICACIÓN AUTOMÁTICA DE VIAJE <---
        var destinationCountries = string.Join(", ", normalizedCountries);
        dbContext.SystemNotifications.Add(new SystemNotification
        {
            Title = $"Viaje registrado — {actorUserName}",
            Message = $"Viaje a {destinationCountries} del {request.StartsAt:dd/MM} al {request.EndsAt:dd/MM}. Producto contratado #{accountProductId}.",
            Type = "Viaje",
            CreatedAt = DateTimeOffset.UtcNow,
            IsRead = false
        });
        // ------------------------------------------

        await dbContext.SaveChangesAsync(cancellationToken);

        return StatusCode(StatusCodes.Status201Created, Map(notice));
    }

    [HttpPost("{noticeId:int}/cancel")]
    [Authorize(Policy = AuthPolicies.WriteAccess)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccountProductTravelNoticeResponse>> CancelAsync(
        int accountProductId,
        int noticeId,
        [FromBody] AccountProductTravelNoticeCancelRequest request,
        CancellationToken cancellationToken)
    {
        var notice = await dbContext.AccountProductTravelNotices
            .Include(currentNotice => currentNotice.Countries)
            .FirstOrDefaultAsync(
                currentNotice => currentNotice.Id == noticeId && currentNotice.AccountProductId == accountProductId,
                cancellationToken);

        if (notice is null)
        {
            return NotFound(BuildProblem(
                StatusCodes.Status404NotFound,
                "Aviso de viaje no encontrado",
                $"No existe un aviso de viaje con el id {noticeId} para el producto contratado {accountProductId}."));
        }

        if (notice.CancelledAt.HasValue)
        {
            return Conflict(BuildProblem(
                StatusCodes.Status409Conflict,
                "Aviso ya cancelado",
                "El aviso de viaje indicado ya fue cancelado."));
        }

        var (actorUserId, actorUserName) = GetCurrentActor();
        notice.CancelledAt = DateTimeOffset.UtcNow;
        notice.CancelledByUserId = actorUserId;
        notice.CancelledByUserName = actorUserName;
        notice.CancellationReason = NormalizationHelper.NormalizeOptionalText(request.Reason);

        // ---> NOTIFICACIÓN DE CANCELACIÓN DE VIAJE <---
        dbContext.SystemNotifications.Add(new SystemNotification
        {
            Title = "Viaje cancelado",
            Message = $"El viaje programado para el producto #{accountProductId} fue cancelado por {actorUserName}.",
            Type = "Sistema",
            CreatedAt = DateTimeOffset.UtcNow,
            IsRead = false
        });
        // ----------------------------------------------

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(Map(notice));
    }

    private static IReadOnlyCollection<string> NormalizeCountries(IReadOnlyCollection<string> countries) =>
        countries
            .Where(country => !string.IsNullOrWhiteSpace(country))
            .Select(country => NormalizationHelper.NormalizeCode(country))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static ProblemDetails? ValidateRequest(
        AccountProductTravelNoticeCreateRequest request,
        IReadOnlyCollection<string> normalizedCountries)
    {
        if (request.EndsAt <= request.StartsAt)
        {
            return BuildProblem(
                StatusCodes.Status400BadRequest,
                "Datos no validos",
                "La fecha de fin del aviso de viaje debe ser posterior a la fecha de inicio.");
        }

        if (normalizedCountries.Count == 0)
        {
            return BuildProblem(
                StatusCodes.Status400BadRequest,
                "Datos no validos",
                "Debes indicar al menos un pais para el aviso de viaje.");
        }

        if (normalizedCountries.Any(country => country.Length != 2))
        {
            return BuildProblem(
                StatusCodes.Status400BadRequest,
                "Datos no validos",
                "Cada pais debe enviarse en formato ISO de dos letras.");
        }

        if (normalizedCountries.Any(country => string.Equals(country, LocalCountryCode, StringComparison.OrdinalIgnoreCase)))
        {
            return BuildProblem(
                StatusCodes.Status400BadRequest,
                "Datos no validos",
                "El aviso de viaje solo aplica a paises internacionales distintos de DO.");
        }

        return null;
    }

    private static AccountProductTravelNoticeResponse Map(AccountProductTravelNotice notice)
    {
        var now = DateTimeOffset.UtcNow;
        var isActive = !notice.CancelledAt.HasValue &&
                       notice.StartsAt <= now &&
                       notice.EndsAt >= now;

        return new AccountProductTravelNoticeResponse(
            notice.Id,
            notice.AccountProductId,
            notice.StartsAt,
            notice.EndsAt,
            notice.Reason,
            notice.Countries.Select(country => country.CountryCode).OrderBy(country => country).ToArray(),
            notice.RequestedByUserId,
            notice.RequestedByUserName,
            notice.CancelledAt,
            notice.CancelledByUserId,
            notice.CancelledByUserName,
            notice.CancellationReason,
            isActive,
            notice.CreatedAt,
            notice.UpdatedAt);
    }
}