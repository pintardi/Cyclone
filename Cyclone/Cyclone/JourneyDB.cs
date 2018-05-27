using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SQLite.Net;
using Xamarin.Forms;
namespace Cyclone
{

    public class JourneyDB
    {
        private SQLiteConnection _sqlconnection;

        public JourneyDB()
        {
            //Usage of DependencyService to let the app know that the implementation has to be taken from either Android or iOS folder
            _sqlconnection = DependencyService.Get<ISQLite>().GetConnection();
            _sqlconnection.CreateTable<Journey>();
        }

        public IEnumerable<Journey> GetJourneys()
        {
            return (from t in _sqlconnection.Table<Journey>() select t).ToList();
        }

        public void AddJourney(Journey journey)
        {
            _sqlconnection.Insert(journey);
        }

        
    }
}
