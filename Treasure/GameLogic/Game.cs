using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Treasure
{
    public class Game
    {
        public readonly IPlayerController[] playerControllers;
        private readonly Random rnd;
        private readonly int seed;

        public int currentPlayer = 0;
        public GameField field;

        public GameParameters gameParameters;
        public List<TurnInfo>[] ActionHistories { get => field.ActionHistories; }

        //public delegate void EventHandl(object sender, EventArgs args);
        public event EventHandler OnTurnDone;

        public Game(GameParameters gameParameters, IPlayerController[] playerControllers, Random rnd)
        {
            this.gameParameters = gameParameters;
            seed = rnd.Next();
            this.rnd = new Random(seed);
            this.playerControllers = playerControllers;
        }

        public Game(GameParameters gameParameters, IPlayerController[] playerControllers, int seed)
        {
            this.gameParameters = gameParameters;
            this.seed = seed;
            rnd = new Random(seed);
            this.playerControllers = playerControllers;
        }

        public bool InitializeField()
        {
            field = new GameField(gameParameters, rnd);
            return field.Generate();
        }        

        public int DoGameLoop(CancellationToken cancellationToken)
        {
            int winner = -1;
            while (winner == -1)
            {
                for (int i = 0;i < gameParameters.PlayerCount;i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();                    
                    var controller = playerControllers[i];
                    PlayerAction m;
                    do
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        m = controller.GetAction(cancellationToken, ActionHistories, field);  
                    }
                    while (!field.Update(i, m));
                    OnTurnDone.Invoke(this,new TurnDoneEventArgs());
                    winner = field.CheckWin();
                    if (winner != -1)
                        break;
                }
            }
            return winner;
        }

        public int Step()//simpler way without threads; unsafe, only for only bots
        {
            var silence = new CancellationToken();
            field.Update(currentPlayer, playerControllers[currentPlayer].GetAction(silence, ActionHistories, field));
            currentPlayer = (currentPlayer + 1) % gameParameters.PlayerCount;
            return field.CheckWin();
        }
    }
}