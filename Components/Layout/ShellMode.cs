namespace QHPFH_ConceptPrototype.Components.Layout;

public enum ShellMode
{
    Std,
    Concept
}

public static class ShellModeExtensions
{
    public const string StorageKey = "pfh-shell-mode";

    public static string ToStorageValue(this ShellMode mode) => mode == ShellMode.Concept ? "concept" : "std";

    public static ShellMode FromStorageValue(string? value) =>
        string.Equals(value, "std", StringComparison.OrdinalIgnoreCase)
            ? ShellMode.Std
            : ShellMode.Concept;
}
