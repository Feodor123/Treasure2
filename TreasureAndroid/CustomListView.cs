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

namespace TreasureAndroid
{
    //[Register("treasureandroid.CustomListView")]
    class CustomListView : ListView
    {
        public CustomListView(Context context) : base(context) { }

        public CustomListView(Context context, Android.Util.IAttributeSet attrs) : base(context, attrs) { }

        public CustomListView(Context context, Android.Util.IAttributeSet attrs, int defStryleAttr) : base(context, attrs, defStryleAttr) { }

        public CustomListView(Context context, Android.Util.IAttributeSet attrs, int defStryleAttr, int defStryleRes) : base(context, attrs, defStryleAttr, defStryleRes) { }

        public CustomListView(IntPtr i, JniHandleOwnership o) : base(i,o) { }      

        public override ViewGroup.LayoutParams LayoutParameters
        {
            get
            {
                var parameters = base.LayoutParameters;
                parameters.Height = CalcucateHeight();
                return parameters;

            }
            set => base.LayoutParameters = value;
        }

        private int CalcucateHeight()
        {
            int totalHeight = PaddingTop + PaddingBottom;

            for (int i = 0; i < Count; i++)
            {
                View listItem = Adapter.GetView(i, null, this);
                /*if () {
                    listItem.setLayoutParams(new LayoutParams(LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT));
                }*/


                listItem.Measure(0, 0);
                totalHeight += listItem.MeasuredHeight * 3;
            }
            return totalHeight + (DividerHeight * (Adapter.Count - 1));
        }
    }
}