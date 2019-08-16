using System;
using System.Timers;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Activities;
using com.FreedomVoice.MobileApp.Android.Adapters;
using com.FreedomVoice.MobileApp.Android.CustomControls;
using com.FreedomVoice.MobileApp.Android.CustomControls.Callbacks;
using com.FreedomVoice.MobileApp.Android.CustomControls.CustomEventArgs;
using com.FreedomVoice.MobileApp.Android.Helpers;
using Message = com.FreedomVoice.MobileApp.Android.Entities.Message;

namespace com.FreedomVoice.MobileApp.Android.Fragments
{
    /// <summary>
    /// Messages tab
    /// </summary>
    public class MessagesFragment : BasePagerFragment
    {
        private int _removedMsgIndex;

        private MessagesRecyclerAdapter _adapter;
        private RecyclerView _recyclerView;
        private ItemTouchHelper _swipeTouchHelper;
        private TextView _noMessagesTextView;
        private RelativeLayout _progressLayout;
        private Button _retryButton;
        private Timer _timer;

        protected override View InitView()
        {
            var view = Inflater.Inflate(Resource.Layout.frag_messages, null, false);
            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.messagesFragment_recyclerView);
            _recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            _recyclerView.AddItemDecoration(new DividerItemDecorator(Activity, Resource.Drawable.divider));
            _noMessagesTextView = view.FindViewById<TextView>(Resource.Id.messagesFragment_noResultText);
            _progressLayout = view.FindViewById<RelativeLayout>(Resource.Id.messagesFragment_progressLayout);
            _retryButton = view.FindViewById<Button>(Resource.Id.messagesFragment_retryButton);
            _retryButton.Click += RetryButtonOnClick;
            _adapter = new MessagesRecyclerAdapter(Context);
            _recyclerView.SetAdapter(_adapter);
            _adapter.ItemClick += MessageViewOnClick;

            var swipeListener = new SwipeCallback(0, ItemTouchHelper.Left | ItemTouchHelper.Right, ContentActivity, Resource.Color.colorRemoveList, Resource.Drawable.ic_action_delete);
            swipeListener.SwipeEvent += OnSwipeEvent;
            _swipeTouchHelper = new ItemTouchHelper(swipeListener);

            _timer = new Timer();
            _timer.Elapsed += TimerOnElapsed;
            return view;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            ContentActivity.RunOnUiThread(delegate
            {
                if (Helper.SelectedExtension == -1)
                    Helper.ForceLoadExtensions();
                else if (Helper.SelectedFolder == -1)
                    Helper.ForceLoadFolders();
                else if (Helper.SelectedMessage == -1)
                    Helper.ForceLoadMessages();
                RestartTimer();
            });
        }

