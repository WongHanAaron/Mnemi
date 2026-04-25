using System.Collections.Generic;

namespace Mnemi.Domain.Entities;

public sealed record ClozeBlank(string Placeholder, IReadOnlyList<ClozeAnswerOption> Options);