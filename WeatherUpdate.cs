using System;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using Server.Extensions.Weather;

namespace DiscordBot
{
    public class WeatherUpdate
    {
        private static Timer _updateTimer = null; 
        public static OpenWeather CurrentWeather = null;

        public static void InitWeatherUpdate()
        {
            Console.WriteLine("[Weather] Initializing Weather");
            _updateTimer = new Timer(900000)
            {
                AutoReset = true
            };

            _updateTimer.Start();
            _updateTimer.Elapsed += _updateTimer_Elapsed;

            
            CurrentWeather = FetchWeather();

            Console.WriteLine("[Weather] Fetched the latest weather.");


        }

        private static void _updateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine($"[Weather] Fetching latest weather.");
            CurrentWeather = FetchWeather();
            Console.WriteLine("[Weather] Fetched latest weather.");
        }

        private static OpenWeather FetchWeather()
        {
            try
            {
                
                using WebClient wc = new WebClient();
                string updatedJson = wc.DownloadString(
                    "https://api.openweathermap.org/data/2.5/weather?id=5368361&mode=json&units=metric&APPID=37c1a999011411a01b4d200ea16e9b9a");
                wc.Dispose();

                OpenWeather currentWeather = JsonConvert.DeserializeObject<OpenWeather>(updatedJson);

                return currentWeather;
            }
            catch (Exception e)
            {
                Console.WriteLine("[Weather] An error has occurred fetching the current weather.");
                Console.WriteLine(e);
                return null;

            }
        }
    }
}