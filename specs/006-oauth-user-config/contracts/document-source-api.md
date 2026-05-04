# Document Source API Contract

**Date**: 2026-05-02  
**Feature**: OAuth User Configuration and Document Source Management

## Overview

This document defines the HTTP API contracts for managing document sources (Google Drive folders and GitHub repositories).

---

## Document Source Endpoints

### List Document Sources

Returns all document sources linked to the current user.

```http
GET /api/document-sources
```

**Authentication**: Required (session cookie)

**Response** (200 OK):
```json
{
  "sources": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440002",
      "provider": "GoogleDrive",
      "displayName": "My Flashcards",
      "isAccessible": true,
      "createdAt": "2026-05-02T10:30:00Z",
      "authConnection": {
        "id": "550e8400-e29b-41d4-a716-446655440001",
        "provider": "Google"
      }
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440003",
      "provider": "GitHub",
      "displayName": "Study Notes Repo",
      "isAccessible": true,
      "createdAt": "2026-05-02T11:00:00Z",
      "authConnection": {
        "id": "550e8400-e29b-41d4-a716-446655440004",
        "provider": "GitHub"
      }
    }
  ]
}
```

---

### Get Document Source Details

Returns detailed information about a specific document source.

```http
GET /api/document-sources/{sourceId}
```

**Path Parameters**:
| Name | Type | Description |
|------|------|-------------|
| `sourceId` | string | UUID of the document source |

**Authentication**: Required (session cookie)

**Response** (200 OK) - Google Drive:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "provider": "GoogleDrive",
  "displayName": "My Flashcards",
  "isAccessible": true,
  "createdAt": "2026-05-02T10:30:00Z",
  "updatedAt": "2026-05-02T10:30:00Z",
  "authConnection": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "provider": "Google"
  },
  "config": {
    "folderId": "1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms",
    "subPath": null
  }
}
```

**Response** (200 OK) - GitHub:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440003",
  "provider": "GitHub",
  "displayName": "Study Notes Repo",
  "isAccessible": true,
  "createdAt": "2026-05-02T11:00:00Z",
  "updatedAt": "2026-05-02T11:00:00Z",
  "authConnection": {
    "id": "550e8400-e29b-41d4-a716-446655440004",
    "provider": "GitHub"
  },
  "config": {
    "repoOwner": "johndoe",
    "repoName": "study-notes",
    "rootPath": "/flashcards",
    "branch": "main"
  }
}
```

**Error Responses**:
- **403 Forbidden**: Source belongs to different user
- **404 Not Found**: Source not found

---

### Create Google Drive Document Source

Creates a new document source linked to a Google Drive folder.

```http
POST /api/document-sources/google-drive
```

**Authentication**: Required (session cookie)

**Request Body**:
```json
{
  "authConnectionId": "550e8400-e29b-41d4-a716-446655440001",
  "displayName": "My Flashcards",
  "folderId": "1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms",
  "subPath": null
}
```

**Field Descriptions**:
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `authConnectionId` | string | Yes | UUID of the Google auth connection |
| `displayName` | string | Yes | User-defined name for this source |
| `folderId` | string | Yes | Google Drive folder ID |
| `subPath` | string | No | Optional subfolder path |

**Response** (201 Created):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "provider": "GoogleDrive",
  "displayName": "My Flashcards",
  "isAccessible": true,
  "createdAt": "2026-05-02T10:30:00Z",
  "authConnection": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "provider": "Google"
  },
  "config": {
    "folderId": "1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms",
    "subPath": null
  }
}
```

**Error Responses**:
- **400 Bad Request**: Invalid folder ID or missing required fields
- **401 Unauthorized**: Auth connection invalid or expired
- **403 Forbidden**: Auth connection belongs to different user
- **404 Not Found**: Auth connection not found
- **409 Conflict**: Folder already linked as document source
- **422 Validation Error**: Cannot access folder (permissions)

---

### Create GitHub Document Source

Creates a new document source linked to a GitHub repository.

```http
POST /api/document-sources/github
```

**Authentication**: Required (session cookie)

**Request Body**:
```json
{
  "authConnectionId": "550e8400-e29b-41d4-a716-446655440004",
  "displayName": "Study Notes Repo",
  "repoOwner": "johndoe",
  "repoName": "study-notes",
  "rootPath": "/flashcards",
  "branch": "main"
}
```

**Field Descriptions**:
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `authConnectionId` | string | Yes | UUID of the GitHub auth connection |
| `displayName` | string | Yes | User-defined name for this source |
| `repoOwner` | string | Yes | Repository owner (user or org) |
| `repoName` | string | Yes | Repository name |
| `rootPath` | string | No | Path within repo (default: "/") |
| `branch` | string | No | Branch name (default: "main") |

**Response** (201 Created):
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440003",
  "provider": "GitHub",
  "displayName": "Study Notes Repo",
  "isAccessible": true,
  "createdAt": "2026-05-02T11:00:00Z",
  "authConnection": {
    "id": "550e8400-e29b-41d4-a716-446655440004",
    "provider": "GitHub"
  },
  "config": {
    "repoOwner": "johndoe",
    "repoName": "study-notes",
    "rootPath": "/flashcards",
    "branch": "main"
  }
}
```

