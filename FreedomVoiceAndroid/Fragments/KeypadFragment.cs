using System;
using System.Linq;
using Android.Support.Design.Widget;
#if DEBUG
using Android.Util;
#endif
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Helpers;
using FreedomVoice.Core.Utils;

namespace com.FreedomVoice.MobileApp.Android.Fragments
{
    /// <summary>
    /// Dialing keypad tab
    /// </summary>
    public class KeypadFragment : CallerFragment
    {
        private string _enteredNumber="";
        private TextView _dialEdit;
        private ImageButton _backspaceButton;
        private LinearLayout _buttonOne;
        private LinearLayout _buttonTwo;
        private LinearLayout _buttonThree;
        private LinearLayout _buttonFour;
        private LinearLayout _buttonFive;
        private LinearLayout _buttonSix;
        private LinearLayout _buttonSeven;
        private LinearLayout _buttonEight;
        private LinearLayout _buttonNine;
        private LinearLayout _buttonZero;
        private Button _buttonStar;
        private Button _buttonHash;
        private FloatingActionButton _buttonDial;
        private bool _cleanOnRestore;

        protected override View InitView()
        {
            var view = Inflater.Inflate(Resource.Layout.frag_keypad, null, false);
            IdSpinner = view.FindViewById<Spinner>(Resource.Id.keypadFragment_idSpinner);
            SingleId = view.FindViewById<TextView>(Resource.Id.keypadFragment_singleId);
            _dialEdit = view.FindViewById<TextView>(Resource.Id.keypadFragment_dialText);
            _backspaceButton = view.FindViewById<ImageButton>(Resource.Id.keypadFragment_backspace);
            _backspaceButton.Click += BackspaceButtonOnClick;
            _backspaceButton.LongClick += BackspaceButtonOnLongClick;
            _buttonOne = view.FindViewById<LinearLayout>(Resource.Id.keypadFragment_buttonOne);
            _buttonOne.Click += (sender, e) => {ButtonDigitOnClick("1");};
            _buttonTwo = view.FindViewById<LinearLayout>(Resource.Id.keypadFragment_buttonTwo);
            _buttonTwo.Click += (sender, e) => { ButtonDigitOnClick("2"); };
            _buttonThree = view.FindViewById<LinearLayout>(Resource.Id.keypadFragment_buttonThree);
            _buttonThree.Click += (sender, e) => { ButtonDigitOnClick("3"); };
            _buttonFour = view.FindViewById<LinearLayout>(Resource.Id.keypadFragment_buttonFour);
            _buttonFour.Click += (sender, e) => { ButtonDigitOnClick("4"); };
            _buttonFive = view.FindViewById<LinearLayout>(Resource.Id.keypadFragment_buttonFive);
            _buttonFive.Click += (sender, e) => { ButtonDigitOnClick("5"); };
            _buttonSix = view.FindViewById<LinearLayout>(Resource.Id.keypadFragment_buttonSix);
            _buttonSix.Click += (sender, e) => { ButtonDigitOnClick("6"); };
            _buttonSeven = view.FindViewById<LinearLayout>(Resource.Id.keypadFragment_buttonSeven);
            _buttonSeven.Click += (sender, e) => { ButtonDigitOnClick("7"); };
            _buttonEight = view.FindViewById<LinearLayout>(Resource.Id.keypadFragment_buttonEight);
            _buttonEight.Click += (sender, e) => { ButtonDigitOnClick("8"); };
            _buttonNine = view.FindViewById<LinearLayout>(Resource.Id.keypadFragment_buttonNine);
            _buttonNine.Click += (sender, e) => { ButtonDigitOnClick("9"); };
            _buttonZero = view.FindViewById<LinearLayout>(Resource.Id.keypadFragment_buttonZero);
            _buttonZero.Click += (sender, e) => { ButtonDigitOnClick("0"); };
            _buttonZero.LongClick += ButtonZeroOnLongClick;
            _buttonStar = view.FindViewById<Button>(Resource.Id.keypadFragment_buttonStar);
            _buttonStar.Click += (sender, e) => { ButtonDigitOnClick("*"); };
            _buttonHash = view.FindViewById<Button>(Resource.Id.keypadFragment_buttonHash);
            _buttonHash.Click += (sender, e) => { ButtonDigitOnClick("#"); };
            _buttonDial = view.FindViewById<FloatingActionButton>(Resource.Id.keypadFragment_buttonDial);
            _buttonDial.Click += ButtonDialOnClick;
            return view;
        }

        public override void OnResume()
        {
            base.OnResume();
            if (!_cleanOnRestore) return;
            _enteredNumber = "";
            SetupNewText();
            _cleanOnRestore = false;
        }

        /// <summary>
        /// Digit button click event
        /// </summary>
        private void ButtonDigitOnClick(string s)
        {
#if DEBUG
            Log.Debug(App.AppPackage, $"KEYPAD: add {s}");
#endif
            _enteredNumber=_enteredNumber.Insert(_enteredNumber.Length, s);
            SetupNewText();
        }

        /// <summary>
        /// Dial click event
        /// </summary>
        private void ButtonDialOnClick(object sender, EventArgs e)
        {
            if ((_enteredNumber.Length == 0)&&(Helper.RecentsDictionary != null)&&(Helper.RecentsDictionary.Count>0))
            {
                var first = Helper.RecentsDictionary.Values.First().PhoneNumber;
                _enteredNumber = first;
                SetupNewText();
            }
            else
                ContentActivity.Call(_enteredNumber);
        }

        /// <summary>
        /// Backspace click event
        /// </summary>
        private void BackspaceButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (_enteredNumber.Length > 0)
            {
                var newString = _enteredNumber.Substring(0, _enteredNumber.Length - 1);
#if DEBUG
                Log.Debug(App.AppPackage, $"KEYPAD: remove symbol {_enteredNumber.Substring(_enteredNumber.Length-1)}");
#endif
                _enteredNumber = newString;
                SetupNewText();
            }
#if DEBUG
            else
                Log.Debug(App.AppPackage, "KEYPAD: nothing to remove");
#endif
        }

        /// <summary>
        /// Zero button long click event
        /// </summary>
        private void ButtonZeroOnLongClick(object sender, View.LongClickEventArgs longClickEventArgs)
        {
#if DEBUG
            Log.Debug(App.AppPackage, "KEYPAD: + added");
#endif
            _enteredNumber = _enteredNumber.Insert(_enteredNumber.Length, "+");
            SetupNewText();
        }

        /// <summary>
        /// Backspace button long click event
        /// </summary>
        private void BackspaceButtonOnLongClick(object sender, View.LongClickEventArgs longClickEventArgs)
        {
#if DEBUG
            Log.Debug(App.AppPackage, "KEYPAD: clear phone");
#endif
            _enteredNumber = "";
            SetupNewText();
        }

        private void SetupNewText()
        {
            _dialEdit.Text = DataFormatUtils.ToPhoneNumber(_enteredNumber);
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            base.OnHelperEvent(args);
            foreach (var code in args.Codes)
            {
                switch (code)
                {
                    case ActionsHelperEventArgs.CallReservationOk:
                        _cleanOnRestore = true;
                        break;
                }
            }
        }
    }
}