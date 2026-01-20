-- Initial MySQL schema for FlexiRent (use migrations for schema evolution)
CREATE DATABASE IF NOT EXISTS flexirent;
USE flexirent;

-- Users
CREATE TABLE Users (
  Id CHAR(36) NOT NULL PRIMARY KEY,
  Email VARCHAR(255) NOT NULL UNIQUE,
  PasswordHash VARCHAR(500) NOT NULL,
  EmailConfirmed TINYINT(1) NOT NULL DEFAULT 0,
  CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE UserRoles (
  Id CHAR(36) NOT NULL PRIMARY KEY,
  Role VARCHAR(100) NOT NULL,
  UserId CHAR(36) NOT NULL,
  FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE TABLE RefreshTokens (
  Id CHAR(36) NOT NULL PRIMARY KEY,
  Token VARCHAR(255),
  UserId CHAR(36),
  ExpiresAt DATETIME,
  Revoked TINYINT(1) DEFAULT 0,
  FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- other tables (use EF Core migrations to create the rest or extend this file)
