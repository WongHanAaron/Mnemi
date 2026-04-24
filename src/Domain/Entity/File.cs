using System;

namespace Mnemi.Domain.Entities;

public sealed record File(
    string Filename,
    string RelativePath,
    DateTime? DateCreated,
    DateTime? DateLastModified,
    string FileContents);