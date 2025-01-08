// Manages the game's flow, and player and AI choices.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum TileType
    {
        Empty,
        X,
        O
    }

    public enum GameState
    {
        Ongoing,
        NoMoreMoves,
        VictoryX,
        VictoryO
    }

    private UIController uiController;

    TileType[,] board;
    public const int BOARD_EDGE_LENGTH = 3;
    int vacantTiles = BOARD_EDGE_LENGTH * BOARD_EDGE_LENGTH;

    public enum Difficulty
    {
        Easy = 1,
        Medium,
        Hard,
    }

    private void Start()
    {
        if(Instance is not null){
            throw new NullReferenceException();
        }
        Instance = this;

        // ASSUMPTION: Script execution order was set correctly.
        uiController = UIController.Instance;
    }

    public void StartGame(Difficulty difficulty)
    {
        aiLogic.Depth = (int)difficulty;
        SetToPlayerTurn();
    }

    public void SetToPlayerTurn()
    {
        uiController.SetTurnText(isPlayerTurn: true);
    }

    public void SetToAITurn()
    {
        uiController.SetTurnText(isPlayerTurn: false);
    }

    public void OnGameOver(GameState gameState)
    {
        uiController.OnGameOver(gameState);
    }
}
