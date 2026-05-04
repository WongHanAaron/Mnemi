using System.Collections.Generic;

namespace Mnemi.Domain.Entities;

public sealed record Document(
    File File,
    string Content,
    IReadOnlyList<Group> DocumentTags);