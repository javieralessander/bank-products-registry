using BankProductsRegistry.Api.Data;
using BankProductsRegistry.Api.Dtos.Reports;
using BankProductsRegistry.Api.Models;
using BankProductsRegistry.Api.Models.Enums;
using BankProductsRegistry.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Services;

public sealed class ReportService(BankProductsDbContext dbContext) : IReportService
{
    private const string LocalCountryCode = "DO";
    private const string InternalScoreMethodology = "interna_v1";
    private const string InternalScoreDisclaimer = "Este score es una heuristica interna del banco y no reemplaza un score de buró externo.";

    public async Task<ClientPortfolioReportDto?> GetClientPortfolioAsync(int clientId, CancellationToken cancellationToken = default)
    {
        var client = await dbContext.Clients
            .AsNoTracking()
            .Include(currentClient => currentClient.AccountProducts)
                .ThenInclude(accountProduct => accountProduct.FinancialProduct)
            .Include(currentClient => currentClient.AccountProducts)
                .ThenInclude(accountProduct => accountProduct.Transactions)
            .FirstOrDefaultAsync(currentClient => currentClient.Id == clientId, cancellationToken);

        if (client is null)
        {
            return null;
        }

        var accountItems = client.AccountProducts
            .OrderBy(accountProduct => accountProduct.AccountNumber)
            .Select(accountProduct =>
            {
                var deposits = accountProduct.Transactions
                    .Where(transaction => transaction.TransactionType == TransactionType.Deposit)
                    .Sum(transaction => transaction.Amount);

                var withdrawals = accountProduct.Transactions
                    .Where(transaction =>
                        transaction.TransactionType == TransactionType.Withdrawal ||
                        transaction.TransactionType == TransactionType.Payment ||
                        transaction.TransactionType == TransactionType.Fee)
                    .Sum(transaction => transaction.Amount);

                return new ClientPortfolioItemDto(
                    accountProduct.Id,
                    accountProduct.AccountNumber,
                    accountProduct.FinancialProduct?.ProductName ?? string.Empty,
                    accountProduct.Status,
                    accountProduct.Amount,
                    accountProduct.OpenDate,
                    accountProduct.Transactions.Count,
                    deposits,
                    withdrawals);
            })
            .ToList();

        return new ClientPortfolioReportDto(
            client.Id,
            $"{client.FirstName} {client.LastName}",
            client.Email,
            accountItems.Count,
            accountItems.Sum(account => account.Amount),
            accountItems.Sum(account => account.Deposits),
            accountItems.Sum(account => account.Withdrawals),
            accountItems);
    }

    public async Task<ClientCreditHistoryReportDto?> GetClientCreditHistoryAsync(int clientId, CancellationToken cancellationToken = default)
    {
        var data = await LoadCreditDataAsync(clientId, cancellationToken);
        if (data is null)
        {
            return null;
        }

        var generatedAt = DateTimeOffset.UtcNow;
        var score = BuildCreditScore(data.Client, data.AccountProducts, generatedAt);
        var overview = BuildCreditOverview(data.AccountProducts, generatedAt);
        var accounts = BuildCreditAccountItems(data.AccountProducts, generatedAt);
        var events = BuildCreditEvents(data.AccountProducts, data.LimitHistoryEntries)
            .OrderByDescending(currentEvent => currentEvent.OccurredAt)
            .ThenBy(currentEvent => currentEvent.Category)
            .Take(20)
            .ToList();

        return new ClientCreditHistoryReportDto(
            data.Client.Id,
            $"{data.Client.FirstName} {data.Client.LastName}",
            data.Client.NationalId,
            data.Client.Email,
            generatedAt,
            score,
            overview,
            accounts,
            events);
    }

    public async Task<ClientCreditScoreReportDto?> GetClientCreditScoreAsync(int clientId, CancellationToken cancellationToken = default)
    {
        var data = await LoadCreditDataAsync(clientId, cancellationToken);
        if (data is null)
        {
            return null;
        }

        return BuildCreditScore(data.Client, data.AccountProducts, DateTimeOffset.UtcNow);
    }

