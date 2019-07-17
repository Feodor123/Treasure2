using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Treasure
{
    public class Game
    {
        private readonly Player[] players;
        private readonly IPlayerController[] playerControllers;
        private readonly Random rnd;
        private readonly int seed;

        private GameField field;

        public GameParameters gameParameters;

        //public delegate void EventHandl(object sender, EventArgs args);
        public event EventHandler OnTurnDone;

        public Game(GameParameters gameParameters)
        {
            this.gameParameters = gameParameters;
            seed = (new Random()).Next();
            rnd = new Random(seed);
            players = gameParameters.Players.Select(_ => new Player(_)).ToArray();
            playerControllers = gameParameters.Players.Select(_ => _.parameters.Controller).ToArray();
        }

        public Game(GameParameters gameParameters, int seed)
        {
            this.gameParameters = gameParameters;
            this.seed = seed;
            rnd = new Random(seed);
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
                    var controller = player.playerHelper.parameters.Controller;
                    List<TurnInfo>[] allTurns = players.Select(_ => _.playerHelper.actionHistory).ToArray();
                    PlayerAction m;
                    do
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        m = controller.GetAction(cancellationToken, allTurns);  
                    }
                    while (!field.Update(player, m));
                    OnTurnDone.Invoke(this,new TurnDoneEventArgs());
                    winner = field.CheckWin();
                    if (winner != null)
                        break;
                }
            }
            return winner;
        }
    }
}