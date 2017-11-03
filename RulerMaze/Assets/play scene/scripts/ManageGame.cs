using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageGame : MonoBehaviour
{
    public Maze maze;

    private bool win = false;
    //各逻辑处理很乱，暂时先这样吧
    private AddGravity addGravityCom;
    private int restBallNum;

	void Start ()
    {
        addGravityCom = GetComponent<AddGravity>();
        restBallNum = addGravityCom.ballNum;
	}

    public void HandleOneBallExitMessage()
    {
        if (restBallNum > 0)
        {
            restBallNum--;
            if (restBallNum == 0)
            {
                win = true;
                //胜利标志
                Time.timeScale = 0.25f;
                Invoke("ResetGame",2.5f);

            }
        }

    }

    void ResetGame()
    {
        Time.timeScale = 1.0f;
        win = false;
        restBallNum = addGravityCom.ballNum;
        maze.ReBuild();
    }

}
