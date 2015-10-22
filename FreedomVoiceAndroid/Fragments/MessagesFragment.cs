using System;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Util;
using Android.Views;
using com.FreedomVoice.MobileApp.Android.Activities;
using com.FreedomVoice.MobileApp.Android.Adapters;
using com.FreedomVoice.MobileApp.Android.Entities;
using com.FreedomVoice.MobileApp.Android.Helpers;
using Message = com.FreedomVoice.MobileApp.Android.Entities.Message;

namespace com.FreedomVoice.MobileApp.Android.Fragments
{
    /// <summary>
    /// Messages tab
    /// </summary>
    public class MessagesFragment : BasePagerFragment, SwipeRefreshLayout.IOnRefreshListener
    {
        private int _removedMsgIndex;
        private bool _remove;

        private Snackbar _snackbar;
        private SnackbarCallback _snackCallback;
        private MessagesRecyclerAdapter _adapter;
        private RecyclerView _recyclerView;
        private SwipeRefreshLayout _swipeRefresh;
        private ItemTouchHelper _swipeTouchHelper;

        protected override View InitView()
        {
            var view = Infantler.Inflate(Resource.Layout.frag_messages, null, false);
            _swipeRefresh = view.FindViewById<SwipeRefreshLayout>(Resource.Id.messagesFragment_swipe);
            _swipeRefresh.SetOnRefreshListener(this);
            _swipeRefresh.SetColorSchemeResources(Resource.Color.colorPullRefreshFirst, Resource.Color.colorPullRefreshSecond, Resource.Color.colorPullRefreshThird);
            _recyclerView = view.FindViewById<RecyclerView>(Resource.Id.messagesFragment_recyclerView);
            _recyclerView.SetLayoutManager(new LinearLayoutManager(Activity));
            _recyclerView.AddItemDecoration(new DividerItemDecorator(Activity, Resource.Drawable.divider));
            _adapter = new MessagesRecyclerAdapter(Context);
            _recyclerView.SetAdapter(_adapter);
            _adapter.ItemClick += MessageViewOnClick;

            var swipeListener = new SwipeCallback(0, ItemTouchHelper.Left | ItemTouchHelper.Right, ContentActivity, Resource.Color.colorRemoveList, Resource.Drawable.ic_action_delete);
            swipeListener.SwipeEvent += OnSwipeEvent;
            _swipeTouchHelper = new ItemTouchHelper(swipeListener);

            _snackCallback = new SnackbarCallback();
            _snackCallback.SnackbarEvent += OnSnackbarDissmiss;
            return _recyclerView;
        }

        private void OnSnackbarDissmiss(object sender, EventArgs args)
        {
            if (_remove)
            {
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
            }
            else
            {
                Log.Debug(App.AppPackage, $"UNDO message {_removedMsgIndex}");
                _remove = false;
                _adapter.InsertItem(Helper.ExtensionsList[Helper.SelectedExtension].Folders[Helper.SelectedFolder].MessagesList[_removedMsgIndex], _removedMsgIndex);
            }
        }

        private void OnUndoClick(View view)
        {
            _remove = false;
        }

        /// <summary>
        /// Message swipe
        /// </summary>
        private void OnSwipeEvent(object sender, SwipeCallbackEventArgs args)
        {
            Log.Debug(App.AppPackage, $"SWIPED message {args.ElementIndex}");
            _remove = true;
            _removedMsgIndex = args.ElementIndex;
            //_snackbar = Snackbar.Make(View, Resource.String.FragmentMessages_remove, Snackbar.LengthLong).SetAction(Resource.String.FragmentMessages_removeUndo, OnUndoClick)
            //    .SetActionTextColor(ContextCompat.GetColor(ContentActivity, Resource.Color.colorUndoList)).SetCallback(_snackCallback);
            //_snackbar.Show();
            _adapter.RemoveItem(args.ElementIndex);
            //TODO: waiting for update support lib
            OnSnackbarDissmiss(this, null);
        }

        /// <summary>
        /// Message item click
        /// </summary>
        private void MessageViewOnClick(object sender, int position)
        {
            if (position > _adapter.CurrentContent.Count) return;
            Log.Debug(App.AppPackage, $"FRAGMENT {GetType().Name}: select item #{position}");
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
            }
            else if (Helper.SelectedFolder != -1)
            {
                Helper.ForceLoadMessages();
                _swipeTouchHelper.AttachToRecyclerView(_recyclerView);
            }
            else
                Helper.ForceLoadFolders();
            ContentActivity.SetToolbarContent();
        }

        public override void OnResume()
        {
            base.OnResume();
            TraceContent();
            _adapter.CurrentContent = Helper.GetCurrent();
        }

        protected override void OnHelperEvent(ActionsHelperEventArgs args)
        {
            foreach (var code in args.Codes)
            {
                switch (code)
                {
                    case ActionsHelperEventArgs.MsgUpdated:
                        TraceContent();
                        if (Helper.SelectedFolder == -1)
                            _swipeTouchHelper.AttachToRecyclerView(null);
                        else
                            _swipeTouchHelper.AttachToRecyclerView(_recyclerView);
                        _adapter.CurrentContent = Helper.GetCurrent();
                        _swipeRefresh.Refreshing = false;
                        break;
                }
            }
        }

        private void TraceContent()
        {
            foreach (var messageItem in Helper.GetCurrent())
            {
                switch (messageItem.GetType().Name)
                {
                    case "Extension":
                        var ext = (Extension)messageItem;
                        Log.Debug(App.AppPackage,
                            $"Extension {ext.ExtensionName} ({ext.Id}) - {ext.MailsCount} unread");
                        break;
                    case "Folder":
                        var fold = (Folder)messageItem;
                        Log.Debug(App.AppPackage,
                            $"Folder {fold.FolderName} - {fold.MailsCount} mails");
                        break;
                    case "Message":
                        var msg = (Message)messageItem;
                        Log.Debug(App.AppPackage,
                            $"Message {msg.Name} ({(msg.Unread?"Unread":"Old")}) - from: {msg.FromNumber} received {msg.MessageDate}");
                        break;
                }
            }
        }

        public void OnRefresh()
        {
            Helper.ForceLoadExtensions();
        }
    }
}