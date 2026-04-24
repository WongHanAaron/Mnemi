# Mnemi Flashcard App Instructions

This project builds a markdown-powered flashcard system for web and future MAUI Blazor platforms.

Use these instructions when making project changes, adding new features, or generating code:

- Primary goal: convert markdown text into flashcards and allow the user to study them interactively.
- Core features:
  - Markdown parsing and flashcard generation
  - Flashcard review mode
  - Definition matching game mode
  - Future guided question sequence mode for multi-step learning
- Project structure expectations:
  - `src/' for all compilable source code
    - 'src/Domain/Domain.csproj' project for core logic and models
    - 'src/Application/Application.csproj' project for integration between the domain logic and external ports. Also contains the interface definitions for external ports
    - For external adapters, follow this project structure:
      - 'src/Adapter.<component-name>.<external-system-short-name>/src/Adapter.<component-name>.<external-system-short-name>.csproj'. Where the 'component-name' is the name of an adapter type, for example 'Source'. Where the 'external-system-short-name' is a short name for the external system, for example 'GoogleDrive' as a source of external content. An example of this would be: 'src/Adapter.Speech.Speechify/src/Adapter.Speech.Speechify.csproj' for an adapter that connects to the Speechify system for Speech execution content
    - 'src/Ui.Shared/Ui.Shared.csproj' project for shareable blazor components between the web app and MAUI
    - 'src/Ui.Web/Ui.Web.csproj' project for hosting of the web application
    - 'src/Ui.Maui/Ui.Maui.csproj' project for hosting of the MAUI Blazor application
  - 'tests/' for all tests
    - 'tests/<projectname>.Tests/<projectname>.Tests.csproj' for tests related to a specific project. For example, 'tests/Domain.Tests/Domain.Tests.csproj' for tests related to the Domain project. For example, 'tests/Adapter.Speech.Speechify.Tests/Adapter.Speech.Speechify.Tests.csproj' for tests related to the 'src/Adapter.Speech.Speechify/src/Adapter.Speech.Speechify.csproj' project
  - 'infra/' for all components required for deploying and hosting the application, such as Dockerfiles, Kubernetes manifests, and maui app generation scripts.
  - 'docs/' for all documentation for this project
    - 'docs/users/' for user-facing documentation, such as markdown files describing how to use the application and its features.
    - 'docs/dev/' for developer-facing documentation, such as markdown files describing the architecture of the application, how to set up a development environment, and how to contribute to the project.
    - all documentation files should have a file naming convention that is readable using lower-case kebab case. For example: 'how-to-use-flashcard-review-mode.md' for a user-facing documentation file describing how to use the flashcard review mode, and 'application-architecture.md' for a developer-facing documentation file describing the architecture of the application.
- Prefer clean separation between platform-agnostic logic and UI-specific implementation.
- Keep code maintainable and simple, with reusable components and services.
- Use keywords in suggestions: flashcards, markdown, definition matching, study mode, guided questions, MAUI Blazor, web app.

Future expansion should preserve a shared core that can be used across web and Blazor platforms.

<!-- SPECKIT START -->
Feature plan: specs/001-domain-models-and-parsing/plan.md

For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan
<!-- SPECKIT END -->
