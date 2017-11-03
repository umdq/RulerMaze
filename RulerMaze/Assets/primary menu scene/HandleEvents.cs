using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//handle menu
public class HandleEvents : MonoBehaviour 
{
    private SceneController sceneController;
    public GameObject currentMenu;
    public Text easyTex;
    public Text hardTex;

    void Start () 
    {
        //initialize from persistent scene
        sceneController = FindObjectOfType<SceneController>();
 
        ShowDifficulty(GameData.instance.mazeDifficulty);
    }


    //handle primary menu
    public void OnClickLoadScene(string sceneName)
    {
        sceneController.FadeAndLoadScene(sceneName);
    }

    public void OnClickExit()
    {
        Application.Quit();
    }

    //包括切换到子菜单、返回主菜单等
    public void OnClickChangeMenu(GameObject dstMenu)
    {
        currentMenu.SetActive(false);
        currentMenu = dstMenu;
        currentMenu.SetActive(true);
    }


    //handle option menu
    //public void OnClickChangeDifficulty(MazeDifficulty difficulty)//MazeDifficulty没提供序列化
    public void OnClickChangeDifficulty(int difficultyInt)
    {
        MazeDifficulty difficulty = (MazeDifficulty)difficultyInt;
        if (difficulty != GameData.instance.mazeDifficulty)
        {
            GameData.instance.mazeDifficulty = difficulty;
            ShowDifficulty(difficulty);
        }
    }

    void ShowDifficulty(MazeDifficulty difficulty)
    {
        if (difficulty == MazeDifficulty.EASY)
        {
            easyTex.fontSize = 80;
            easyTex.fontStyle = FontStyle.Bold;
            hardTex.fontSize = 50;
            hardTex.fontStyle = FontStyle.Italic;
        }
        else
        {
            hardTex.fontSize = 80;
            hardTex.fontStyle = FontStyle.Bold;
            easyTex.fontSize = 50;
            easyTex.fontStyle = FontStyle.Italic;
        }
    }

}
