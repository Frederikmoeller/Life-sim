using Godot;

public class SaveData
{
    public string Version { get; set; } = "0.1b";
    public SaveMetadata Metadata { get; set; } = new();
    public string ScenePath { get; set; } = "";
    public PlayerData Player { get; set; } = new();
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