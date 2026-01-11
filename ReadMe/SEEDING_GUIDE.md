# Seed Data Configuration Guide

## Overview

The `SeedData` class in `Data/SeedData.cs` initializes roles and super-admin users on application startup. To improve security, seed credentials are **no longer hard-coded** in the codebase. Instead, they are read from configuration (environment variables, user-secrets, or `appsettings.json`).

## Configuration Options

Seed users can be configured via:

1. **Environment Variables** (recommended for production)
2. **User Secrets** (for local development)
3. **appsettings.json** (for reference; should remain null in repo)

Configuration keys follow the pattern:
- `Seed:{AdminType}:Email`
- `Seed:{AdminType}:UserName`
- `Seed:{AdminType}:Password`

Where `{AdminType}` is one of:
- `PrimaryAdmin`
- `SecondaryAdmin`
- `PhotoBizAdmin`

## Usage Examples

### Option 1: Environment Variables (Production)

```bash
# Set environment variables before running:
export Seed__PrimaryAdmin__Email="admin@example.com"
export Seed__PrimaryAdmin__UserName="admin"
export Seed__PrimaryAdmin__Password="SecurePassword123!@#"

export Seed__SecondaryAdmin__Email="secondary@example.com"
export Seed__SecondaryAdmin__UserName="secondary"
export Seed__SecondaryAdmin__Password="SecurePassword456!@#"

export Seed__PhotoBizAdmin__Email="photobiz@example.com"
export Seed__PhotoBizAdmin__UserName="photobizadmin"
export Seed__PhotoBizAdmin__Password="SecurePassword789!@#"

dotnet run
```

### Option 2: User Secrets (Local Development)

```bash
# Initialize user-secrets (one time)
dotnet user-secrets init

# Set secrets
dotnet user-secrets set "Seed:PrimaryAdmin:Email" "mail@coryroberts.net"
dotnet user-secrets set "Seed:PrimaryAdmin:UserName" "admin"
dotnet user-secrets set "Seed:PrimaryAdmin:Password" "YourSecurePassword123!"

dotnet user-secrets set "Seed:SecondaryAdmin:Email" "secondary@example.com"
dotnet user-secrets set "Seed:SecondaryAdmin:UserName" "secondary"
dotnet user-secrets set "Seed:SecondaryAdmin:Password" "YourSecurePassword456!@#"

dotnet user-secrets set "Seed:PhotoBizAdmin:Email" "photobiz@example.com"
dotnet user-secrets set "Seed:PhotoBizAdmin:UserName" "photobizadmin"
dotnet user-secrets set "Seed:PhotoBizAdmin:Password" "YourSecurePassword789!@#"

dotnet run
```

### Option 3: appsettings.Development.json (Local Development)

Create or edit `appsettings.Development.json`:

```json
{
  "Seed": {
    "PrimaryAdmin": {
      "Email": "admin@example.com",
      "UserName": "admin",
      "Password": "YourSecurePassword123!@#"
    },
    "SecondaryAdmin": {
      "Email": "secondary@example.com",
      "UserName": "secondary",
      "Password": "YourSecurePassword456!@#"
    },
    "PhotoBizAdmin": {
      "Email": "photobiz@example.com",
      "UserName": "photobizadmin",
      "Password": "YourSecurePassword789!@#"
    }
  }
}
```

**Note:** `appsettings.Development.json` is typically in `.gitignore` to prevent leaking local credentials.

## Auto-Generated Passwords

If **no password** is configured for a seed user, the application will:
1. Generate a secure random password using `PasswordGenerator.GenerateSecurePassword()`
2. Log a **warning** with the message: `"No password provided for seed user {Email}; generated a secure password. Rotate after first login."`

You **must** manually capture this password from the logs and rotate it after the first login.

## Logging & Troubleshooting

Seeding operations are fully logged to help diagnose issues:

- **Info**: Role/user created successfully, user already exists
- **Warning**: No password configured, role creation skipped
- **Error**: User creation failed, invalid configuration

Check application logs during startup to verify seed operations.

## Security Best Practices

✅ **DO:**
- Use **environment variables** or **user-secrets** for production
- Rotate seed credentials after first login
- Use strong, complex passwords meeting the policy (12+ chars, uppercase, lowercase, digit, non-alphanumeric)
- Monitor logs for seed operation errors
- Never commit credentials to the repository

❌ **DON'T:**
- Hard-code credentials in source files
- Leave default/test passwords in production
- Skip rotating auto-generated passwords
- Share seed credentials in unencrypted channels

## Force Resetting Seed Users

If you need to reset a seed user (e.g., after deployment), simply:

1. Delete the user from the database
2. Restart the application with the same seed configuration
3. Seeding is idempotent and will recreate the user

## Disabling Seeding

To skip seed user creation for a deployment, simply omit the corresponding configuration keys. The seeding will log an info message and continue without creating that user.

---

For questions or issues, refer to `Program.cs` and `Data/SeedData.cs`.
