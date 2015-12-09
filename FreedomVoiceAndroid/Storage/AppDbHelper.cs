using System;
using System.Collections.Generic;
using Android.Content;
using Android.Database.Sqlite;
using com.FreedomVoice.MobileApp.Android.Entities;

namespace com.FreedomVoice.MobileApp.Android.Storage
{
    /// <summary>
    /// Database cache helper
    /// </summary>
    public class AppDbHelper : SQLiteOpenHelper
    {
        private static volatile AppDbHelper _instance;
        private static readonly object DbLocker = new object();
        private const string DbName = "fvdb.db";
        private const int DbVersion = 4;

        private const string TableNameAccounts = "Accounts";
        private const string TableNameCallerId = "CallerIDs";
        private const string TableNameAccountCallerLink = "AccountsCallerIDs";
        private const string TableRecents = "Recents";
        private const string ColumnPk = "_id";

        private const string ColumnAccountName = "account";
        private const string ColumnAccountState = "accstate";
        private const string ColumnCallerId = "callerid";
        private const string ColumnAccountLink = "idaccountlink";
        private const string ColumnCallerIdLink = "idcalleridlink";

        private const string ColumnPhone = "phone";
        private const string ColumnDate = "date";

        /// <summary>
        /// Get application DB helper instance
        /// </summary>
        public static AppDbHelper Instance(Context context)
        {
            if (_instance != null) return _instance;
            lock (DbLocker)
            {
                if (_instance == null)
                    _instance = new AppDbHelper(context);
            }
            return _instance;
        }


        private AppDbHelper(Context context) : base(context, DbName, null, DbVersion)
        {}

        public override void OnCreate(SQLiteDatabase db)
        {
            var accTableScript = $"create table {TableNameAccounts} ({ColumnPk} integer primary key autoincrement, {ColumnAccountName} integer not null, {ColumnAccountState} integer not null);";
            var callerTableScript = $"create table {TableNameCallerId} ({ColumnPk} integer primary key autoincrement, {ColumnCallerId} integer not null);";
            var accCallerLinkTableScript = $"create table {TableNameAccountCallerLink} ({ColumnPk} integer primary key autoincrement, {ColumnAccountLink} integer not null, {ColumnCallerIdLink} integer not null);";
            var recentsTableScript = $"create table {TableRecents} ({ColumnPk} integer primary key autoincrement, {ColumnPhone} integer not null, {ColumnDate} integer not null, {ColumnAccountLink} integer not null);";
            db.ExecSQL(accTableScript);
            db.ExecSQL(callerTableScript);
            db.ExecSQL(accCallerLinkTableScript);
            db.ExecSQL(recentsTableScript);
        }

        public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
        {
            if (newVersion == oldVersion) return;
            var dropAccCallerScript = $"drop table if exists {TableNameAccountCallerLink};";
            var dropRecentsScript = $"drop table if exists {TableRecents};";
            var dropAccScript = $"drop table if exists {TableNameAccounts};";
            var dropCallerScript = $"drop table if exists {TableNameCallerId};";
            db.ExecSQL(dropAccCallerScript);
            db.ExecSQL(dropRecentsScript);
            db.ExecSQL(dropAccScript);
            db.ExecSQL(dropCallerScript);
            OnCreate(db);
        }

        /// <summary>
        /// Drop saved cache
        /// </summary>
        public void DropCache()
        {
            var db = WritableDatabase;
            var dropAccCallerScript = $"delete from {TableNameAccountCallerLink};";
            var dropRecentsScript = $"delete from {TableRecents};";
            var dropAccScript = $"delete from {TableNameAccounts};";
            var dropCallerScript = $"delete from {TableNameCallerId};";
            db.ExecSQL(dropAccCallerScript);
            db.ExecSQL(dropRecentsScript);
            db.ExecSQL(dropAccScript);
            db.ExecSQL(dropCallerScript);
            db.Close();
        }

        public IEnumerable<Recent> GetRecents(string accountName)
        {
            var res = new List<Recent>();
            var selection = $"SELECT * FROM {TableRecents} WHERE {ColumnAccountLink} IN (SELECT {ColumnPk} FROM {TableNameAccounts} WHERE {ColumnAccountName}='{accountName}') ORDER BY {ColumnDate} DESC";
            var db = ReadableDatabase;
            var cursor = db.RawQuery(selection, null);
            if ((cursor != null) && (cursor.Count > 0))
            {
                var phoneColumnIndex = cursor.GetColumnIndex(ColumnPhone);
                var dateColumnIndex = cursor.GetColumnIndex(ColumnDate);
                cursor.MoveToFirst();
                while (!cursor.IsAfterLast)
                {
                    var phone = cursor.GetString(phoneColumnIndex);
                    var date = DateTime.FromFileTimeUtc(cursor.GetLong(dateColumnIndex));
                    res.Add(new Recent(phone, date));
                    cursor.MoveToNext();
                }
            }
            cursor?.Close();
            db.Close();
            return res;
        }

