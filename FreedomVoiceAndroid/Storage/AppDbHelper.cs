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
        private const string DbName = "fvdb.db";
        private const int DbVersion = 2;

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

        public AppDbHelper(Context context) : base(context, DbName, null, DbVersion)
        {}

        public override void OnCreate(SQLiteDatabase db)
        {
            var accTableScript = $"create table {TableNameAccounts} ({ColumnPk} integer primary key autoencrement, {ColumnAccountName} integer not null, {ColumnAccountState} integer not null);";
            var callerTableScript = $"create table {TableNameCallerId} ({ColumnPk} integer primary key autoencrement, {ColumnCallerId} integer not null);";
            var accCallerLinkTableScript = $"create table {TableNameAccountCallerLink} ({ColumnPk} integer primary key autoencrement, {ColumnAccountLink} integer not null, {ColumnCallerIdLink} integer not null);";
            var recentsTableScript = $"create table {TableRecents} ({ColumnPk} integer primary key autoencrement, {ColumnPhone} integer not null, {ColumnDate} integer not null, {ColumnAccountLink} integer not null);";
            db.ExecSQL(accTableScript);
            db.ExecSQL(callerTableScript);
            db.ExecSQL(accCallerLinkTableScript);
            db.ExecSQL(recentsTableScript);
        }

        public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
        {
            if (newVersion == oldVersion) return;
            DropCache(db);
            OnCreate(db);
        }

        public void DropCache()
        {
            var db = WritableDatabase;
            DropCache(db);
            db.Close();
        }

        private void DropCache(SQLiteDatabase db)
        {
            var dropAccCallerScript = $"drop table if exists {TableNameAccountCallerLink};";
            var dropRecentsScript = $"drop table if exists {TableRecents};";
            var dropAccScript = $"drop table if exists {TableNameAccounts};";
            var dropCallerScript = $"drop table if exists {TableNameCallerId};";
            db.ExecSQL(dropAccCallerScript);
            db.ExecSQL(dropRecentsScript);
            db.ExecSQL(dropAccScript);
            db.ExecSQL(dropCallerScript);
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

        private void InsertPresentationNumbers(Account account, List<string> presentationNumbers, SQLiteDatabase db)
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
            var selection = $"SELECT * FROM {TableNameAccounts} WHERE {ColumnAccountState}={account.AccountName}";
            var cursor = db.RawQuery(selection, null);
            long index;
            if ((cursor == null) || (cursor.Count == 0))
                index = db.Insert(TableNameAccounts, null, content);
            else
            {
                cursor.MoveToFirst();
                index = cursor.GetColumnIndex(ColumnPk);
                cursor.Close();
                db.Update(TableNameAccounts, content, $"{ColumnPk}={index}", null);
            }
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
            
        }
    }
}