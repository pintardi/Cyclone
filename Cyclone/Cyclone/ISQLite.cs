using System;
using System.Collections.Generic;
using System.Text;
using SQLite.Net;

namespace Cyclone
{
    public interface ISQLite
    {
        SQLiteConnection GetConnection();
    }
}
