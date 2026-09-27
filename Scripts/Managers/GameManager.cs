using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public enum GameModes
    {
        MAIN_MENU,
        GAME_SCENE
    }



    [Header("Current Game Mode")]
    public GameModes gameMode;

    
   




    public void Update()
    {
        //if the current scene is equal to the main menu scene...
        //set the current game mode to be the main menu

        //else if the current scene is equal to the game scene...
        //set the current game mode to be equal to the 
    }
}
