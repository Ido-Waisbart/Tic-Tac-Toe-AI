using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    [SerializeField] GameObject _gameStartPanel;
    [SerializeField] GameObject _gameOverPanel;
    private GameObject _currentGameMenuPanel;
    [SerializeField] TMP_Text _turnText;
    [SerializeField] TMP_Text _winText;
    [SerializeField] protected Sprite spriteX;
    [SerializeField] protected Sprite spriteO;
    [SerializeField] protected Image[] tileImages;
    [SerializeField] protected Color tileImageActiveColor;


    public const string PLAYER_TURN_STRING = "Player turn!";
    public const string AI_TURN_STRING = "AI turn!";
    public const string WIN_STRING = "You won!";
    public const string LOSS_STRING = "You lost!";
    public const string TIE_STRING = "Tie!";
    
    void Start()
    {
        if(Instance != null){
            throw new NullReferenceException();
        }
        Instance = this;

        _currentGameMenuPanel = _gameStartPanel;
        InitializeTilePositionDatas();
    }

    void InitializeTilePositionDatas(){
        int index = 0;
        foreach(Image tileImage in tileImages){
            TilePositionData data = tileImage.GetComponent<TilePositionData>();
            data.x = index % 3;
            data.y = index / 3;
            index++;
        }
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

    // Unity Event
    public void OnBtn_PlayAgainstAI(int difficulty){
        _currentGameMenuPanel.SetActive(false);
        GameManager.Instance.StartGame((GameManager.Difficulty)difficulty);
    }

    // Unity Event
    public void OnBtn_ChooseTile(TilePositionData tileData){
        GameManager.Instance.PlayerMadeMove(tileData);
        var tileImage = tileImages[tileData.IndexInGrid];
        tileImage.sprite = spriteX;
        tileImage.color = tileImageActiveColor;
        tileImage.GetComponent<Button>().enabled = false;
    }

    /*public void OnAIChoseTile(TilePositionData tileData){
        GameManager.Instance.AIMadeMove(tileData);
        var tileImage = tileImages[tileData.IndexInGrid];
        tileImage.sprite = spriteO;
        tileImage.color = tileImageActiveColor;
        tileImage.GetComponent<Button>().enabled = false;
    }*/
}
