using Godot;
using System;
using System.Collections.Generic;

public partial class ItemDatabase : Node
{
    public static ItemDatabase Instance { get; private set; }

    [Export(PropertyHint.File, "*.csv")]
    public string CsvPath = "res://Data/items.csv";

    private readonly Dictionary<string, ItemDefinition> _items = new();
    private readonly Dictionary<string, Texture2D> _iconCache = new();

    public override void _Ready()
    {
        Instance = this;
        LoadFromCsv(CsvPath);
    }

    public bool LoadFromCsv(string path)
    {
        _items.Clear();
        _iconCache.Clear();

        if (string.IsNullOrWhiteSpace(path) || !FileAccess.FileExists(path))
        {
            GD.PrintErr($"Item database CSV not found: {path}");
            return false;
        }

        using FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        return LoadFromCsvText(file.GetAsText());
    }

    public bool LoadFromCsvText(string csvText)
    {
        _items.Clear();
        _iconCache.Clear();

        List<Dictionary<string, string>> rows = CsvParser.Parse(csvText);

        foreach (Dictionary<string, string> row in rows)
        {
            ItemDefinition item = new ItemDefinition
            {
                Id = Get(row, "id"),
                DisplayName = Get(row, "display_name"),
                Description = Get(row, "description"),
                MaxStack = ParseInt(Get(row, "max_stack"), 99),
                SellValue = ParseInt(Get(row, "sell_value"), 0),
                BuyValue = ParseInt(Get(row, "buy_value"), 0),
                Usable = ParseBool(Get(row, "usable")),
                Consumable = ParseBool(Get(row, "consumable")),
                IconPath = Get(row, "icon")
            };

            if (string.IsNullOrWhiteSpace(item.Id))
                continue;

            if (string.IsNullOrWhiteSpace(item.DisplayName))
                item.DisplayName = item.Id;

            _items[item.Id] = item;
        }

        return _items.Count > 0;
    }

    public bool Exists(string itemId)
    {
        return !string.IsNullOrWhiteSpace(itemId) && _items.ContainsKey(itemId);
    }

    public ItemDefinition Get(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return null;

        return _items.TryGetValue(itemId, out ItemDefinition item) ? item : null;
    }

    public string GetDisplayName(string itemId)
    {
        return Get(itemId)?.DisplayName ?? itemId;
    }

    public int GetMaxStack(string itemId)
    {
        ItemDefinition item = Get(itemId);
        return item != null ? Mathf.Max(1, item.MaxStack) : 99;
    }

    public int GetSellValue(string itemId)
    {
        return Get(itemId)?.SellValue ?? 0;
    }

    public int GetBuyValue(string itemId)
    {
        return Get(itemId)?.BuyValue ?? 0;
    }

    public bool IsUsable(string itemId)
    {
        return Get(itemId)?.Usable ?? false;
    }

    public bool IsConsumable(string itemId)
    {
        return Get(itemId)?.Consumable ?? false;
    }

    public Texture2D GetIcon(string itemId)
    {
        ItemDefinition item = Get(itemId);
        if (item == null || string.IsNullOrWhiteSpace(item.IconPath))
            return null;

        if (_iconCache.TryGetValue(item.IconPath, out Texture2D cached))
            return cached;

        Texture2D icon = GD.Load<Texture2D>(item.IconPath);

        if (icon != null)
            _iconCache[item.IconPath] = icon;
        else
            GD.PrintErr($"Failed to load item icon: {item.IconPath} (item: {itemId})");

        return icon;
    }

    private static string Get(Dictionary<string, string> row, string key)
    {
        return row != null && row.TryGetValue(key, out string value) ? value : "";
    }

    private static int ParseInt(string value, int fallback)
    {
        return int.TryParse(value, out int result) ? result : fallback;
    }

    private static bool ParseBool(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        value = value.Trim().ToLowerInvariant();
        return value is "true" or "1" or "yes" or "y";
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }
}