    private async Task<ClientCreditData?> LoadCreditDataAsync(int clientId, CancellationToken cancellationToken)
    {
        var client = await dbContext.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(currentClient => currentClient.Id == clientId, cancellationToken);

        if (client is null)
        {
            return null;
        }

        var accountProducts = await dbContext.AccountProducts
            .AsNoTracking()
            .Where(accountProduct => accountProduct.ClientId == clientId)
            .Include(accountProduct => accountProduct.FinancialProduct)
            .Include(accountProduct => accountProduct.Transactions)
            .Include(accountProduct => accountProduct.LimitProfile)
            .Include(accountProduct => accountProduct.Blocks)
            .OrderBy(accountProduct => accountProduct.AccountNumber)
            .ToListAsync(cancellationToken);

        var accountProductIds = accountProducts.Select(accountProduct => accountProduct.Id).ToArray();
        var limitHistoryEntries = accountProductIds.Length == 0
            ? []
            : await dbContext.AccountProductLimitHistoryEntries
                .AsNoTracking()
                .Where(entry => accountProductIds.Contains(entry.AccountProductId))
                .ToListAsync(cancellationToken);

        return new ClientCreditData(client, accountProducts, limitHistoryEntries);
    }

    private static ClientCreditScoreReportDto BuildCreditScore(
        Client client,
        IReadOnlyCollection<AccountProduct> accountProducts,
        DateTimeOffset generatedAt)
    {
        var score = 650;
        var factors = new List<ClientCreditScoreFactorDto>();

        var activeProducts = accountProducts.Count(accountProduct =>
            accountProduct.Status == AccountProductStatus.Active ||
            accountProduct.Status == AccountProductStatus.Pending ||
            accountProduct.Status == AccountProductStatus.Delinquent);

        var oldestOpenDate = accountProducts.Count == 0
            ? (DateTimeOffset?)null
            : accountProducts.Min(accountProduct => accountProduct.OpenDate);

        if (accountProducts.Count == 0)
        {
            score -= 180;
            factors.Add(new ClientCreditScoreFactorDto(
                "Historial insuficiente",
                -180,
                "El cliente aun no tiene productos contratados registrados en el sistema."));
        }
        else if (oldestOpenDate.HasValue)
        {
            var relationshipDays = (generatedAt - oldestOpenDate.Value).TotalDays;
            if (relationshipDays >= 365)
            {
                score += 25;
                factors.Add(new ClientCreditScoreFactorDto(
                    "Antigüedad de relación",
                    25,
                    "El cliente mantiene productos con mas de un ano de antigüedad."));
            }
            else if (relationshipDays >= 180)
            {
                score += 15;
                factors.Add(new ClientCreditScoreFactorDto(
                    "Antigüedad de relación",
                    15,
                    "El cliente mantiene una relacion activa sostenida por al menos seis meses."));
            }
        }

        var productTypeCount = accountProducts
            .Select(accountProduct => accountProduct.FinancialProduct?.ProductType)
            .Where(productType => productType.HasValue)
            .Select(productType => productType!.Value)
            .Distinct()
            .Count();

        if (productTypeCount >= 3)
        {
            score += 25;
            factors.Add(new ClientCreditScoreFactorDto(
                "Diversidad de productos",
                25,
                "El cliente maneja varios tipos de productos, lo que mejora la lectura interna de comportamiento."));
        }
        else if (productTypeCount == 2)
        {
            score += 15;
            factors.Add(new ClientCreditScoreFactorDto(
                "Diversidad de productos",
                15,
                "El cliente tiene mas de un tipo de producto activo en el banco."));
        }
        else if (productTypeCount == 1 && activeProducts > 0)
        {
            score += 5;
            factors.Add(new ClientCreditScoreFactorDto(
                "Diversidad de productos",
                5,
                "El cliente tiene un historial basico con al menos un producto activo."));
        }

        var paymentTransactions = accountProducts
            .SelectMany(accountProduct => accountProduct.Transactions)
            .Where(transaction => transaction.TransactionType == TransactionType.Payment)
            .ToList();

        var paymentImpact = paymentTransactions.Count switch
        {
            >= 6 => 35,
            >= 3 => 20,
            >= 1 => 10,
            _ => -20
        };
        score += paymentImpact;
        factors.Add(new ClientCreditScoreFactorDto(
            "Actividad de pagos",
            paymentImpact,
            paymentImpact >= 0
                ? $"Se registran {paymentTransactions.Count} pagos internos en el historial del cliente."
                : "No se registran pagos internos suficientes para fortalecer el score."));

        var delinquentProducts = accountProducts.Count(accountProduct => accountProduct.Status == AccountProductStatus.Delinquent);
        if (delinquentProducts > 0)
        {
            var delinquencyImpact = -Math.Min(160, delinquentProducts * 70);
            score += delinquencyImpact;
            factors.Add(new ClientCreditScoreFactorDto(
                "Mora interna",
                delinquencyImpact,
                $"El cliente tiene {delinquentProducts} producto(s) marcados en mora."));
        }
        else if (activeProducts > 0)
        {
            score += 20;
            factors.Add(new ClientCreditScoreFactorDto(
                "Mora interna",
                20,
                "No hay productos marcados en mora dentro del historial actual."));
        }

        var fraudBlockEvents = accountProducts
            .SelectMany(accountProduct => accountProduct.Blocks)
            .Count(block => block.BlockType == AccountProductBlockType.Fraud);

        if (fraudBlockEvents > 0)
        {
            var fraudImpact = -Math.Min(180, fraudBlockEvents * 90);
            score += fraudImpact;
            factors.Add(new ClientCreditScoreFactorDto(
                "Eventos de fraude",
                fraudImpact,
                $"Se detectaron {fraudBlockEvents} bloqueo(s) por fraude en el historial interno."));
        }
        else
        {
            score += 10;
            factors.Add(new ClientCreditScoreFactorDto(
                "Eventos de fraude",
                10,
                "No se registran bloqueos por fraude en el historial del cliente."));
        }

        var activeBlockedProducts = accountProducts.Count(accountProduct => GetActiveBlock(accountProduct, generatedAt) is not null);
        if (activeBlockedProducts > 0)
        {
            var activeBlockImpact = -Math.Min(60, activeBlockedProducts * 20);
            score += activeBlockImpact;
            factors.Add(new ClientCreditScoreFactorDto(
                "Bloqueos activos",
                activeBlockImpact,
                $"Hay {activeBlockedProducts} producto(s) con bloqueo activo al momento del reporte."));
        }
        else if (activeProducts > 0)
        {
            score += 10;
            factors.Add(new ClientCreditScoreFactorDto(
                "Bloqueos activos",
                10,
                "No hay productos actualmente bloqueados."));
        }

        var creditAccountsWithLimit = accountProducts
            .Where(accountProduct =>
                accountProduct.LimitProfile?.CreditLimitTotal is > 0m)
            .ToList();

        var approvedCredit = creditAccountsWithLimit.Sum(accountProduct => accountProduct.LimitProfile!.CreditLimitTotal!.Value);
        var currentCreditExposure = creditAccountsWithLimit.Sum(accountProduct => accountProduct.Amount);
        decimal? utilizationRatio = approvedCredit > 0m
            ? currentCreditExposure / approvedCredit
            : null;

        if (utilizationRatio.HasValue)
        {
            var utilizationImpact = utilizationRatio.Value switch
            {
                <= 0.30m => 60,
                <= 0.50m => 35,
                <= 0.75m => 10,
                <= 0.90m => -20,
                _ => -70
            };

            score += utilizationImpact;
            factors.Add(new ClientCreditScoreFactorDto(
                "Utilizacion de credito",
                utilizationImpact,
                $"La utilizacion interna del credito aprobado es de {utilizationRatio.Value:P1}."));
        }

        score = Math.Clamp(score, 300, 850);

        return new ClientCreditScoreReportDto(
            client.Id,
            $"{client.FirstName} {client.LastName}",
            score,
            GetRiskBand(score),
            InternalScoreMethodology,
            InternalScoreDisclaimer,
            generatedAt,
            utilizationRatio,
            delinquentProducts,
            fraudBlockEvents,
            activeBlockedProducts,
            factors);
    }