        /// <summary>
        /// Single recent insertion
        /// </summary>
        /// <param name="accountName">selected account</param>
        /// <param name="recent">single recent</param>
        public void InsertRecent(string accountName, Recent recent)
        {
            var selection = $"SELECT {ColumnPk} FROM {TableNameAccounts} WHERE {ColumnAccountName}='{accountName}'";
            var content = new ContentValues();
            content.Put(ColumnPhone, recent.PhoneNumber);
            content.Put(ColumnDate, recent.CallDate.ToFileTimeUtc());
            var db = WritableDatabase;
            var cursor = db.RawQuery(selection, null);
            if ((cursor == null) || (cursor.Count == 0))
            {
                cursor?.Close();
                return;
            }
            cursor.MoveToFirst();
            var index = cursor.GetLong(cursor.GetColumnIndex(ColumnPk));
            if (index != -1)
            {
                content.Put(ColumnAccountLink, index);
                db.Insert(TableRecents, null, content);
            }
            db.Close();
        }

        public void RemoveRecent(string accountName, string phone)
        {
            RemoveRecents(accountName, phone);
        }

        /// <summary>
        /// Insert list of recents
        /// </summary>
        /// <param name="accountName">account</param>
        /// <param name="recents">recents list</param>
        public void InsertRecents(string accountName, IEnumerable<Recent> recents)
        {
            var selection = $"SELECT {ColumnPk} FROM {TableNameAccounts} WHERE {ColumnAccountName}='{accountName}'";
            var db = WritableDatabase;
            var cursor = db.RawQuery(selection, null);
            if ((cursor == null) || (cursor.Count == 0))
            {
                cursor?.Close();
                return;
            }
            cursor.MoveToFirst();
            var index = cursor.GetLong(cursor.GetColumnIndex(ColumnPk));
            if (index != -1)
            {
                foreach (var recent in recents)
                {
                    var content = new ContentValues();
                    content.Put(ColumnPhone, recent.PhoneNumber);
                    content.Put(ColumnDate, recent.CallDate.ToFileTimeUtc());
                    content.Put(ColumnAccountLink, index);
                    db.Insert(TableRecents, null, content);
                }             
            }
            db.Close();
        }

        /// <summary>
        /// Insert new accounts or update old
        /// </summary>
        /// <param name="accountsList">Accounts list</param>
        public void InsertAccounts(List<Account> accountsList)
        {
            var db = WritableDatabase;
            foreach (var account in accountsList)
            {
                InsertSingleAccount(account, db);
            }
            db.Close();
        }

        /// <summary>
        /// Insert new presentation numbers or update old
        /// </summary>
        /// <param name="account">Account for presentation numbers</param>
        /// <param name="presentationNumbers">Presentation numbers list</param>
        public void InsertPresentationNumbers(Account account, List<string> presentationNumbers)
        {
            var db = WritableDatabase;
            InsertPresentationNumbers(account, presentationNumbers, db);
            db.Close();
        }

        /// <summary>
        /// Insertion without link
        /// </summary>
        /// <param name="presentationNumbers">presentation numbers list</param>
        public void InsertPresentationNumbers(IEnumerable<string> presentationNumbers)
        {
            var db = WritableDatabase;
            foreach (var presentationNumber in presentationNumbers)
            {
                var content = new ContentValues();
                content.Put(ColumnCallerId, presentationNumber);
                db.Insert(TableNameCallerId, null, content);
            }
            db.Close();
        }

        private void InsertPresentationNumbers(Account account, IEnumerable<string> presentationNumbers, SQLiteDatabase db)
        {
            var selection = $"SELECT * FROM {TableNameAccounts} WHERE {ColumnAccountState}={account.AccountName}";
            var cursor = db.RawQuery(selection, null);
            if ((cursor == null) || (cursor.Count == 0))
                return;
            
            long index;
            try
            {
                cursor.MoveToFirst();
                index = cursor.GetColumnIndex(ColumnPk);
                cursor.Close();
            }
            catch (Exception)
            {
                index = -1;
            }
            if (index == -1) return;
            foreach (var presentationNumber in presentationNumbers)
            {
                InsertSingleCallerId(index, presentationNumber, db);
            }
        }

