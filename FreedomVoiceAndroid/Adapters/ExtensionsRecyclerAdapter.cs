using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Entities;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    /// <summary>
    /// SelectExtension recycler view adapter
    /// </summary>
    public class ExtensionsRecyclerAdapter : RecyclerView.Adapter
    {
        private readonly Context _context;
        private List<Extension> _extensionsList;

        public ExtensionsRecyclerAdapter(Context context)
        {
            _context = context;
            _extensionsList = new List<Extension>();
        }

        public ExtensionsRecyclerAdapter(List<Extension> extensionsList, Context context)
        {
            _context = context;
            _extensionsList = extensionsList;
        }

        /// <summary>
        /// Extensions list
        /// </summary>
        public List<Extension> AccountsList
        {
            get { return _extensionsList; }
            set
            {
                _extensionsList = value;
                NotifyDataSetChanged();
            }
        }

        /// <summary>
        /// Get extension by position
        /// </summary>
        /// <param name="position">position number</param>
        /// <returns>extension entity</returns>
        public Extension GetExtension(int position)
        {
            return (_extensionsList.Count < position) ? null : _extensionsList[position];
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as ViewHolder;
            if (viewHolder == null) return;
            viewHolder.ExtensionName.Text = _extensionsList[position].ExtensionName;
            switch (_extensionsList[position].MailsCount)
            {
                case 0:
                    viewHolder.ExtensionInfo.Text = _context.GetString(Resource.String.ActivityExtension_noMessages);
                    viewHolder.ExtensionInfo.SetTextColor(_context.Resources.GetColor(Resource.Color.textColorSecondary));
                    return;
                case 1:
                    viewHolder.ExtensionInfo.Text = _context.GetString(Resource.String.ActivityExtension_oneMessage);
                    break;
                default:
                    viewHolder.ExtensionInfo.Text = $"{_extensionsList[position].MailsCount} {_context.GetString(Resource.String.ActivityExtension_moreMessages)}";
                    break;
            }
            viewHolder.ExtensionInfo.SetTextColor(_context.Resources.GetColor(Resource.Color.textColorNewMessages));
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_extension, parent, false);
            return new ViewHolder(itemView);
        }

        /// <summary>
        /// Extensions list length
        /// </summary>
        public override int ItemCount => _extensionsList?.Count ?? 0;

        /// <summary>
        /// Extension selection viewholder
        /// </summary>
        private class ViewHolder : RecyclerView.ViewHolder
        {
            /// <summary>
            /// Extension name field
            /// </summary>
            public TextView ExtensionName { get; private set; }

            /// <summary>
            /// Extension information field
            /// </summary>
            public TextView ExtensionInfo { get; private set; }

            public ViewHolder(View itemView) : base(itemView)
            {
                ExtensionName = itemView.FindViewById<TextView>(Resource.Id.itemExt_title);
                ExtensionInfo = itemView.FindViewById<TextView>(Resource.Id.itemExt_info);
            }
        }
    }
}