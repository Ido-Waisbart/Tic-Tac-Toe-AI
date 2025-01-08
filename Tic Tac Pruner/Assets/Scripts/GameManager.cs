// Manages the game's flow, and player and AI choices.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
        WaitingForX,
        WaitingForO,
        NoMoreMoves,
        VictoryX,
        VictoryO
    }

    [SerializeField] protected AILogic aiLogic;
    private EventSystem eventSystem;
    private UIController uiController;

    GameState gameState;
    TileType[,] board;
    public const int BOARD_EDGE_LENGTH = 3;
    int vacantTiles = BOARD_EDGE_LENGTH * BOARD_EDGE_LENGTH;
    TileType lastPerformingPlayerTile;
    Vector2Int lastPerformedMove;

    public enum Difficulty
    {
        Easy = 1,
        Medium,
        Hard,
    }

    private void Start()
    {
        if(Instance != null){
            throw new NullReferenceException();
        }
        Instance = this;

        // ASSUMPTION: Script execution order was set correctly.
        uiController = UIController.Instance;
        eventSystem = EventSystem.current;
        board = new TileType[BOARD_EDGE_LENGTH,BOARD_EDGE_LENGTH];
    }

    public void StartGame(Difficulty difficulty)
    {
        // aiLogic.Depth = (int)difficulty;
        SetToPlayerTurn();
    }

    public void SetToPlayerTurn()
    {
        uiController.SetTurnText(isPlayerTurn: true);
        eventSystem.enabled = true;
        print("Waiting for player choice.");
    }

    public void SetToAITurn()
    {
        uiController.SetTurnText(isPlayerTurn: false);
        eventSystem.enabled = false;
        StartCoroutine(WaitForAIMove());
    }
    
    public void PlayerMadeMove(TilePositionData xy){
        board[xy.x, xy.y] = TileType.X;
        vacantTiles--;
        // UIController.
        SetToAITurn();
    }

    private IEnumerator WaitForAIMove(){
        // Vector2Int move = aiLogic.MakeMove(board, vacantTiles, lastPerformingPlayerTile, lastPerformedMove);
        yield return new WaitForSeconds(1);
        // print("TODO: AI played: " + move.ToString());
        //vacantTiles--;
        SetToPlayerTurn();
    }

    public void OnGameOver()
    {
        uiController.OnGameOver(gameState);
    }
}
