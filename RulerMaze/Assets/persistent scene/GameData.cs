using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MazeDifficulty//note it's not complexity
{
    EASY=0,
    HARD=1,
};

//做成black board会不会更好…
public class GameData : MonoBehaviour{
    //singleton pattern in Uninty3D
    public static GameData instance=null;
    void Awake()
    {
        //for singleton
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy (gameObject);
    }

    //data:
    public MazeDifficulty mazeDifficulty=MazeDifficulty.EASY;

}


