using Godot;
using System.Collections.Generic;

public partial class InventoryManager : Node
{
    public static InventoryManager Instance { get; private set; }

    private InventoryData _inventory = new();

    public override void _Ready()
    {
        Instance = this;
    }

    public int Money => _inventory.Money;

    public void SetMoney(int amount)
    {
        _inventory.Money = Mathf.Max(0, amount);
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0)
            return;

        _inventory.Money += amount;
    }

    public bool CanSpend(int amount)
    {
        return amount > 0 && _inventory.Money >= amount;
    }

    public bool SpendMoney(int amount)
    {
        if (!CanSpend(amount))
            return false;

        _inventory.Money -= amount;
        return true;
    }

    public int GetItemCount(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return 0;

        return _inventory.Items.TryGetValue(itemId, out int count) ? count : 0;
    }

    public bool HasItem(string itemId, int amount = 1)
    {
        if (amount <= 0)
            return true;

        return GetItemCount(itemId) >= amount;
    }

    public bool AddItem(string itemId, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            return false;

        if (ItemDatabase.Instance != null && !ItemDatabase.Instance.Exists(itemId))
        {
            GD.PrintErr($"Unknown item id: {itemId}");
            return false;
        }

        int maxStack = ItemDatabase.Instance?.GetMaxStack(itemId) ?? 99;
        int current = GetItemCount(itemId);
        int next = Mathf.Min(current + amount, maxStack);

        _inventory.Items[itemId] = next;
        return true;
    }

    public bool RemoveItem(string itemId, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            return false;

        if (!_inventory.Items.TryGetValue(itemId, out int current) || current < amount)
            return false;

        current -= amount;

        if (current <= 0)
            _inventory.Items.Remove(itemId);
        else
            _inventory.Items[itemId] = current;

        return true;
    }

    public void Clear()
    {
        _inventory = new InventoryData();
    }

    public InventoryData GetSaveData()
    {
        return new InventoryData
        {
            Money = _inventory.Money,
            Items = new Dictionary<string, int>(_inventory.Items)
        };
    }

    public void Load(InventoryData data)
    {
        _inventory = data ?? new InventoryData();
        _inventory.Items ??= new Dictionary<string, int>();
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }
}