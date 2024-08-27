using DashMaster.Models;
using DashMaster.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Extensions.Caching.Memory;
using System.Windows.Threading;

namespace DashMaster.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly TrackWeather _trackWeather;
        private DispatcherTimer timer;
        public RelayCommand OpenStreamerCommand { get; set; }

        private ObservableCollection<StreamerItem> _streamersTracking;
        public TrackWeather trackWeather {  get; set; }
        private WeatherItem _weather;

        private string _streamerToAdd;

        public TrackStreamerTime TrackStreamerTime { get; set; }
        public ObservableCollection<StreamerItem> StreamersTracking
        {
            get => _streamersTracking;
            set
            {
                _streamersTracking = value; 
                OnPropertyChanged(); 
            }
        }

        public string StreamerToAdd
        {
            get => _streamerToAdd;
            set
            {
                if (_streamerToAdd != value)
                {
                    _streamerToAdd = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _currentTime;
        public string CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WeatherDisplay)); // Notify that WeatherDisplay has changed
            }
        }

        public WeatherItem Weather
        {
            get => _weather;
            set
            {
                _weather = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WeatherDisplay));
            }
        }
        public string WeatherDisplay => $"{Weather?.LocationName} - {CurrentTime}";

        public DashboardViewModel(TrackWeather trackWeather)
        {

            Console.WriteLine("HELLO FROM CONSTRUCTOR");
            TrackStreamerTime = new TrackStreamerTime();
            StreamersTracking = new ObservableCollection<StreamerItem>();
            _trackWeather = trackWeather;
            Weather = new WeatherItem();

            AddStreamer("Bendixboy");
            AddStreamer("Aarimous");

            InitializeAsync().ConfigureAwait(false);

            CurrentTime = DateTime.Now.ToString("HH:mm");

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private async Task InitializeAsync()
        {
            await InitializeWeatherAsync();
            
        }


        private async Task InitializeWeatherAsync()
        {
            Weather = await _trackWeather.GetWeatherData("Aarhus");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CurrentTime = DateTime.Now.ToString("HH:mm");
        }

        public async Task AddStreamer(string streamerName)
        {
            string streamDuration = await TrackStreamerTime.StreamerTime(streamerName);
            bool isLive = !streamDuration.Contains("offline");

            StreamersTracking.Add(new StreamerItem
            {
                Name = streamerName,
                IsLive = isLive,
                StreamDuration = streamDuration,
                OpenStreamerCommand = new RelayCommand(param => OpenStreamerInBrowser(streamerName)) // Assign the command
            });
        }

        private void OpenStreamerInBrowser(string streamerName)
        {
            var destinationUrl = $"https://www.twitch.tv/{streamerName}";
            var sInfo = new System.Diagnostics.ProcessStartInfo(destinationUrl)
            {
                UseShellExecute = true,
            };
            System.Diagnostics.Process.Start(sInfo);
        }



    }
}