    private static ClientCreditOverviewDto BuildCreditOverview(
        IReadOnlyCollection<AccountProduct> accountProducts,
        DateTimeOffset generatedAt)
    {
        var activeProducts = accountProducts.Count(accountProduct =>
            accountProduct.Status == AccountProductStatus.Active ||
            accountProduct.Status == AccountProductStatus.Pending ||
            accountProduct.Status == AccountProductStatus.Delinquent);

        var delinquentProducts = accountProducts.Count(accountProduct => accountProduct.Status == AccountProductStatus.Delinquent);
        var activeBlockedProducts = accountProducts.Count(accountProduct => GetActiveBlock(accountProduct, generatedAt) is not null);
        var fraudBlockEvents = accountProducts
            .SelectMany(accountProduct => accountProduct.Blocks)
            .Count(block => block.BlockType == AccountProductBlockType.Fraud);

        var creditAccountsWithLimit = accountProducts
            .Where(accountProduct => accountProduct.LimitProfile?.CreditLimitTotal is > 0m)
            .ToList();

        var totalPayments = accountProducts
            .SelectMany(accountProduct => accountProduct.Transactions)
            .Where(transaction => transaction.TransactionType == TransactionType.Payment)
            .Sum(transaction => transaction.Amount);

        var totalCharges = accountProducts
            .SelectMany(accountProduct => accountProduct.Transactions)
            .Where(transaction =>
                transaction.TransactionType == TransactionType.Withdrawal ||
                transaction.TransactionType == TransactionType.Payment ||
                transaction.TransactionType == TransactionType.Transfer ||
                transaction.TransactionType == TransactionType.Fee)
            .Sum(transaction => transaction.Amount);

        return new ClientCreditOverviewDto(
            accountProducts.Count,
            activeProducts,
            delinquentProducts,
            activeBlockedProducts,
            fraudBlockEvents,
            creditAccountsWithLimit.Sum(accountProduct => accountProduct.Amount),
            creditAccountsWithLimit.Sum(accountProduct => accountProduct.LimitProfile!.CreditLimitTotal!.Value),
            totalPayments,
            totalCharges,
            accountProducts.Count == 0 ? null : accountProducts.Min(accountProduct => accountProduct.OpenDate));
    }

