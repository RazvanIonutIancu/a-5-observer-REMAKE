using Godot;
using System;

public partial class AudioPlayer : Node
{
    Player player;

    public delegate void MobAudio();

    public MobAudio onMobKill;



	public override void _Ready()
	{
        player = GetNode<Player>("/root/Map/PlayerNode/Player");

        player.onHurt += PlayHurt;
        player.onHurt += PlayHit;
        player.onJump += PlayJump;

        onMobKill += KillMob;
    }








	private void PlayHurt()
	{
		GetNode<AudioStreamPlayer>("PlayerHurtSound").Play();
	}

    private void PlayJump()
    {
        GetNode<AudioStreamPlayer>("PlayerJumpSound").Play();
    }

    private void PlayHit()
    {
        GetNode<AudioStreamPlayer>("PlayerHitSound").Play();
    }

    private void KillMob()
    {
        GetNode<AudioStreamPlayer>("ClickSound").Play();
    }

}
