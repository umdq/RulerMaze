using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HandleUI : MonoBehaviour {
    public Text modeText;
    public new Camera camera;
    public readonly float orthographicSize=4.2f;
    public Maze maze;
    public Slider slider;

    private bool view3D=false;
    private SceneController sceneController;
    private AddGravity addGravity;
    private LineRenderer mazePath;

	void Start ()
    {
        modeText.text="3D mode";
        //from persistent scene
        sceneController = FindObjectOfType<SceneController>();
        addGravity = GetComponent<AddGravity>();
        mazePath = maze.GetComponent<LineRenderer>();
	}
	
	void Update ()
    {
		
	}

    public void HandleSwitchViewMode()
    {
        if (view3D)
        {
            view3D = false;
            camera.orthographic = true;
            camera.orthographicSize = orthographicSize;
            modeText.text="3D mode";
        }
        else
        {
            view3D = true;
            camera.orthographic = false;
            modeText.text="2D mode";
        }
    }

    public void HandleRebuildMaze()
    {
        //reload scene，but keep UI components status.As a result,it seems that only the Maze are rebound
        //——》哈哈，覆盖重载没法做到保留原场景信息，除非序列化
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        maze.ReBuild();

        addGravity.ResetBalls();
    }

    public void HandleReturnToMenu()//可以拓为switch to scene
    {
        sceneController.FadeAndLoadScene("primary menu");
    }

    public void HandleShowPathDown()
    {
        mazePath.enabled = true;//GO才有active
    }

    public void HandleShowPathUp()
    {
        mazePath.enabled = false;
    }


    public void HandleSliderChange()
    {
        addGravity.gScale=1.0f+slider.value;
    }
}