**Error Responses**:
- **400 Bad Request**: Invalid repository details
- **401 Unauthorized**: Auth connection invalid or expired
- **403 Forbidden**: Auth connection belongs to different user
- **404 Not Found**: Auth connection or repository not found
- **409 Conflict**: Repository/path already linked as document source
- **422 Validation Error**: Cannot access repository (permissions)

---

### Update Document Source

Updates the display name of a document source.

```http
PATCH /api/document-sources/{sourceId}
```

**Path Parameters**:
| Name | Type | Description |
|------|------|-------------|
| `sourceId` | string | UUID of the document source |

**Authentication**: Required (session cookie)

**Request Body**:
```json
{
  "displayName": "Updated Name"
}
```

**Field Descriptions**:
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `displayName` | string | Yes | New display name (1-200 chars) |

**Response** (200 OK): Updated source object (same as GET response)

**Error Responses**:
- **400 Bad Request**: Invalid display name
- **403 Forbidden**: Source belongs to different user
- **404 Not Found**: Source not found

---

### Delete Document Source

Removes a document source link.

```http
DELETE /api/document-sources/{sourceId}
```

**Path Parameters**:
| Name | Type | Description |
|------|------|-------------|
| `sourceId` | string | UUID of the document source |

**Authentication**: Required (session cookie)

**Response** (204 No Content): Source removed successfully

**Error Responses**:
- **403 Forbidden**: Source belongs to different user
- **404 Not Found**: Source not found

---

### Check Source Accessibility

Checks if a document source is currently accessible (validates permissions).

```http
POST /api/document-sources/{sourceId}/check-access
```

**Path Parameters**:
| Name | Type | Description |
|------|------|-------------|
| `sourceId` | string | UUID of the document source |

**Authentication**: Required (session cookie)

**Response** (200 OK):
```json
{
  "isAccessible": true,
  "checkedAt": "2026-05-02T12:00:00Z"
}
```

**Response** (200 OK - Inaccessible):
```json
{
  "isAccessible": false,
  "errorMessage": "Folder not found or access denied",
  "checkedAt": "2026-05-02T12:00:00Z"
}
```

**Error Responses**:
- **403 Forbidden**: Source belongs to different user
- **404 Not Found**: Source not found

---

## Browse Endpoints

### List Google Drive Folders

Returns folders accessible via the specified Google auth connection.

```http
GET /api/browse/google-drive/folders
```

**Query Parameters**:
| Name | Type | Required | Description |
|------|------|----------|-------------|
| `authConnectionId` | string | Yes | UUID of the Google auth connection |
| `parentFolderId` | string | No | Parent folder ID (omit for root) |

**Authentication**: Required (session cookie)

**Response** (200 OK):
```json
{
  "folders": [
    {
      "id": "1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms",
      "name": "My Flashcards",
      "path": "/My Drive/My Flashcards",
      "isAccessible": true
    },
    {
      "id": "1CyiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE3upms",
      "name": "Study Notes",
      "path": "/My Drive/Study Notes",
      "isAccessible": true
    }
  ]
}
```

**Error Responses**:
- **401 Unauthorized**: Auth connection invalid or expired
- **403 Forbidden**: Auth connection belongs to different user
- **404 Not Found**: Auth connection not found

---

### List GitHub Repositories

Returns repositories accessible via the specified GitHub auth connection.

```http
GET /api/browse/github/repos
```

**Query Parameters**:
| Name | Type | Required | Description |
|------|------|----------|-------------|
| `authConnectionId` | string | Yes | UUID of the GitHub auth connection |

**Authentication**: Required (session cookie)

**Response** (200 OK):
```json
{
  "repositories": [
    {
      "owner": "johndoe",
      "name": "study-notes",
      "fullName": "johndoe/study-notes",
      "isPrivate": false,
      "defaultBranch": "main"
    },
    {
      "owner": "acme-corp",
      "name": "training-materials",
      "fullName": "acme-corp/training-materials",
      "isPrivate": true,
      "defaultBranch": "master"
    }
  ]
}
```

**Error Responses**:
- **401 Unauthorized**: Auth connection invalid or expired
- **403 Forbidden**: Auth connection belongs to different user
- **404 Not Found**: Auth connection not found

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
| `Unauthorized` | 401 | No valid session or expired auth connection |
| `Forbidden` | 403 | Authenticated but not authorized for this resource |
| `NotFound` | 404 | Resource not found |
| `Conflict` | 409 | Resource already exists |
| `ValidationError` | 422 | Invalid request data or cannot access external resource |
| `RateLimitExceeded` | 429 | Too many requests |
| `ExternalServiceError` | 502 | Error communicating with Google Drive or GitHub |
| `InternalError` | 500 | Server error |

---

## Rate Limits

| Endpoint | Limit | Window |
|----------|-------|--------|
| `GET /api/document-sources` | 100 | 1 minute |
| `POST /api/document-sources/*` | 10 | 1 minute |
| `PATCH /api/document-sources/*` | 20 | 1 minute |
| `DELETE /api/document-sources/*` | 10 | 1 minute |
| `GET /api/browse/*` | 30 | 1 minute |

Rate limit headers included in responses:
```http
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1714645200
```
