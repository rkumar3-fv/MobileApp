using Android.Content;
using Android.Database.Sqlite;

namespace com.FreedomVoice.MobileApp.Android.Storage
{
    public class AppDbHelper : SQLiteOpenHelper
    {
        private const string DbName = "fvdb.db";
        private const int DbVersion = 1;

        private const string TableNameAccounts = "Accounts";
        private const string TableNameCallerId = "CallerIDs";
        private const string TableNameAccountCallerLink = "AccountsCallerIDs";
        private const string TableRecents = "Recents";
        private const string ColumnPk = "_id";

        private const string ColumnAccountName = "account";
        private const string ColumnCallerId = "callerid";
        private const string ColumnAccountLink = "idaccountlink";
        private const string ColumnCallerIdLink = "idcalleridlink";

        private const string ColumnPhone = "phone";
        private const string ColumnDate = "date";

        public AppDbHelper (Context context) : base(context, DbName, null, DbVersion)
        {}

        public override void OnCreate(SQLiteDatabase db)
        {
            var accTableScript = $"create table {TableNameAccounts} ({ColumnPk} integer primary key, {ColumnAccountName} integer not null);";
            var callerTableScript = $"create table {TableNameCallerId} ({ColumnPk} integer primary key, {ColumnCallerId} integer not null);";
            var accCallerLinkTableScript = $"create table {TableNameAccountCallerLink} ({ColumnPk} integer primary key, {ColumnAccountLink} integer not null, {ColumnCallerIdLink} integer not null);";
            var recentsTableScript = $"create table {TableRecents} ({ColumnPk} integer primary key, {ColumnPhone} integer not null, {ColumnDate} integer not null, {ColumnAccountLink} integer not null);";
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
    }
}