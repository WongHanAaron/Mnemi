using System.Collections.Generic;

namespace Mnemi.Domain.Entities;

public sealed record Group(IReadOnlyList<string> Segments)
{
    public string DisplayPath => string.Join("::", Segments);
}