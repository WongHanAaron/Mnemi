# Project Structure Guide

## Overview

The Mnemi flashcard system follows a clean architecture with clear separation of concerns across layers.

```
Mnemi/
├── src/                           # Source code
│   ├── Domain/                    # Domain layer (core business logic)
│   ├── Application/               # Application layer (orchestration, ports)
│   ├── Adapter.*/                 # Adapters for external systems (speech, storage, etc.)
│   ├── Ui.Shared/                 # Shared Blazor components
│   ├── Ui.Web/                    # Web application (Blazor WebAssembly)
│   └── Ui.Maui/                   # MAUI Blazor application (cross-platform)
├── tests/                         # Test projects
│   ├── Domain.Tests/              # Domain logic tests
│   ├── Application.Tests/         # Integration tests
│   └── Ui.Shared.Tests/           # Component tests
├── infra/                         # Infrastructure and deployment
│   ├── docker/                    # Docker configurations
│   ├── k8s/                       # Kubernetes manifests
│   └── ci-cd/                     # CI/CD pipeline definitions
├── docs/                          # Documentation
│   ├── users/                     # User-facing documentation
│   └── dev/                       # Developer documentation
└── Mnemi.sln                      # Solution file
```

## Layer Responsibilities

### Domain (`src/Domain/`)
- **Purpose**: Core business logic, models, and entities
- **Dependencies**: None (no external packages)
- **Responsibilities**:
  - Flashcard entity and aggregate definitions
  - Scheduling algorithm implementation
  - Markdown parsing logic
  - Tag hierarchy system

### Application (`src/Application/`)
- **Purpose**: Integration between domain and external systems
- **Dependencies**: Domain, external adapters
- **Responsibilities**:
  - Application service interfaces
  - Port definitions (speech, storage, markdown rendering)
  - Use case orchestration
  - Configuration and dependency injection

### Adapters (`src/Adapter.*`)
- **Purpose**: External system integrations
- **Naming**: `Adapter.<Component>.<System>` (e.g., `Adapter.Speech.Speechify`)
- **Examples**:
  - `Adapter.Speech.Speechify/` - Speech synthesis
  - `Adapter.Storage.GoogleDrive/` - Cloud storage
  - `Adapter.Renderer.CommonMark/` - Markdown rendering

### UI Shared (`src/Ui.Shared/`)
- **Purpose**: Reusable Blazor components
- **Platforms**: Web (WebAssembly) and MAUI
- **Components**:
  - Flashcard review UI
  - Definition matching game
  - Tag/group navigation
  - Study progress indicators

### UI Web (`src/Ui.Web/`)
- **Purpose**: Browser-based application
- **Type**: Blazor WebAssembly
- **Entry Point**: `Program.cs`
- **Responsibilities**:
  - Web-specific routing and layout
  - Browser storage (IndexedDB)
  - Web-based state management

### UI MAUI (`src/Ui.Maui/`)
- **Purpose**: Cross-platform mobile and desktop
- **Type**: MAUI Blazor
- **Platforms**: iOS, Android, Windows, macOS
- **Responsibilities**:
  - Platform-specific features
  - Mobile optimizations
  - Native integrations

## Project Dependencies

```
Domain (no dependencies)
  ↑
  ├── Application
  │   ├── Adapter.* (parallel)
  │   └── Ui.Shared
  │       ├── Ui.Web
  │       └── Ui.Maui
```

## Testing Strategy

- **Domain.Tests**: Unit tests for domain logic (fastest, most isolated)
- **Application.Tests**: Integration tests with mocked adapters
- **Ui.Shared.Tests**: Component tests for reusable UI

## Adding New Projects

### New Adapter
1. Create folder: `src/Adapter.<Component>.<System>/`
2. Create project file: `Adapter.<Component>.<System>.csproj`
3. Create test folder: `tests/Adapter.<Component>.<System>.Tests/`
4. Update `Mnemi.sln`

### New External Adapter Example
```
src/Adapter.Source.Notion/
├── src/
│   ├── Adapter.Source.Notion.csproj
│   ├── NotionCardSource.cs
│   └── NotionClient.cs
└── (tests/Adapter.Source.Notion.Tests/)
```

Reference Application port definitions in `src/Application/`.
