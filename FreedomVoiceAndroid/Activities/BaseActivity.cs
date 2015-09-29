using Android.OS;
using Android.Support.V7.App;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    public abstract class BaseActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }
    }
}