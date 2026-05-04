# Feature Specification: OAuth User Configuration and Document Source Management

**Feature Branch**: `006-oauth-user-config`  
**Created**: 2026-05-02  
**Status**: Draft  
**Input**: User description: "I would like to implement the user configuration backed by OAuth such that users can create accounts, and setup repositories to begin retrieving flashcards from. Include UI changes to add a new user login and account creation and then a UI for linking new document sources"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Account Creation via OAuth (Priority: P1)

As a new user, I want to sign up for Mnemi using my existing Google or GitHub account so that I can quickly start using the application without creating a new password.

**Why this priority**: This is the entry point for all users. Without authentication, no other features can be accessed. OAuth reduces friction and improves security by leveraging trusted identity providers.

**Independent Test**: A new user can click "Sign in with Google" or "Sign in with GitHub", complete the OAuth flow, and arrive at the Mnemi dashboard with a new account created automatically.

**Acceptance Scenarios**:

1. **Given** a user visits Mnemi for the first time, **When** they click "Sign in with Google" and authorize the application, **Then** a new user account is created with their Google email and display name, and they are redirected to the dashboard.
2. **Given** a user visits Mnemi for the first time, **When** they click "Sign in with GitHub" and authorize the application, **Then** a new user account is created with their GitHub email and display name, and they are redirected to the dashboard.
3. **Given** a user already has a Mnemi account linked to Google, **When** they sign in with Google again, **Then** they are authenticated to their existing account without creating a duplicate.
4. **Given** a user cancels the OAuth flow, **When** they return to Mnemi, **Then** they remain unauthenticated and see the login page with a friendly message.

---

### User Story 2 - Linking Google Drive Document Sources (Priority: P1)

As an authenticated user, I want to connect a Google Drive folder containing my flashcard markdown files so that Mnemi can retrieve and display my study content.

**Why this priority**: Document sources are the foundation of the flashcard system. Users need to link their content before they can study. Google Drive is a primary storage target.

**Independent Test**: An authenticated user can browse their Google Drive, select a folder, and see it appear as a linked document source in their settings. The system can list markdown files from that folder.

**Acceptance Scenarios**:

1. **Given** an authenticated user with a Google OAuth connection, **When** they navigate to "Add Document Source" and select Google Drive, **Then** they can browse their Drive folders and select one to link.
2. **Given** a user selects a Google Drive folder, **When** they confirm the selection, **Then** a new document source is created with the folder ID, display name, and associated with their Google auth connection.
3. **Given** a user has linked a Google Drive folder, **When** they view their document sources list, **Then** they see the folder name, provider (Google Drive), and options to edit or remove it.
4. **Given** a user attempts to link a folder they don't have access to, **When** the selection is made, **Then** they see an error message explaining the permission issue.

---

### User Story 3 - Linking GitHub Repository Document Sources (Priority: P1)

As an authenticated user, I want to connect a GitHub repository (or a subpath within it) containing my flashcard markdown files so that Mnemi can retrieve and display my study content.

**Why this priority**: GitHub is a primary storage target for technical users who version-control their notes. Supporting repositories enables collaboration and versioned flashcard content.

**Independent Test**: An authenticated user can browse their accessible GitHub repositories, select one (optionally specifying a subpath), and see it appear as a linked document source in their settings.

**Acceptance Scenarios**:

1. **Given** an authenticated user with a GitHub OAuth connection, **When** they navigate to "Add Document Source" and select GitHub, **Then** they can browse their accessible repositories.
2. **Given** a user selects a GitHub repository, **When** they optionally specify a root path (e.g., "/notes/flashcards") and confirm, **Then** a new document source is created with the repo owner, name, path, and associated with their GitHub auth connection.
3. **Given** a user has linked a GitHub repository, **When** they view their document sources list, **Then** they see the repository name, provider (GitHub), path, and options to edit or remove it.
4. **Given** a user attempts to link a repository they don't have access to, **When** the selection is made, **Then** they see an error message explaining the permission issue.

---

### User Story 4 - Managing Multiple Document Sources (Priority: P2)

