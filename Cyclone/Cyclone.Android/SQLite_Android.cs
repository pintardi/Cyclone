using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cyclone.Droid;
using Xamarin.Forms;
using SQLite.Net;

[assembly: Dependency(typeof(SQLite_Android))]

namespace Cyclone.Droid
{
    public class SQLite_Android : ISQLite
    {
        public SQLite.Net.SQLiteConnection GetConnection()
        {
            var filename = "Journey.db3";
            var documentspath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var path = Path.Combine(documentspath, filename);

            var platform = new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid();
            var connection = new SQLite.Net.SQLiteConnection(platform, path);
            return connection;
        }
    }
}