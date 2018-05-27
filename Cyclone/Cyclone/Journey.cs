using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace Cyclone
{
    public class Journey
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public double Distance { get; set; }

        public string Unit { get; set; }

        public Journey()
        {

        }
    }
}
