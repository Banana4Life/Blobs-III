using Godot;

namespace LD56;

public partial class Audio : Node
{
    private AudioStreamPlayer backgroundMusic = new();

    public float BackgroundVolumeLinear
    {
        get => Mathf.DbToLinear(backgroundMusic.VolumeDb);
        set => backgroundMusic.VolumeDb = Mathf.LinearToDb(value);
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
}