    private static IReadOnlyCollection<ClientCreditAccountHistoryItemDto> BuildCreditAccountItems(
        IReadOnlyCollection<AccountProduct> accountProducts,
        DateTimeOffset generatedAt) =>
        accountProducts
            .OrderBy(accountProduct => accountProduct.AccountNumber)
            .Select(accountProduct =>
            {
                var approvedCreditLimit = accountProduct.LimitProfile?.CreditLimitTotal;
                decimal? utilizationRatio = approvedCreditLimit is > 0m
                    ? accountProduct.Amount / approvedCreditLimit.Value
                    : null;

                var totalPayments = accountProduct.Transactions
                    .Where(transaction => transaction.TransactionType == TransactionType.Payment)
                    .Sum(transaction => transaction.Amount);

                var lastPaymentDate = accountProduct.Transactions
                    .Where(transaction => transaction.TransactionType == TransactionType.Payment)
                    .OrderByDescending(transaction => transaction.TransactionDate)
                    .Select(transaction => (DateTimeOffset?)transaction.TransactionDate)
                    .FirstOrDefault();

                var activeBlock = GetActiveBlock(accountProduct, generatedAt);

                return new ClientCreditAccountHistoryItemDto(
                    accountProduct.Id,
                    accountProduct.AccountNumber,
                    accountProduct.FinancialProduct?.ProductName ?? string.Empty,
                    accountProduct.FinancialProduct?.ProductType ?? ProductType.SavingsAccount,
                    accountProduct.Status,
                    accountProduct.OpenDate,
                    accountProduct.Amount,
                    approvedCreditLimit,
                    utilizationRatio,
                    accountProduct.Transactions.Count,
                    totalPayments,
                    lastPaymentDate,
                    activeBlock is not null,
                    activeBlock?.BlockType);
            })
            .ToList();

