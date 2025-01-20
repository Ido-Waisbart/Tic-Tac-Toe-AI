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
        Player,
        AI
    }

    public enum GameState
    {
        WaitingForPlayer,
        WaitingForAI,
        NoMoreMoves,
        VictoryPlayer,
        VictoryAI
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
    bool isPlayerX;  // Randomized in each round.

    public enum Difficulty
    {
        // 1 is practically making random choices.
        Easy = 2,
        Medium = 4,
        Hard = 6,
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
        isPlayerX = UnityEngine.Random.Range(0, 2) == 0;
        aiLogic.Depth = (int)difficulty;
        aiLogic.IsPlayerX = isPlayerX;
        board = new TileType[BOARD_EDGE_LENGTH, BOARD_EDGE_LENGTH];
        vacantTiles = BOARD_EDGE_LENGTH * BOARD_EDGE_LENGTH;
        uiController.SetIsPlayerX(isPlayerX);
        uiController.ClearBoard();
        SetToPlayerTurn();
    }

    public GameState GetGameState(){
        return gameState;
    }

    public void SetToPlayerTurn()
    {
        uiController.SetTurnText(isPlayerTurn: true);
        eventSystem.enabled = true;
        gameState = GameState.WaitingForPlayer;
    }

    public void SetToAITurn()
    {
        uiController.SetTurnText(isPlayerTurn: false);
        eventSystem.enabled = false;
        gameState = GameState.WaitingForAI;
        StartCoroutine(WaitForAIMove());
    }

    void CheckForGameOver(TileType lastPerformingPlayerTile, Vector2Int lastPerformedMove){
        List<Vector2Int[]> linesToCheck = new List<Vector2Int[]>();

        bool isMoveOnFirstDiagonal = lastPerformedMove.x == lastPerformedMove.y;
        bool isMoveOnSecondDiagonal = lastPerformedMove.x == BOARD_EDGE_LENGTH - lastPerformedMove.y - 1;
        if (isMoveOnFirstDiagonal)
            linesToCheck.Add(new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 1), new Vector2Int(2, 2) });
        if (isMoveOnSecondDiagonal)
            linesToCheck.Add(new Vector2Int[] { new Vector2Int(0, 2), new Vector2Int(1, 1), new Vector2Int(2, 0) });

        linesToCheck.Add(new Vector2Int[] { new Vector2Int(0, lastPerformedMove.y), new Vector2Int(1, lastPerformedMove.y), new Vector2Int(2, lastPerformedMove.y) });

        linesToCheck.Add(new Vector2Int[] { new Vector2Int(lastPerformedMove.x, 0), new Vector2Int(lastPerformedMove.x, 1), new Vector2Int(lastPerformedMove.x, 2) });

        bool isAWinningLine;
        foreach (var line in linesToCheck)
        {
            isAWinningLine = true;
            foreach (var tile in line)
            {
                if (board[tile.x, tile.y] != lastPerformingPlayerTile)
                {
                    isAWinningLine = false;
                    break;
                }
            }
            if (isAWinningLine)
            {
                gameState = lastPerformingPlayerTile == TileType.Player ? GameState.VictoryPlayer : GameState.VictoryAI;
                return;
            }
        }

        if(vacantTiles == 0) {
            gameState = GameState.NoMoreMoves;
            return;
        }
        gameState = lastPerformingPlayerTile == TileType.Player ?
            GameState.WaitingForPlayer : GameState.WaitingForAI;
    }
    
    public void PlayerMadeMove(TilePositionData xy){
        board[xy.x, xy.y] = TileType.Player;
        lastPerformedMove = new Vector2Int(xy.x, xy.y);
        vacantTiles--;
        CheckForGameOver(TileType.Player, new Vector2Int(xy.x, xy.y));  // Updates gameState.
        switch (gameState)
        {
            case GameState.NoMoreMoves:
            case GameState.VictoryAI:
            case GameState.VictoryPlayer:
                OnGameOver();
                break;
            case GameState.WaitingForPlayer:
                SetToAITurn();
                break;
            default:
                throw new Exception("Unhandlable game state: " + gameState.ToString());
        }
    }

    private IEnumerator WaitForAIMove(){
        Vector2Int move = aiLogic.MakeMove(board, vacantTiles, TileType.Player, lastPerformedMove);
        bool isDebug = true;  // Temp measure.
        if(isDebug) print(move);
        yield return new WaitForSeconds(1);
        uiController.OnAIChoseTile(move);
        board[move.x, move.y] = TileType.AI;
        vacantTiles--;
        CheckForGameOver(TileType.AI, move);  // Updates gameState.
        switch (gameState)
        {
            case GameState.NoMoreMoves:
            case GameState.VictoryAI:
            case GameState.VictoryPlayer:
                eventSystem.enabled = true;
                OnGameOver();
                break;
            case GameState.WaitingForAI:
                SetToPlayerTurn();
                break;
            default:
                throw new Exception("Unhandlable game state: " + gameState.ToString());
        }
    }

    public void OnGameOver()
    {
        uiController.OnGameOver(gameState);
    }
}
