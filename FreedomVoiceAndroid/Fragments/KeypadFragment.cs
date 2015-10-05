using Android.OS;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Helpers;

namespace com.FreedomVoice.MobileApp.Android.Fragments
{
    /// <summary>
    /// Dialing keypad tab
    /// </summary>
    public class KeypadFragment : BasePagerFragment
    {
        private EditText _dialEdit;
        private ImageButton _backspaceButton;
        private Button _buttonOne;
        private Button _buttonTwo;
        private Button _buttonThree;
        private Button _buttonFour;
        private Button _buttonFive;
        private Button _buttonSix;
        private Button _buttonSeven;
        private Button _buttonEight;
        private Button _buttonNine;
        private Button _buttonZero;
        private Button _buttonStar;
        private Button _buttonHash;
        private Button _buttonDial;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.frag_keypad, container, false);
            _dialEdit = view.FindViewById<EditText>(Resource.Id.keypadFragment_dialText);
            _backspaceButton = view.FindViewById<ImageButton>(Resource.Id.keypadFragment_backspace);
            _buttonOne = view.FindViewById<Button>(Resource.Id.keypadFragment_buttonOne);
            _buttonTwo = view.FindViewById<Button>(Resource.Id.keypadFragment_buttonTwo);
            _buttonThree = view.FindViewById<Button>(Resource.Id.keypadFragment_buttonThree);
            _buttonFour = view.FindViewById<Button>(Resource.Id.keypadFragment_buttonFour);
            _buttonFive = view.FindViewById<Button>(Resource.Id.keypadFragment_buttonFive);
            _buttonSix = view.FindViewById<Button>(Resource.Id.keypadFragment_buttonSix);
            _buttonSeven = view.FindViewById<Button>(Resource.Id.keypadFragment_buttonSeven);
            _buttonEight = view.FindViewById<Button>(Resource.Id.keypadFragment_buttonEight);
            _buttonNine = view.FindViewById<Button>(Resource.Id.keypadFragment_buttonNine);
            _buttonZero = view.FindViewById<Button>(Resource.Id.keypadFragment_buttonZero);
            _buttonStar = view.FindViewById<Button>(Resource.Id.keypadFragment_buttonStar);
            _buttonHash = view.FindViewById<Button>(Resource.Id.keypadFragment_buttonHash);
            _buttonDial = view.FindViewById<Button>(Resource.Id.keypadFragment_buttonDial);
            return view;
        }

        public override void OnResume()
        {
            base.OnResume();
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            
        }
    }
}