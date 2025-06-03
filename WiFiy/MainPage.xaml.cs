using MauiWifiManager;
using MauiWifiManager.Abstractions;
using Microsoft.Maui.ApplicationModel;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Timer = System.Timers.Timer;

namespace WiFiy
{
    public partial class MainPage : ContentPage
    {

        public class Wifi {
            public string SSID { get; set; }
            public string BSSID { get; set; }
            public string PASSWORD { get; set; }

            
        };

        public ObservableCollection<Wifi> Networks { get; set; }
        private Timer timer;

        private async void ConnectButton(object sender, EventArgs e)
        {
            var button = sender as Button;
            var wifi = button?.BindingContext as Wifi;

            if (wifi != null)
            {

                await DisplayAlert("Connecting", $"Connecting to {wifi.SSID}", "OK");
                var res = await CrossWifiManager.Current.ConnectWifi(wifi.SSID, wifi.PASSWORD);
                if (res.ErrorCode == WifiErrorCodes.Success)
                {

                    await DisplayPromptAsync("CONNECTED", $"Successfully connected to {wifi.SSID}", "L7WA", "", wifi.PASSWORD);
                }
                else
                    await DisplayAlert("Wi-Fi Info", res.ErrorMessage, "OK");
                }
        }
        private async void CopyButton(object sender, EventArgs e)
        {
            var button = sender as Button;
            var wifi = button?.BindingContext as Wifi;

            if (wifi != null)
            {
                await Clipboard.SetTextAsync(wifi.PASSWORD);
            }
        }

        public async Task Scan()
        {
            PermissionStatus status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (!(status == PermissionStatus.Granted || DeviceInfo.Current.Platform == DevicePlatform.WinUI))
            {
                await DisplayAlert("", "LOCATION ACCESS NOT GRANTED", "Ok");
            }

            var res = await CrossWifiManager.Current.ScanWifiNetworks();


            if (res.ErrorCode != WifiErrorCodes.Success)
            {
                await DisplayAlert("", "Couldn't get wifi list. try later", "Ok");
            }



            Networks.Clear();
            foreach (var wifi in res.Data)
            {
                string password = wifi.Bssid.ToString().Replace(":", "").ToUpper();

                if (wifi.Ssid != null && wifi.Ssid.StartsWith("ADSL_inwi"))
                {
                    string lastfour = wifi?.Ssid?.Substring((int)wifi?.Ssid?.Length - 4);

                    
                    password = password.Substring(0, password.Length - 4) + lastfour;
                }

                Networks.Add(new Wifi
                {
                    SSID = wifi.Ssid,
                    BSSID = wifi.Bssid.ToString(),
                    PASSWORD = password

                });
            }

            BindingContext = this;



        }
        

        public MainPage()
        {
            Networks = new ObservableCollection<Wifi> { };

            timer = new Timer(TimeSpan.FromSeconds(7));
            timer.Elapsed += OnTick;
            timer.Start();

            
            InitializeComponent();
            

        }

        private void OnTick(object sender, ElapsedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                
                await Scan();
            });
        }

       
    }
}
