using Godot;
using System.Text.Json;

public partial class SaveManager : Node
{
    public static SaveManager Instance { get; private set; }


    public override void _Ready()
    {
        Instance = this;

        EnsureSaveFolder();
    }


    public void SaveGame(SaveData data, int slot)
    {
        string json = JsonSerializer.Serialize(
            data,
            new JsonSerializerOptions
            {
                WriteIndented = true
            }
        );


        using FileAccess file = FileAccess.Open(
            GetSavePath(slot),
            FileAccess.ModeFlags.Write
        );

        file.StoreString(json);

        GD.Print($"Filed saved to {GetSavePath(slot)}");
    }


    public SaveData LoadSaveData(int slot)
    {
        if(!FileAccess.FileExists(GetSavePath(slot)))
        {
            GD.Print("No save found.");
            return null;
        }


        using FileAccess file = FileAccess.Open(
            GetSavePath(slot),
            FileAccess.ModeFlags.Read
        );


        string json = file.GetAsText();


        return JsonSerializer.Deserialize<SaveData>(json);
    }


    public bool HasSave(int slot)
    {
        return FileAccess.FileExists(GetSavePath(slot));
    }


    public void DeleteSave(int slot)
    {
        if(FileAccess.FileExists(GetSavePath(slot)))
        {
            DirAccess.RemoveAbsolute(GetSavePath(slot));
        }
    }

    private string GetSavePath(int slot)
    {
        return $"user://Saves/save_slot_{slot}.json";
    }

    private void EnsureSaveFolder()
    {
        DirAccess dir = DirAccess.Open("user://");

        if(!dir.DirExists("Saves"))
        {
            dir.MakeDir("Saves");
        }
    }


    public override void _ExitTree()
    {
        if(Instance == this)
            Instance = null;
    }
}