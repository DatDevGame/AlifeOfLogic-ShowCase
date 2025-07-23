using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Index2D
{
    public int row;
    public int column;

    public Index2D(int r, int c)
    {
        this.row = r;
        this.column = c;
    }

    public override string ToString()
    {
        return "(" + row + ", " + column + ")";
    }
}
