namespace Treasure
{
    public enum TerrainType
    {
        Field,
        Water,
        Swamp,
        Hole,
        Home,
    }

    public enum BorderType
    {
        Empty,
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

    public enum ActionType
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

    public enum Orientation
    {
        None,
        Vertical,
        Horisontal,
    }
}