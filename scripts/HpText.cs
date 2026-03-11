using Godot;
using System;

public partial class HpText : Label3D
{
    Player player;

    int hpToShow;
    string deathMessage = "Dead";


    public override void _Ready()
    {
        player = GetParent<Player>();
        player.onHpChange += Update;
    }




    private void Update()
    {
        hpToShow = player.currentHP;
        if (hpToShow > 0)
        {
            Text = hpToShow.ToString();
        }
        else
        {
            Text = deathMessage;
        }
    }



}
