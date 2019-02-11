using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerHolder
{
    private bool ready;
    private Player player;
    private MenuPlayer menuPlayer;

    public PlayerHolder(Player player, MenuPlayer menuPlayer)
    {
        this.ready = false;
        this.player = player;
        this.menuPlayer = menuPlayer;
    }

    public Player Player { get => player; }
    public MenuPlayer MenuPlayer { get => menuPlayer; }
    public bool Ready { get => ready; set => ready = value; }
}
