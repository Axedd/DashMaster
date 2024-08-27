using System;
using System.ComponentModel;
using System.Windows.Input;
using DashMaster.Data;
using DashMaster.MVVM;
using DashMaster.View;
using Microsoft.Extensions.DependencyInjection;

namespace DashMaster.ViewModels
{

    public class MainViewModel : INotifyPropertyChanged
    {

        private readonly IServiceProvider _serviceProvider;
        private object _currentView;

        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        private readonly DatabaseService _databaseService;

        

        public ICommand ShowHomeViewCommand { get; }
        public ICommand ShowDashboardViewCommand { get; }
        public ICommand ShowLibraryViewCommand { get; }

        public MainViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            _databaseService = new DatabaseService();
            _databaseService.CreateDatabase();

            ShowHomeViewCommand = new RelayCommand(_ => Navigate<DashboardView>());
            ShowDashboardViewCommand = new RelayCommand(_ => Navigate<DashboardView>());
            ShowLibraryViewCommand = new RelayCommand(_ => Navigate<LibraryView>());

            // Set initial view
            Navigate<DashboardView>();
        }

        private void Navigate<T>() where T : class
        {
            try
            {
                CurrentView = _serviceProvider.GetRequiredService<T>();
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log errors)
                Console.WriteLine($"Error resolving service: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}