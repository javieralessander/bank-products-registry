using BankProductsRegistry.Api.Data;
using BankProductsRegistry.Api.Models;
using BankProductsRegistry.Api.Models.Enums;
using BankProductsRegistry.Api.Services.Interfaces;
using BankProductsRegistry.Api.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Services;

public sealed class AccountProductBlockService(BankProductsDbContext dbContext) : IAccountProductBlockService
{
    public async Task<AccountProductBlock?> GetActiveBlockAsync(int accountProductId, CancellationToken cancellationToken = default)
    {
        var blocks = await GetActiveBlocksAsync([accountProductId], cancellationToken);
        return blocks.GetValueOrDefault(accountProductId);
    }

    public async Task<IReadOnlyDictionary<int, AccountProductBlock>> GetActiveBlocksAsync(
        IReadOnlyCollection<int> accountProductIds,
        CancellationToken cancellationToken = default)
    {
        if (accountProductIds.Count == 0)
        {
            return new Dictionary<int, AccountProductBlock>();
        }

        var candidateBlocks = await dbContext.AccountProductBlocks
            .Where(block => accountProductIds.Contains(block.AccountProductId) && block.ReleasedAt == null)
            .OrderBy(block => block.AccountProductId)
            .ThenByDescending(block => block.StartsAt)
            .ThenByDescending(block => block.Id)
            .ToListAsync(cancellationToken);

        var activeBlocks = new Dictionary<int, AccountProductBlock>();
        var expiredBlocks = new List<AccountProductBlock>();

        foreach (var block in candidateBlocks)
        {
            if (activeBlocks.ContainsKey(block.AccountProductId))
            {
                continue;
            }

            if (IsExpiredTemporaryBlock(block))
            {
                expiredBlocks.Add(block);
                continue;
            }

            activeBlocks[block.AccountProductId] = block;
        }

        if (expiredBlocks.Count > 0)
        {
            foreach (var expiredBlock in expiredBlocks)
            {
                expiredBlock.ReleasedAt = expiredBlock.EndsAt;
                expiredBlock.ReleasedByUserName = "system";
                expiredBlock.ReleaseReason = "Bloqueo temporal vencido automaticamente.";

                dbContext.AccountProductAuditEntries.Add(new AccountProductAuditEntry
                {
                    AccountProductId = expiredBlock.AccountProductId,
                    AccountProductBlockId = expiredBlock.Id,
                    Action = AccountProductAuditAction.BlockExpired,
                    PerformedByUserId = null,
                    PerformedByUserName = "system",
                    Detail = "El bloqueo temporal expiro automaticamente al finalizar su vigencia."
                });
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return activeBlocks;
    }

    public async Task RecordAuditAsync(
        int accountProductId,
        AccountProductAuditAction action,
        int? performedByUserId,
        string? performedByUserName,
        string detail,
        int? accountProductBlockId,
        CancellationToken cancellationToken = default)
    {
        dbContext.AccountProductAuditEntries.Add(new AccountProductAuditEntry
        {
            AccountProductId = accountProductId,
            AccountProductBlockId = accountProductBlockId,
            Action = action,
            PerformedByUserId = performedByUserId,
            PerformedByUserName = string.IsNullOrWhiteSpace(performedByUserName) ? "system" : performedByUserName.Trim(),
            Detail = NormalizationHelper.NormalizeOptionalText(detail) ?? string.Empty
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool IsExpiredTemporaryBlock(AccountProductBlock block) =>
        block.BlockType == AccountProductBlockType.Temporary &&
        block.EndsAt.HasValue &&
        block.EndsAt.Value <= DateTimeOffset.UtcNow;
}
