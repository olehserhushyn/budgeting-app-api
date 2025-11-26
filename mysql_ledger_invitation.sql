CREATE TABLE LedgerInvitation (
    Id CHAR(36) NOT NULL PRIMARY KEY,
    LedgerId CHAR(36) NOT NULL,
    InvitedEmail VARCHAR(255) NOT NULL,
    InvitedRoleId CHAR(36) NOT NULL,
    InviterUserId CHAR(36) NOT NULL,
    Status INT NOT NULL,
    Token VARCHAR(128) NOT NULL,
    CreatedAt DATETIME NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    INDEX idx_LedgerId (LedgerId),
    INDEX idx_InvitedEmail (InvitedEmail),
    INDEX idx_Token (Token)
); 