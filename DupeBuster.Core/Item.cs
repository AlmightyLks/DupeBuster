using System.IO.Abstractions;

namespace DupeBuster.Core;

public record struct Item(IFileInfo FileInfo, IdentificationResult Value);