        /// <summary>
        /// Single account insertion implementation
        /// </summary>
        /// <param name="account">Account</param>
        /// <param name="db">Writable database</param>
        private void InsertSingleAccount(Account account, SQLiteDatabase db)
        {
            var content = new ContentValues();
            content.Put(ColumnAccountName, account.AccountName);
            content.Put(ColumnAccountState, account.AccountState?1:0);
            var selection = $"SELECT * FROM {TableNameAccounts} WHERE {ColumnAccountName}='{account.AccountName}'";
            var cursor = db.RawQuery(selection, null);
            long index;
            if ((cursor == null) || (cursor.Count == 0))
                index = db.Insert(TableNameAccounts, null, content);
            else
            {
                cursor.MoveToFirst();
                index = cursor.GetColumnIndex(ColumnPk);  
                db.Update(TableNameAccounts, content, $"{ColumnPk}={index}", null);
            }
            cursor?.Close();
            if ((account.PresentationNumbers == null) || (account.PresentationNumbers.Count <= 0) || index == -1)
                return;
            foreach (var presentationNumber in account.PresentationNumbers)
            {
                InsertSingleCallerId(index, presentationNumber, db);
            }
        }

        /// <summary>
        /// Single Caller ID insertion implementation
        /// </summary>
        /// <param name="accountId">Account PK</param>
        /// <param name="callerId">Caller ID</param>
        /// <param name="db">Writable database</param>
        private void InsertSingleCallerId(long accountId, string callerId, SQLiteDatabase db)
        {
            var content = new ContentValues();
            content.Put(ColumnCallerId, callerId);
            var selection = $"SELECT * FROM {TableNameCallerId} WHERE {ColumnCallerId}='{callerId}'";
            var cursor = db.RawQuery(selection, null);
            long index;
            if ((cursor == null) || (cursor.Count == 0))
                index = db.Insert(TableNameCallerId, null, content);
            else
            {
                cursor.MoveToFirst();
                index = cursor.GetColumnIndex(ColumnPk);
                db.Update(TableNameCallerId, content, $"{ColumnPk}={index}", null);
            }
            cursor?.Close();
            var contentLink = new ContentValues();
            contentLink.Put(ColumnAccountLink, accountId);
            contentLink.Put(ColumnCallerIdLink, index);
            var linkSelection = $"SELECT * FROM {TableNameAccountCallerLink} WHERE ({ColumnAccountLink}={accountId}) AND ({ColumnCallerIdLink}={index})";
            var cursorLink = db.RawQuery(linkSelection, null);
            if ((cursorLink == null) || (cursorLink.Count == 0))
                db.Insert(TableNameAccountCallerLink, null, contentLink);
            cursorLink?.Close();
        }

        /// <summary>
        /// Get accounts list
        /// </summary>
        /// <returns>Accounts list</returns>
        public List<Account> GetAccounts()
        {
            var selectionQuery = $"select * from {TableNameAccounts}";
            var accountsList = new List<Account>();
            var db = ReadableDatabase;
            var accountsCursor = db.RawQuery(selectionQuery, null);
            if ((accountsCursor != null) && (accountsCursor.Count > 0))
            {
                var columnPkIndex = accountsCursor.GetColumnIndex(ColumnPk);
                var columnNameIndex = accountsCursor.GetColumnIndex(ColumnAccountName);
                var columnStateIndex = accountsCursor.GetColumnIndex(ColumnAccountState);
                accountsCursor.MoveToFirst();
                while (!accountsCursor.IsAfterLast)
                {
                    var accountId = accountsCursor.GetLong(columnPkIndex);
                    var accountName = accountsCursor.GetString(columnNameIndex);
                    var accountState = accountsCursor.GetLong(columnStateIndex) == 1;
                    var callerIds = GetCallerIds(db, accountId);
                    accountsList.Add(new Account(accountName, callerIds, accountState));
                    accountsCursor.MoveToNext();
                }
            }
            accountsCursor?.Close();
            db.Close();
            return accountsList;
        }

        /// <summary>
        /// Get all caller IDs
        /// </summary>
        /// <returns>caller IDs list</returns>
        public List<string> GetCallerIds()
        {
            var selectionQuery = $"select * from {TableNameCallerId}";
            var iDsList = new List<string>();
            var db = ReadableDatabase;
            var cursor = db.RawQuery(selectionQuery, null);
            if ((cursor != null) && (cursor.Count > 0))
            {
                var columnCallerIdIndex = cursor.GetColumnIndex(ColumnCallerId);
                cursor.MoveToFirst();
                while (!cursor.IsAfterLast)
                {
                    iDsList.Add(cursor.GetString(columnCallerIdIndex));
                    cursor.MoveToNext();
                }
            }
            cursor?.Close();
            db.Close();
            return iDsList;
        } 

