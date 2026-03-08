using BankProductsRegistry.Api.Data;
using BankProductsRegistry.Api.Dtos.Reports;
using BankProductsRegistry.Api.Models.Enums;
using BankProductsRegistry.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Services;

public sealed class ReportService(BankProductsDbContext dbContext) : IReportService
{
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
}