        /// <summary>
        /// Message swipe
        /// </summary>
        private void OnSwipeEvent(object sender, SwipeCallbackEventArgs args)
        {
            _removedMsgIndex = args.ElementIndex;
            _adapter.RemoveItem(args.ElementIndex);
            if (Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].FolderName ==
                GetString(Resource.String.FragmentMessages_folderTrash))
                Helper.DeleteMessage(_removedMsgIndex);
            else
                Helper.RemoveMessage(_removedMsgIndex);
            Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].TotalMailsCount--;
            if (Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MessagesList[_removedMsgIndex].Unread)
            {
                Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MailsCount--;
                Helper.ExtensionsList[Helper.SelectedExtension].MailsCount--;
            }
            Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MessagesList.RemoveAt(_removedMsgIndex);
            if (_adapter.ItemCount != 0) return;
            if (_noMessagesTextView.Visibility == ViewStates.Invisible)
            {
                _noMessagesTextView.Text = GetString(Resource.String.FragmentMessages_no);
                _noMessagesTextView.Visibility = ViewStates.Visible;
            }
            if (_recyclerView.Visibility == ViewStates.Visible)
                _recyclerView.Visibility = ViewStates.Invisible;
        }

        /// <summary>
        /// Message item click
        /// </summary>
        private void MessageViewOnClick(object sender, int position)
        {
            if (position >= _adapter.CurrentContent.Count) return;
            _adapter.CurrentContent = Helper.GetNext(position);
            if (Helper.SelectedMessage != -1)
            {
                Intent intent;
                switch (Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MessagesList[Helper.SelectedMessage].MessageType)
                {
                    case Message.TypeVoice:
                        intent = new Intent(ContentActivity, typeof(VoiceMailActivity));
                        break;
                    case Message.TypeRec:
                        intent = new Intent(ContentActivity, typeof(VoiceRecordActivity));
                        break;
                    default:
                        intent = new Intent(ContentActivity, typeof(FaxActivity));
                        break;
                }
                StartActivity(intent);
                return;
            }
            ContentActivity.SetToolbarContent();
            if ((Helper.GetCurrent().Count == 0)&&(_progressLayout.Visibility == ViewStates.Gone))
                _progressLayout.Visibility = ViewStates.Visible;
            if ((Helper.GetCurrent().Count > 0) && (_progressLayout.Visibility == ViewStates.Visible))
                _progressLayout.Visibility = ViewStates.Gone;
            if (Helper.SelectedFolder != -1)
            {
                Helper.ForceLoadMessages();
                _swipeTouchHelper.AttachToRecyclerView(_recyclerView);
            }
            else if (Helper.SelectedExtension != -1)
                Helper.ForceLoadFolders();
            RestartTimer();
        }

        public override void OnResume()
        {
            base.OnResume();
            if (_retryButton.Visibility == ViewStates.Visible)
                _retryButton.Visibility = ViewStates.Invisible;
            if (_noMessagesTextView.Visibility == ViewStates.Visible)
            {
                _noMessagesTextView.Visibility = ViewStates.Invisible;
                _noMessagesTextView.Text = "";
            }
            _adapter.CurrentContent = Helper.GetCurrent();
            if (_adapter.CurrentContent.Count == 0)
            {
                if (_progressLayout.Visibility == ViewStates.Gone)
                    _progressLayout.Visibility = ViewStates.Visible;
            }
            else
            {
                if (_progressLayout.Visibility == ViewStates.Visible)
                    _progressLayout.Visibility = ViewStates.Gone;
            }
            if (Helper.SelectedAccount == null) return;
            if (Helper.SelectedFolder != -1)
                Helper.ForceLoadMessages();
            else if (Helper.SelectedExtension != -1)
                Helper.ForceLoadFolders();
            else
                Helper.ForceLoadExtensions();
            RestartTimer();
        }

        public override void OnPause()
        {
            base.OnPause();
            if (_timer.Enabled)
                _timer.Stop();
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            foreach (var code in args.Codes)
            {
                switch (code)
                {
                    case ActionsHelperEventArgs.MsgUpdated:
                        if (_timer.Enabled)
                            _timer.Stop();
                        if (this == ContentActivity.CurrentPagerFragment()) ContentActivity.SetToolbarContent();
                        if ((Helper.SelectedExtension == -1) || (Helper.SelectedFolder == -1))
                            _swipeTouchHelper.AttachToRecyclerView(null);
                        else
                            _swipeTouchHelper.AttachToRecyclerView(_recyclerView);

                        _adapter.CurrentContent = Helper.GetCurrent();
                        if ((Helper.SelectedFolder != -1) && (Helper.GetCurrent().Count == 0))
                        {
                            if (_noMessagesTextView.Visibility == ViewStates.Invisible)
                            {
                                _noMessagesTextView.Text = GetString(Resource.String.FragmentMessages_no);
                                _noMessagesTextView.Visibility = ViewStates.Visible;
                            }
                            if (_recyclerView.Visibility == ViewStates.Visible)
                                _recyclerView.Visibility = ViewStates.Invisible;
                            if (_progressLayout.Visibility == ViewStates.Visible)
                                _progressLayout.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            if (_noMessagesTextView.Visibility == ViewStates.Visible)
                            {
                                _noMessagesTextView.Text = "";
                                _noMessagesTextView.Visibility = ViewStates.Invisible;
                            }
                            if (_recyclerView.Visibility == ViewStates.Invisible)
                                _recyclerView.Visibility = ViewStates.Visible;
                            if (_progressLayout.Visibility == ViewStates.Visible)
                                _progressLayout.Visibility = ViewStates.Gone;
                        }
                        if (_retryButton.Visibility == ViewStates.Visible)
                            _retryButton.Visibility = ViewStates.Invisible;
                        RestartTimer();
                        break;
                    case ActionsHelperEventArgs.MsgUpdateFailed:
                        if ((_adapter.CurrentContent == null) || (_adapter.CurrentContent.Count == 0))
                        {
                            if (_noMessagesTextView.Visibility == ViewStates.Invisible)
                            {
                                _noMessagesTextView.Text = GetString(Resource.String.FragmentMessages_connection);
                                _noMessagesTextView.Visibility = ViewStates.Visible;
                            }
                            if (_recyclerView.Visibility == ViewStates.Visible)
                                _recyclerView.Visibility = ViewStates.Invisible;
                            if (_progressLayout.Visibility == ViewStates.Visible)
                                _progressLayout.Visibility = ViewStates.Gone;
                            if (_retryButton.Visibility == ViewStates.Invisible)
                                _retryButton.Visibility = ViewStates.Visible;
                        }
                        break;
                    case ActionsHelperEventArgs.MsgUpdateFailedInternal:
                        if ((_adapter.CurrentContent == null) || (_adapter.CurrentContent.Count == 0))
                        {
                            if (_noMessagesTextView.Visibility == ViewStates.Invisible)
                            {
                                _noMessagesTextView.Text = GetString(Resource.String.FragmentMessages_internal);
                                _noMessagesTextView.Visibility = ViewStates.Visible;
                            }
                            if (_recyclerView.Visibility == ViewStates.Visible)
                                _recyclerView.Visibility = ViewStates.Invisible;
                            if (_progressLayout.Visibility == ViewStates.Visible)
                                _progressLayout.Visibility = ViewStates.Gone;
                            if (_retryButton.Visibility == ViewStates.Invisible)
                                _retryButton.Visibility = ViewStates.Visible;
                        }
                        break;
                    case ActionsHelperEventArgs.MsgUpdateFailedAirplane:
                        if ((_adapter.CurrentContent == null) || (_adapter.CurrentContent.Count == 0))
                        {
                            if (_noMessagesTextView.Visibility == ViewStates.Invisible)
                            {
                                _noMessagesTextView.Text = GetString(Resource.String.FragmentMessages_airplane);
                                _noMessagesTextView.Visibility = ViewStates.Visible;
                            }
                            if (_recyclerView.Visibility == ViewStates.Visible)
                                _recyclerView.Visibility = ViewStates.Invisible;
                            if (_progressLayout.Visibility == ViewStates.Visible)
                                _progressLayout.Visibility = ViewStates.Gone;
                            if (_retryButton.Visibility == ViewStates.Invisible)
                                _retryButton.Visibility = ViewStates.Visible;
                        }
                        break;
                }
            }
        }

        private void RestartTimer()
        {
            if (_timer.Enabled) return;
            if (!(Math.Abs(_timer.Interval - 100) > -0.5)) return;
            if (!(Math.Abs(Helper.PollingInterval) > 0)) return;
            _timer.Interval = Helper.PollingInterval;
            _timer.Start();
        }

        private void RetryButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (_retryButton.Visibility != ViewStates.Visible) return;
            if (_progressLayout.Visibility == ViewStates.Gone)
                _progressLayout.Visibility = ViewStates.Visible;
            if (_noMessagesTextView.Visibility == ViewStates.Visible)
                _noMessagesTextView.Visibility = ViewStates.Invisible;
            _noMessagesTextView.Text = "";
            _retryButton.Visibility = ViewStates.Invisible;
            if (Helper.SelectedFolder != -1)
                Helper.ForceLoadMessages();
            else if (Helper.SelectedExtension != -1)
                Helper.ForceLoadFolders();
            else
                Helper.ForceLoadExtensions();
            RestartTimer();
        }
    }
}