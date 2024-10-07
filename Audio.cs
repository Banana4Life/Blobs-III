using Godot;

namespace LD56;

public partial class Audio : Node
{
    public static Audio Instance { get; private set; }
    
    private AudioStreamPlayer backgroundMusic = new();
    private AudioStream splat;

    public float BackgroundVolumeLinear
    {
        get => Mathf.DbToLinear(backgroundMusic.VolumeDb);
        set => backgroundMusic.VolumeDb = Mathf.LinearToDb(value);
    }
    
    public override void _Ready()
    {
        Instance = this;
        splat = GD.Load<AudioStream>("res://audio/splat.wav");
        StartBackgroundMusic();
    }

    public void StartBackgroundMusic()
    {
        BackgroundVolumeLinear = 0.1f;
        backgroundMusic.Stream = GD.Load<AudioStream>("res://audio/background-music.mp3");
        backgroundMusic.Autoplay = true;
        backgroundMusic.Finished += () => backgroundMusic.Play(); 
        AddChild(backgroundMusic);
    }

    public void StopBackgroundMusic()
    {
        backgroundMusic.Stop();
        RemoveChild(backgroundMusic);
    }

    public void PlayOneShot(AudioStream audioStream, float volumeLinear = 0.1f, Node node = null)
    {
        var player = new AudioStreamPlayer();
        player.Stream = audioStream;
        player.Autoplay = true;
        player.VolumeDb = Mathf.LinearToDb(volumeLinear);
        player.Finished += () => RemoveChild(player); 
        (node ?? this).AddChild(player);
    }

    public void PlayOneShotAt(Vector2 position, AudioStream audioStream, float volumeLinear = 0.1f, Node node = null)
    {
        var player = new AudioStreamPlayer2D();
        player.GlobalPosition = position;
        player.Stream = audioStream;
        player.Autoplay = true;
        player.VolumeDb = Mathf.LinearToDb(volumeLinear);
        player.Finished += () => RemoveChild(player); 
        (node ?? this).AddChild(player);
    }

    public void Splat(float volumeLinear = 0.1f, Node node = null)
    {
        PlayOneShot(splat, volumeLinear, node);
    }

    public void SplatAt(Vector2 position, float volumeLinear = 0.1f, Node node = null)
    {
        PlayOneShotAt(position, splat, volumeLinear, node);
    }
}