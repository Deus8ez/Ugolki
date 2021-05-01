using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameRules;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    Dictionary<string, GameObject> squares;
    Dictionary<string, GameObject> pieces;
    GameRules.Board board;
    Controller controller;
    Robot robot;
    State state;
    GameObject item;
    Transform clickedItem;
    int x;
    int y;
    bool singlePlayer;
    string key;
    string squareString;
    Text st;
    string status;
    int whiteCount = 0;
    int brownCount = 0;
    bool gameover = false;

    enum State
    {
        none,
        picked
    }
    public Board()
    {
        squares = new Dictionary<string, GameObject>();
        pieces = new Dictionary<string, GameObject>();
        board = new GameRules.Board();
        controller = new Controller(board);
        robot = new Robot(controller, board);    
        singlePlayer = StateBridge.AIGame;
    }

    void Start()
    {
        st = GameObject.Find("StatusObject/Canvas/Text").GetComponent<Text>();

        if (StateBridge.LoadSave)
        {
            GameData save = SaveAndLoad.LoadGame();
            board.board = save._board;
        }
        else
        {
            board.Init();
        }

        InitGameObjects();
        SetPieces();

        if (!StateBridge.AIGame)
        {
            GameObject saveButton = GameObject.Find("Save");
            saveButton.SetActive(false);
        }
    }

    void InitGameObjects()
    {
        for(int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                key = "" + x + y;
                GameObject square = GameObject.Find((x + y) % 2 == 0 ? "BlackSquare" : "WhiteSquare");
                squares[key] = Instantiate(square);
                squares[key].transform.position = new Vector2(x, y);

                pieces[key] = Instantiate(GameObject.Find("none"));
                pieces[key].transform.position = squares[key].transform.position;
                pieces[key].name = "none";
            }
        }
    }

    void SetPieces()
    {
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                key = "" + x + y;
                string squareString = board.GetSquareAt(x, y).piece.ToString();
                if (pieces[key].name == squareString)
                {
                    continue;
                }

                pieces[key] = Instantiate(GameObject.Find(squareString));
                pieces[key].transform.position = squares[key].transform.position;
                pieces[key].name = squareString;
            }
        }
    }

    void ReRender()
    {

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {

                squareString = board.GetSquareAt(x, y).piece.ToString();
                key = "" + x + y;

                if (pieces[key].name == squareString)
                {
                    continue;
                }

                if (squareString == "marker")
                {
                   
                    if (squares[key].name == "BlackSquare(Clone)")
                    {
                        pieces[key] = Instantiate(GameObject.Find("blackMarker"));
                    }
                    else
                    {
                        pieces[key] = Instantiate(GameObject.Find("whiteMarker"));
                    }

                    pieces[key].transform.position = squares[key].transform.position;
                    continue;
                }

                Destroy(pieces[key].transform.gameObject);
                pieces[key] = Instantiate(GameObject.Find(squareString));
                pieces[key].transform.position = squares[key].transform.position;
                pieces[key].name = squareString;
            }
        }
    }

    public delegate void Renderer();

    // Update is called once per frame
    void Update()
    {
        Action(new Renderer(ReRender));
        st.text = status;
    }

    public void Action(Renderer markSetter)
    {

        CheckWhoIsWinner();


        Debug.Log(state);
        switch (state)
        {
            case State.none:
                if (!gameover && IsMouseButtonPressed())
                {
                    if (Pick())
                    {
                        x = (int)clickedItem.localPosition.x;
                        y = (int)clickedItem.localPosition.y;
                        key = "" + x + y;
                        controller.SetFrom((x, y));
                        if (controller.IsPiece())
                        {
                            controller.CheckExistingSkips();
                            markSetter();
                            state = State.picked;
                        }
                        else
                        {
                            if (status.Length <= 15) 
                            { 
                                status += "Неверная фигура";
                            } 
                        }

                        return;
                    };
                }
                break;
            case State.picked:
                if (!gameover && IsMouseButtonPressed())
                {
                    if (Pick())
                    {
                        x = (int)clickedItem.localPosition.x;
                        y = (int)clickedItem.localPosition.y;

                        if (controller.GetFrom().x == x && controller.GetFrom().y == y)
                        {
                            board.ClearMarkers();
                            state = State.none;
                            markSetter();
                            controller.SetFrom((0,0));
                            return;
                        }

                        if(controller.IsMarked((x, y)))
                        {

                            controller.SetTo((x, y));

                            if (singlePlayer)
                            {

                                    controller.Jump();
                                    markSetter();
                                    robot.MakeMove();
                                    robot.Mark();
                                    markSetter();
                                    board.ClearMarkers();
                                    state = State.none;
                                    status = "";

                            }
                            else
                            {

                                    controller.Jump();
                                    markSetter();
                                    controller.SetTurn();
                                    board.ClearMarkers();
                                    state = State.none;
                                    status = "";
                                    status = controller.whiteMoves ? "Ход красных. " : "Ход черных. ";

                            }
                        } else
                        {
                            status = "Не правильный ход";
                        }

                        return;
                    }
                }
                break;
        }

    }

    bool IsMouseButtonPressed()
    {
        return Input.GetMouseButtonDown(0);
    }

    Vector2 GetClickPosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    Transform GetItemAt(Vector2 position)
    {
        RaycastHit2D[] figures = Physics2D.RaycastAll(position, position, 0.5f);
        if (figures.Length == 0)
        {
            return null;
        }

        return figures[0].transform;
    }

    bool Pick()
    {
        Vector2 clickPosition = GetClickPosition();
        clickedItem = GetItemAt(clickPosition);
        if (clickedItem == null)
        {
            return false;
        }
        Debug.Log(clickedItem.gameObject.name);
        item = clickedItem.gameObject;
        Debug.Log(item.name);
        return true;
    }

    public void CheckWhoIsWinner()
    {
        whiteCount = 0;
        brownCount = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 4; j++)
            {

                    string brownKey = "" + j + i;
                    string whiteKey = "" + (7-j) + (7-i);
                    
                    if (pieces[whiteKey].name == "whitePiece")
                    {
                        whiteCount++;
                    }

                    if (pieces[brownKey].name == "brownPiece")
                    {
                        brownCount++;
                    }
                    
            }
        }

        if (whiteCount == 12 && brownCount < 12)
        {
            status = "Красные победили";
        }
        if (brownCount == 12 && whiteCount < 12)
        {
            status = "Черные победили";
        }

        if(status == "Черные победили" || status == "Красные победили")
        {
            gameover = true;
        }
    }

    public void SaveGame()
    {
        SaveAndLoad.SaveGame(new BoardState(board.board)); 
    }
}

