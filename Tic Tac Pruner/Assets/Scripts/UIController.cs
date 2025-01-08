using System;
using TMPro;
using UnityEngine;
using static GameManager;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    [SerializeField] GameObject _gameStartPanel;
    [SerializeField] GameObject _gameOverPanel;
    private GameObject _currentGameMenuPanel;
    [SerializeField] TMP_Text _turnText;
    [SerializeField] TMP_Text _winText;

    public const string PLAYER_TURN_STRING = "Player turn!";
    public const string AI_TURN_STRING = "AI turn!";
    public const string WIN_STRING = "You won!";
    public const string LOSS_STRING = "You lost!";
    public const string TIE_STRING = "Tie!";
    
    void Start()
    {
        if(Instance is not null){
            throw new NullReferenceException();
        }
        Instance = this;

        _currentGameMenuPanel = _gameStartPanel;
    }

    public void SetTurnText(bool isPlayerTurn){
        _turnText.text = isPlayerTurn ? PLAYER_TURN_STRING : AI_TURN_STRING;
    }

    public void OnGameOver(GameState gameState){
        SetWinText(gameState);
        _currentGameMenuPanel = _gameOverPanel;
        _currentGameMenuPanel.SetActive(true);
    }

    void SetWinText(GameState gameState){
        switch(gameState){
            case GameState.NoMoreMoves:
                _winText.text = TIE_STRING;
                break;
            case GameState.VictoryX:
                _winText.text = TIE_STRING;
                break;
            case GameState.VictoryO:
                _winText.text = TIE_STRING;
                break;
            default:
                throw new ArgumentException("Unexpected game state on game over: " + gameState.ToString());
        }
    }

    // Enums are not displayed natively on button event parameters in the inspector.
    // public void OnBtn_PlayAgainstAI(GameManager.Difficulty difficulty){}

    public void OnBtn_PlayAgainstAI(int difficulty){
        _currentGameMenuPanel.SetActive(false);
        GameManager.Instance.StartGame((GameManager.Difficulty)difficulty);
    }
}
