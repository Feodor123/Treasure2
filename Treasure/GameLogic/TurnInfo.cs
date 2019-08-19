using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treasure
{
    public class TurnInfo
    {
        public List<ActionInfo> actions;

        public TurnInfo(List<ActionInfo> preActions)
        {
            actions = preActions;
        }

        public TurnInfo()
        {
            actions = new List<ActionInfo>();
        }
    }

    public class ActionInfo
    {
        public PlayerAction action;
        public List<TileInfo> tiles;

        public static ActionInfo DieAction(TileInfo tile) { return new ActionInfo(new PlayerAction(ActionType.Die, Direction.None), new List<TileInfo>() { tile }); }

        public ActionInfo(PlayerAction action, List<TileInfo> tiles)
        {
            this.action = action;
            this.tiles = tiles;
        }
    }

    public class TileInfo
    {
        public List<Stuff> stuff = new List<Stuff>();
        public int intParam;
        public MoveResult moveResult;

        public TileInfo(MoveResult moveResult, int intParam = -1)
        {
            this.intParam = intParam;
            this.moveResult = moveResult;
        }

        public TileInfo(MoveResult moveResult, List<Stuff> stuff, int intParam = -1)
        {
            this.stuff = stuff;
            this.intParam = intParam;
            this.moveResult = moveResult;
        }
    }
}