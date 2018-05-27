using System;
using Xamarin.Forms;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.Geolocator;


namespace Cyclone
{
    public partial class MainPage : CarouselPage
    {
        public JourneyDB _database;
        Page MyPage;
        public MainPage()
        {

            InitializeComponent();

            _database = new JourneyDB();
            var journey = _database.GetJourneys();
            string measurementunit = "0";
            MyPage = this;
            this.SizeChanged += (sender, e) => PageSize_Changed(this);
           
            MessagingCenter.Subscribe<AppHandler, string>(this, "MeasurementUnit_Changed", (sender, arg) =>
            {
                measurementunit = arg;
                if (measurementunit == "1")
                {
                    journey = _database.GetJourneys();
                    foreach (var item in journey)
                    {
                        item.Distance = item.Distance * 0.621371;
                        item.Unit = "mi";
                    }
                }
                else
                {
                    journey = _database.GetJourneys();
                    foreach (var item in journey)
                    {
                        item.Unit = "km";
                    }
                }
                JourneysListView.ItemsSource = journey;
            });

            MessagingCenter.Subscribe<AppHandler, ListView>(this, "AHDeviceList_Changed", (sender, arg) =>
            {
                BluetoothDevicesListView.ItemsSource = arg.ItemsSource;
            });

            MessagingCenter.Subscribe<AppHandler, string>(this, "JourneysList_Changed", (sender, arg) =>
             {
                 measurementunit = arg;
                 if (measurementunit == "1")
                 {
                     journey = _database.GetJourneys();
                     foreach (var item in journey)
                     {
                         item.Distance = item.Distance * 0.621371;
                         item.Unit = "mi";
                     }
                 }
                 else
                 {
                     journey = _database.GetJourneys();
                     foreach (var item in journey)
                     {
                         item.Unit = "km";
                     }
                 }
                 JourneysListView.ItemsSource = journey;
             });

            MessagingCenter.Subscribe<AppHandler>(this, "AHSettingsWheelRadius_Invalid", (sender) =>
            {
                DisplayAlert("INPUT INVALID", "Only numbers are allowed and the wheel radius has to be bigger than 0", "OK");
            });

            MessagingCenter.Subscribe<AppHandler>(this, "AHBluetooth_isOff", (sender) =>
            {
                DisplayAlert("BLUETOOTH IS OFF", "Turn on bluetooth and try again", "OK");
            });

            MessagingCenter.Subscribe<Model.BluetoothHandler>(this, "AHBluetooth_isOff", (sender) =>
            {
                DisplayAlert("BLUETOOTH IS OFF", "Turn on bluetooth and try again", "OK");
            });

            MessagingCenter.Subscribe<Model.BluetoothHandler>(this, "BHNoDeviceSelected", (sender) =>
            {
                DisplayAlert("NO DEVICE IS SELECTED", "Select a device and try again", "OK");
            });

            MessagingCenter.Subscribe<Model.BluetoothHandler>(this, "BHConnectToDeviceFail", (sender) =>
            {
                DisplayAlert("CONNECTION FAIL", "Can't connect to device", "OK");
            });

            if (measurementunit == "1")
            {
                journey = _database.GetJourneys();
                foreach (var item in journey)
                {
                    item.Distance = item.Distance * 0.621371;
                    item.Unit = "mi";
                }
            }
            else
            {
                journey = _database.GetJourneys();
                foreach (var item in journey)
                {
                    item.Unit = "km";
                }
            }

            JourneysListView.ItemsSource = journey;
        }

        public int PageSizeIsChanged { get; set; }

        void PageSize_Changed(Page page)
        {
            MessagingCenter.Send<MainPage, int>(this, "PageSize_Changed", (int)page.Height);
        }

        public void btnStartJourney_Tapped(object sender, EventArgs args)
        {
            MessagingCenter.Send<MainPage>(this, "btnStartJourney_Tapped");
            _database = new JourneyDB();
            var journey = _database.GetJourneys();
            JourneysListView.ItemsSource = journey;

        }

        public void btnResetSettings_Tapped(object sender, EventArgs args)
        {
            MessagingCenter.Send<MainPage>(this, "btnResetSettings_Tapped");
        }

        public void btnScanBluetooth_Tapped(object sender, EventArgs args)
        {
            MessagingCenter.Send<MainPage>(this, "btnScanBluetooth_Tapped");
        }

        public void btnConnectBluetooth_Tapped(object sender, EventArgs args)
        {
            MessagingCenter.Send<MainPage>(this, "btnConnectBluetooth_Tapped");
        }

        public void BluetoothDeviceSelected(object sender, SelectedItemChangedEventArgs e)
        {
            IDevice device = BluetoothDevicesListView.SelectedItem as IDevice;
            MessagingCenter.Send<MainPage, IDevice>(this, "lvBluetoothDevice_Selected", device);
        }

        string geotext { get; set; }

    }
}
