using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct Grid
{
    public int x;
    public int y;

    public Grid(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static Grid operator -(Grid a, Grid b)
    {
        int diffX = a.x - b.x;
        int diffY = a.y - b.y;
        return new Grid(diffX, diffY);
    }

    public static Grid operator +(Grid a, Grid b)
    {
        int sumX = a.x + b.x;
        int sumY = a.y + b.y;
        return new Grid(sumX, sumY);
    }

    public static Grid operator *(Grid a, int value)
    {
        return new Grid(a.x * value, a.y * value);
    }
    public static Grid operator *(int value, Grid a)
    {
        return new Grid(a.x * value, a.y * value);
    }

    public static Grid zero => new Grid(0, 0);
    public static Grid horMoveOffset => new Grid(1, 0);
    public static Grid verMoveOffset => new Grid(0, 1);
    public static Grid forwardSlashOffset => new Grid(1, 1);
    public static Grid backwardSlashOffset => new Grid(1, -1);

    public override string ToString() => $"({x}, {y})";

}

public static class GridUtil
{
    public static int Max(this Grid grid) => System.Math.Max(grid.x, grid.y);
    public static int Min(this Grid grid) => System.Math.Min(grid.x, grid.y);

    public static bool IsNest(this Grid grid, Grid otherGrid) => grid.Distance(otherGrid) <= 1;
    public static int Distance(this Grid grid, Grid otherGrid)
    {
        var diff = grid.Diff(otherGrid);
        return System.Math.Max(System.Math.Abs(diff.x), System.Math.Abs(diff.y));
    }
    public static Grid Diff(this Grid grid, Grid otherGrid) => otherGrid - grid;
}
