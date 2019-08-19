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

        public int currentPlayer = 0;
        public GameField field;

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
            field = new GameField(gameParameters, players, rnd);
            return field.Generate();
        }        

        public Player DoGameLoop(CancellationToken cancellationToken)
        {
            Player winner = null;
            while (winner == null)
            {
                for (int i = 0;i < players.Length;i++)
                {
                    var player = players[i];
                    cancellationToken.ThrowIfCancellationRequested();                    
                    var controller = player.playerHelper.parameters.Controller;
                    List<TurnInfo>[] allTurns = players.Select(_ => _.playerHelper.actionHistory).ToArray();
                    PlayerAction m;
                    do
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        m = controller.GetAction(cancellationToken, allTurns, field);  
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

        public Player Step()//simpler way without threads; unsafe, only for only bots
        {
            var silence = new CancellationToken();
            var player = players[currentPlayer];
            currentPlayer = (currentPlayer + 1) % players.Length;
            var controller = player.playerHelper.parameters.Controller;
            List<TurnInfo>[] allTurns = players.Select(_ => _.playerHelper.actionHistory).ToArray();
            field.Update(player, controller.GetAction(silence, allTurns, field));
            return field.CheckWin();
        }
    }
}