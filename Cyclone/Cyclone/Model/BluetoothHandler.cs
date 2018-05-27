using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;

namespace Cyclone.Model
{
    public class BluetoothHandler
    {
        IBluetoothLE ble;
        IList<IService> Services;
        ICharacteristic Characteristic;
        IService Service;
        IAdapter adapter;
        ObservableCollection<IDevice> deviceList;
        Guid prevguid;
        IDevice device;
        IDevice prevdevice;
        string status;
        string prevdevicename;
        byte[] bytes;
        public BluetoothHandler()
        {
            status = "NOT CONNECTED";
            ble = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;
            deviceList = new ObservableCollection<IDevice>();
            PrevDeviceIsFound = 0;


            //Check if in the previous session a bluetooth device was saved
            if (Application.Current.Properties.ContainsKey("BluetoothPrevSession"))
            {
               prevguid = (Guid)Application.Current.Properties["BluetoothPrevSession"];
               PrevDeviceIsFound = 1;
            }

            if (Application.Current.Properties.ContainsKey("BluetoothPrevSessionName"))
            {
                prevdevicename = (string)Application.Current.Properties["BluetoothPrevSessionName"];
            }

        }

        public int PrevDeviceIsFound {get;set;}

        //Scan for ble-devices
        public async void ScanForDevices()
        {
            Stopwatch test = new Stopwatch();
            test.Restart();           
            
            deviceList.Clear();           
            
            status = "SCANNING";
            
            adapter.DeviceDiscovered += (s, a) =>
            {
                deviceList.Add(a.Device);
            };
            if (!ble.Adapter.IsScanning)
            {
                await adapter.StartScanningForDevicesAsync();
            }            
        }        

        //Update the sensor value
        public async void UpdateValue()
        {
            try
            {
                if (Characteristic != null)
                {
                    if (SelectedDevice.State == DeviceState.Connected)
                    {
                        status = "CONNECTED";
                        prevdevice = device;
                        Characteristic.ValueUpdated += (o, args) =>
                        {
                            bytes = args.Characteristic.Value;
                        };
                        await Characteristic.StartUpdatesAsync();
                    }
                    else
                    {
                        bytes = null;
                        prevguid = SelectedDevice.Id;
                        prevdevicename = SelectedDevice.Name;
                        Task[] t = { GetConnectionToKnownDevice() };
                        await Task.Factory.ContinueWhenAll(t, _ => GetServiceAndCharacteristic());
                    }
                }
                else
                {
                    status = "NOT CONNECTED";
                }
            }
            catch (System.Exception e)
            {
                bytes = null;
            }

        }

        //Separating the value to get cumulative wheel revolution
        public string GetCumulativeWheelRevolution()
        {
           int i;
            if(bytes != null)
            {
                byte[] _byte = { bytes[1], bytes[2], bytes[3], bytes[4] };
                i = BitConverter.ToInt16(_byte,0);
            }
            else
            {
                i = 0;
            }

            return i.ToString();
        }

        //Separating the value to get cumulative wheel revolution
        public string GetLastWheelEventTime()
        {
            uint i;
            if (bytes != null)
            {
                byte[] _byte = { bytes[5], bytes[6]};
                i = BitConverter.ToUInt16(_byte, 0);
            }
            else
            {
                i = 0;
            }

            return i.ToString();
        }


        public async void UpdateCharacteristic()
        {
            {
                Characteristic.ValueUpdated += (o, args) =>
                {
                    var bytes = args.Characteristic.Value;
                };
                await Characteristic.StartUpdatesAsync();
                if (Characteristic != null)
                {
                    var help = Characteristic.Value;
                }
            }         
            
        }

        public async Task GetServiceAndCharacteristic()
        {
            if(SelectedDevice != null)
            {
                Task[] t = { GetService() };
                await Task.Factory.ContinueWhenAll(t, _ => GetCharacteristic());
                var id = SelectedDevice.Id;
                Application.Current.Properties["BluetoothPrevSession"] = id;
                var test = SelectedDevice.Name;
                Application.Current.Properties["BluetoothPrevSessionName"] = test;
                await Application.Current.SavePropertiesAsync();
            }
            
        }

        public async Task GetService()
        {
            if(adapter.ConnectedDevices.Count > 0)
            {
                if(SelectedDevice == null)
                {
                    SelectedDevice = adapter.ConnectedDevices[0];
                }
                Service = await SelectedDevice.GetServiceAsync(new Guid(6166, 0, 4096, 128, 0, 0, 128, 95, 155, 52, 251));
            }
           
        }

        public async Task GetCharacteristic()
        {
            if (Service != null)
            {
                //wanted characteristic has guid number
                Characteristic = await Service.GetCharacteristicAsync(Guid.Parse("00002a5b-0000-1000-8000-00805f9b34fb"));                
            }
        }

        public async Task GetConnection(IDevice Device)
        {
            SelectedDevice = Device;
            try
            {
                status = "CONNECTING";
                if (SelectedDevice != null)
                {
                    await adapter.ConnectToDeviceAsync(SelectedDevice);
                    //Task[] t = { adapter.ConnectToDeviceAsync(SelectedDevice) };
                    //await Task.Factory.ContinueWhenAll(t, _ => status = "CONNECTED");
                }
                else
                {
                    MessagingCenter.Send<BluetoothHandler>(this, "BHNoDeviceSelected");
                }             
                
            }
            catch (DeviceConnectionException ex)
            {
                //Could not connect to the device
                MessagingCenter.Send<BluetoothHandler>(this, "BHConnectToDeviceFail");
            }
        }

        public string GetDeviceName()
        {
            if (SelectedDevice != null)
            {                
                if(SelectedDevice.Name == null)
                {
                    return prevdevicename;
                }
                else
                {
                    return SelectedDevice.Name;
                }
                
            }
            else
            {
                return "";
            }
        }

        //if th device from previous session is found then this method should be used
        public async Task GetConnectionToKnownDevice()
        {          
            try
            {
                status = "CONNECTING";

                
                Task[] t = { adapter.ConnectToKnownDeviceAsync(prevguid) };
                await Task.Factory.ContinueWhenAll(t, _ => SetSelectedDevice_ToPrevDevice());
            }
            catch (DeviceConnectionException ex)
            {
                //Could not connect to the device
                MessagingCenter.Send<BluetoothHandler>(this, "BHConnectToDeviceFail");
            }
        }

        public void SetSelectedDevice_ToPrevDevice()
        {
            if(adapter.ConnectedDevices.Count > 0)
            {
                //if (SelectedDevice != null)
                //{
                    SelectedDevice = adapter.ConnectedDevices[0];
                //}
            }
        }
        public ObservableCollection<IDevice> UpdateDeviceList()
        {
            return deviceList;
        }

        public IDevice SelectedDevice { get; set; }

        public string UpdateStatus()
        {
            if(!ble.IsOn)
            {
                status = "Not Connected";
                MessagingCenter.Send<BluetoothHandler>(this, "AHBluetooth_isOff");
            }
            if(adapter == null)
            {
                status = "Not Connected";
            }
            return status;
        }

        public string DeviceStatus()
        {
            
            return ble.State.ToString();
        }
    }
}
