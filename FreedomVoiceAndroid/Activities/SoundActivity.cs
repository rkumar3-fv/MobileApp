using com.FreedomVoice.MobileApp.Android.Utils;

namespace com.FreedomVoice.MobileApp.Android.Activities
{
    /// <summary>
    /// Message with audio content details
    /// </summary>
    public abstract class SoundActivity : MessageDetailsActivity
    {
        protected override void OnResume()
        {
            base.OnResume();
            MessageStamp.Text = DataFormatUtils.ToDuration(Msg.Length);
        }
    }
}