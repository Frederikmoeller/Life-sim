using Godot;

public sealed class ItemDefinition
{
    public string Id { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Description { get; set; } = "";
    public int MaxStack { get; set; } = 99;
    public int SellValue { get; set; } = 0;
    public int BuyValue { get; set; } = 0;
    public bool Usable { get; set; } = false;
    public bool Consumable { get; set; } = false;
    public string IconPath { get; set; } = "";
}