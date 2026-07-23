using Godot;

public partial class AudioManager : Node
{
    public static AudioManager Instance { get; private set; }

    private const string MASTER_BUS = "Master";
    private const string MUSIC_BUS = "Music";
    private const string SFX_BUS = "SFX";

    private AudioStreamPlayer _musicPlayer;

    private float _masterVolume = 1f;
    private float _musicVolume = 1f;
    private float _sfxVolume = 1f;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;

        EnsureMusicPlayer();
        ApplyVolumes();
    }

    public void PlayMusic(AudioStream stream, bool restartIfSame = false)
    {
        EnsureMusicPlayer();

        if (_musicPlayer.Stream == stream && _musicPlayer.Playing && !restartIfSame)
            return;

        _musicPlayer.Stream = stream;
        _musicPlayer.Play();
    }

    public void StopMusic()
    {
        if (_musicPlayer != null)
            _musicPlayer.Stop();
    }

    public void SetMasterVolume(float value)
    {
        _masterVolume = Mathf.Clamp(value, 0f, 1f);
        ApplyBusVolume(MASTER_BUS, _masterVolume);
    }

    public void SetMusicVolume(float value)
    {
        _musicVolume = Mathf.Clamp(value, 0f, 1f);
        ApplyBusVolume(MUSIC_BUS, _musicVolume);
    }

    public void SetSfxVolume(float value)
    {
        _sfxVolume = Mathf.Clamp(value, 0f, 1f);
        ApplyBusVolume(SFX_BUS, _sfxVolume);
    }

    public float GetMasterVolume() => _masterVolume;
    public float GetMusicVolume() => _musicVolume;
    public float GetSfxVolume() => _sfxVolume;

    private void EnsureMusicPlayer()
    {
        if (_musicPlayer != null)
            return;

        _musicPlayer = new AudioStreamPlayer
        {
            Bus = MUSIC_BUS,
            ProcessMode = ProcessModeEnum.Always
        };

        AddChild(_musicPlayer);
    }

    private void ApplyVolumes()
    {
        ApplyBusVolume(MASTER_BUS, _masterVolume);
        ApplyBusVolume(MUSIC_BUS, _musicVolume);
        ApplyBusVolume(SFX_BUS, _sfxVolume);
    }

    private void ApplyBusVolume(string busName, float linear)
    {
        int busIndex = AudioServer.GetBusIndex(busName);
        if (busIndex == -1)
            return;

        AudioServer.SetBusVolumeDb(busIndex, LinearToDb(linear));
    }

    private float LinearToDb(float value)
    {
        return value <= 0.0001f ? -80f : Mathf.LinearToDb(value);
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }
}