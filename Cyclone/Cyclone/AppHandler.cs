using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;

namespace Cyclone
{
    public class AppHandler : INotifyPropertyChanged
    {
        //Updater for bindable properties
        public event PropertyChangedEventHandler PropertyChanged;

        //Fontsize
        int pageheight = 0;
        int fontverybig = 50;
        int fontbig = 40;
        int fontmedium = 30;
        int fontsmall = 20;
        int fontverysmall = 10;

        //Bindable Properties : CYCLING
        double ahcyclingspeed = 0.0, ahcyclingavgspeed = 0.0, ahcyclingdistance = 0.0;
        string ahtemperature;
        string btnstartjourneytxt = "START JOURNEY";
        TimeSpan ahcyclingduration;

        //Bindable Properties : STATISTICS
        double ahstatisticstotaldistance, ahstatisticsavgspeed;
        TimeSpan ahstatisticstotalduration;
        ListView ahstatisticsjourneys = new ListView();
        string ahstatisticstableunit = "km";

        //Bindable Properties : SETTINGS
        string ahsettingsmeasurementunit = "0";
        string ahsettingsmeasurementunittxt = "km";
        string ahsettingswheelradius = "2000";
        string ahsettingspolepairs = "28";

        //Bindable Properties : BLUETOOTH
        ListView ahbluetoothdevicelist = new ListView();
        string ahbluetoothstatus = "NOT CONNECTED";
        string ahbluetoothdevicename = "";
        

        //Variable from BLE        
        int ahbluetoothisconnected = 0;

        //Variable for BLE
        IDevice ahbluetoothdevice;
        int btnScanBluetooth_isTapped = 0;

        //Model-Class as a new object
        Model.TemperatureHandler ahtemperaturehandler = new Model.TemperatureHandler();
        Model.JourneyHandler ahjourneyhandler = new Model.JourneyHandler();
        Model.StatisticsHandler ahstatisticshandler = new Model.StatisticsHandler();
        Model.BluetoothHandler ahbluetoothhandler = new Model.BluetoothHandler();

        //help variable
        int journeyisrunning = 0;
        int timespandivider = 0;

        public AppHandler()
        {

            //MessagingCenter : Subscribe message
            MessagingCenter.Subscribe<MainPage>(this, "btnStartJourney_Tapped", (sender) =>
            {
                btnStartJourney_Tapped();
            });

            MessagingCenter.Subscribe<MainPage>(this, "btnResetSettings_Tapped", (sender) =>
            {
                btnResetSettings_Tapped();
            });

            MessagingCenter.Subscribe<MainPage>(this, "btnScanBluetooth_Tapped", (sender) =>
            {
                btnScanBluetooth_isTapped = 1;                
            });

            MessagingCenter.Subscribe<MainPage>(this, "btnConnectBluetooth_Tapped", (sender) =>
            {
                btnConnectBluetooth_Tapped();
            });

            MessagingCenter.Subscribe<MainPage, IDevice>(this, "lvBluetoothDevice_Selected", (sender, device) =>
            {
                ahbluetoothdevice = device;
            }); 

            MessagingCenter.Subscribe<MainPage, int>(this, "PageSize_Changed", (sender, arg) =>
            {
                pageheight = arg;
                FontVeryBig = (int)(arg * 0.2);
                FontBig = (int)(arg * 0.07);
                FontMedium = (int)(arg * 0.06);
                FontSmall = (int)(arg * 0.03);
                FontVerySmall = (int)(arg * 0.01);
            });


            //Initialization
            WheelRadius = 2000;
            AHTemperature = ahtemperaturehandler.UpdateTemperature();
            AHCyclingDuration = ahjourneyhandler.UpdateDuration();
            UpdateAvgSpeed();           
            ahstatisticshandler.UpdateTotalDistance();
            AHStatisticsAvgSpeed = ahstatisticshandler.AvgSpeed;
            AHStatisticsTotalDistance = ahstatisticshandler.TotalDistance;
            AHStatisticsTotalDuration = ahstatisticshandler.TotalDuration;
            UpdateStatistics();
            
            //Checking if previous device is found
            if(ahbluetoothhandler.PrevDeviceIsFound == 1)
            {
                if (ahbluetoothhandler.DeviceStatus() == "On")
                {
                    ConnectBluetooth_PrevDevice();
                }
            }

            //Main loop
            Device.StartTimer(TimeSpan.FromMilliseconds(1000), () =>
            {
                UpdateAvgSpeed();
                AHBluetoothLastWheelEventTime = ahbluetoothhandler.GetLastWheelEventTime();
                AHBluetoothCumulativeWheelRevolution = ahbluetoothhandler.GetCumulativeWheelRevolution();
                AHCyclingDuration = ahjourneyhandler.UpdateDuration();
                AHCyclingSpeed = ahjourneyhandler.UpdateSpeed(AHBluetoothCumulativeWheelRevolution, AHBluetoothLastWheelEventTime, AHSettingsWheelRadius, AHSettingsPolePairs);
                
                AHCyclingDistance = ahjourneyhandler.UpdateDistance(AHBluetoothCumulativeWheelRevolution, AHSettingsWheelRadius, AHSettingsPolePairs);

                if (timespandivider % 25 == 0)
                {
                    ahbluetoothhandler.UpdateValue();
                    AHBluetoothStatus = ahbluetoothhandler.UpdateStatus();
                    AHBluetoothDeviceName = ahbluetoothhandler.GetDeviceName();
                }

                if(btnScanBluetooth_isTapped == 1)
                {
                    btnScanBluetooth_Tapped();
                    btnScanBluetooth_isTapped = 0;
                }

                if(ahbluetoothisconnected == 1)
                {
                    ahbluetoothhandler.UpdateCharacteristic();
                }
                
                if (timespandivider % 4 == 0)
                {
                    
                    AHTemperature = ahtemperaturehandler.UpdateTemperature();
                }


                timespandivider = timespandivider + 1;
                return true;
            });
        }

