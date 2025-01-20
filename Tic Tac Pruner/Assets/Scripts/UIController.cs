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
    [SerializeField] protected Color tileImageClickableColor;


    public const string PLAYER_TURN_STRING = "Player turn!";
    public const string AI_TURN_STRING = "AI turn!";
    public const string WIN_STRING = "You won!";
    public const string LOSS_STRING = "You lost!";
    public const string TIE_STRING = "Tie!";

    bool isPlayerX;
    
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

    // The game uses a trick to change the player to O, without actually changing the logic.
    // If the player is O, then the images are changed, and the game state interpretation changes, ONLY.
    // See: SetWinText()
    //      OnBtn_ChooseTile()  (Under the hood, the player is X either way.)
    //      OnAIChoseTile()     (Under the hood, the player is X either way.)
    public void SetIsPlayerX(bool isPlayerX){
        this.isPlayerX = isPlayerX;
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
            case GameState.VictoryPlayer:
                _winText.text = WIN_STRING;
                break;
            case GameState.VictoryAI:
                _winText.text = LOSS_STRING;
                break;
            default:
                throw new ArgumentException("Unexpected game state on game over: " + gameState.ToString());
        }
    }

    public void ClearBoard(){
        foreach(var tileImage in tileImages){
            tileImage.sprite = null;
            tileImage.color = tileImageClickableColor;
            tileImage.GetComponent<Button>().enabled = true;  // TODO OPTIONAL: There's probably more GetComponent()'s than needed, but this small project doesn't necessitate this fix.
        }
    }

    // Enums are not displayed natively on button event parameters in the inspector.
    // public void OnBtn_PlayAgainstAI(GameManager.Difficulty difficulty){}

    // Unity Event
    public void OnBtn_PlayAgainstAI(string difficultyString){
        _currentGameMenuPanel.SetActive(false);
        GameManager.Difficulty difficulty = (GameManager.Difficulty)System.Enum.Parse( typeof(GameManager.Difficulty), difficultyString);
        GameManager.Instance.StartGame(difficulty);
    }

    // Unity Event
    /*public void OnBtn_PlayAgainstAI(int difficulty){
        _currentGameMenuPanel.SetActive(false);
        GameManager.Instance.StartGame((GameManager.Difficulty)difficulty);
    }*/

    // Unity Event
    public void OnBtn_ChooseTile(TilePositionData tileData){
        GameManager.Instance.PlayerMadeMove(tileData);
        var tileImage = tileImages[tileData.IndexInGrid];
        tileImage.sprite = isPlayerX ? spriteX : spriteO;
        tileImage.color = tileImageActiveColor;
        tileImage.GetComponent<Button>().enabled = false;
    }

    public void OnAIChoseTile(Vector2Int tileIndices){
        var tileImage = tileImages[tileIndices.x + tileIndices.y * 3];
        tileImage.sprite = isPlayerX ? spriteO : spriteX;
        tileImage.color = tileImageActiveColor;
        tileImage.GetComponent<Button>().enabled = false;
    }
}
