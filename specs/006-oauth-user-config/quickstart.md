# Quickstart: OAuth User Configuration and Document Source Management

**Date**: 2026-05-02  
**Feature**: [spec.md](spec.md) | [plan.md](plan.md) | [data-model.md](data-model.md)

## Prerequisites

- .NET 8.0 SDK
- Google OAuth 2.0 credentials (Client ID and Secret)
- GitHub OAuth App credentials (Client ID and Secret)
- SQLite (included via Entity Framework Core)

## Environment Setup

### 1. Google OAuth Credentials

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing
3. Navigate to **APIs & Services > Credentials**
4. Click **Create Credentials > OAuth client ID**
5. Configure consent screen (External for testing)
6. Select **Web application** as application type
7. Add authorized redirect URI: `https://localhost:5001/api/auth/google/callback`
8. Save Client ID and Client Secret

### 2. GitHub OAuth App

1. Go to GitHub Settings > Developer settings > OAuth Apps
2. Click **New OAuth App**
3. Set Authorization callback URL: `https://localhost:5001/api/auth/github/callback`
4. Save Client ID and Client Secret

### 3. Configuration

Create `appsettings.Development.json` in `src/Ui.Web/`:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    },
    "GitHub": {
      "ClientId": "your-github-client-id",
      "ClientSecret": "your-github-client-secret"
    }
  },
  "ConnectionStrings": {
    "MnemiDb": "Data Source=mnemi.db"
  }
}
```

## Running the Application

### 1. Build the Solution

```bash
dotnet build
```

### 2. Run Database Migrations

```bash
cd src/Adapter.Persistence.Sqlite
dotnet ef database update
```

### 3. Run the Web Application

```bash
cd src/Ui.Web
dotnet run
```

Navigate to `https://localhost:5001`

## User Flow

### First-Time User

1. **Landing Page**: User sees welcome screen with "Sign in with Google" and "Sign in with GitHub" buttons
2. **OAuth Flow**: User clicks a provider, completes OAuth consent
3. **Account Creation**: System automatically creates user account
4. **Dashboard**: User arrives at dashboard with prompt to add document source
5. **Add Document Source**:
   - Click "Add Source"
   - Select provider (Google Drive or GitHub)
   - Browse and select folder/repository
   - Confirm to link
6. **Study**: User can now browse flashcards from linked sources

### Returning User

1. **Automatic Sign-In**: If session valid, user goes directly to dashboard
2. **Session Expired**: User clicks sign-in button, completes OAuth (no account creation)
3. **Manage Sources**: User can add/remove document sources from settings

## API Testing

### Sign In (Browser)

Navigate to:
- Google: `https://localhost:5001/api/auth/google/login`
- GitHub: `https://localhost:5001/api/auth/github/login`

### Get Current User (Authenticated)

```bash
curl -b cookies.txt https://localhost:5001/api/auth/me
```

### List Document Sources

```bash
curl -b cookies.txt https://localhost:5001/api/document-sources
```

### Create Google Drive Source

```bash
curl -X POST \
  -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{
    "authConnectionId": "your-auth-connection-id",
    "displayName": "My Flashcards",
    "folderId": "your-folder-id"
  }' \
  https://localhost:5001/api/document-sources/google-drive
```

## Project Structure Overview

```
src/
├── Domain/                          # User, AuthConnection, DocumentSourceConfig entities
├── Application/                     # IAuthService, IUserRepository, IDocumentSourceRepository
├── Adapter.Auth.OAuth/              # GoogleAuthHandler, GitHubAuthHandler, TokenEncryption
├── Adapter.Persistence.Sqlite/      # EF Core entities, repositories, MnemiDbContext
├── Ui.Shared/                       # LoginButton, UserMenu, DocumentSourceList components
├── Ui.Web/                          # Web host, auth middleware, WebAuthStateProvider
└── Ui.Maui/                         # MAUI host (future), MauiAuthStateProvider
```

## Key Components

### Domain Entities

- **User**: Aggregate root with email, display name
- **AuthConnection**: OAuth tokens (encrypted), provider info
- **DocumentSourceConfig**: Linked folder/repo configuration

### Services

- **TokenEncryptionService**: AES-256 encryption for OAuth tokens
- **UserService**: Account creation, provider linking
- **DocumentSourceService**: Source CRUD, accessibility checks

### UI Components

- **LoginButton.razor**: OAuth provider selection
- **UserMenu.razor**: Profile, settings, logout
- **DocumentSourceList.razor**: Manage linked sources
- **AddDocumentSourceDialog.razor**: Browse and link sources

## Testing

### Unit Tests

```bash
dotnet test tests/Domain.Tests
dotnet test tests/Application.Tests
```

### Integration Tests

```bash
dotnet test tests/Adapter.Persistence.Sqlite.Tests
dotnet test tests/Adapter.Auth.OAuth.Tests
```

### E2E Tests

```bash
cd tests/Ui.E2E.Playwright
dotnet test
```

## Troubleshooting

### OAuth Callback Fails

- Verify redirect URIs match exactly in Google/GitHub settings
- Check `appsettings.Development.json` has correct credentials
- Ensure HTTPS is enabled for localhost

### Database Errors

- Delete `mnemi.db` to reset (development only)
- Run `dotnet ef database update` to apply migrations

### Token Encryption Errors

- Data Protection keys are stored in `%APPDATA%/ASP.NET/DataProtection-Keys`
- Clearing this folder will invalidate existing sessions

## Next Steps

1. Implement Domain entities (User, AuthConnection, DocumentSourceConfig)
2. Create SQLite persistence adapter with EF Core
3. Implement OAuth handlers for Google and GitHub
4. Build UI components for authentication and document source management
5. Add API controllers for auth and document source endpoints
6. Write integration and E2E tests

See [tasks.md](tasks.md) (generated by `/speckit.tasks`) for detailed implementation tasks.