        //BUTTON : SCAN
        //Scan around and if BLE-Device found, put it into list
        //MessagingCenter : announce that the list has been changed
        private void btnScanBluetooth_Tapped()
        {
            if(ahbluetoothhandler.DeviceStatus() == "Off")
            {
                MessagingCenter.Send<AppHandler>(this, "AHBluetooth_isOff");
                return;
            }
            ahbluetoothhandler.ScanForDevices();
            AHBluetoothStatus = ahbluetoothhandler.UpdateStatus();
            ahbluetoothdevicelist.ItemsSource = ahbluetoothhandler.UpdateDeviceList();
            MessagingCenter.Send<AppHandler, ListView>(this, "AHDeviceList_Changed", ahbluetoothdevicelist);
            
        }

        //Develop connection to BLE-Device from previous session
        private async void ConnectBluetooth_PrevDevice()
        {
            Task[] t = { ahbluetoothhandler.GetConnectionToKnownDevice() };
            await Task.Factory.ContinueWhenAll(t, _ => ahbluetoothhandler.GetServiceAndCharacteristic());

            AHBluetoothStatus = ahbluetoothhandler.UpdateStatus();
            AHBluetoothDeviceName = ahbluetoothhandler.GetDeviceName();
        }

        //BUTTON : CONNECT 
        //Develop connection to found BLE-Device
        private  async void btnConnectBluetooth_Tapped()
        {
            Task[] t = { ahbluetoothhandler.GetConnection(ahbluetoothdevice) };
            await Task.Factory.ContinueWhenAll(t, _ => ahbluetoothhandler.GetServiceAndCharacteristic());
            AHBluetoothStatus = ahbluetoothhandler.UpdateStatus();
            AHBluetoothDeviceName = ahbluetoothhandler.GetDeviceName();
           
        }

        //BUTTON : RESET SETTINGS
        //Reset settings to the initial value
        private void btnResetSettings_Tapped()
        {
            AHSettingsWheelRadius = "2000";
            AHSettingsMeasurementUnit = "0";
            AHSettingsPolePairs = "28";
        }

        //Property : Font
        public int FontVeryBig
        {
            get
            {
                return fontverybig;
            }
            set
            {
                if(fontverybig != value)
                {
                    fontverybig = value;
                    OnPropertyChanged("FontVeryBig");
                }
            }
        }

        public int FontBig
        {
            get
            {
                return fontbig;
            }
            set
            {
                if (fontbig != value)
                {
                    fontbig = value;
                    OnPropertyChanged("FontBig");
                }
            }
        }

        public int FontMedium
        {
            get
            {
                return fontmedium;
            }
            set
            {
                if (fontmedium != value)
                {
                    fontmedium = value;
                    OnPropertyChanged("FontMedium");
                }
            }
        }

        public int FontSmall
        {
            get
            {
                return fontsmall;
            }
            set
            {
                if (fontsmall != value)
                {
                    fontsmall = value;
                    OnPropertyChanged("FontSmall");
                }
            }
        }

        public int FontVerySmall
        {
            get
            {
                return fontsmall;
            }
            set
            {
                if (fontverysmall != value)
                {
                    fontverysmall = value;
                    OnPropertyChanged("FontVerySmall");
                }
            }
        }

        //Bindable Property
        public string AHTemperature
        {
            get
            {
                return ahtemperature;
            }
            set
            {
                if(ahtemperature!=value)
                {
                    ahtemperature = value;
                    OnPropertyChanged("AHTemperature");
                }
            }
        }

