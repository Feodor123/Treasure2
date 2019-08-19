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
using Treasure;

namespace TreasureAndroid.UserInterface
{
    class BasicPlayerParameters : PlayerParameters
    {
        public string name;
        public int colorNumber;
        public BasicPlayerParameters(IPlayerController controller,string name,int colorNumber) : base(controller)
        {
            this.name = name;
            this.colorNumber = colorNumber;
        }
    }
}