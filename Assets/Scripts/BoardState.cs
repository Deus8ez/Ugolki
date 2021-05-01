using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameRules;

public class BoardState 
{
    public Square[,] board;

    public BoardState(Square[,] aBoard) 
    {
        board = aBoard;
    }

}
