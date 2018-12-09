using Android.App;
using System.Collections.Generic;
using System.Threading;
using TreasureNative.GameLogic;

namespace TreasureNative.UserInterface
{
    public class SignalingPlayerContoller : IPlayerController
    {

        public void Initialize(GameActivity activity)
        {
            this.activity = activity;
        }

        private EventWaitHandle actionPostedEvent = new ManualResetEvent(false);

        private PlayerAction action;

        private GameActivity activity;

        public PlayerAction GetAction(CancellationToken cancellationToken, List<TurnInfo>[] turns)
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