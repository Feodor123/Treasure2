using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Treasure;
using TreasureAndroid.UserInterface;

using Microsoft.Xna.Framework;

namespace TreasureAndroid
{
    [Activity(Label = "GameSettingsActivity")]
    public class GameSettingsActivity : AndroidGameActivity, SeekBar.IOnSeekBarChangeListener
    {
        private View background;
        SeekBar widthBar;
        SeekBar heightBar;
        SeekBar playerBar;
        SeekBar portalBar;
        SeekBar swampBar;
        SeekBar swampSizeBar;
        Dictionary<SeekBar, int> SeekBarMinimum = new Dictionary<SeekBar, int>();
        Dictionary<SeekBar, TextView> SeekBarText = new Dictionary<SeekBar, TextView>();
        Dictionary<SeekBar, int> SeekBarValue = new Dictionary<SeekBar, int>();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_game_settings);            

            widthBar = AddSeekBar(Resource.Id.width_bar, Resource.Id.width_bar_value, 6, 12);
            heightBar = AddSeekBar(Resource.Id.height_bar, Resource.Id.height_bar_value, 6, 12);
            playerBar = AddSeekBar(Resource.Id.player_bar, Resource.Id.player_bar_value, 1, 4);
            portalBar = AddSeekBar(Resource.Id.portal_bar, Resource.Id.portal_bar_value, 2, 5);
            swampBar = AddSeekBar(Resource.Id.swamp_bar, Resource.Id.swamp_bar_value, 0, 6);
            swampSizeBar = AddSeekBar(Resource.Id.swamp_size_bar, Resource.Id.swamp_size_bar_value, 1, 8);
            FindViewById<Button>(Resource.Id.start_new_game).Click += TryStartGame;
            var root = FindViewById<FrameLayout>(Resource.Id.root);

            var g = new BackAnimator();            
            background = (View)g.Services.GetService(typeof(View));
            background.SetZ(-20);
            root.AddView(background);

            Thread thread = new Thread(() => g.Run());
            thread.Start();
        }

        private SeekBar AddSeekBar(int SeekId,int textId,int minValue, int maxValue)
        {
            var bar = FindViewById<SeekBar>(SeekId);
            var text = FindViewById<TextView>(textId);
            SeekBarValue.Add(bar, minValue);
            SeekBarMinimum.Add(bar, minValue);
            bar.Max = maxValue - minValue;
            SeekBarText.Add(bar, text);
            bar.SetOnSeekBarChangeListener(this);
            text.Text = minValue.ToString();
            return bar;
        }

        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {          
            SeekBarValue[seekBar] = progress + SeekBarMinimum[seekBar];
            SeekBarText[seekBar].Text = SeekBarValue[seekBar].ToString();
        }

        public void OnStopTrackingTouch(SeekBar seekBar)
        {

        }

        public void OnStartTrackingTouch(SeekBar seekBar)
        {

        }

        private void TryStartGame(object sender, EventArgs e)
        {
            var gameParameters = new GameParameters()
            {
                FieldHeight = SeekBarValue[heightBar],
                FieldWidth = SeekBarValue[widthBar],
                PortalCount = SeekBarValue[portalBar],
                SwampCount = SeekBarValue[swampBar],
                SwampSize = SeekBarValue[swampSizeBar],
            };

            PlayerHelper[] players = new PlayerHelper[SeekBarValue[playerBar]];

            for (int i = 0; i < players.Length; i++)
                players[i] = new PlayerHelper(new BasicPlayerParameters(new SignalingPlayerContoller(), $"Player {i + 1}", i));

            gameParameters.Players = players;
            Treasure.Game game = new Treasure.Game(gameParameters);
            if (game.InitializeField())
            {
                ActivityBridge.game = game;
                StartActivity(typeof(GameActivity));
            }
            else
            {
                Toast.MakeText(this, "Ohh... Your parametres is so hard! Choose something else.", ToastLength.Short).Show();
            }           
        }
    }
}