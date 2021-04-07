using System.Collections.Generic;
using UnityEngine;

public class Square
{
    public Obj obj;
    public int x;
    public int y;
    public int health;
    public Transform trans;

    public Square(Obj _obj, int _x, int _y, int _health, Transform _trans)
    {
        obj = _obj;
        x = _x;
        y = _y;
        health = _health;
        trans = _trans;
    }
}

public class WorldGrid : MonoBehaviour
{
    public static int tabWidth, tabHeight;

    private static List<Square>[,] squares;

    public void CreateTab(int width, int height)
    {
        tabWidth = Mathf.CeilToInt(width / 5.0f);
        tabHeight = Mathf.CeilToInt(height / 5.0f);

        squares = new List<Square>[tabWidth, tabHeight];
        for (int y = 0; y < tabHeight; y++)
        {
            for (int x = 0; x < tabWidth; x++)
            {
                squares[x, y] = new List<Square>();
            }
        }
    }

    public static void SetSquare(Square newSquare)
    {
        int tabX = Mathf.FloorToInt(newSquare.x / 5.0f);
        int tabY = Mathf.FloorToInt(newSquare.y / 5.0f);

        for (int i = 0; i < squares[tabX, tabY].Count; i++)
        {
            Square square = squares[tabX, tabY][i];
            if (square.x == newSquare.x && square.y == newSquare.y)
            {
                squares[tabX, tabY][i] = newSquare;
                return;
            }
        }

        squares[tabX, tabY].Add(newSquare);
    }
    public static Square GetSquare(int x, int y)
    {
        int tabX = Mathf.FloorToInt(x / 5.0f);
        int tabY = Mathf.FloorToInt(y / 5.0f);

        if(tabX < 0 || tabX >= tabWidth || tabY < 0 || tabY >= tabHeight) { return null; }

        foreach (Square square in squares[tabX, tabY])
        {
            if(square.x == x && square.y == y)
            {
                return square;
            }
        }

        return null;
    }
    public static void RemoveSquare(int x, int y)
    {
        int tabX = Mathf.FloorToInt(x / 5.0f);
        int tabY = Mathf.FloorToInt(y / 5.0f);

        foreach (Square square in squares[tabX, tabY])
        {
            if (square.x == x && square.y == y)
            {
                squares[tabX, tabY].Remove(square);
                return;
            }
        }
    }
}