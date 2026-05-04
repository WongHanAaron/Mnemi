# Authentication API Contract

**Date**: 2026-05-02  
**Feature**: OAuth User Configuration and Document Source Management

## Overview

This document defines the HTTP API contracts for OAuth authentication, session management, and user account operations.

---

## OAuth Endpoints

### Initiate Google OAuth Flow

Initiates the OAuth flow with Google.

```http
GET /api/auth/google/login
```

**Query Parameters**:
| Name | Type | Required | Description |
|------|------|----------|-------------|
| `returnUrl` | string | No | URL to redirect after successful authentication |

**Response**:
- **302 Redirect**: Redirects to Google's OAuth consent screen

---

### Google OAuth Callback

Callback endpoint for Google OAuth (registered with Google).

```http
GET /api/auth/google/callback
```

**Query Parameters**:
| Name | Type | Required | Description |
|------|------|----------|-------------|
| `code` | string | Conditional | Authorization code (present on success) |
| `state` | string | Conditional | CSRF protection state |
| `error` | string | Conditional | Error code (present on failure) |
| `error_description` | string | No | Human-readable error description |

**Responses**:
- **302 Redirect to `/`**: Authentication successful, session cookie set
- **302 Redirect to `/login?error={message}`**: Authentication failed

---

### Initiate GitHub OAuth Flow

Initiates the OAuth flow with GitHub.

```http
GET /api/auth/github/login
```

**Query Parameters**:
| Name | Type | Required | Description |
|------|------|----------|-------------|
| `returnUrl` | string | No | URL to redirect after successful authentication |

**Response**:
- **302 Redirect**: Redirects to GitHub's OAuth authorization page

---

### GitHub OAuth Callback

Callback endpoint for GitHub OAuth (registered with GitHub).

```http
GET /api/auth/github/callback
```

**Query Parameters**:
| Name | Type | Required | Description |
|------|------|----------|-------------|
| `code` | string | Conditional | Authorization code (present on success) |
| `state` | string | Conditional | CSRF protection state |
| `error` | string | Conditional | Error code (present on failure) |
| `error_description` | string | No | Human-readable error description |

**Responses**:
- **302 Redirect to `/`**: Authentication successful, session cookie set
- **302 Redirect to `/login?error={message}`**: Authentication failed

---

## Session Management

### Get Current User

Returns the currently authenticated user's information.

```http
GET /api/auth/me
```

**Authentication**: Required (session cookie)

**Response** (200 OK):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "displayName": "John Doe",
  "createdAt": "2026-05-02T10:00:00Z",
  "authConnections": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "provider": "Google",
      "scopes": "openid email profile https://www.googleapis.com/auth/drive.readonly",
      "createdAt": "2026-05-02T10:00:00Z"
    }
  ]
}
```

**Response** (401 Unauthorized):
```json
{
  "error": "Unauthorized",
  "message": "No active session"
}
```

---

### Sign Out

Signs out the current user and clears the session.

```http
POST /api/auth/logout
```

**Authentication**: Required (session cookie)

**Response** (200 OK):
```json
{
  "message": "Signed out successfully"
}
```

**Response** (401 Unauthorized):
```json
{
  "error": "Unauthorized",
  "message": "No active session"
}
```

---

## Account Management

### Link Additional OAuth Provider

Links a new OAuth provider to the existing account.

```http
POST /api/auth/link/{provider}
```

**Path Parameters**:
| Name | Type | Description |
|------|------|-------------|
| `provider` | string | OAuth provider: `google` or `github` |

**Authentication**: Required (session cookie)

**Response**:
- **302 Redirect**: Redirects to provider's OAuth flow

**Error Responses**:
- **400 Bad Request**: Provider already linked
- **409 Conflict**: Provider account linked to different user

---

### Unlink OAuth Provider

Removes an OAuth provider connection from the account.

```http
DELETE /api/auth/connections/{connectionId}
```

**Path Parameters**:
| Name | Type | Description |
|------|------|-------------|
| `connectionId` | string | UUID of the auth connection |

**Authentication**: Required (session cookie)

**Response** (204 No Content): Connection removed successfully

**Error Responses**:
- **400 Bad Request**: Cannot remove last connection
- **403 Forbidden**: Connection belongs to different user
- **404 Not Found**: Connection not found

---

## Error Response Format

All error responses follow this structure:

```json
{
  "error": "ErrorCode",
  "message": "Human-readable error description",
  "details": {
    "field": "Additional context"
  }
}
```

**Common Error Codes**:
| Code | HTTP Status | Description |
|------|-------------|-------------|
| `Unauthorized` | 401 | No valid session |
| `Forbidden` | 403 | Authenticated but not authorized |
| `NotFound` | 404 | Resource not found |
| `Conflict` | 409 | Resource already exists |
| `ValidationError` | 422 | Invalid request data |
| `RateLimitExceeded` | 429 | Too many requests |
| `InternalError` | 500 | Server error |

---

## Rate Limits

| Endpoint | Limit | Window |
|----------|-------|--------|
| `/api/auth/*/login` | 5 | 1 minute |
| `/api/auth/*/callback` | 10 | 1 minute |
| `/api/auth/me` | 100 | 1 minute |
| `/api/auth/logout` | 20 | 1 minute |
| `/api/auth/link/*` | 5 | 1 minute |

Rate limit headers included in responses:
```http
X-RateLimit-Limit: 5
X-RateLimit-Remaining: 3
X-RateLimit-Reset: 1714645200
```
