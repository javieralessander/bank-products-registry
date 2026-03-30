using BankProductsRegistry.Api.Models;
using BankProductsRegistry.Api.Models.Enums;

namespace BankProductsRegistry.Api.Services.Interfaces;

public interface IAccountProductBlockService
{
    Task<AccountProductBlock?> GetActiveBlockAsync(int accountProductId, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<int, AccountProductBlock>> GetActiveBlocksAsync(
        IReadOnlyCollection<int> accountProductIds,
        CancellationToken cancellationToken = default);
    Task RecordAuditAsync(
        int accountProductId,
        AccountProductAuditAction action,
        int? performedByUserId,
        string? performedByUserName,
        string detail,
        int? accountProductBlockId,
        CancellationToken cancellationToken = default);
}
