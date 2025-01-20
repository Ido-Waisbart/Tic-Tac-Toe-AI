using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameManager;

public class AILogic : MonoBehaviour
{
  // const int BUILT_IN_DEFAULT_DEPTH = 3;  // For the sake of using this depth as a defaul
  [SerializeField] protected int _depth = 3;  // Practically, it's the difficulty. Changable.
  public int Depth { set { _depth = value; } }
  public bool IsPlayerX { private get; set; }

  void Start(){
    //TestAlgorithm();  // Uncomment to run tests on Start().
  }

  //  Test several board layouts.
  //  Note: These boards are displayed differently, as if the x and y were swapped.
  void TestAlgorithm(){
    TileType[,] testBoard_trivial = new TileType[,]{
      {TileType.AI, TileType.Player, TileType.AI,},
      {TileType.Player, TileType.Empty, TileType.Player,},
      {TileType.Player, TileType.AI, TileType.Player,},
    };
    print(MakeMove(testBoard_trivial, 1, TileType.Player, new Vector2Int(0, 1)));
    // Should choose [1,1]. Other options should be impossible to choose, as they're not empty.

    
    
    TileType[,] testBoard_megaSimple = new TileType[,]{
      {TileType.AI, TileType.Player, TileType.AI,},
      {TileType.Player, TileType.Empty, TileType.Player,},
      {TileType.Player, TileType.AI, TileType.Empty,},
    };
    print(MakeMove(testBoard_megaSimple, 2, TileType.Player, new Vector2Int(0, 1)));
    // Should choose [1,1]. The only other option, [2,2], would cause a loss for the AI.

    
    
    TileType[,] testBoard_abuRass = new TileType[,]{
      {TileType.AI, TileType.Player, TileType.AI,},
      {TileType.Player, TileType.Empty, TileType.Player,},
      {TileType.Empty, TileType.AI, TileType.Empty,},
    };
    print(MakeMove(testBoard_abuRass, 3, TileType.Player, new Vector2Int(0, 1)));
    // Should choose [1,1]. Both of the other options, [2,0] and [2,2], would cause a loss for the AI.

    

    TileType[,] testBoard_myPersonalOtherPain = new TileType[,]{
      {TileType.Player, TileType.Empty, TileType.Empty,},
      {TileType.Empty, TileType.Empty, TileType.AI,},
      {TileType.Player, TileType.Empty, TileType.Empty,},
    };
    print(MakeMove(testBoard_myPersonalOtherPain, 6, TileType.Player, new Vector2Int(2, 0), 2));
    print(MakeMove(testBoard_myPersonalOtherPain, 6, TileType.Player, new Vector2Int(2, 0), (int)GameManager.Difficulty.Hard));  // If there's a difference, I've coded it wrong.
    // Should choose [0,1]. All of the other options, would cause a loss for the AI, as the player would choose [0,1].
    // Depth required: 2
    // Only a large enough depth would have the AI realize it's a lost game for it, prompting it to choose randomly.



    TileType[,] testBoard_myPersonalPain = new TileType[,]{
      {TileType.Player, TileType.AI, TileType.Player,},
      {TileType.Empty, TileType.Empty, TileType.Empty,},
      {TileType.Empty, TileType.Empty, TileType.Empty,},
    };
    print(MakeMove(testBoard_myPersonalPain, 6, TileType.Player, new Vector2Int(0, 2), 4));
    // Should choose [1,1]. All of the other options, would cause a loss for the AI, as the player would choose [1,1].
    // Depth required: 4
    
    TileType[,] testBoard_quickLoss = new TileType[,]{
      {TileType.Player, TileType.Empty, TileType.Empty,},
      {TileType.Empty, TileType.Empty, TileType.Empty,},
      {TileType.AI, TileType.Empty, TileType.Empty,},
    };
    print(MakeMove(testBoard_myPersonalPain, 7, TileType.Player, new Vector2Int(0, 2), (int)GameManager.Difficulty.Hard));
    // Should give up.
    // Depth required: Uncalculated as of now. 6 must succeed.
  }

  public Vector2Int MakeMove(TileType[,] board, int vacantTiles, TileType lastPerformingPlayerTile, Vector2Int lastPerformedMove, int? optionalDepth = null)
  {
    List<Vector2Int> topMoves;
    TileType[,] boardCopy = board.Clone() as TileType[,];  // "it really copies the values, not a shallow copy," - https://stackoverflow.com/questions/15725840/copy-one-2d-array-to-another-2d-array
    int bestOptionScore = MinMaxing(boardCopy, vacantTiles, int.MinValue, int.MaxValue, true, lastPerformingPlayerTile, lastPerformedMove,
      optionalDepth == null ? _depth : (int)optionalDepth, true, out topMoves, out _);
    
    // A print of the currently considered moves.
    // The chosen move will be a random one of these.
    // For debugging purposes.
    print("Potential best moves (score: " + bestOptionScore.ToString() + "): " + Vector2IntListToString(topMoves));
    switch(bestOptionScore)
    {
      case 1:
        print("This game is a sure win for the AI.");
        break;
      case -1:
        print("This game is a lost cause for the AI.");
        break;
    }
    
    return topMoves[UnityEngine.Random.Range(0, topMoves.Count)];
  }

