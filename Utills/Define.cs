using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define 
{
    public enum Scene
    {
        Unknown,
        Login,
        Lobby,
        Inventory,
        Game,
        MultiGame,

    }

    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount,
    }

    public enum UIEvent
    {
        Click,
        BeginDrag,
        Drag,
        EndDrag,
        Drop,
      
            
    }

    public enum MouseEvent
    {
        Press,
        Click
    }
    public enum CameraMode
    {
        QuarterView,
    }

    public enum TurnMode
    {
        Random,
        My,
        Other
    }
}
