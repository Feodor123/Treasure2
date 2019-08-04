using Android.App;
using System.Collections.Generic;
using System.Threading;
using Treasure;

namespace TreasureAndroid
{
    public class SignalingPlayerContoller : IPlayerController
    {

        private EventWaitHandle actionPostedEvent = new ManualResetEvent(false);

        private PlayerAction action;

        public PlayerAction GetAction(CancellationToken cancellationToken, List<TurnInfo>[] turns, GameField field)
        {
            while (!actionPostedEvent.WaitOne(100))
                cancellationToken.ThrowIfCancellationRequested();
            actionPostedEvent.Reset();
            return action;
        }

        public void PerformAction(PlayerAction action)
        {
            this.action = action;
            actionPostedEvent.Set();
        }
    }
}