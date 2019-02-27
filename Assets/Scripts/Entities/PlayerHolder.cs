using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerConstraints
{
    private bool readyAndAlive;
    private MenuPlayer menuPlayer;

    public PlayerConstraints(MenuPlayer menuPlayer)
    {
        this.readyAndAlive = false;
        this.menuPlayer = menuPlayer;
    }

    public void SetReadyAndAlive(bool ready)
    {
        this.readyAndAlive = ready;
        this.menuPlayer.SetReady(ready);
    }

    public MenuPlayer MenuPlayer { get => menuPlayer; }
    public bool ReadyAndAlive { get => readyAndAlive; set => readyAndAlive = value; }
}
