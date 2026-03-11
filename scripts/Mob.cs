using Godot;
using System;
using System.Collections.Generic;
using System.Security;

public partial class Mob : Node3D
{
    // Delegates and observer types
    public delegate void MobEvent(Mob mob);
    public delegate void MobFlavourEvent();


    public MobFlavourEvent OnIdle;
    public MobFlavourEvent OnChase;
    public MobFlavourEvent OnWait;
    public MobEvent OnMobDeath;
    public MobFlavourEvent OnMobFlavourDeath;

    public int mobPoolIndex;



    [Export]
    private Timer chaseTimer;

    [Export]
    private Timer deathTimer;

    private Player player;
    private AudioPlayer audio;



    // NPC states
    enum States
    {
        IDLE,
        CHASE,
        WAIT,
        DEAD
    }

    private States currentState;

    private bool playerIsInChaseArea;
    private bool isTouchingPlayer;

    private float movementSpeed = 2.0f;

    public override void _Ready()
    {
        player = GetNode<Player>("/root/Map/PlayerNode/Player");
        audio = GetNode<AudioPlayer>("/root/Map/AudioController");
    }

    // ***************************
    //
    //  SPAWN LOGIC
    //
    // ***************************
    public void SpawnMob()
    {
        float minSpawnDistance = 5f;
        float maxSpawnDistance = 10f;

        float distanceFromPlayer = (float)GD.RandRange(minSpawnDistance, maxSpawnDistance);
        Vector3 spawnPosition;

        // Now to check is spawnpoint is empty

        while (true)
        {
            GD.Print("Trying to find position");
            Vector3 directionFromPlayer = new Vector3((float)GD.RandRange(-1, 1), 0f, (float)GD.RandRange(-1, 1)).Normalized();

            spawnPosition = player.GlobalPosition + distanceFromPlayer * directionFromPlayer;
            spawnPosition.Y = 1f;

            if (CheckCollisionAtPoint(spawnPosition))
            {
                GD.Print("Found position!");
                break;
            }
        }


        GlobalPosition = spawnPosition;


        playerIsInChaseArea = false;
        isTouchingPlayer = false;
        currentState = States.IDLE;
        if (OnIdle != null)
        {
            OnIdle();
        }



        GetNode<MeshInstance3D>("CharacterBody3D/BodyDead").Visible = false;
        GetNode<MeshInstance3D>("CharacterBody3D/Body").Visible = true;
    }

    private bool CheckCollisionAtPoint(Vector3 point)
    {

        CollisionShape3D bodyCollisionShape = GetNode<CollisionShape3D>("../Mob/CharacterBody3D/Body/Area3D/CollisionShape3D");
        Shape3D bodyShape = bodyCollisionShape.Shape;
        Transform3D bodyTransform = bodyCollisionShape.Transform;

        // Create query with the properties above
        PhysicsShapeQueryParameters3D bodyQuery = new PhysicsShapeQueryParameters3D();
        bodyQuery.Shape = bodyShape;
        bodyQuery.Transform = new Transform3D(bodyCollisionShape.GlobalTransform.Basis, point);
        bodyQuery.CollideWithBodies = true;
        bodyQuery.CollideWithAreas = true;

        // Check for collision
        PhysicsDirectSpaceState3D currentSpaceState = GetWorld3D().DirectSpaceState;
        Godot.Collections.Array<Godot.Collections.Dictionary> collisions = currentSpaceState.IntersectShape(bodyQuery, maxResults: 1);

        return collisions.Count == 0;
    }





    // Process

    public override void _PhysicsProcess(double delta)
    {
        

        // Calls appropriate functions based on state
        switch(currentState)
        {
            case States.IDLE:
                // potential idle logic? maybe?
                break;
            case States.CHASE:
                if(isTouchingPlayer)
                {
                    SwitchState(States.WAIT);
                }
                else
                {
                    ChasePlayer(delta);
                }
                break;
            case States.WAIT:
                if (chaseTimer.IsStopped())
                {
                    if (playerIsInChaseArea)
                    {
                        SwitchState(States.CHASE);
                    }
                    else
                    {
                        SwitchState(States.IDLE);
                    }
                }
                break;
            case States.DEAD:
                if (deathTimer.IsStopped())
                {
                    if (OnMobDeath != null)
                    {
                        OnMobDeath(this);
                    }
                }
                break;

        }


    }




    private void SwitchState(States newState)
    {

        switch(newState)
        {
            case States.IDLE:
                currentState = newState;
                if (OnIdle != null)
                {
                    OnIdle();
                }
                break;
            case States.WAIT:
                if (currentState != newState)
                {
                    currentState = newState;

                    if (OnWait != null)
                    {
                        OnWait();
                    }

                    player.onHurt();
                    chaseTimer.Start();
                    GD.Print("Player touched!");
                }
                break;
            case States.CHASE:
                currentState = newState;

                if (OnChase != null)
                {
                    OnChase();
                }
                break;
            case States.DEAD:
                currentState = newState;
                GetNode<MeshInstance3D>("CharacterBody3D/BodyDead").Visible = true;
                GetNode<MeshInstance3D>("CharacterBody3D/Body").Visible = false;

                OnMobFlavourDeath();
                deathTimer.Start();
                break;
        }
    }




    // **************
    //
    //  MOVEMENT
    // I HATE GODOT
    //
    // **************


    private void ChasePlayer(double delta)
    {
        GD.Print("Chasing player at:" + player.GlobalPosition.ToString());

        if (player == null)
        {
            return;
        }

        Vector3 direction = player.GlobalPosition - GlobalPosition;
        direction.Y = 0;
        direction = direction.Normalized();

        GlobalPosition += direction * movementSpeed * (float)delta;


    }










    // **************
    //
    //  On actions
    //
    // **************

    // Aggro area
    public void OnPlayerEnterArea(Node3D body)
    {
        if (body == player && currentState != States.DEAD)
        {
            GD.Print("Player entered follow area!");
            SwitchState(States.CHASE);
            playerIsInChaseArea = true;
        }
    }

    public void OnPlayerExitArea(Node3D body)
    {
        if (body == player && currentState != States.DEAD)
        {
            GD.Print("Player exited follow area!");
            SwitchState(States.IDLE);
            playerIsInChaseArea = false;
        }
    }


    // Hits
    public void OnPlayerHit(Node3D body)
    {
        if (body == player && currentState != States.DEAD)
        {
            isTouchingPlayer = true;
            SwitchState(States.WAIT);
        }
    }

    // This is to avoid endless chasing if the player remains in touch with the mob
    public void OnPlayerExitAfterHit(Node3D body)
    {
        if (body == player && currentState != States.DEAD)
        {
            GD.Print("Not touching player");
            isTouchingPlayer = false;
        }
    }

    public void OnPlayerJumpOnHead(Node3D body)
    {
        if (body == player && currentState != States.DEAD)
        {
            audio.onMobKill();
            GD.Print("Player jumped on head!");
            SwitchState(States.DEAD);
        }
    }

}
