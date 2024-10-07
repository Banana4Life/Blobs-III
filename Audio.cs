using Godot;

namespace LD56;

public partial class Audio : Node
{
    public static Audio Instance { get; private set; }
    
    private AudioStreamPlayer backgroundMusic = new();
    private AudioStream splat = GD.Load<AudioStream>("res://audio/splat.wav");
    private AudioStream plop = GD.Load<AudioStream>("res://audio/plop.wav");
    private AudioStream ding = GD.Load<AudioStream>("res://audio/ding.wav");

    public float BackgroundVolumeLinear
    {
        get => Mathf.DbToLinear(backgroundMusic.VolumeDb);
        set => backgroundMusic.VolumeDb = Mathf.LinearToDb(value);
    }

    public Audio()
    {
        Instance = this;
    }

    public override void _Ready()
    {
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

    public void Plop(Node node)
    {
        PlayOneShot(plop, 0.5f, node);
    }
    
    public void Splat(float volumeLinear = 0.1f, Node node = null)
    {
        PlayOneShot(splat, volumeLinear, node);
    }

    public void Ding()
    {
        PlayOneShot(ding, 0.4f);
    }
    public void SplatAt(Vector2 position, float volumeLinear = 0.1f, Node node = null)
    {
        PlayOneShotAt(position, splat, volumeLinear, node);
    }
}