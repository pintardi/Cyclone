using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using Xamarin.Forms;
using Cyclone.iOS;

[assembly: Dependency(typeof(SQLite_iOS))]

namespace Cyclone.iOS
{
    public class SQLite_iOS : ISQLite
    {
        public SQLite.Net.SQLiteConnection GetConnection()
        {
            var fileName = "Journey.db3";
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var libraryPath = Path.Combine(documentsPath, "..", "Library");
            var path = Path.Combine(libraryPath, fileName);

            var platform = new SQLite.Net.Platform.XamarinIOS.SQLitePlatformIOS();
            var connection = new SQLite.Net.SQLiteConnection(platform, path);

            return connection;
        }
    }
}