As a user with multiple content locations, I want to view, edit, and remove my linked document sources so that I can organize my flashcard content across different providers and folders.

**Why this priority**: Power users will have content spread across multiple locations. Managing these sources is essential for organization, though not required for basic functionality.

**Independent Test**: A user can view all their linked sources in a settings page, rename a source, change its configuration, or remove it entirely without affecting other sources.

**Acceptance Scenarios**:

1. **Given** a user has multiple document sources linked, **When** they visit the document sources settings page, **Then** they see a list of all sources with their provider, name, and linked date.
2. **Given** a user views their document sources, **When** they click "Edit" on a source, **Then** they can modify the display name and see updated information.
3. **Given** a user views their document sources, **When** they click "Remove" on a source and confirm, **Then** the source is unlinked and no longer appears in their list or deck enumeration.
4. **Given** a user removes a document source, **When** the action completes, **Then** their flashcards from that source are no longer available, but their study progress files remain in the external storage (Google Drive/GitHub).

---

### User Story 5 - Login and Session Management (Priority: P2)

As a returning user, I want to sign in to my existing account and have my session persist securely so that I don't need to re-authenticate frequently.

**Why this priority**: Session management is critical for user experience. Users expect to stay logged in across browser sessions, but security requires proper token handling.

**Independent Test**: A user can sign in, close the browser, reopen it, and still be authenticated. They can also explicitly log out, which clears their session.

**Acceptance Scenarios**:

1. **Given** a user has previously authenticated, **When** they return to Mnemi within the session lifetime, **Then** they are automatically signed in and see their dashboard.
2. **Given** a user is authenticated, **When** they click "Sign Out", **Then** their session is cleared and they are redirected to the login page.
3. **Given** a user's session expires or their OAuth token is revoked externally, **When** they attempt to access protected content, **Then** they are redirected to re-authenticate.
4. **Given** a user has multiple OAuth providers linked to the same account, **When** they sign in with either provider, **Then** they access the same account and see all their document sources.

---

### User Story 6 - Linking Additional OAuth Providers (Priority: P3)

As a user who initially signed up with Google, I want to later link my GitHub account (or vice versa) so that I can access document sources from both providers without creating separate accounts.

**Why this priority**: This provides flexibility for users who use both Google Drive and GitHub, but it's not required for basic functionality since users can choose their primary provider at signup.

**Independent Test**: A user signed up with Google can navigate to account settings, click "Link GitHub Account", complete the OAuth flow, and then add GitHub repositories as document sources.

**Acceptance Scenarios**:

1. **Given** a user authenticated with Google, **When** they navigate to account settings and click "Link GitHub Account", **Then** they complete the GitHub OAuth flow and the connection is added to their existing account.
2. **Given** a user has linked multiple providers, **When** they view their account settings, **Then** they see all connected providers with options to unlink each (except the last one).
3. **Given** a user attempts to link a provider account already linked to another Mnemi user, **When** the OAuth completes, **Then** they see an error explaining the account is already associated with a different user.

---

### Edge Cases

- **What happens when OAuth token expires during an operation?** The system should attempt to refresh the token using the stored refresh token. If refresh fails, the user is prompted to re-authenticate.
- **How does the system handle a user revoking access externally?** On the next API call, the system detects the revoked token, marks the auth connection as invalid, and prompts the user to re-authorize.
- **What if a linked document source (folder/repo) is deleted or becomes inaccessible?** The source remains in the user's list but is marked as "unavailable" with an error indicator. The user can remove it or attempt to reconnect.
- **How are duplicate document sources prevented?** The system checks for existing sources with the same provider, folder ID (Drive), or repo/path combination (GitHub) before creating a new one.
- **What happens when a user deletes their account?** All AuthConnections and DocumentSources are removed from the database. The user's content in Google Drive/GitHub remains untouched.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST support OAuth 2.0 authentication with Google for Google Drive access.
- **FR-002**: System MUST support OAuth 2.0 authentication with GitHub for repository access.
- **FR-003**: System MUST create a user account automatically upon first successful OAuth authentication.
- **FR-004**: System MUST map multiple OAuth providers to a single user account based on matching email addresses.
- **FR-005**: System MUST securely store OAuth access tokens and refresh tokens with encryption at rest.
- **FR-006**: System MUST allow users to link multiple Google Drive folders as document sources.
- **FR-007**: System MUST allow users to link multiple GitHub repositories (with optional subpaths) as document sources.
- **FR-008**: System MUST validate that users have appropriate permissions before allowing document source linking.
- **FR-009**: System MUST provide a UI for users to view all their linked document sources.
- **FR-010**: System MUST allow users to remove document sources, which stops the system from accessing that content.
- **FR-011**: System MUST maintain user sessions with secure, HTTP-only cookies.
- **FR-012**: System MUST provide a sign-out function that clears the user session.
- **FR-013**: System MUST display appropriate error messages when OAuth flows fail or are cancelled.
- **FR-014**: System MUST allow users to link additional OAuth providers to their existing account.
- **FR-015**: System MUST prevent users from linking the same external provider account to multiple Mnemi users.

