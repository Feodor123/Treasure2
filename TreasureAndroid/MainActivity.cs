using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Microsoft.Xna.Framework;

using System.Threading;

namespace TreasureAndroid.UserInterface
{
    [Activity(MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : AndroidGameActivity
    {
        private Button playButton;
        private Button rulesButton;

        private View background;

        protected override void OnCreate(Bundle savedInstanceState)
        {      
            RequestWindowFeature(WindowFeatures.NoTitle);            
            base.OnCreate(savedInstanceState);

            var g = new BackAnimator();

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);


            playButton = FindViewById<Button>(Resource.Id.start_game);
            rulesButton = FindViewById<Button>(Resource.Id.game_rules);
            var root = FindViewById<FrameLayout>(Resource.Id.root);         

            background = (View)g.Services.GetService(typeof(View));

            background.SetZ(-20);

            root.AddView(background);

            playButton.Click += PlayButton_Click;
            rulesButton.Click += RulesButton_Click;

            Thread thread = new Thread(() => g.Run());
            thread.Start();
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(GameSettingsActivity));
            //new Android.Content.Intent(this, typeof(GameActivity));
        }
        private void RulesButton_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(RulesActivity));
            //new Android.Content.Intent(this, typeof(GameActivity));
        }
    }
}

