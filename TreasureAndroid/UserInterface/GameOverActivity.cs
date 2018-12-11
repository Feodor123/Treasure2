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

namespace TreasureAndroid
{
    [Activity(Label = "GameOverActivity")]
    public class GameOverActivity : Activity
    {
        private string _winnerName = null;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_game_over);

            var b = Intent.Extras;

            if (b != null)
            {
                _winnerName = b.GetString("player");
            }
            
            var winnerView = FindViewById<TextView>(Resource.Id.winner_text);
            winnerView.Text = _winnerName + " won!!!";
            FindViewById<Button>(Resource.Id.start_new_game).Click += NewGame;
        }
        private void NewGame(object sender, System.EventArgs e)
        {
            StartActivity(typeof(GameSettingsActivity));
        }
    }
}