    private static IReadOnlyCollection<ClientCreditHistoryEventDto> BuildCreditEvents(
        IReadOnlyCollection<AccountProduct> accountProducts,
        IReadOnlyCollection<AccountProductLimitHistoryEntry> limitHistoryEntries)
    {
        var events = new List<ClientCreditHistoryEventDto>();
        var accountLookup = accountProducts.ToDictionary(accountProduct => accountProduct.Id);

        foreach (var accountProduct in accountProducts)
        {
            var productName = accountProduct.FinancialProduct?.ProductName ?? "Producto";
            events.Add(new ClientCreditHistoryEventDto(
                accountProduct.OpenDate,
                "producto",
                "Producto abierto",
                $"Se abrio {productName} con numero {accountProduct.AccountNumber}.",
                "info"));

            if (accountProduct.Status == AccountProductStatus.Delinquent)
            {
                events.Add(new ClientCreditHistoryEventDto(
                    accountProduct.UpdatedAt,
                    "riesgo",
                    "Producto en mora",
                    $"El producto {accountProduct.AccountNumber} figura actualmente en mora.",
                    "warning"));
            }

            foreach (var block in accountProduct.Blocks)
            {
                events.Add(new ClientCreditHistoryEventDto(
                    block.StartsAt,
                    "bloqueo",
                    GetBlockTitle(block.BlockType),
                    $"Producto {accountProduct.AccountNumber}. Motivo: {block.Reason}.",
                    GetBlockSeverity(block.BlockType)));

                if (block.ReleasedAt.HasValue)
                {
                    events.Add(new ClientCreditHistoryEventDto(
                        block.ReleasedAt.Value,
                        "bloqueo",
                        "Bloqueo liberado",
                        $"Se libero un bloqueo sobre el producto {accountProduct.AccountNumber}. Motivo: {block.ReleaseReason ?? "N/A"}.",
                        "info"));
                }
            }

            foreach (var payment in accountProduct.Transactions.Where(transaction => transaction.TransactionType == TransactionType.Payment))
            {
                events.Add(new ClientCreditHistoryEventDto(
                    payment.TransactionDate,
                    "pago",
                    "Pago registrado",
                    $"Pago de {payment.Amount:F2} aplicado al producto {accountProduct.AccountNumber}.",
                    "info"));
            }
        }

        foreach (var limitHistoryEntry in limitHistoryEntries)
        {
            if (!accountLookup.TryGetValue(limitHistoryEntry.AccountProductId, out var accountProduct))
            {
                continue;
            }

            events.Add(new ClientCreditHistoryEventDto(
                limitHistoryEntry.CreatedAt,
                "limite",
                GetLimitHistoryTitle(limitHistoryEntry.ChangeType),
                $"Producto {accountProduct.AccountNumber}. Motivo: {limitHistoryEntry.Reason}.",
                "info"));
        }

        return events;
    }

    private static AccountProductBlock? GetActiveBlock(AccountProduct accountProduct, DateTimeOffset asOf) =>
        accountProduct.Blocks
            .Where(block =>
                block.ReleasedAt is null &&
                (block.BlockType != AccountProductBlockType.Temporary ||
                 !block.EndsAt.HasValue ||
                 block.EndsAt.Value > asOf))
            .OrderByDescending(block => block.StartsAt)
            .ThenByDescending(block => block.Id)
            .FirstOrDefault();

    private static string GetRiskBand(int score) =>
        score switch
        {
            >= 780 => "excelente",
            >= 720 => "bueno",
            >= 660 => "estable",
            >= 600 => "sensible",
            _ => "alto_riesgo"
        };

    private static string GetBlockTitle(AccountProductBlockType blockType) =>
        blockType switch
        {
            AccountProductBlockType.Fraud => "Bloqueo por fraude",
            AccountProductBlockType.Permanent => "Bloqueo permanente",
            _ => "Bloqueo temporal"
        };

    private static string GetBlockSeverity(AccountProductBlockType blockType) =>
        blockType switch
        {
            AccountProductBlockType.Fraud => "critical",
            AccountProductBlockType.Permanent => "warning",
            _ => "warning"
        };

    private static string GetLimitHistoryTitle(AccountProductLimitChangeType changeType) =>
        changeType switch
        {
            AccountProductLimitChangeType.InitialConfiguration => "Configuracion inicial de limites",
            AccountProductLimitChangeType.BaseLimitUpdated => "Actualizacion de limites base",
            _ => "Ajuste temporal de limites"
        };

    private sealed record ClientCreditData(
        Client Client,
        IReadOnlyCollection<AccountProduct> AccountProducts,
        IReadOnlyCollection<AccountProductLimitHistoryEntry> LimitHistoryEntries);
}
