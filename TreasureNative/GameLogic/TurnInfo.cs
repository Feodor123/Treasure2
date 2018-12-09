using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreasureNative.GameLogic
{
    public class TurnInfo
    {
        public static Dictionary<MoveResult, string> moveResultEmoji;
        public static Dictionary<Direction, string> directionEmoji;
        public static Dictionary<StuffType, string> stuffEmoji;
        public static Dictionary<Action, string> actionEmoji;
        public static Dictionary<int, string> digitEmoji;
        public List<PlayerAction> playerActions;
        public List<TileInfo> tileInfo;

        static TurnInfo()
        {
            directionEmoji = new Dictionary<Direction, string>()
            {
                {Direction.None,""},
                {Direction.Up,"\u2B06\uFE0F"},
                {Direction.Down,"\u2B07\uFE0F"},
                {Direction.Left,"\u2B05\uFE0F"},
                {Direction.Right,"\u27A1\uFE0F"},
            };
            moveResultEmoji = new Dictionary<MoveResult, string>()
             {
                {MoveResult.Wall,"\uD83D\uDEA7"},
                {MoveResult.Swamp,"\uD83D\uDC0A"},
                {MoveResult.Field,"\uD83C\uDF3B"},
                {MoveResult.Water,"\uD83C\uDF0A"},
                {MoveResult.Hole,"\uD83D\uDD73\uFE0F"},
                {MoveResult.Home, "\u26FA"},               
                {MoveResult.Grate,"#\u20E3" }
             };
            stuffEmoji = new Dictionary<StuffType, string>()
            {
                {StuffType.Treasure,"\uD83D\uDCB0"},
            };
            actionEmoji = new Dictionary<Action, string>()
            {
                {Action.None,""},
                {Action.Die,"\u2620"},
                {Action.Go,""},
                {Action.Shoot,"\uD83D\uDD2B"},
            };
            digitEmoji = new Dictionary<int, string>()
            {
                { 0,"0\u20E3"},
                { 1,"1\u20E3"},
                { 2,"2\u20E3"},
                { 3,"3\u20E3"},
                { 4,"4\u20E3"},
                { 5,"5\u20E3"},
                { 6,"6\u20E3"},
                { 7,"7\u20E3"},
                { 8,"8\u20E3"},
                { 9,"9\u20E3"},
                { -1,""},
            };
        }

        public TurnInfo(List<PlayerAction> playerActions, List<TileInfo> tileInfo)
        {
            this.playerActions = playerActions;
            this.tileInfo = tileInfo;
        }

        public override string ToString()
        {
            string s = "";
            foreach (var pa in playerActions)
            {
                s += directionEmoji[pa.direction];
                s += actionEmoji[pa.action];
                s += digitEmoji[pa.intParam];
            }
            return s + string.Concat(tileInfo.Select(_ => _.ToString()));
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

        public override string ToString()
        {
            string s = TurnInfo.moveResultEmoji[moveResult] + TurnInfo.digitEmoji[intParam];
            foreach (var st in stuff)
            {
                s += TurnInfo.stuffEmoji[st.type];
                if (st.count > 1)
                    s += TurnInfo.digitEmoji[st.count];
            }
            return s;
        }
    }
}