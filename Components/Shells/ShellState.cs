namespace QHPFH_ConceptPrototype.Components.Shells;

public enum ShellMode
{
    Std,
    Concept
}

public static class ShellState
{
    public const string ShellModeStorageKey = "pfh-shell-mode";
    public const string ConceptNavCompactStorageKey = "pfh-concept-nav-compact";

    public static string ToStorage(this ShellMode mode) => mode == ShellMode.Std ? "std" : "concept";

    public static ShellMode FromStorage(string? value) =>
        string.Equals(value, "std", StringComparison.OrdinalIgnoreCase) ? ShellMode.Std : ShellMode.Concept;
}
