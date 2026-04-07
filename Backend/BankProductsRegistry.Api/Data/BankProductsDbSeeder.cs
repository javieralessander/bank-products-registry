using BankProductsRegistry.Api.Models;
using BankProductsRegistry.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BankProductsRegistry.Api.Data;

public static class BankProductsDbSeeder
{
    public static async Task SeedAsync(BankProductsDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await SeedFinancialProductsAsync(dbContext, cancellationToken);
        await SeedClientsAsync(dbContext, cancellationToken);
        await SeedEmployeesAsync(dbContext, cancellationToken);
        await SeedAccountProductsAsync(dbContext, cancellationToken);
        await SeedAccountProductLimitsAsync(dbContext, cancellationToken);
        await SeedTransactionsAsync(dbContext, cancellationToken);
    }

    private static async Task SeedFinancialProductsAsync(BankProductsDbContext dbContext, CancellationToken cancellationToken)
    {
        var existingProductNames = await dbContext.FinancialProducts
            .AsNoTracking()
            .Select(product => product.ProductName)
            .ToListAsync(cancellationToken);

        var knownProducts = existingProductNames.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var productsToAdd = new[]
        {
            new FinancialProduct
            {
                ProductName = "Tarjeta Clasica",
                ProductType = ProductType.CreditCard,
                InterestRate = 39.50m,
                Description = "Tarjeta de credito para consumo general con limite inicial basico.",
                Status = ProductStatus.Active,
                Currency = "DOP",
                MinimumOpeningAmount = 5000m
            },
            new FinancialProduct
            {
                ProductName = "Prestamo Personal Flex",
                ProductType = ProductType.Loan,
                InterestRate = 18.75m,
                Description = "Prestamo personal para gastos de consumo y consolidacion.",
                Status = ProductStatus.Active,
                Currency = "DOP",
                MinimumOpeningAmount = 25000m
            },
            new FinancialProduct
            {
                ProductName = "Inversion Crece 12M",
                ProductType = ProductType.Investment,
                InterestRate = 7.25m,
                Description = "Producto de inversion a doce meses con rendimiento fijo.",
                Status = ProductStatus.Active,
                Currency = "DOP",
                MinimumOpeningAmount = 10000m
            },
            new FinancialProduct
            {
                ProductName = "Certificado Premium 24M",
                ProductType = ProductType.Certificate,
                InterestRate = 8.10m,
                Description = "Certificado financiero a veinticuatro meses para clientes recurrentes.",
                Status = ProductStatus.Active,
                Currency = "DOP",
                MinimumOpeningAmount = 50000m
            },
            new FinancialProduct
            {
                ProductName = "Cuenta Ahorro Plus",
                ProductType = ProductType.SavingsAccount,
                InterestRate = 2.50m,
                Description = "Cuenta de ahorro para clientes personales con apertura simplificada.",
                Status = ProductStatus.Active,
                Currency = "DOP",
                MinimumOpeningAmount = 1000m
            }
        };

        var missingProducts = productsToAdd
            .Where(product => !knownProducts.Contains(product.ProductName))
            .ToList();

        if (missingProducts.Count == 0)
        {
            return;
        }

        dbContext.FinancialProducts.AddRange(missingProducts);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedClientsAsync(BankProductsDbContext dbContext, CancellationToken cancellationToken)
    {
        var existingNationalIds = await dbContext.Clients
            .AsNoTracking()
            .Select(client => client.NationalId)
            .ToListAsync(cancellationToken);

        var knownNationalIds = existingNationalIds.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var clientsToAdd = new[]
        {
            new Client
            {
                FirstName = "Ana",
                LastName = "Perez",
                NationalId = "40200010001",
                Email = "ana.perez@demo.local",
                Phone = "8095550101",
                IsActive = true
            },
            new Client
            {
                FirstName = "Luis",
                LastName = "Gomez",
                NationalId = "40200010002",
                Email = "luis.gomez@demo.local",
                Phone = "8095550102",
                IsActive = true
            },
            new Client
            {
                FirstName = "Carla",
                LastName = "Rodriguez",
                NationalId = "40200010003",
                Email = "carla.rodriguez@demo.local",
                Phone = "8095550103",
                IsActive = true
            }
        };

        var missingClients = clientsToAdd
            .Where(client => !knownNationalIds.Contains(client.NationalId))
            .ToList();

        if (missingClients.Count == 0)
        {
            return;
        }

        dbContext.Clients.AddRange(missingClients);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedEmployeesAsync(BankProductsDbContext dbContext, CancellationToken cancellationToken)
    {
        var existingCodes = await dbContext.Employees
            .AsNoTracking()
            .Select(employee => employee.EmployeeCode)
            .ToListAsync(cancellationToken);

        var knownCodes = existingCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var employeesToAdd = new[]
        {
            new Employee
            {
                FirstName = "Sistema",
                LastName = "Solicitudes Web",
                EmployeeCode = "EMP000",
                Email = "solicitudes@bank.local",
                Department = "Sistema",
                IsActive = true
            },
            new Employee
            {
                FirstName = "Rosa",
                LastName = "Martinez",
                EmployeeCode = "EMP001",
                Email = "rosa.martinez@demo.local",
                Department = "Operaciones",
                IsActive = true
            },
            new Employee
            {
                FirstName = "Daniel",
                LastName = "Herrera",
                EmployeeCode = "EMP002",
                Email = "daniel.herrera@demo.local",
                Department = "Negocios",
                IsActive = true
            }
        };

        var missingEmployees = employeesToAdd
            .Where(employee => !knownCodes.Contains(employee.EmployeeCode))
            .ToList();

        if (missingEmployees.Count == 0)
        {
            return;
        }

        dbContext.Employees.AddRange(missingEmployees);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedAccountProductsAsync(BankProductsDbContext dbContext, CancellationToken cancellationToken)
    {
        var clients = await dbContext.Clients
            .AsNoTracking()
            .ToDictionaryAsync(client => client.NationalId, cancellationToken);

        var employees = await dbContext.Employees
            .AsNoTracking()
            .ToDictionaryAsync(employee => employee.EmployeeCode, cancellationToken);

        var products = await dbContext.FinancialProducts
            .AsNoTracking()
            .ToDictionaryAsync(product => product.ProductName, cancellationToken);

        var existingAccountNumbers = await dbContext.AccountProducts
            .AsNoTracking()
            .Select(account => account.AccountNumber)
            .ToListAsync(cancellationToken);

        var knownAccountNumbers = existingAccountNumbers.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var accountSeeds = new[]
        {
            new AccountProductSeed("0001002001", "40200010001", "Cuenta Ahorro Plus", "EMP001", 35000m, UtcDate(2026, 1, 15), null, AccountProductStatus.Active),
            new AccountProductSeed("0001002002", "40200010002", "Prestamo Personal Flex", "EMP002", 180000m, UtcDate(2026, 2, 1), UtcDate(2029, 2, 1), AccountProductStatus.Active),
            new AccountProductSeed("0001002003", "40200010003", "Inversion Crece 12M", "EMP001", 90000m, UtcDate(2026, 1, 20), UtcDate(2027, 1, 20), AccountProductStatus.Active),
            new AccountProductSeed("0001002004", "40200010001", "Tarjeta Clasica", "EMP002", 15000m, UtcDate(2026, 2, 10), null, AccountProductStatus.Active)
        };

        var accountsToAdd = new List<AccountProduct>();

        foreach (var accountSeed in accountSeeds)
        {
            if (knownAccountNumbers.Contains(accountSeed.AccountNumber))
            {
                continue;
            }

            if (!clients.TryGetValue(accountSeed.ClientNationalId, out var client) ||
                !employees.TryGetValue(accountSeed.EmployeeCode, out var employee) ||
                !products.TryGetValue(accountSeed.ProductName, out var product))
            {
                continue;
            }

            accountsToAdd.Add(new AccountProduct
            {
                ClientId = client.Id,
                FinancialProductId = product.Id,
                EmployeeId = employee.Id,
                AccountNumber = accountSeed.AccountNumber,
                Amount = accountSeed.Amount,
                OpenDate = accountSeed.OpenDate,
                MaturityDate = accountSeed.MaturityDate,
                Status = accountSeed.Status
            });
        }

        if (accountsToAdd.Count == 0)
        {
            return;
        }

        dbContext.AccountProducts.AddRange(accountsToAdd);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedTransactionsAsync(BankProductsDbContext dbContext, CancellationToken cancellationToken)
    {
        var accounts = await dbContext.AccountProducts
            .AsNoTracking()
            .ToDictionaryAsync(account => account.AccountNumber, cancellationToken);

        var existingReferences = await dbContext.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.ReferenceNumber != null)
            .Select(transaction => transaction.ReferenceNumber!)
            .ToListAsync(cancellationToken);

        var knownReferences = existingReferences.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var transactionSeeds = new[]
        {
            new TransactionSeed("DEP-0001", "0001002001", TransactionType.Deposit, TransactionChannel.Branch, 20000m, UtcDate(2026, 1, 15), "Deposito inicial de apertura", "DO"),
            new TransactionSeed("DEP-0002", "0001002001", TransactionType.Deposit, TransactionChannel.Branch, 15000m, UtcDate(2026, 2, 3), "Ahorro quincenal", "DO"),
            new TransactionSeed("PRE-0001", "0001002002", TransactionType.Deposit, TransactionChannel.BackOffice, 200000m, UtcDate(2026, 2, 1), "Desembolso inicial del prestamo", "DO"),
            new TransactionSeed("PAG-0001", "0001002002", TransactionType.Payment, TransactionChannel.Branch, 20000m, UtcDate(2026, 2, 28), "Pago de la primera cuota", "DO"),
            new TransactionSeed("INV-0001", "0001002003", TransactionType.Deposit, TransactionChannel.Branch, 90000m, UtcDate(2026, 1, 20), "Aporte inicial de inversion", "DO")
        };

        var transactionsToAdd = new List<BankTransaction>();

        foreach (var transactionSeed in transactionSeeds)
        {
            if (knownReferences.Contains(transactionSeed.ReferenceNumber) ||
                !accounts.TryGetValue(transactionSeed.AccountNumber, out var account))
            {
                continue;
            }

            transactionsToAdd.Add(new BankTransaction
            {
                AccountProductId = account.Id,
                TransactionType = transactionSeed.TransactionType,
                TransactionChannel = transactionSeed.TransactionChannel,
                Amount = transactionSeed.Amount,
                TransactionDate = transactionSeed.TransactionDate,
                Description = transactionSeed.Description,
                ReferenceNumber = transactionSeed.ReferenceNumber,
                CountryCode = transactionSeed.CountryCode
            });
        }

        if (transactionsToAdd.Count == 0)
        {
            return;
        }

        dbContext.Transactions.AddRange(transactionsToAdd);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedAccountProductLimitsAsync(BankProductsDbContext dbContext, CancellationToken cancellationToken)
    {
        var accounts = await dbContext.AccountProducts
            .AsNoTracking()
            .ToDictionaryAsync(account => account.AccountNumber, cancellationToken);

        var existingLimitAccountIds = await dbContext.AccountProductLimits
            .AsNoTracking()
            .Select(limit => limit.AccountProductId)
            .ToListAsync(cancellationToken);

        var knownAccountIds = existingLimitAccountIds.ToHashSet();
        var limitsToAdd = new List<AccountProductLimit>();

        if (accounts.TryGetValue("0001002004", out var creditCardAccount) && !knownAccountIds.Contains(creditCardAccount.Id))
        {
            limitsToAdd.Add(new AccountProductLimit
            {
                AccountProductId = creditCardAccount.Id,
                CreditLimitTotal = 20000m,
                DailyConsumptionLimit = 15000m,
                PerTransactionLimit = 12000m,
                AtmWithdrawalLimit = 6000m,
                InternationalConsumptionLimit = 8000m
            });
        }

        if (limitsToAdd.Count == 0)
        {
            return;
        }

        dbContext.AccountProductLimits.AddRange(limitsToAdd);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static DateTimeOffset UtcDate(int year, int month, int day) =>
        new(year, month, day, 0, 0, 0, TimeSpan.Zero);

    private sealed record AccountProductSeed(
        string AccountNumber,
        string ClientNationalId,
        string ProductName,
        string EmployeeCode,
        decimal Amount,
        DateTimeOffset OpenDate,
        DateTimeOffset? MaturityDate,
        AccountProductStatus Status);

    private sealed record TransactionSeed(
        string ReferenceNumber,
        string AccountNumber,
        TransactionType TransactionType,
        TransactionChannel TransactionChannel,
        decimal Amount,
        DateTimeOffset TransactionDate,
        string Description,
        string CountryCode);
}
