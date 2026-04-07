namespace BankProductsRegistry.Api.Dtos.Reports;

/// <summary>Datos para PDF de movimientos por producto y rango de fechas (portal cliente).</summary>
public sealed record ClientTransactionStatementDto(
    int ClientId,
    string ClientName,
    string Email,
    DateOnly FromDate,
    DateOnly ToDate,
    DateTimeOffset GeneratedAtUtc,
    IReadOnlyList<StatementAccountSectionDto> Accounts);

public sealed record StatementAccountSectionDto(
    int AccountProductId,
    string AccountNumber,
    string ProductName,
    IReadOnlyList<StatementTransactionRowDto> Rows);

public sealed record StatementTransactionRowDto(
    DateTimeOffset TransactionDate,
    string TransactionTypeLabel,
    string? ReferenceNumber,
    string? Description,
    /// <summary>Positivo = depósito; negativo = salidas.</summary>
    decimal SignedAmount,
    /// <summary>Saldo del producto después de aplicar este movimiento (periodo ordenado cronológicamente).</summary>
    decimal BalanceAfter);