        //Update the average speed during a journey
        public void UpdateAvgSpeed()
        {
            if (AHCyclingDuration.TotalSeconds > 0)
            {                
                AHCyclingAvgSpeed = (3600 * ahcyclingdistance) / AHCyclingDuration.TotalSeconds;
            }
            else
            {
                AHCyclingAvgSpeed = 0.0;
            }           
              
        }

        //Bindable Property
        public TimeSpan AHCyclingDuration
        {
            get
            {
                return ahcyclingduration;
            }
            set
            {
                if(ahcyclingduration != value)
                {
                    ahcyclingduration = value;
                    OnPropertyChanged("AHCyclingDuration");
                }
            }
        }

        //Bindable Property
        public double AHCyclingSpeed
        {
            get
            {
                if(AHSettingsMeasurementUnit == "0")
                {
                    return ahcyclingspeed;
                }
                else
                {
                    return ahcyclingspeed * 0.621371;
                }
                
            }
            set
            {
                if (ahcyclingspeed != value)
                {
                    ahcyclingspeed = value;
                    OnPropertyChanged("AHCyclingSpeed");
                }
            }
        }

        //Bindable Property
        public double AHCyclingAvgSpeed
        {
            get
            {
                if (AHSettingsMeasurementUnit == "0")
                {
                    return ahcyclingavgspeed;
                }
                else
                {
                    return ahcyclingavgspeed * 0.621371;
                }
            }
            set
            {
                if (ahcyclingavgspeed != value)
                {
                    ahcyclingavgspeed = value;
                    OnPropertyChanged("AHCyclingAvgSpeed");
                }
            }
        }

        //Bindable Property
        public double AHCyclingDistance
        {
            get
            {
                if (AHSettingsMeasurementUnit == "0")
                {
                    return ahcyclingdistance;
                }
                else
                {
                    return ahcyclingdistance * 0.621371;
                }
            }
            set
            {
                if (ahcyclingdistance != value)
                {
                    ahcyclingdistance = value;
                    OnPropertyChanged("AHCyclingDistance");
                }
            }
        }

        //Bindable Property
        public string AHStatisticsTableUnit
        {
            get
            {
                return ahstatisticstableunit;
            }
            set
            {
                if(ahstatisticstableunit != value)
                {
                    ahstatisticstableunit = value;
                    OnPropertyChanged("AHStatisticsTableUnit");
                }
            }
        }

        //Bindable Property
        public double AHStatisticsTotalDistance
        {
            get
            {
                if (AHSettingsMeasurementUnit == "0")
                {
                    return ahstatisticstotaldistance;
                }
                else
                {
                    return ahstatisticstotaldistance * 0.621371;
                }
            }
            set
            {
                if (ahstatisticstotaldistance != value)
                {
                    ahstatisticstotaldistance = value;
                    OnPropertyChanged("AHStatisticsTotalDistance");
                }
            }
        }

        //Bindable Property
        public double AHStatisticsAvgSpeed
        {
            get
            {
                if (AHSettingsMeasurementUnit == "0")
                {
                    return ahstatisticsavgspeed;
                }
                else
                {
                    return ahstatisticsavgspeed * 0.621371;
                }
            }
            set
            {
                if (ahstatisticsavgspeed != value)
                {
                    ahstatisticsavgspeed = value;
                    OnPropertyChanged("AHStatisticsAvgSpeed");
                }
            }
        }

        //Bindable Property
        public TimeSpan AHStatisticsTotalDuration
        {
            get
            {
                return ahstatisticstotalduration;
            }
            set
            {
                if (ahstatisticstotalduration != value)
                {
                    ahstatisticstotalduration = value;
                    OnPropertyChanged("AHStatisticsTotalDuration");
                }
            }
        }

        ListView AHStatisticsJourneys
        {
            get
            {
                return ahstatisticsjourneys;
            }
            set
            {
                if(ahstatisticsjourneys!=value)
                {
                    ahstatisticsjourneys = value;
                    OnPropertyChanged("AHStatisticsJourneys");
                }
            }
        }

        //This method will run, when the unit has been changed
        //Update the properties related to statistics
        public void UpdateStatistics()
        {
            IEnumerable<Journey> ahjourney;
            JourneyDB ahdatabase = new JourneyDB();
            ahjourney = ahdatabase.GetJourneys();
            if(AHSettingsMeasurementUnit=="0")
            {
                AHStatisticsJourneys.ItemsSource = ahjourney;
            }
            else
            {
                foreach (var journey in ahjourney)
                {
                    journey.Distance = journey.Distance * 0.621371;
                }
                AHStatisticsJourneys.ItemsSource = ahjourney;
            }
        }

