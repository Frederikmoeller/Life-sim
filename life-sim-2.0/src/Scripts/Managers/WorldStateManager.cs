using Godot;
using System.Collections.Generic;

public partial class WorldStateManager : Node
{
    public static WorldStateManager Instance { get; private set; }


    private WorldData _world = new();


    public override void _Ready()
    {
        Instance = this;
    }


    public void SetFlag(string key, bool value)
    {
        _world.Flags[key] = value;
    }


    public bool GetFlag(string key)
    {
        return _world.Flags.TryGetValue(
            key,
            out bool value
        ) && value;
    }


    public void SetValue(string key, int value)
    {
        _world.Values[key] = value;
    }


    public int GetValue(string key)
    {
        return _world.Values.TryGetValue(
            key,
            out int value
        ) ? value : 0;
    }

    public void SetString(string key, string value)
    {
        _world.Choices[key] = value;
    }


    public string GetString(string key)
    {
        return _world.Choices.TryGetValue(
            key,
            out string value
        ) ? value : "";
    }


    public WorldData GetSaveData()
    {
        return _world;
    }


    public void Load(WorldData data)
    {
        _world = data ?? new WorldData();
    }
}