### Key Entities *(include if feature involves data)*

- **User**: Represents a person using Mnemi. Contains identity information (email, display name) and creation timestamp. A user can have multiple authentication connections and document sources.
- **AuthConnection**: Represents an OAuth connection between a Mnemi user and an external identity provider (Google or GitHub). Contains the provider type, provider-specific user ID, encrypted access token, encrypted refresh token, expiration time, and granted scopes.
- **DocumentSource**: Represents a linked external location containing flashcard content. Associated with a user and an auth connection. For Google Drive, stores the folder ID. For GitHub, stores the repository owner, name, and optional root path. Includes a user-defined display name.

### Non-Functional Requirements *(optional)*

- **NFR-001**: OAuth token storage MUST use industry-standard encryption (AES-256 or equivalent).
- **NFR-002**: Session cookies MUST be HTTP-only, Secure, and SameSite=Strict.
- **NFR-003**: All authentication endpoints MUST implement rate limiting to prevent brute force attacks.
- **NFR-004**: OAuth flows MUST complete within 30 seconds under normal network conditions.
- **NFR-005**: The UI MUST clearly indicate which OAuth providers are available and which are already linked.

## Success Criteria *(mandatory)*

- Users can complete OAuth sign-up in under 2 minutes from landing page to dashboard.
- Users can link their first document source within 3 minutes of account creation.
- 95% of OAuth authentication attempts succeed on the first try.
- Users can view and manage all their document sources from a single settings page.
- Session persistence works across browser restarts for the configured session lifetime.
- Users can link at least 2 different OAuth providers to a single account.
- Document sources remain accessible as long as the external permissions are valid.

## Assumptions *(optional)*

- Users have existing Google or GitHub accounts with access to their flashcard content.
- Users store flashcard content in Markdown files within Google Drive folders or GitHub repositories.
- OAuth providers (Google, GitHub) maintain high availability and don't significantly change their APIs.
- Users understand basic OAuth consent screens and will grant necessary permissions.
- The application will be hosted over HTTPS in production environments.

## Dependencies *(optional)*

- Google OAuth 2.0 API and Google Drive API access.
- GitHub OAuth 2.0 API and GitHub Repository API access.
- Database for storing user accounts, auth connections, and document source configurations.
- Encryption key management for secure token storage.

## Out of Scope *(optional)*

- Email/password authentication (OAuth-only for this iteration).
- Automatic token refresh scheduling (refresh on-demand when expired).
- Real-time sync via webhooks (polling-based access for initial implementation).
- Team/organization accounts and sharing (single-user focus).
- Migration of existing anonymous/local data to authenticated accounts.
- Custom OAuth providers beyond Google and GitHub.

## Notes *(optional)*

- The backend should remain thin and stateless with respect to flashcard content. All card data and study progress lives in external storage.
- Consider implementing PKCE (Proof Key for Code Exchange) for enhanced OAuth security.
- The UI should guide first-time users through the authentication and document source linking flow with clear call-to-action buttons.
- Document source display names should default to the folder name (Drive) or repository name (GitHub) but be editable by users.
