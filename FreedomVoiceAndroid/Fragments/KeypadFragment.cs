using System;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Telephony;
using Android.Util;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Adapters;
using com.FreedomVoice.MobileApp.Android.Dialogs;
using com.FreedomVoice.MobileApp.Android.Helpers;
using FreedomVoice.Core.Utils;

namespace com.FreedomVoice.MobileApp.Android.Fragments
{
    /// <summary>
    /// Dialing keypad tab
    /// </summary>
    public class KeypadFragment : BasePagerFragment
    {
        private string _enteredNumber="";
        private Spinner _idSpinner;
        private EditText _dialEdit;
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

        protected override View InitView()
        {
            var view = Infantler.Inflate(Resource.Layout.frag_keypad, null, false);
            _idSpinner = view.FindViewById<Spinner>(Resource.Id.keypadFragment_idSpinner);
            _idSpinner.ItemSelected += (sender, args) =>
            {
                Helper.SelectedAccount.SelectedPresentationNumber = args.Position;
                Log.Debug(App.AppPackage, $"PRESENTATION NUMBER SET to {DataFormatUtils.ToPhoneNumber(Helper.SelectedAccount.PresentationNumber)}");
            };
            _dialEdit = view.FindViewById<EditText>(Resource.Id.keypadFragment_dialText);
            _dialEdit.KeyListener = null;
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
            _buttonStar.Click += (sender, e) => { ButtonDigitOnClick(""); };
            _buttonHash = view.FindViewById<Button>(Resource.Id.keypadFragment_buttonHash);
            _buttonHash.Click += (sender, e) => { ButtonDigitOnClick(""); };
            _buttonDial = view.FindViewById<FloatingActionButton>(Resource.Id.keypadFragment_buttonDial);
            _buttonDial.Click += ButtonDialOnClick;
            return view;
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            var adapter = new CallerIdSpinnerAdapter(Activity, Helper.SelectedAccount.PresentationNumbers);
            _idSpinner.Adapter = adapter;
        }

        public override void OnResume()
        {
            base.OnResume();
            _idSpinner.SetSelection(Helper.SelectedAccount.SelectedPresentationNumber);
        }

        /// <summary>
        /// Digit button click event
        /// </summary>
        private void ButtonDigitOnClick(string s)
        {
            Log.Debug(App.AppPackage, $"KEYPAD: add {s}");
            _enteredNumber=_enteredNumber.Insert(_enteredNumber.Length, s);
            SetupNewText();
        }

        /// <summary>
        /// Dial click event
        /// </summary>
        private void ButtonDialOnClick(object sender, EventArgs e)
        {
            if ((Helper.PhoneNumber == null)||(Helper.PhoneNumber.Length == 0))
            {
                var noCellularDialog = new NoCellularDialogFragment();
                noCellularDialog.Show(ContentActivity.SupportFragmentManager, GetString(Resource.String.DlgCellular_title));
            }
            else
            {
                var normalizedNumber = PhoneNumberUtils.NormalizeNumber(_enteredNumber);
                Log.Debug(App.AppPackage, $"KEYPAD: dial to {DataFormatUtils.ToPhoneNumber(normalizedNumber)}");
                Helper.Call(normalizedNumber);
            }
        }

        /// <summary>
        /// Backspace click event
        /// </summary>
        private void BackspaceButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (_enteredNumber.Length > 0)
            {
                var newString = _enteredNumber.Substring(0, _enteredNumber.Length - 1);
                Log.Debug(App.AppPackage, $"KEYPAD: remove symbol {_enteredNumber.Substring(_enteredNumber.Length-1)}");
                _enteredNumber = newString;
                SetupNewText();
            }
            else
                Log.Debug(App.AppPackage, $"KEYPAD: nothing to remove");
        }

        /// <summary>
        /// Zero button long click event
        /// </summary>
        private void ButtonZeroOnLongClick(object sender, View.LongClickEventArgs longClickEventArgs)
        {
            Log.Debug(App.AppPackage, $"KEYPAD: + added");
            _enteredNumber = _enteredNumber.Insert(_enteredNumber.Length, "+");
            SetupNewText();
        }

        /// <summary>
        /// Backspace button long click event
        /// </summary>
        private void BackspaceButtonOnLongClick(object sender, View.LongClickEventArgs longClickEventArgs)
        {
            Log.Debug(App.AppPackage, $"KEYPAD: clear phone");
            _enteredNumber = "";
            SetupNewText();
        }

        private void SetupNewText()
        {
            _dialEdit.Text = DataFormatUtils.ToPhoneNumber(_enteredNumber);
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            
        }
    }
}