        /// <summary>
        /// Get caller IDs for selected account
        /// </summary>
        /// <param name="accountName">selected account name</param>
        /// <returns>List of caller IDs</returns>
        public List<string> GetCallerIds(string accountName)
        {
            List<string> res;
            var db = ReadableDatabase;
            var selectionQuery = $"select * from {TableNameAccounts} where {ColumnAccountName}={accountName}";
            var cursor = db.RawQuery(selectionQuery, null);
            if ((cursor != null) && (cursor.Count > 0))
            {
                cursor.MoveToFirst();
                var id = cursor.GetLong(cursor.GetColumnIndex(ColumnPk));
                cursor.Close();
                res = GetCallerIds(db, id);
            }
            else
            {
                cursor?.Close();
                res = new List<string>();
            }
            db.Close();
            return res;
        }

        /// <summary>
        /// Remove caller IDs by account
        /// </summary>
        /// <param name="accountName">account name</param>
        public void RemoveCallerIds(string accountName)
        {
            var db = WritableDatabase;
            var selectionQuery = $"select {ColumnPk} from {TableNameAccounts} where {ColumnAccountName}='{accountName}'";
            var cursor = db.RawQuery(selectionQuery, null);
            if ((cursor!=null) && (cursor.Count>0))
            {
                cursor.MoveToFirst();
                var index = cursor.GetLong(cursor.GetColumnIndex(ColumnPk));
                var removeQuery = $"delete from {TableNameCallerId} where {ColumnPk} in (select {ColumnCallerIdLink} from {TableNameAccountCallerLink} where {ColumnAccountLink}={index});";
                var removeLinksQurey = $"delete from {TableNameAccountCallerLink} where {ColumnAccountLink}={index};";
                db.ExecSQL(removeQuery);
                db.ExecSQL(removeLinksQurey);
            }
            cursor?.Close();
            db.Close();
        }

        /// <summary>
        /// Remove recents for selected account
        /// </summary>
        /// <param name="accountName">Account name</param>
        public void RemoveRecents(string accountName)
        {
            RemoveRecents(accountName, null);
        }
        
        private void RemoveRecents(string accountName, string name)
        {
            var db = WritableDatabase;
            var selectionQuery = $"select {ColumnPk} from {TableNameAccounts} where {ColumnAccountName}='{accountName}'";
            var cursor = db.RawQuery(selectionQuery, null);
            if ((cursor != null) && (cursor.Count > 0))
            {
                cursor.MoveToFirst();
                var index = cursor.GetLong(cursor.GetColumnIndex(ColumnPk));
                string removeQuery;
                if (!string.IsNullOrEmpty(name))
                    removeQuery = $"delete from {TableRecents} where (({ColumnAccountLink}={index}) AND ({ColumnPhone}='{name}'));";
                else
                    removeQuery = $"delete from {TableRecents} where {ColumnAccountLink}={index};";
                db.ExecSQL(removeQuery);
            }
            cursor?.Close();
            db.Close();
        }

        /// <summary>
        /// Drop all recents in database
        /// </summary>
        public void DropRecents()
        {
            var db = WritableDatabase;
            var removeQuery = $"delete from {TableRecents};";
            db.ExecSQL(removeQuery);
            db.Close();
        }

        /// <summary>
        /// Drop caller IDs with links
        /// </summary>
        public void DropCallerIds()
        {
            var db = WritableDatabase;
            var removeQuery = $"delete from {TableNameCallerId};";
            var removeLinksQurey = $"delete from {TableNameAccountCallerLink};";
            db.ExecSQL(removeQuery);
            db.ExecSQL(removeLinksQurey);
            db.Close();
        }

        private List<string> GetCallerIds(SQLiteDatabase db, long accountId)
        {
            var result = new List<string>();
            var selectionQuery = $"select * from {TableNameCallerId} where {ColumnPk} IN (select {ColumnCallerIdLink} from {TableNameAccountCallerLink} where {ColumnAccountLink}={accountId})";
            var callerCursor = db.RawQuery(selectionQuery, null);
            if ((callerCursor != null) && (callerCursor.Count > 0))
            {
                var columnCallerIdIndex = callerCursor.GetColumnIndex(ColumnCallerId);
                callerCursor.MoveToFirst();
                while (!callerCursor.IsAfterLast)
                {
                    result.Add(callerCursor.GetString(columnCallerIdIndex));
                    callerCursor.MoveToNext();
                }
            }
            callerCursor?.Close();
            return result;
        }
    }
}