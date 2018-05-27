/* TEMPERATURHANDLER
 * This file handles the connection to the weather api server. 
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

using System.Net.Http;
using Newtonsoft.Json;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.Geolocator;

namespace Cyclone.Model
{
    public class TemperatureHandler
    {
        Weather thweather = new Weather();
        string thtemperature;
        string thlatitude = "35", thlongitude = "139";
        public TemperatureHandler()
        {
           
        }

        //Try to connect to internet
        public static async Task<dynamic> getDataFromService(string pQueryString)
        {
            try
            {
                HttpClient client = new HttpClient();
                var response = await client.GetAsync(pQueryString);
                dynamic data = null;
                if (response != null)
                {
                    string json = response.Content.ReadAsStringAsync().Result;
                    data = JsonConvert.DeserializeObject(json);
                }
                return data;
            }
            catch (System.Net.Http.HttpRequestException e)
            {
                return null;
            }            
        }


        //Retrieve weather information from openweathermap.org
        public static async Task<Weather> RetrieveWeather(string latitude, string longitude)
        {
            //Sign up for a free API key at http://openweathermap.org/appid  
            string key = "2f6c3a7f5d22f01958afbfee84c0b059";
            string queryString = "http://api.openweathermap.org/data/2.5/weather?lat="
                + latitude + "&lon=" + longitude + "&appid=" + key + "&units=Metric";

            dynamic results = await getDataFromService(queryString).ConfigureAwait(false);
            if (results != null)
            {
                if (results["weather"] != null)
                {
                    Weather weather = new Weather();

                    //weather.Temperature = (string)(results["main"]["temp"]) + " °C";  
                    int help = (results["main"]["temp"]);
                    weather.Temperature = help.ToString();

                    return weather;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                Weather weather = new Weather();
                weather.Temperature = "NC";
                return weather;
            }
        }

        public async void RetrieveTemperature()
        {
            thweather = await RetrieveWeather(thlatitude, thlongitude);            
        }      
        
        public string UpdateTemperature()
        {
            GetLocation();
            RetrieveTemperature();

            if(thweather != null)
            {
                return thweather.Temperature;
            }
            else
            {
                return "XX";
            }
        }


        //Get location from the device
        public async void GetLocation()
        {
            try
            {


                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Location))
                    {
                        //await DisplayAlert("Need location", "Gunna need that location", "OK");                        
                    }

                    var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Location });
                    status = results[Permission.Location];
                }

                if (status == PermissionStatus.Granted)
                {
                    TimeSpan i;

                    var results = await CrossGeolocator.Current.GetPositionAsync();
                    string text = "Lat: " + results.Latitude + " Long: " + results.Longitude;
                    thlatitude = (results.Latitude).ToString();
                    thlongitude = (results.Longitude).ToString();
                    
                }
                else if (status != PermissionStatus.Unknown)
                {
                    //await DisplayAlert("Location Denied", "Can not continue, try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                //LabelGeolocation.Text = "Error: " + ex;
            }
        }
    }
}
