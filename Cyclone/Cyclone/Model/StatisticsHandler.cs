using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
using SQLite.Net;
using SQLitePCL;

namespace Cyclone.Model
{
    public class StatisticsHandler
    {
        public JourneyDB SHDatabase;
        
        
        public StatisticsHandler()
        {
            SHDatabase = new JourneyDB();
            SHJourneys = SHDatabase.GetJourneys();
            
        }

        public void UpdateTotalDistance()
        {
            SHDatabase = new JourneyDB();
            SHJourneys = SHDatabase.GetJourneys();
            TotalDistance = 0;

            //Add distance and duration from each data in the database
            foreach (var journey in SHJourneys)
            {
                TotalDistance += journey.Distance;
                TotalDuration += journey.Duration;
            }

            if(TotalDuration.TotalSeconds < 0.1)
            {
                AvgSpeed = 0;
            }
            else
            {
                AvgSpeed = 3.6 * (TotalDistance / TotalDuration.TotalSeconds) * 1000;
            }
            
            
        }
        
        public IEnumerable<Journey> SHJourneys { get; set; }
    
        public double TotalDistance { get; set; }

        public TimeSpan TotalDuration { get; set; }

        public double AvgSpeed { get; set; }
    


    }
}
