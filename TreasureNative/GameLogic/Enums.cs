﻿namespace TreasureNative.GameLogic
{
    public enum TerrainType
    {
        Unknown,
        Field,
        Water,
        Swamp,
        Hole,
        Home,
        Bridge,
    }

    public enum BorderType
    {
        Unknown,
        Empty,
        Wall,
        ShootedWall,
        BreakableWall,
        UnbreakableWall,
        Grate,
    }

    public enum Direction
    {
        Up,
        Right,
        Down,
        Left,
        None,
    }

    public enum Action
    {
        None,
        Die,
        Go,
        Shoot,
    }

    public enum MoveResult
    {
        Wall,
        Grate,
        Field,
        Water,
        Swamp,
        Hole,
        Home,
    }

    public enum StuffType
    {
        Treasure,
    }

    public enum State
    {
        None = 0,
        OnBridge = 1,
    }

    public enum Orientation
    {
        None,
        Vertical,
        Horisontal,
    }
}