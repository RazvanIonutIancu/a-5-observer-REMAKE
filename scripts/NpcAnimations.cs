using Godot;
using System;

public partial class NpcAnimations : AnimationPlayer
{
	Mob mob;



    public override void _Ready()
    {
        mob = GetParent<Mob>();
        mob.OnIdle += PlayIdle;
        mob.OnChase += PlayMoving;
        mob.OnWait += PlayIdle;
        mob.OnMobFlavourDeath += PlayDead;
    }



    private void PlayIdle()
    {
        Play("Idle");
    }

    private void PlayMoving()
    {
        Play("Moving");
    }

    private void PlayDead()
    {
        Play("Dead");
    }


}
