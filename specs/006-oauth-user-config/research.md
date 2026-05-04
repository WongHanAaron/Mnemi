# Research: OAuth User Configuration and Document Source Management

**Date**: 2026-05-02  
**Feature**: [spec.md](spec.md) | [plan.md](plan.md)

## Research Questions Answered

### 1. OAuth 2.0 Implementation in ASP.NET Core

**Decision**: Use ASP.NET Core's built-in OAuth handlers with PKCE support

**Rationale**:
- Microsoft provides official, maintained packages for Google and GitHub OAuth
- Built-in PKCE (Proof Key for Code Exchange) support for enhanced security
- Seamless integration with ASP.NET Core authentication middleware
- Supports both server-side and Blazor WebAssembly authentication patterns

**Implementation approach**:
```csharp
// In Program.cs
builder.Services.AddAuthentication()
    .AddGoogle(options => {
        options.ClientId = configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
        options.Scope.Add("https://www.googleapis.com/auth/drive.readonly");
    })
    .AddGitHub(options => {
        options.ClientId = configuration["Authentication:GitHub:ClientId"]!;
        options.ClientSecret = configuration["Authentication:GitHub:ClientSecret"]!;
        options.Scope.Add("repo");
    });
```

**Alternatives considered**:
- Manual OAuth implementation: Rejected - too error-prone, security risks
- IdentityServer4/Duende: Rejected - overkill for simple OAuth consumer scenario
- Third-party libraries (Okta, Auth0): Rejected - adds external dependency and cost

### 2. Token Encryption Strategy

**Decision**: Use ASP.NET Core Data Protection APIs with AES-256

**Rationale**:
- Data Protection is the standard approach in ASP.NET Core for encrypting data at rest
- Automatic key management and rotation
- Industry-standard AES-256 encryption
- Simple API: `IDataProtector.Protect()` / `Unprotect()`

**Implementation**:
```csharp
public class TokenEncryptionService
{
    private readonly IDataProtector _protector;
    
    public TokenEncryptionService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("Mnemi.OAuth.Tokens");
    }
    
    public string Encrypt(string plainText) => _protector.Protect(plainText);
    public string Decrypt(string cipherText) => _protector.Unprotect(cipherText);
}
```

**Alternatives considered**:
- Manual AES implementation: Rejected - key management complexity
- Windows DPAPI: Rejected - not cross-platform
- Azure Key Vault: Rejected - adds cloud dependency, overkill for local SQLite

### 3. Database Selection

**Decision**: SQLite for user configuration storage

**Rationale**:
- Zero-configuration, file-based database
- Suitable for single-user-focused application
- Entity Framework Core has excellent SQLite support
- Easy backup/restore (just copy the file)
- No separate database server required

**Schema approach**:
- Three main tables: Users, AuthConnections, DocumentSources
- Foreign key relationships for data integrity
- Indexed on frequently queried columns (UserId, Provider, Email)

**Alternatives considered**:
- PostgreSQL: Rejected - overkill for configuration-only storage
- In-memory storage: Rejected - data loss on restart
- JSON files: Rejected - no ACID guarantees, concurrency issues

### 4. Session Management

**Decision**: Cookie-based authentication with ASP.NET Core Identity patterns

**Rationale**:
- HTTP-only cookies prevent XSS attacks
- Secure flag ensures HTTPS-only transmission
- SameSite=Strict prevents CSRF attacks
- Built-in session expiration and sliding renewal

**Cookie configuration**:
```csharp
options.Cookie.HttpOnly = true;
options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
options.Cookie.SameSite = SameSiteMode.Strict;
options.ExpireTimeSpan = TimeSpan.FromDays(7);
options.SlidingExpiration = true;
```

**Alternatives considered**:
- JWT tokens: Rejected - harder to revoke, XSS vulnerability if stored in localStorage
- Session storage: Rejected - requires server-side session store, complicates scaling

### 5. Blazor WebAssembly Authentication

**Decision**: Use `Microsoft.AspNetCore.Components.WebAssembly.Authentication` with custom AuthStateProvider

**Rationale**:
- Official Microsoft package for Blazor WASM auth
- Supports cookie-based authentication (not just JWT)
- Integrates with ASP.NET Core backend auth
- Provides `AuthorizeView`, `AuthorizeRouteView` components

**Implementation pattern**:
- Server handles OAuth flows and issues auth cookie
- WASM client uses `AuthenticationStateProvider` to check auth status
- API calls automatically include credentials
- Custom `AuthGuard` component for protected routes

**Alternatives considered**:
- Manual auth state management: Rejected - complex, error-prone
- Third-party auth libraries: Rejected - unnecessary dependency

### 6. UI Component Design

**Decision**: Create reusable components in Ui.Shared for cross-platform use

**Components needed**:
- `LoginButton.razor` - OAuth provider selection buttons
- `UserMenu.razor` - User dropdown with profile/logout
- `AuthGuard.razor` - Wrapper for protected content
- `DocumentSourceList.razor` - List of linked sources
- `AddDocumentSourceDialog.razor` - Modal for adding sources
- `GoogleDriveBrowser.razor` - Folder selection UI

**Rationale**:
- Shared components work in both Web and MAUI Blazor
- Consistent UI across platforms
- Easier maintenance and testing

### 7. Rate Limiting Strategy

**Decision**: Use ASP.NET Core Rate Limiting middleware

**Configuration**:
- Authentication endpoints: 5 requests per minute per IP
- Token refresh: 10 requests per minute per user
- General API: 100 requests per minute per user

**Rationale**:
- Built-in middleware in .NET 8
- Flexible policy configuration
- Prevents brute force attacks on auth endpoints

### 8. OAuth Scope Requirements

**Google Drive**:
- `openid`, `email`, `profile` - Basic identity
- `https://www.googleapis.com/auth/drive.readonly` - Read files and folders
- `https://www.googleapis.com/auth/drive.file` - Read/write files created by app (optional for progress)

**GitHub**:
- `read:user`, `user:email` - Basic identity
- `repo` - Read repository contents (private repos)
- `read:org` - Read organization repos (if needed)

## Summary of Decisions

| Area | Decision | Rationale |
|------|----------|-----------|
| OAuth Implementation | ASP.NET Core built-in handlers | Official, secure, maintained |
| Token Encryption | Data Protection (AES-256) | Standard, automatic key management |
| Database | SQLite | Zero-config, file-based, EF Core support |
| Session Management | HTTP-only Secure cookies | XSS/CSRF protection, built-in expiration |
| Blazor Auth | Official WASM auth package | Integration with backend, reusable components |
| UI Components | Ui.Shared project | Cross-platform reuse (Web + MAUI) |
| Rate Limiting | Built-in middleware | Simple configuration, effective protection |
