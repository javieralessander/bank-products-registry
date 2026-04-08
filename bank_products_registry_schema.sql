-- =============================================================================
-- Esquema MySQL (solo estructura + tabla __EFMigrationsHistory). Sin datos.
-- Generado con: dotnet ef migrations script --idempotent
--   (desde Backend/BankProductsRegistry.Api, salida -o ../../bank_products_registry_schema.sql)
-- Motor: MySQL 8, charset utf8mb4. Para recrear: ejecutar sobre una base vacia.
-- =============================================================================

CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308205923_InitialCreateMySql') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308205923_InitialCreateMySql') THEN

    CREATE TABLE `Clients` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `FirstName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `LastName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `NationalId` varchar(25) CHARACTER SET utf8mb4 NOT NULL,
        `Email` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
        `Phone` varchar(25) CHARACTER SET utf8mb4 NOT NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_Clients` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308205923_InitialCreateMySql') THEN

    CREATE TABLE `Employees` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `FirstName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `LastName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `EmployeeCode` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        `Email` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
        `Department` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `IsActive` tinyint(1) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_Employees` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308205923_InitialCreateMySql') THEN

    CREATE TABLE `FinancialProducts` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ProductName` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
        `ProductType` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        `InterestRate` decimal(9,4) NOT NULL,
        `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Currency` varchar(3) CHARACTER SET utf8mb4 NOT NULL,
        `MinimumOpeningAmount` decimal(18,2) NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_FinancialProducts` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308205923_InitialCreateMySql') THEN

    CREATE TABLE `AccountProducts` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ClientId` int NOT NULL,
        `FinancialProductId` int NOT NULL,
        `EmployeeId` int NOT NULL,
        `AccountNumber` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        `Amount` decimal(18,2) NOT NULL,
        `OpenDate` datetime(6) NOT NULL,
        `MaturityDate` datetime(6) NULL,
        `Status` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_AccountProducts` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AccountProducts_Clients_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `Clients` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_AccountProducts_Employees_EmployeeId` FOREIGN KEY (`EmployeeId`) REFERENCES `Employees` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_AccountProducts_FinancialProducts_FinancialProductId` FOREIGN KEY (`FinancialProductId`) REFERENCES `FinancialProducts` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308205923_InitialCreateMySql') THEN

    CREATE TABLE `Transactions` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `AccountProductId` int NOT NULL,
        `TransactionType` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Amount` decimal(18,2) NOT NULL,
        `TransactionDate` datetime(6) NOT NULL,
        `Description` varchar(300) CHARACTER SET utf8mb4 NULL,
        `ReferenceNumber` varchar(60) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_Transactions` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Transactions_AccountProducts_AccountProductId` FOREIGN KEY (`AccountProductId`) REFERENCES `AccountProducts` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308205923_InitialCreateMySql') THEN

    CREATE UNIQUE INDEX `IX_AccountProducts_AccountNumber` ON `AccountProducts` (`AccountNumber`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308205923_InitialCreateMySql') THEN

    CREATE INDEX `IX_AccountProducts_ClientId` ON `AccountProducts` (`ClientId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308205923_InitialCreateMySql') THEN

    CREATE INDEX `IX_AccountProducts_EmployeeId` ON `AccountProducts` (`EmployeeId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308205923_InitialCreateMySql') THEN

    CREATE INDEX `IX_AccountProducts_FinancialProductId` ON `AccountProducts` (`FinancialProductId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308205923_InitialCreateMySql') THEN

    CREATE UNIQUE INDEX `IX_Clients_Email` ON `Clients` (`Email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308205923_InitialCreateMySql') THEN

    CREATE UNIQUE INDEX `IX_Clients_NationalId` ON `Clients` (`NationalId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308205923_InitialCreateMySql') THEN

    CREATE UNIQUE INDEX `IX_Employees_Email` ON `Employees` (`Email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308205923_InitialCreateMySql') THEN

    CREATE UNIQUE INDEX `IX_Employees_EmployeeCode` ON `Employees` (`EmployeeCode`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308205923_InitialCreateMySql') THEN

    CREATE INDEX `IX_FinancialProducts_ProductName_ProductType` ON `FinancialProducts` (`ProductName`, `ProductType`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308205923_InitialCreateMySql') THEN

    CREATE INDEX `IX_Transactions_AccountProductId` ON `Transactions` (`AccountProductId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308205923_InitialCreateMySql') THEN

    CREATE INDEX `IX_Transactions_ReferenceNumber` ON `Transactions` (`ReferenceNumber`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260308205923_InitialCreateMySql') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260308205923_InitialCreateMySql', '9.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327213502_AddAuthenticationAndAuthorization') THEN

    CREATE TABLE `AspNetRoles` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Name` varchar(256) CHARACTER SET utf8mb4 NULL,
        `NormalizedName` varchar(256) CHARACTER SET utf8mb4 NULL,
        `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_AspNetRoles` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327213502_AddAuthenticationAndAuthorization') THEN

    CREATE TABLE `AspNetUsers` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `FullName` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
        `IsActive` tinyint(1) NOT NULL DEFAULT TRUE,
        `UserName` varchar(256) CHARACTER SET utf8mb4 NULL,
        `NormalizedUserName` varchar(256) CHARACTER SET utf8mb4 NULL,
        `Email` varchar(256) CHARACTER SET utf8mb4 NULL,
        `NormalizedEmail` varchar(256) CHARACTER SET utf8mb4 NULL,
        `EmailConfirmed` tinyint(1) NOT NULL,
        `PasswordHash` longtext CHARACTER SET utf8mb4 NULL,
        `SecurityStamp` longtext CHARACTER SET utf8mb4 NULL,
        `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 NULL,
        `PhoneNumber` longtext CHARACTER SET utf8mb4 NULL,
        `PhoneNumberConfirmed` tinyint(1) NOT NULL,
        `TwoFactorEnabled` tinyint(1) NOT NULL,
        `LockoutEnd` datetime(6) NULL,
        `LockoutEnabled` tinyint(1) NOT NULL,
        `AccessFailedCount` int NOT NULL,
        CONSTRAINT `PK_AspNetUsers` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327213502_AddAuthenticationAndAuthorization') THEN

    CREATE TABLE `AspNetRoleClaims` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `RoleId` int NOT NULL,
        `ClaimType` longtext CHARACTER SET utf8mb4 NULL,
        `ClaimValue` longtext CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_AspNetRoleClaims` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AspNetRoleClaims_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327213502_AddAuthenticationAndAuthorization') THEN

    CREATE TABLE `AspNetUserClaims` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `UserId` int NOT NULL,
        `ClaimType` longtext CHARACTER SET utf8mb4 NULL,
        `ClaimValue` longtext CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_AspNetUserClaims` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AspNetUserClaims_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327213502_AddAuthenticationAndAuthorization') THEN

    CREATE TABLE `AspNetUserLogins` (
        `LoginProvider` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ProviderKey` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `ProviderDisplayName` longtext CHARACTER SET utf8mb4 NULL,
        `UserId` int NOT NULL,
        CONSTRAINT `PK_AspNetUserLogins` PRIMARY KEY (`LoginProvider`, `ProviderKey`),
        CONSTRAINT `FK_AspNetUserLogins_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327213502_AddAuthenticationAndAuthorization') THEN

    CREATE TABLE `AspNetUserRoles` (
        `UserId` int NOT NULL,
        `RoleId` int NOT NULL,
        CONSTRAINT `PK_AspNetUserRoles` PRIMARY KEY (`UserId`, `RoleId`),
        CONSTRAINT `FK_AspNetUserRoles_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_AspNetUserRoles_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327213502_AddAuthenticationAndAuthorization') THEN

    CREATE TABLE `AspNetUserTokens` (
        `UserId` int NOT NULL,
        `LoginProvider` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `Name` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `Value` longtext CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_AspNetUserTokens` PRIMARY KEY (`UserId`, `LoginProvider`, `Name`),
        CONSTRAINT `FK_AspNetUserTokens_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327213502_AddAuthenticationAndAuthorization') THEN

    CREATE TABLE `RefreshTokens` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ApplicationUserId` int NOT NULL,
        `TokenHash` varchar(128) CHARACTER SET utf8mb4 NOT NULL,
        `ExpiresAt` datetime(6) NOT NULL,
        `RevokedAt` datetime(6) NULL,
        `ReplacedByTokenHash` varchar(128) CHARACTER SET utf8mb4 NULL,
        `CreatedByIp` varchar(64) CHARACTER SET utf8mb4 NULL,
        `RevokedByIp` varchar(64) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_RefreshTokens` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_RefreshTokens_AspNetUsers_ApplicationUserId` FOREIGN KEY (`ApplicationUserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327213502_AddAuthenticationAndAuthorization') THEN

    CREATE INDEX `IX_AspNetRoleClaims_RoleId` ON `AspNetRoleClaims` (`RoleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327213502_AddAuthenticationAndAuthorization') THEN

    CREATE UNIQUE INDEX `RoleNameIndex` ON `AspNetRoles` (`NormalizedName`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327213502_AddAuthenticationAndAuthorization') THEN

    CREATE INDEX `IX_AspNetUserClaims_UserId` ON `AspNetUserClaims` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327213502_AddAuthenticationAndAuthorization') THEN

    CREATE INDEX `IX_AspNetUserLogins_UserId` ON `AspNetUserLogins` (`UserId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327213502_AddAuthenticationAndAuthorization') THEN

    CREATE INDEX `IX_AspNetUserRoles_RoleId` ON `AspNetUserRoles` (`RoleId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327213502_AddAuthenticationAndAuthorization') THEN

    CREATE INDEX `EmailIndex` ON `AspNetUsers` (`NormalizedEmail`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327213502_AddAuthenticationAndAuthorization') THEN

    CREATE UNIQUE INDEX `UserNameIndex` ON `AspNetUsers` (`NormalizedUserName`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327213502_AddAuthenticationAndAuthorization') THEN

    CREATE INDEX `IX_RefreshTokens_ApplicationUserId_ExpiresAt` ON `RefreshTokens` (`ApplicationUserId`, `ExpiresAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327213502_AddAuthenticationAndAuthorization') THEN

    CREATE UNIQUE INDEX `IX_RefreshTokens_TokenHash` ON `RefreshTokens` (`TokenHash`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327213502_AddAuthenticationAndAuthorization') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260327213502_AddAuthenticationAndAuthorization', '9.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327224733_AddAccountProductBlocksAndAudit') THEN

    CREATE TABLE `AccountProductBlocks` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `AccountProductId` int NOT NULL,
        `BlockType` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Reason` varchar(300) CHARACTER SET utf8mb4 NOT NULL,
        `StartsAt` datetime(6) NOT NULL,
        `EndsAt` datetime(6) NULL,
        `AppliedByUserId` int NULL,
        `AppliedByUserName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `ReleasedAt` datetime(6) NULL,
        `ReleasedByUserId` int NULL,
        `ReleasedByUserName` varchar(100) CHARACTER SET utf8mb4 NULL,
        `ReleaseReason` varchar(300) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_AccountProductBlocks` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AccountProductBlocks_AccountProducts_AccountProductId` FOREIGN KEY (`AccountProductId`) REFERENCES `AccountProducts` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327224733_AddAccountProductBlocksAndAudit') THEN

    CREATE TABLE `AccountProductAuditEntries` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `AccountProductId` int NOT NULL,
        `AccountProductBlockId` int NULL,
        `Action` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        `PerformedByUserId` int NULL,
        `PerformedByUserName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Detail` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_AccountProductAuditEntries` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AccountProductAuditEntries_AccountProductBlocks_AccountProdu~` FOREIGN KEY (`AccountProductBlockId`) REFERENCES `AccountProductBlocks` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_AccountProductAuditEntries_AccountProducts_AccountProductId` FOREIGN KEY (`AccountProductId`) REFERENCES `AccountProducts` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327224733_AddAccountProductBlocksAndAudit') THEN

    CREATE INDEX `IX_AccountProductAuditEntries_AccountProductBlockId` ON `AccountProductAuditEntries` (`AccountProductBlockId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327224733_AddAccountProductBlocksAndAudit') THEN

    CREATE INDEX `IX_AccountProductAuditEntries_AccountProductId_CreatedAt` ON `AccountProductAuditEntries` (`AccountProductId`, `CreatedAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327224733_AddAccountProductBlocksAndAudit') THEN

    CREATE INDEX `IX_AccountProductBlocks_AccountProductId_ReleasedAt_StartsAt` ON `AccountProductBlocks` (`AccountProductId`, `ReleasedAt`, `StartsAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327224733_AddAccountProductBlocksAndAudit') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260327224733_AddAccountProductBlocksAndAudit', '9.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327225652_AddAccountProductLimitsAndTransactionContext') THEN

    ALTER TABLE `Transactions` ADD `CountryCode` varchar(2) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327225652_AddAccountProductLimitsAndTransactionContext') THEN

    ALTER TABLE `Transactions` ADD `TransactionChannel` varchar(20) CHARACTER SET utf8mb4 NOT NULL DEFAULT '';

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327225652_AddAccountProductLimitsAndTransactionContext') THEN

    CREATE TABLE `AccountProductLimits` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `AccountProductId` int NOT NULL,
        `CreditLimitTotal` decimal(18,2) NULL,
        `DailyConsumptionLimit` decimal(18,2) NULL,
        `PerTransactionLimit` decimal(18,2) NULL,
        `AtmWithdrawalLimit` decimal(18,2) NULL,
        `InternationalConsumptionLimit` decimal(18,2) NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_AccountProductLimits` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AccountProductLimits_AccountProducts_AccountProductId` FOREIGN KEY (`AccountProductId`) REFERENCES `AccountProducts` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327225652_AddAccountProductLimitsAndTransactionContext') THEN

    CREATE TABLE `AccountProductLimitTemporaryAdjustments` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `AccountProductId` int NOT NULL,
        `CreditLimitTotal` decimal(18,2) NULL,
        `DailyConsumptionLimit` decimal(18,2) NULL,
        `PerTransactionLimit` decimal(18,2) NULL,
        `AtmWithdrawalLimit` decimal(18,2) NULL,
        `InternationalConsumptionLimit` decimal(18,2) NULL,
        `StartsAt` datetime(6) NOT NULL,
        `EndsAt` datetime(6) NOT NULL,
        `Reason` varchar(300) CHARACTER SET utf8mb4 NOT NULL,
        `ApprovedByUserId` int NULL,
        `ApprovedByUserName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_AccountProductLimitTemporaryAdjustments` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AccountProductLimitTemporaryAdjustments_AccountProducts_Acco~` FOREIGN KEY (`AccountProductId`) REFERENCES `AccountProducts` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327225652_AddAccountProductLimitsAndTransactionContext') THEN

    CREATE TABLE `AccountProductLimitHistoryEntries` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `AccountProductId` int NOT NULL,
        `TemporaryAdjustmentId` int NULL,
        `ChangeType` varchar(40) CHARACTER SET utf8mb4 NOT NULL,
        `PreviousCreditLimitTotal` decimal(18,2) NULL,
        `NewCreditLimitTotal` decimal(18,2) NULL,
        `PreviousDailyConsumptionLimit` decimal(18,2) NULL,
        `NewDailyConsumptionLimit` decimal(18,2) NULL,
        `PreviousPerTransactionLimit` decimal(18,2) NULL,
        `NewPerTransactionLimit` decimal(18,2) NULL,
        `PreviousAtmWithdrawalLimit` decimal(18,2) NULL,
        `NewAtmWithdrawalLimit` decimal(18,2) NULL,
        `PreviousInternationalConsumptionLimit` decimal(18,2) NULL,
        `NewInternationalConsumptionLimit` decimal(18,2) NULL,
        `EffectiveFrom` datetime(6) NOT NULL,
        `EffectiveTo` datetime(6) NULL,
        `Reason` varchar(300) CHARACTER SET utf8mb4 NOT NULL,
        `PerformedByUserId` int NULL,
        `PerformedByUserName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_AccountProductLimitHistoryEntries` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AccountProductLimitHistoryEntries_AccountProductLimitTempora~` FOREIGN KEY (`TemporaryAdjustmentId`) REFERENCES `AccountProductLimitTemporaryAdjustments` (`Id`) ON DELETE SET NULL,
        CONSTRAINT `FK_AccountProductLimitHistoryEntries_AccountProducts_AccountPro~` FOREIGN KEY (`AccountProductId`) REFERENCES `AccountProducts` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327225652_AddAccountProductLimitsAndTransactionContext') THEN

    CREATE INDEX `IX_AccountProductLimitHistoryEntries_AccountProductId_CreatedAt` ON `AccountProductLimitHistoryEntries` (`AccountProductId`, `CreatedAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327225652_AddAccountProductLimitsAndTransactionContext') THEN

    CREATE INDEX `IX_AccountProductLimitHistoryEntries_TemporaryAdjustmentId` ON `AccountProductLimitHistoryEntries` (`TemporaryAdjustmentId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327225652_AddAccountProductLimitsAndTransactionContext') THEN

    CREATE UNIQUE INDEX `IX_AccountProductLimits_AccountProductId` ON `AccountProductLimits` (`AccountProductId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327225652_AddAccountProductLimitsAndTransactionContext') THEN

    CREATE INDEX `IX_AccountProductLimitTemporaryAdjustments_AccountProductId_Sta~` ON `AccountProductLimitTemporaryAdjustments` (`AccountProductId`, `StartsAt`, `EndsAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327225652_AddAccountProductLimitsAndTransactionContext') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260327225652_AddAccountProductLimitsAndTransactionContext', '9.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327230040_AddTravelNoticesForInternationalValidation') THEN

    CREATE TABLE `AccountProductTravelNotices` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `AccountProductId` int NOT NULL,
        `StartsAt` datetime(6) NOT NULL,
        `EndsAt` datetime(6) NOT NULL,
        `Reason` varchar(300) CHARACTER SET utf8mb4 NOT NULL,
        `RequestedByUserId` int NULL,
        `RequestedByUserName` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `CancelledAt` datetime(6) NULL,
        `CancelledByUserId` int NULL,
        `CancelledByUserName` varchar(100) CHARACTER SET utf8mb4 NULL,
        `CancellationReason` varchar(300) CHARACTER SET utf8mb4 NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_AccountProductTravelNotices` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AccountProductTravelNotices_AccountProducts_AccountProductId` FOREIGN KEY (`AccountProductId`) REFERENCES `AccountProducts` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327230040_AddTravelNoticesForInternationalValidation') THEN

    CREATE TABLE `AccountProductTravelNoticeCountries` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `TravelNoticeId` int NOT NULL,
        `CountryCode` varchar(2) CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NOT NULL,
        CONSTRAINT `PK_AccountProductTravelNoticeCountries` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_AccountProductTravelNoticeCountries_AccountProductTravelNoti~` FOREIGN KEY (`TravelNoticeId`) REFERENCES `AccountProductTravelNotices` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327230040_AddTravelNoticesForInternationalValidation') THEN

    CREATE UNIQUE INDEX `IX_AccountProductTravelNoticeCountries_TravelNoticeId_CountryCo~` ON `AccountProductTravelNoticeCountries` (`TravelNoticeId`, `CountryCode`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327230040_AddTravelNoticesForInternationalValidation') THEN

    CREATE INDEX `IX_AccountProductTravelNotices_AccountProductId_StartsAt_EndsAt` ON `AccountProductTravelNotices` (`AccountProductId`, `StartsAt`, `EndsAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260327230040_AddTravelNoticesForInternationalValidation') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260327230040_AddTravelNoticesForInternationalValidation', '9.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260406194740_AddSystemNotificationsTable') THEN

    CREATE TABLE `SystemNotifications` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Title` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Message` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Type` longtext CHARACTER SET utf8mb4 NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `IsRead` tinyint(1) NOT NULL,
        CONSTRAINT `PK_SystemNotifications` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260406194740_AddSystemNotificationsTable') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260406194740_AddSystemNotificationsTable', '9.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260407143000_AddClientUserLink') THEN

    ALTER TABLE `AspNetUsers` ADD `ClientId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260407143000_AddClientUserLink') THEN

    CREATE UNIQUE INDEX `IX_AspNetUsers_ClientId` ON `AspNetUsers` (`ClientId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260407143000_AddClientUserLink') THEN

    ALTER TABLE `AspNetUsers` ADD CONSTRAINT `FK_AspNetUsers_Clients_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `Clients` (`Id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260407143000_AddClientUserLink') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260407143000_AddClientUserLink', '9.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260407180000_AddApplicationUserProfileColumns') THEN

    ALTER TABLE `AspNetUsers` ADD `FirstName` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260407180000_AddApplicationUserProfileColumns') THEN

    ALTER TABLE `AspNetUsers` ADD `LastName` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260407180000_AddApplicationUserProfileColumns') THEN

    ALTER TABLE `AspNetUsers` ADD `NationalId` varchar(25) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260407180000_AddApplicationUserProfileColumns') THEN

    ALTER TABLE `AspNetUsers` ADD `Phone` varchar(25) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260407180000_AddApplicationUserProfileColumns') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260407180000_AddApplicationUserProfileColumns', '9.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

