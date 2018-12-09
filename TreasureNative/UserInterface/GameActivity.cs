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

using TreasureNative.GameLogic;

namespace TreasureNative.UserInterface
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
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
        PlayerHelper[] players;

        CancellationTokenSource cancellationTokenSource;
        Thread gameThread;        

        View root;
        WebView historyView;    

        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.loading);

            game = ActivityBridge.game;
            game.OnTurnDone += (sender,args) => { RunOnUiThread(() => UseResult(sender,args)); };
            players = game.gameParameters.Players;

            foreach (var p in players)
                p.parameters.Controller.Initialize(this);
            
            cancellationTokenSource = new CancellationTokenSource();
            gameThread = new Thread(() =>
            {
                try
                {
                    Player winner = game.DoGameLoop(cancellationTokenSource.Token);
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

            FindViewById<Button>(Resource.Id.action_up).Click += (s, a) => PerformPlayerAction(new PlayerAction(GameLogic.Action.Go, Direction.Up));
            FindViewById<Button>(Resource.Id.action_down).Click += (s, a) => PerformPlayerAction(new PlayerAction(GameLogic.Action.Go, Direction.Down));
            FindViewById<Button>(Resource.Id.action_left).Click += (s, a) => PerformPlayerAction(new PlayerAction(GameLogic.Action.Go, Direction.Left));
            FindViewById<Button>(Resource.Id.action_right).Click += (s, a) => PerformPlayerAction(new PlayerAction(GameLogic.Action.Go, Direction.Right));

            FindViewById<Button>(Resource.Id.action_up).LongClick += (s, a) => PerformPlayerAction(new PlayerAction(GameLogic.Action.Shoot, Direction.Up));
            FindViewById<Button>(Resource.Id.action_down).LongClick += (s, a) => PerformPlayerAction(new PlayerAction(GameLogic.Action.Shoot, Direction.Down));
            FindViewById<Button>(Resource.Id.action_left).LongClick += (s, a) => PerformPlayerAction(new PlayerAction(GameLogic.Action.Shoot, Direction.Left));
            FindViewById<Button>(Resource.Id.action_right).LongClick += (s, a) => PerformPlayerAction(new PlayerAction(GameLogic.Action.Shoot, Direction.Right));

            historyView = FindViewById<WebView>(Resource.Id.game_text);
            root = FindViewById(Resource.Id.root);
            PrepareResultRead(0);

            gameThread.Start();
        }

        private string EscapeHtml(string text)
        {
            return text.Replace("<", "&lt;").Replace(">", "&gt;").Replace("&", "&amp;");
        }       

        private string GenerateHtml()
        {
            StringBuilder sb = new StringBuilder();
            int w = (int)(100.0 / players.Length);
            sb.Append("<html><head><meta charset=\"UTF-8\"><style>\nbody {\noverflow: scroll;\n}\ntd {\npadding: 5px;\n}\n.pc {\nwidth: " + w + "%;\n}</style></head><body><table><tr><td>#</td>");
            for (int i = 0; i < players.Length; i++)
                sb.AppendFormat("<td class=\"pc\">{0}</td>", EscapeHtml(players[i].parameters.Name));

            sb.Append("</tr>");

            int rowCount = players.Max(_ => _.actionHistory.Count);

            for (int i = 0; i < rowCount; i++)
            {
                sb.AppendFormat("<tr><td>{0}</td>", i + 1);

                for (int j = 0; j < players.Length; j++)
                {
                    var h = players[j].actionHistory;
                    var s = i < h.Count ? h[i].ToString() : "";
                    sb.AppendFormat("<td class=\"pc\">{0}</td>", EscapeHtml(s));
                }

                sb.Append("</tr>");
            }
            sb.Append("</table></body></html>");
            return sb.ToString();
        }

        private void PrepareResultRead(int player)
        {
            currentPlayer = player;
            root.Background = new ColorDrawable(PlayerColors[players[currentPlayer].parameters.Color]);
            historyView.LoadDataWithBaseURL(null, GenerateHtml(), "text/html", "utf-8", null);
            historyView.ScrollBy(0, 10005000);
        }

        public void UseResult(object sender, EventArgs e)
        {
            PrepareResultRead((currentPlayer + 1) % players.Length);
        }

        private void PerformPlayerAction(PlayerAction action)
        {
            players[currentPlayer].parameters.Controller.PerformAction(action);
        }

        private void PlayerWon(Player player)
        {
            var intent = new Intent(this, typeof(GameOverActivity));
            Bundle b = new Bundle();
            b.PutString("player", player.playerHelper.parameters.Name);
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