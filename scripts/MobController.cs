using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class MobController : Node
{

    private PackedScene mobScene;

    [Export]
    private int maxNumOfMobs = 5;

    private Mob[] mobPool;
    private Queue<int> availableIndices;






    public override void _Ready()
    {
        mobScene = GD.Load<PackedScene>("res://scenes/mob.tscn");


        // QUEUE
        mobPool = new Mob[maxNumOfMobs];
        availableIndices = new Queue<int>();

        for (int i = 0; i < maxNumOfMobs; i++)
        {
            Mob mobInstance = mobScene.Instantiate<Mob>();
            AddChild(mobInstance);
            mobInstance.OnMobDeath += HandleMobDeath;
            mobInstance.mobPoolIndex = i;

            mobPool[i] = mobInstance;
            availableIndices.Enqueue(i);


            GD.Print("Spawned mob at " + mobPool[i].GetIndex().ToString() + "Queued index: " + availableIndices.Count.ToString());

        }

        while (availableIndices.Count > 0)
        {
            RespawnMob();
        }


    }





    // *********************
    //
    //  METHODS
    //
    // *********************

    private void HandleMobDeath(Mob deadMob)
    {
        deadMob.Visible = false;
        deadMob.SetPhysicsProcess(false);
        deadMob.SetProcess(false);
        deadMob.OnMobDeath -= HandleMobDeath;
        ReturnToQueue(deadMob.mobPoolIndex);

        RespawnMob();
    }


    private void RespawnMob()
    {

        Mob newMob = GetAvailableMob();

        newMob.SpawnMob();
        newMob.Visible = true;
        newMob.SetPhysicsProcess(true);
        newMob.SetProcess(true);
        newMob.OnMobDeath += HandleMobDeath;
    }




    // *********************
    //
    //  QUEUE HANDLERS
    //
    // *********************

    public Mob GetAvailableMob()
    {
        int index = availableIndices.Dequeue();
        return mobPool[index];
    }

    public void ReturnToQueue(int index)
    {
        availableIndices.Enqueue(index);
    }


}
