CREATE TABLE UserBudget (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    UserId CHAR(36) NOT NULL,
    BudgetId CHAR(36) NOT NULL,
    RoleId CHAR(36) NOT NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    INDEX idx_UserId (UserId),
    INDEX idx_BudgetId (BudgetId),
    INDEX idx_RoleId (RoleId)
); 