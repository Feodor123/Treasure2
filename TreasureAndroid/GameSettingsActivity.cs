using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Android;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Treasure;
using TreasureAndroid.UserInterface;

using Microsoft.Xna.Framework;
using Java.Lang;
using Android.Views.InputMethods;

namespace TreasureAndroid
{
    [Activity(Label = "GameSettingsActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class GameSettingsActivity : /*AndroidGameActivity*/ListActivity, SeekBar.IOnSeekBarChangeListener
    {
        const int MaxPlayerCount = 10;

        private View background;
        SeekBarWrap widthBar;
        SeekBarWrap heightBar;
        SeekBarWrap portalBar;
        SeekBarWrap swampBar;
        SeekBarWrap swampSizeBar;
        CheckBox throughCheckBox;
        LinearLayout linearLayout;
        ListView playersList;
        InfoAdapter adapter;

        Dictionary<SeekBar, SeekBarWrap> dict;
        List<PlayerInfo> playersInfo;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_game_settings);

            dict = new Dictionary<SeekBar, SeekBarWrap>();
            playersInfo = new List<PlayerInfo>();

            linearLayout = FindViewById<LinearLayout>(Resource.Id.linear_layout);

            throughCheckBox = FindViewById<CheckBox>(Resource.Id.through_check_box);
            widthBar = new SeekBarWrap(6,12, 
                FindViewById<SeekBar>(Resource.Id.width_bar), 
                FindViewById<TextView>(Resource.Id.width_bar_name), 
                FindViewById<TextView>(Resource.Id.width_bar_value),this);            
            heightBar = new SeekBarWrap(6,12,
                FindViewById<SeekBar>(Resource.Id.height_bar),
                FindViewById<TextView>(Resource.Id.height_bar_name),
                FindViewById<TextView>(Resource.Id.height_bar_value), this);
            /*
            playerBar = new SeekBarWrap(1, 4,
                FindViewById<SeekBar>(Resource.Id.player_bar),
                FindViewById<TextView>(Resource.Id.player_bar_name),
                FindViewById<TextView>(Resource.Id.player_bar_value), this);
                */
            portalBar = new SeekBarWrap(2, 5,
                FindViewById<SeekBar>(Resource.Id.portal_bar),
                FindViewById<TextView>(Resource.Id.portal_bar_name),
                FindViewById<TextView>(Resource.Id.portal_bar_value), this);
            swampBar = new SeekBarWrap(0, 8,
                FindViewById<SeekBar>(Resource.Id.swamp_bar),
                FindViewById<TextView>(Resource.Id.swamp_bar_name),
                FindViewById<TextView>(Resource.Id.swamp_bar_value), this);
            swampSizeBar = new SeekBarWrap(0, 8,
                FindViewById<SeekBar>(Resource.Id.swamp_size_bar),
                FindViewById<TextView>(Resource.Id.swamp_size_bar_name),
                FindViewById<TextView>(Resource.Id.swamp_size_bar_value), this);
            playersList = FindViewById<ListView>(Android.Resource.Id.List);

            dict.Add(widthBar.seekBar, widthBar);
            dict.Add(heightBar.seekBar, heightBar);
            //dict.Add(playerBar.seekBar, playerBar);
            dict.Add(portalBar.seekBar, portalBar);
            dict.Add(swampBar.seekBar, swampBar);
            dict.Add(swampSizeBar.seekBar, swampSizeBar);
            FindViewById<Button>(Resource.Id.start_new_game).Click += TryStartGame;
            FindViewById<Button>(Resource.Id.add_player_button).Click += AddPlayer;
            FindViewById<Button>(Resource.Id.remove_player_button).Click += RemovePlayer;
            /*
            var root = FindViewById<FrameLayout>(Resource.Id.root);
            var g = new BackAnimator();            
            background = (View)g.Services.GetService(typeof(View));
            background.SetZ(-20);
            root.AddView(background);
            Thread thread = new Thread(() => g.Run());
            thread.Start();
            */
            adapter = new InfoAdapter(this, Resource.Layout.player_prop, playersInfo);
            ListAdapter = adapter;
            //playersList.Adapter = adapter;
        }

        class C : Java.Lang.Object, ListView.IOnTouchListener
        {
            public IntPtr Handle => throw new NotImplementedException();

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public bool OnTouch(View v, MotionEvent e)
            {
                MotionEventActions action = e.Action;
                switch (action)
                {
                    case MotionEventActions.Down:
                        v.Parent.RequestDisallowInterceptTouchEvent(true);
                        break;

                    case MotionEventActions.Up:
                        // Allow ScrollView to intercept touch events.
                        v.Parent.RequestDisallowInterceptTouchEvent(false);
                        break;
                }

                // Handle ListView touch events.
                v.OnTouchEvent(e);
                return true;
            }
        }

        public void AddPlayer(object sender, EventArgs e)
        {
            AddPlayer();
            adapter = new InfoAdapter(this, Resource.Layout.player_prop, playersInfo);
            ListAdapter = adapter;
        }

        public void AddPlayer()
        {
            if (playersInfo.Count < MaxPlayerCount)
            {
                EditText et = (EditText)LayoutInflater.Inflate(Resource.Layout.edittext_dialog, null, false);
                AlertDialog.Builder ad = new AlertDialog.Builder(this);
                et.Text = GetFreeName();
                ad.SetTitle("Player's name");
                ad.SetView(et);
                var dialog = ad.Create();
                et.KeyPress += (sender, e) =>
                {
                    if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                    {
                        string name = et.Text;
                        HideKeyboard(et);
                        dialog.Dismiss();

                        ad = new AlertDialog.Builder(this);
                        ad.SetTitle("Player's type");
                        dialog = ad.Create();
                        ad.SetItems(System.Enum.GetValues(typeof(PlayerType)).Cast<PlayerType>().Select(_ => _.ToString()).ToArray(), (sender2, e2) => {
                            playersInfo.Add(new PlayerInfo(et.Text,  (PlayerType)e2.Which));
                            ((AlertDialog)sender2).Dismiss();
                            adapter.NotifyDataSetChanged();
                        });
                        ad.Show();
                    }
                    else
                    {
                        e.Handled = false;
                    }
                };
                dialog.Show();
                //dialog.DismissEvent += (a, b) => { HideKeyboard(et); };
                et.SelectAll();
                ShowKeyboard(et);
            }

            void ShowKeyboard(View pView)
            {
                pView.RequestFocus();

                InputMethodManager inputMethodManager = GetSystemService(InputMethodService) as InputMethodManager;
                inputMethodManager.ShowSoftInput(pView, ShowFlags.Forced);
                inputMethodManager.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);
            }

            void HideKeyboard(View pView)
            {
                InputMethodManager inputMethodManager = GetSystemService(InputMethodService) as InputMethodManager;
                inputMethodManager.HideSoftInputFromWindow(pView.WindowToken, HideSoftInputFlags.None);
            }
        }

