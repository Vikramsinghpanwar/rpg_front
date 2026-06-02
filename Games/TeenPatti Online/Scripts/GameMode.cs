using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class GameMode
{
    [System.Serializable]
    public enum Modes
    {
        privateGame,
        publicGame,
        practiceGame,
    }

    public static Modes mode { get; set; }
    public static Modes mode_ludo { get; set; }

    public static int tableEntryFee;

}
