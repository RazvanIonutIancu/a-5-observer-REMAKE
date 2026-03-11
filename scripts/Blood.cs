using Godot;
using System;

public partial class Blood : GpuParticles3D
{

	Player player;



	public override void _Ready()
	{
        player = GetNode<Player>("/root/Map/PlayerNode/Player");


		player.onHurt += EmitBlood;
    }





	private void EmitBlood()
	{
		Vector3 position = player.GlobalPosition;
		position.Y += 1f;
		GlobalPosition = position;

		// This is just a workaround to get the particle to emit multiple times if needed
		Restart();
		Emitting = true;
	}


}