using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;

namespace TreasureAndroid
{
    [Activity(Label = "RulesActivity")]
    public class RulesActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_rules);

            var webView = FindViewById<WebView>(Resource.Id.rules_webview);
            webView.LoadUrl("file:///android_asset/TreasureRules/TreasureRules.html");

        }
    }
}