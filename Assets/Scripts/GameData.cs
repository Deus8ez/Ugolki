using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameRules;

[System.Serializable]
public class GameData 
{
    public Square[,] _board;

    public GameData(BoardState state)
    {
        _board = state.board;
    }
}
