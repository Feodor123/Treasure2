using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;

using Treasure;
using TreasureAndroid.UserInterface;

namespace TreasureAndroid
{
    [Activity(ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class GameActivity : Activity
    {
        private static readonly Color[] PlayerColors = new Color[]
        {
            Color.Red,
            Color.Blue,
            Color.Green,
            Color.Yellow,
            Color.Black,
            Color.White,
            Color.Purple,
            Color.Pink,
        };


        Game game;

        int currentPlayer;
        List<TurnInfo>[] actionHistories;
        PlayerParameters[] parameters;
        IPlayerController[] controllers;

        CancellationTokenSource cancellationTokenSource;
        Thread gameThread;        

        View root;
        TextView historyView;    

        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.loading);

            game = ActivityBridge.game;
            game.OnTurnDone += (sender,args) => RunOnUiThread(() => UseResult(sender,args));
            actionHistories = game.ActionHistories;
            parameters = ActivityBridge.playersParameters;
            controllers = game.playerControllers;

            cancellationTokenSource = new CancellationTokenSource();
            gameThread = new Thread(() =>
            {
                try
                {
                    int winner = game.DoGameLoop(cancellationTokenSource.Token);
                    RunOnUiThread(() => PlayerWon(winner));
                }
                catch (System.OperationCanceledException)
                { }
            });
            RunOnUiThread(PostGeneration);
        }
/*
        protected void onSaveInstanceState(Bundle outState)
        {
            super.onSaveInstanceState(outState);
            outState.putInt("count", cnt);
            Log.d(LOG_TAG, "onSaveInstanceState");
        }

        protected void onRestoreInstanceState(Bundle savedInstanceState)
        {
            super.onRestoreInstanceState(savedInstanceState);
            cnt = savedInstanceState.getInt("count");
            Log.d(LOG_TAG, "onRestoreInstanceState");
        }
*/
        private void PostGeneration()
        {
            Toast.MakeText(this, "Welcome to the treasure hunting society!", ToastLength.Short).Show();                       

            SetContentView(Resource.Layout.activity_game);

            FindViewById<Button>(Resource.Id.action_up).Click += (s, a) => PerformPlayerAction(new PlayerAction(ActionType.Go, Direction.Up));
            FindViewById<Button>(Resource.Id.action_down).Click += (s, a) => PerformPlayerAction(new PlayerAction(ActionType.Go, Direction.Down));
            FindViewById<Button>(Resource.Id.action_left).Click += (s, a) => PerformPlayerAction(new PlayerAction(ActionType.Go, Direction.Left));
            FindViewById<Button>(Resource.Id.action_right).Click += (s, a) => PerformPlayerAction(new PlayerAction(ActionType.Go, Direction.Right));

            FindViewById<Button>(Resource.Id.action_up).LongClick += (s, a) => PerformPlayerAction(new PlayerAction(ActionType.Shoot, Direction.Up));
            FindViewById<Button>(Resource.Id.action_down).LongClick += (s, a) => PerformPlayerAction(new PlayerAction(ActionType.Shoot, Direction.Down));
            FindViewById<Button>(Resource.Id.action_left).LongClick += (s, a) => PerformPlayerAction(new PlayerAction(ActionType.Shoot, Direction.Left));
            FindViewById<Button>(Resource.Id.action_right).LongClick += (s, a) => PerformPlayerAction(new PlayerAction(ActionType.Shoot, Direction.Right));

            historyView = FindViewById<TextView>(Resource.Id.game_text);
            historyView.MovementMethod = new Android.Text.Method.ScrollingMovementMethod();
            root = FindViewById(Resource.Id.root);
            PrepareResultRead(0);

            gameThread.Start();
        }       

        private string GenerateText()
        {
            List<string> text = new List<string>();
            for (int i = 0;i < actionHistories[0].Count;i++)
            {
                text.Add($"Turn {i+1}:");
                for (int j = 0;j < actionHistories.Length; j++)
                {
                    var tf = actionHistories[j][i];
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"{parameters[j].name}: ");
                    List<string> strs = new List<string>();
                    int k;
                    for (k = 0; k < tf.actions.Count && tf.actions[k].action.type == ActionType.Die; k++) ;
                    if (k != 0)
                    {
                        switch (k)
                        {
                            case 1:
                                strs.Add("You died");
                                break;
                            case 2:
                                strs.Add("You died twice");
                                break;
                            case 3:
                                strs.Add("You died thrice");
                                break;
                            default:
                                strs.Add($"You died {k} times");
                                break;
                        }
                    }
                    if (k < tf.actions.Count)
                    {
                        strs.Add($"{tf.actions[k].action.type.ToString()} {tf.actions[k].action.direction.ToString()}");
                        foreach (var t in tf.actions[k].tiles)
                        {
                            if (t.moveResult == MoveResult.Home)
                            {
                                if (t.intParam - 1 == j)
                                {
                                    strs.Add("Your Home");
                                }
                                else
                                {
                                    strs.Add($"{parameters[t.intParam - 1].name}\'s Home");
                                }
                            }
                            else
                            {
                                string str = t.moveResult.ToString();
                                if (t.intParam != -1)
                                    str += $" {t.intParam}";
                                if (t.stuff.Count != 0)
                                {
                                    str += " with " + string.Join(", ", t.stuff.Select(_ => _.type.ToString()));
                                }
                                strs.Add(str);
                            }
                        }
                        k++;
                    }
                    if (k < tf.actions.Count)
                        strs.Add("Died");
                    sb.Append(string.Join(" - ", strs));
                    text.Add(sb.ToString());
                    if ((j == currentPlayer) && (i == actionHistories[j].Count - 1))
                        goto BREAK1;
                }
            }
            BREAK1:;
            return string.Join("\n", text);
        }

        private void PrepareResultRead(int player)
        {
            currentPlayer = player;
            root.Background = new ColorDrawable(PlayerColors[parameters[currentPlayer].colorNumber]);
            historyView.Text = GenerateText();
        }

        public void UseResult(object sender, EventArgs e)
        {
            PrepareResultRead((currentPlayer + 1) % game.gameParameters.PlayerCount);
        }

        private void PerformPlayerAction(PlayerAction action)
        {
            controllers[currentPlayer].PerformAction(action);
        }

        private void PlayerWon(int playerInd)
        {
            var intent = new Intent(this, typeof(GameOverActivity));
            Bundle b = new Bundle();
            b.PutString("player", parameters[playerInd].name);
            intent.PutExtras(b);

            Finish();
            StartActivity(intent);
        }

        protected override void OnDestroy()
        {
            cancellationTokenSource.Cancel();
            gameThread.Join();
            base.OnDestroy();
        }
    }
}