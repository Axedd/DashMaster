using DashMaster.ViewModels;
using DashMaster.View;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.Caching.Memory;
using DashMaster.Models;


namespace DashMaster
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            // Set up the DI container
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Views
            services.AddTransient<DashboardView>();
            services.AddTransient<LibraryView>();

            // Register ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<LibraryViewModel>();

            services.AddTransient<TrackWeather>();

            services.AddMemoryCache();

            // Register the MainWindow (it may also receive other dependencies)
            services.AddTransient<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Resolve the MainWindow instance using the service provider
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

            // Set the DataContext (ViewModel) for MainWindow if needed
            mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();

            // Show the MainWindow
            mainWindow.Show();

            base.OnStartup(e);
        }
    }

}