  // TODO: What comes first, out, or a parameter with an argument?
  // TODO: Pass over a copy of the board, for MinMaxing to freely alter without consequence.
  public int MinMaxing(TileType[,] boardCopy, int vacantTiles, int alpha, int beta, bool isAIsSimulatedTurn, TileType lastPerformingPlayerTile, Vector2Int lastPerformedMove, int depth, bool isFirstCall, out List<Vector2Int>? topMoves, out bool wasPruned)
  {
    int row, col;
    int eval;

    bool isGameOver = vacantTiles == 0;
    GameState gameWinnerState = CheckForImaginaryGameOver(boardCopy, vacantTiles, lastPerformingPlayerTile, lastPerformedMove);
    bool isGameStateOnGameOver = (gameWinnerState != GameState.WaitingForAI && gameWinnerState != GameState.WaitingForPlayer);
    
    wasPruned = false;
    topMoves = new List<Vector2Int>();  // TODO: Temp value to remove error.

    if (isGameStateOnGameOver || depth == 0)
    {
      if (!isGameStateOnGameOver)
      {
        // Game's not over, but depth is 0. No one is in the lead.
        return 0;
      }
      switch (gameWinnerState)
      {
        case GameState.VictoryPlayer:
          return -1;
        case GameState.VictoryAI:
          return 1;
        case GameState.NoMoreMoves:
          return 0;
        default:
          throw new ArgumentException("Improper gameState in MinMaxing(): " + gameWinnerState);
      }
    }




    int bestEval = isAIsSimulatedTurn ? int.MinValue : int.MaxValue;  // Maximizing? Start from minus infinity and get bigger.
    for (row = 0; row < BOARD_EDGE_LENGTH; row++)
    {
      for (col = 0; col < BOARD_EDGE_LENGTH; col++)
      {
        // Simulate the placement of a mark on the tile [row, col].

        if (boardCopy[row, col] != TileType.Empty) continue;  // Skip taken tiles.

        TileType nextPerformingPlayerTile = lastPerformingPlayerTile == TileType.Player ? TileType.AI : TileType.Player;
        Vector2Int nextPerformedMove = new Vector2Int(row, col);
        List<Vector2Int> newBestMoves = new List<Vector2Int>();
        boardCopy[row, col] = nextPerformingPlayerTile;
        bool wasNextPruned;
        eval = MinMaxing(boardCopy, vacantTiles - 1, alpha, beta, !isAIsSimulatedTurn, nextPerformingPlayerTile, nextPerformedMove, depth - 1, false, out newBestMoves, out wasNextPruned);
        boardCopy[row, col] = TileType.Empty;
        
        if(isAIsSimulatedTurn)
        {
          if (eval > bestEval)
          {
            bestEval = eval;

            // The best options for the root call are stored, for one of them to be chosen randomly.
            if (isFirstCall)
            {
              topMoves.Clear();
              topMoves.Add(nextPerformedMove);
            }
          }
          else if (eval >= bestEval && isFirstCall && !wasNextPruned)
          {
            // This move is the as strong as the current strongest move. It might be chosen, randomly.
            // If it was pruned, then this move was chosen to be ignored.
            topMoves.Add(nextPerformedMove);
          }
          alpha = Mathf.Max(alpha, eval);
        }
        else
        {
          bestEval = Math.Min(bestEval, eval);
          beta = Math.Min(beta, eval);
        }
        
        if (beta <= alpha)  // Is the minimal and best option for the player, WORSE, than the maximal and best option for the AI? SKIP.
        {
          wasPruned = true;
          return bestEval;  // Equivalent to breaking out of both the loops.
        }
      }
    }

    return bestEval;
  }

  GameState CheckForImaginaryGameOver(TileType[,] board, int vacantTiles, TileType lastPerformingPlayerTile, Vector2Int lastPerformedMove)
  {
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
        return lastPerformingPlayerTile == TileType.Player ? GameState.VictoryPlayer : GameState.VictoryAI;
      }
    }

    if (vacantTiles == 0)
    {
      return GameState.NoMoreMoves;
    }
    return lastPerformingPlayerTile == TileType.Player ?
        GameState.WaitingForPlayer : GameState.WaitingForAI;
  }

  private string Vector2IntListToString(List<Vector2Int> vectors){
    string stringified = "";
    for(int i = 0; i < vectors.Count; i++){
      Vector2Int topMove = vectors[i];
      stringified += "[" + i + "] ("
      + topMove.x.ToString() + ", " + topMove.y.ToString() + ")\n";
    }
    return stringified;
  }
}
