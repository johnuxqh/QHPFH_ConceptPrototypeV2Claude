namespace QHPFH_ConceptPrototype.Components.Shells;

public sealed class NavigationState
{
    public string ActivePrimaryKey { get; private set; } = "home";
    public string ActiveSubItem { get; private set; } = string.Empty;

    public event Action? Changed;

    public void SetActive(string primaryKey, string subItem)
    {
        ActivePrimaryKey = primaryKey;
        ActiveSubItem = subItem;
        Changed?.Invoke();
    }
}