        //Bindable Property
        public string AHSettingsMeasurementUnit
        {
            get
            {
                return ahsettingsmeasurementunit;
            }
            set
            {
                if (ahsettingsmeasurementunit!=value)
                {
                    
                    ahsettingsmeasurementunit = value;
                    if (ahsettingsmeasurementunit == "0")
                    {
                        AHSettingsMeasurementUnitTxt = "km";
                    }
                    else
                    {
                        AHSettingsMeasurementUnitTxt = "mi";
                    }
                    MessagingCenter.Send<AppHandler, string>(this, "MeasurementUnit_Changed", value );
                    OnPropertyChanged("AHStatisticsAvgSpeed");
                    OnPropertyChanged("AHStatisticsTotalDistance");
                    OnPropertyChanged("AHStatisticsTotalDuration ");
                    OnPropertyChanged("AHCyclingSpeed");
                    OnPropertyChanged("AHSettingsMeasurementUnit");
                }
            }
        }

        //Bindable Property
        public string AHSettingsMeasurementUnitTxt
        {
            get
            {
                return ahsettingsmeasurementunittxt;
            }
            set
            {
                if (ahsettingsmeasurementunittxt != value)
                {
                    ahsettingsmeasurementunittxt = value;
                    OnPropertyChanged("AHSettingsMeasurementUnitTxt");
                }
            }
        }

        //Bindable Property
        public string AHSettingsWheelRadius
        {
            get
            {
                return ahsettingswheelradius;
            }
            set
            {
                if(ahsettingswheelradius != value)
                {
                    ahsettingswheelradius = value;
                    double help;
                    double.TryParse(value, out help);
                    if(help == 0)
                    {
                        help = 2000;
                        MessagingCenter.Send<AppHandler>(this, "AHSettingsWheelRadius_Invalid");
                    }
                    WheelRadius = help;
                    OnPropertyChanged("AHSettingsWheelRadius");
                }
            }
        }

        //Bindable Property
        public string AHSettingsPolePairs
        {
            get
            {
                return ahsettingspolepairs;
            }
            set
            {
                if(ahsettingspolepairs != value)
                {
                    ahsettingspolepairs = value;
                    double help;
                    double.TryParse(value, out help);
                    PolePairs = help;
                }
            }
        }

        //Bindable Property
        public double PolePairs { get; set; }

        //Bindable Property
        public string AHBluetoothStatus
        {
            get
            {
                return ahbluetoothstatus;
            }
            set
            {
                if(ahbluetoothstatus!=value)
                {
                    ahbluetoothstatus = value;
                    OnPropertyChanged("AHBluetoothStatus");
                }
            }
        }

        //Bindable Property
        public string AHBluetoothCumulativeWheelRevolution { get; set; }

        //Bindable Property
        public string AHBluetoothLastWheelEventTime { get; set; }

        //Bindable Property
        public string AHBluetoothDeviceName
        {
            get
            {
                return ahbluetoothdevicename;
            }
            set
            {
                if(ahbluetoothdevicename != value)
                {
                    ahbluetoothdevicename = value;
                    OnPropertyChanged("AHBluetoothDeviceName");

                }
            }
        }

        public double WheelRadius { get; set; }

        //Bindable Property
        public string btnStartJourneyTxt
        {
            get
            {
                return btnstartjourneytxt;
            }
            set
            {
                if(btnstartjourneytxt != value)
                {
                    btnstartjourneytxt = value;
                    OnPropertyChanged("btnStartJourneyTxt");
                }
            }
        }

        //BUTTON : START JOURNEY
        //Change text to STOP JOURNEY
        //START : Start a new journey
        //STOP : Stop the journey and add it to the database
        public void btnStartJourney_Tapped()
        {
            if (btnStartJourneyTxt == "START JOURNEY")
            {
                
                
    
                btnStartJourneyTxt = "STOP JOURNEY";
                ahjourneyhandler = new Model.JourneyHandler();
                ahjourneyhandler.UpdateDistance(AHBluetoothCumulativeWheelRevolution, AHSettingsWheelRadius, AHSettingsPolePairs);
                ahjourneyhandler.StartJourney();
                
                
            }
            else 
            {
                btnStartJourneyTxt = "START JOURNEY";
                ahjourneyhandler.StopJourney();
                ahstatisticshandler.UpdateTotalDistance();
                AHStatisticsAvgSpeed = ahstatisticshandler.AvgSpeed;
                AHStatisticsTotalDistance = ahstatisticshandler.TotalDistance;
                AHStatisticsTotalDuration = ahstatisticshandler.TotalDuration;
                MessagingCenter.Send<AppHandler, string>(this, "JourneysList_Changed",AHSettingsMeasurementUnit);

            }
        }        

        //If a property has been changed, this method will run
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
