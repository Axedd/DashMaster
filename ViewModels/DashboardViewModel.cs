using DashMaster.Models;
using DashMaster.MVVM;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DashMaster.Services;

namespace DashMaster.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly TrackWeather _trackWeather;
        private readonly DispatcherTimer _timer;

        public RelayCommand OpenStreamerCommand { get; set; }

        private ObservableCollection<StreamerItem> _streamersTracking;
        public ObservableCollection<StreamerItem> StreamersTracking
        {
            get => _streamersTracking;
            set
            {
                _streamersTracking = value;
                OnPropertyChanged();
            }
        }

        private WeatherItem _weather;
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

        private string _streamerToAdd;
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

        public string WeatherDisplay => $"{Weather?.LocationName} - {CurrentTime}";

        // Added TrackStreamerTime property
        public TrackStreamerTime TrackStreamerTime { get; private set; }

        public DashboardViewModel(TrackWeather trackWeather)
        {
            _trackWeather = trackWeather;
            TrackStreamerTime = new TrackStreamerTime(); // Instantiate TrackStreamerTime
            StreamersTracking = new ObservableCollection<StreamerItem>();
            Weather = new WeatherItem();
            _timer = InitializeTimer();

            // Add streamers
            AddInitialStreamers();
            InitializeWeatherAsync().ConfigureAwait(false);
        }

        private DispatcherTimer InitializeTimer()
        {
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
            return timer;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CurrentTime = DateTime.Now.ToString("HH:mm");
        }

        private async Task InitializeWeatherAsync()
        {
            Weather = await _trackWeather.GetWeatherData("Aarhus");
        }

        private void AddInitialStreamers()
        {
            AddStreamer("Bendixboy").ConfigureAwait(false);
            AddStreamer("Aarimous").ConfigureAwait(false);
        }

        public async Task AddStreamer(string streamerName)
        {
            // Ensure TrackStreamerTime is instantiated before using it
            string streamDuration = await TrackStreamerTime.StreamerTime(streamerName);
            bool isLive = !streamDuration.Contains("offline");

            StreamersTracking.Add(CreateStreamerItem(streamerName, isLive, streamDuration));
        }

        private StreamerItem CreateStreamerItem(string streamerName, bool isLive, string streamDuration)
        {
            return new StreamerItem
            {
                Name = streamerName,
                IsLive = isLive,
                StreamDuration = streamDuration,
                OpenStreamerCommand = new RelayCommand(param => OpenStreamerInBrowser(streamerName)) // Assign the command
            };
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