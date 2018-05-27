using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace Cyclone.Model
{
    public class JourneyHandler
    {
        Journey JHJourney;
        JourneyDB JHJourneyDB;
        Stopwatch JHStopwatch = new Stopwatch();
        double JHWheelRevolutionOld, JHWheelRevolutionNew;
        int JHIsRunning = 0;
        //int JHWheelRevolution[5];
        List<double> _jwhrold = new List<double>();
        List<double> _jwhrnew = new List<double>();
        List<double> _jwhltold = new List<double>();
        List<double> _jwhltnew = new List<double>();
        List<double> _jspeed = new List<double>();
        //int JH
        double JHSpeedOld = 0.0;
        double JHPolePairs;
        double JHLastWheelEventTimeOld, JHLastWheelEventTimeNew;
        //double JHTimeSpanBetweenCall;
        

        public JourneyHandler()
        {
            JHJourney = new Journey();
            StartDistance = 0;
            JHWheelRevolutionOld = 0;
            JHWheelRevolutionNew = 0;
            JHLastWheelEventTimeNew = 0;
            JHLastWheelEventTimeOld = 0;

        }

        public void StartJourney()
        {
            JHStopwatch.Restart();
            JHIsRunning = 1;
            JHJourney.StartTime = DateTime.Now;
            StartDistance = SensorDistance;
        }

        public void StopJourney()
        {

            JHStopwatch.Stop();
            JHJourney.Duration = JHStopwatch.Elapsed;
            JHJourney.Distance = Distance;
            JHJourneyDB = new JourneyDB();
            JHJourneyDB.AddJourney(JHJourney);
            JHIsRunning = 0;
            StartDistance = 0;
            
        }

        public double StartDistance { get; set; }

        public double SensorDistance { get; set; }

        public double Distance { get; set; }

        public int JourneyIsRunning { get; set; }

        public double UpdateDistance(string CumulativeWheelRevolution, string WheelRadius, string PolePairs)
        {
            int help_cumulativewheelrevolution, help_wheelradius, help_polepairs;

            int.TryParse(CumulativeWheelRevolution, out help_cumulativewheelrevolution);
            
            int.TryParse(WheelRadius, out help_wheelradius);
            int.TryParse(PolePairs, out help_polepairs);
            SensorDistance = (((help_cumulativewheelrevolution) * (help_wheelradius*0.001))/help_polepairs)*0.001 ; // Formula for the Distance  
            
          

            if(JHIsRunning == 0 && StartDistance == 0)
            {
                return SensorDistance;
            }
            else 
            {
                Distance = SensorDistance - StartDistance;
                return Distance;
            }
        }

        public TimeSpan UpdateDuration()
        {
            if(JHStopwatch.IsRunning)
            {
                return JHStopwatch.Elapsed;
            }
            else
            {
                TimeSpan _timespan = new TimeSpan();
                return _timespan; //return 00:00:00;
            }
        }

        public double UpdateSpeed(string CumulativeWheelRevolution, string LastWheelEventTime, string WheelRadius, string PolePairs)
        {
            
            int help_cumulativewheelrevolution, help_lastwheeleventtime, help_wheelradius, help_polepairs;

            int.TryParse(CumulativeWheelRevolution, out help_cumulativewheelrevolution);
            int.TryParse(LastWheelEventTime, out help_lastwheeleventtime);
            int.TryParse(WheelRadius, out help_wheelradius);
            int.TryParse(PolePairs, out help_polepairs);
            JHWheelRevolutionNew = help_cumulativewheelrevolution;
            JHLastWheelEventTimeNew = help_lastwheeleventtime;
            
            double TimeSpanBetweenCall;
            double RevolutionBetweenCall = JHWheelRevolutionNew - JHWheelRevolutionOld;

            //Prevent an overflow
            if(JHLastWheelEventTimeOld > JHLastWheelEventTimeNew)
            {
                TimeSpanBetweenCall = (JHLastWheelEventTimeNew + 65536) - JHLastWheelEventTimeOld;
            }
            else
            {
                TimeSpanBetweenCall = JHLastWheelEventTimeNew - JHLastWheelEventTimeOld;
            }
            _jwhltold.Add(JHLastWheelEventTimeOld);
            _jwhltnew.Add(JHLastWheelEventTimeNew);
            _jwhrold.Add(JHWheelRevolutionOld);
            _jwhrnew.Add(JHWheelRevolutionNew);

           
            if((TimeSpanBetweenCall < 0.1)||(RevolutionBetweenCall < 0.1))
            {
                //No changes made if the time difference too small 
                JHSpeed = JHSpeedOld;
                
                
            }            
            else
            {
                JHSpeed = ((RevolutionBetweenCall * (double)help_wheelradius * 1.024 * 3.6) / TimeSpanBetweenCall) / help_polepairs;
                if(JHSpeedOld < 0.01)
                {
                    JHSpeedOld = JHSpeed;
                    JHSpeed = 0;

                }
                else
                {
                    JHSpeedOld = JHSpeed;
                }
                JHWheelRevolutionOld = JHWheelRevolutionNew;
                JHLastWheelEventTimeOld = JHLastWheelEventTimeNew;               
            }
            _jspeed.Add(JHSpeed);
            return JHSpeed; 
        }

        public double JHSpeed { get; set; }
    }
}
