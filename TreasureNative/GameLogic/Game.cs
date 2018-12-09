using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TreasureNative.GameLogic
{
    public class Game
    {
        private readonly Player[] players;
        private readonly IPlayerController[] playerControllers;
        private readonly Random rnd;

        private GameField field;

        public GameParameters gameParameters;

        //public delegate void EventHandl(object sender, EventArgs args);
        public event EventHandler OnTurnDone;

        public Game(GameParameters gameParameters)
        {
            this.gameParameters = gameParameters;
            rnd = new Random();
            players = gameParameters.Players.Select(_ => new Player(_)).ToArray();
            playerControllers = gameParameters.Players.Select(_ => _.parameters.Controller).ToArray();
        }

        public bool InitializeField()
        {
            field = new GameField(gameParameters.FieldWidth, gameParameters.FieldHeight,gameParameters.PortalCount,gameParameters.SwampCount,gameParameters.SwampSize,5,true, players, rnd);
            return field.Generate();
        }

        public Player DoGameLoop(CancellationToken cancellationToken)
        {
            Player winner = null;
            while (winner == null)
            {
                foreach (var player in players)
                {

                    cancellationToken.ThrowIfCancellationRequested();
                    player.playerHelper.actionHistory.Add(new TurnInfo(player.actions,new List<TileInfo>()));//actions - all his dies, TileInfo list is empty because he don't move
                    var controller = player.playerHelper.parameters.Controller;
                    List<TurnInfo>[] allTurns = players.Select(_ => _.playerHelper.actionHistory).ToArray();
                    TurnInfo r = null;
                    do
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var m = controller.GetAction(cancellationToken, allTurns);
                        r = field.Update(player, m);
                        AddHistory(player.playerHelper,r);                       
                    }
                    while (r == null);
                    OnTurnDone(this,new TurnDoneEventArgs());
                    winner = field.CheckWin();
                    if (winner != null)
                        break;
                }
            }
            return winner;
        }

        private void AddHistory(PlayerHelper playerHelper, TurnInfo turnInfo)
        {
            playerHelper.actionHistory.RemoveAt(playerHelper.actionHistory.Count - 1);
            playerHelper.actionHistory.Add(turnInfo);
        }
    }
}