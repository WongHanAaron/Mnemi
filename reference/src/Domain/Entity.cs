// Domain: Core business logic and models for the Mnemi flashcard system
// This project contains no external dependencies by design.
// It defines the domain models, entities, and core business logic.

namespace Mnemi.Domain;

/// <summary>
/// Base entity class for all domain models
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; }
}
