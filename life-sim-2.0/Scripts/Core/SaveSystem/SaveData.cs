using Godot;
using System.Collections.Generic;

public class SaveData
{
    public string Version { get; set; } = "0.1b";
    public SaveMetadata Metadata { get; set; } = new();
    public string ScenePath { get; set; } = "";
    public PlayerData Player { get; set; } = new();
    public WorldData World { get; set; } = new();
    public InventoryData Inventory { get; set; } = new();
}

public class PlayerData
{
    public float PositionX { get; set; }
    public float PositionY { get; set; }
}

public class SaveMetadata
{
    public string SaveName { get; set; } = "New Save";

    public long CreatedAt { get; set; }

    public long LastPlayed { get; set; }
}

public class WorldData
{
    public Dictionary<string, bool> Flags { get; set; } = new();

    public Dictionary<string, int> Values { get; set; } = new();

    public Dictionary<string, float> Amounts { get; set; } = new();

    public Dictionary<string, string> Choices { get; set; } = new();
}

public class InventoryData
{
    public int Money { get; set; }
    public Dictionary<string, int> Items { get; set; } = new();
}