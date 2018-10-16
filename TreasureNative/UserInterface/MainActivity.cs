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

namespace TreasureNative.UserInterface
{
    [Activity(MainLauncher = true)]
    public class MainActivity : Activity
    {
        private Button playButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            playButton = FindViewById<Button>(Resource.Id.start_game);

            playButton.Click += PlayButton_Click;
        }

        private void PlayButton_Click(object sender, System.EventArgs e)
        {
            StartActivity(typeof(GameSettingsActivity));
            //new Android.Content.Intent(this, typeof(GameActivity));
        }
    }
}