        string GetFreeName()
        {
            for (int i = 1; ; ++i)
            {
                if (playersInfo.Count(_ => { return _.name == $"player {i}"; }) == 0)
                {
                    return $"player {i}";
                }
            }
        }

        public void RemovePlayer(object sender, EventArgs e)
        {
            if (playersInfo.Count > 0)
            {
                playersInfo.RemoveAt(playersInfo.Count - 1);
                adapter.NotifyDataSetChanged();
            }
        }

        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {          
            dict[seekBar].Value = progress;
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
                FieldHeight = widthBar.Value,
                FieldWidth = heightBar.Value,
                PortalCount = portalBar.Value,
                SwampCount = swampBar.Value,
                SwampSize = swampSizeBar.Value,
                Through = throughCheckBox.Checked,
            };

            PlayerHelper[] players = new PlayerHelper[playersInfo.Count];

            Dictionary<PlayerType, Func<IPlayerController>> dict = new Dictionary<PlayerType, Func<IPlayerController>>()
            {
                {PlayerType.Human, () => new SignalingPlayerContoller()},
                {PlayerType.RandomBot, () => new BotController(gameParameters)},
            };

            for (int i = 0; i < players.Length; i++)
                players[i] = new PlayerHelper(new BasicPlayerParameters(dict[playersInfo[i].type](), playersInfo[i].name, i));

            gameParameters.Players = players;
            Treasure.Game game = new Treasure.Game(gameParameters);
            if (game.InitializeField())
            {
                ActivityBridge.game = game;
                //Finish();
                StartActivity(typeof(GameActivity));
            }
            else
            {
                Toast.MakeText(this, "Ohh... Your parametres is so hard! Choose something else.", ToastLength.Short).Show();
            }           
        }
        

        class SeekBarWrap
        {
            public SeekBar seekBar;
            public TextView nameView;
            public TextView seekBarValue;
            public int Value
            {
                get
                {
                    return val;
                }
                set
                {
                    val = value + minVal;
                    seekBarValue.Text = val.ToString();
                }
            }

            private int val;
            private int minVal;
            private int maxVal;

            public SeekBarWrap(int minVal,int maxVal, SeekBar seekBar, TextView nameView, TextView seekBarValue, GameSettingsActivity activity)
            {
                this.seekBar = seekBar;
                this.nameView = nameView;
                this.seekBarValue = seekBarValue;
                this.minVal = minVal;
                this.maxVal = maxVal;

                seekBar.Max = maxVal - minVal;
                Value = 0;
                seekBar.SetOnSeekBarChangeListener(activity);
            }
        }

        class PlayerInfo
        {
            public string name;
            public PlayerType type;

            public PlayerInfo(string name, PlayerType type = PlayerType.Human)
            {
                this.name = name;
                this.type = type;
            }
        }

        class InfoAdapter : BaseAdapter
        {
            private Context context;
            private List<PlayerInfo> playersInfo = new List<PlayerInfo>();
            private int recourseId;

            public InfoAdapter(Context context, int recourseId, List<PlayerInfo> objects) : base()
            {
                playersInfo = objects;
                this.context = context;
                this.recourseId = recourseId;
            }

            public override int Count
            {
                get
                {
                    return playersInfo.Count;
                }
            }

            public override Java.Lang.Object GetItem(int pos)
            {
                throw new NotImplementedException();
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override View GetView(int pos, View v, ViewGroup parent)
            {
                v = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.player_prop, parent, false);

                TextView nameView = v.FindViewById<TextView>(Resource.Id.name);
                TextView typeView = v.FindViewById<TextView>(Resource.Id.type);

                var holder = v.Tag;

                if (nameView != null)
                {
                    nameView.Text = playersInfo[pos].name;
                }

                if (typeView != null)
                {
                    typeView.Text = playersInfo[pos].type.ToString();
                }
                return v;
            }
        }

        enum PlayerType
        {
            Human,
            RandomBot,
        }
    }
}