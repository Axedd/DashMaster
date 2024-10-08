﻿using DashMaster.Models;
using DashMaster.MVVM;
using DashMaster.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Data.Sqlite;
using System.Windows.Media.Imaging;

namespace DashMaster.ViewModels
{
    public class LibraryViewModel : ViewModelBase
    {
        private readonly ApplicationScannerService _scannerService;
        public ObservableCollection<ApplicationModel> Applications { get; set; }
        private bool _isRemovingApps;

        private int _selectedCount;
        public int SelectedCount
        {
            get => _selectedCount;
            set
            {
                if (_selectedCount != value)
                {
                    _selectedCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsRemovingApps
        {
            get => _isRemovingApps;
            set
            {
                if (_isRemovingApps != value)
                {
                    _isRemovingApps = value;
                    Console.WriteLine($"HELLO {_isRemovingApps}");
                    OnPropertyChanged(nameof(IsRemovingApps));
                }
            }
        }

        public ICommand OpenFolderCommand { get; set; }
        public ICommand OpenFileCommand { get; set; }
        public ICommand ClearApplications { get; set; }
        public ICommand MakeAppsRemoveable {  get; set; }
        public ICommand DeleteSelectedAppsCommand { get; set; }


        public LibraryViewModel()
        {
            _scannerService = new ApplicationScannerService();
            Applications = new ObservableCollection<ApplicationModel>();
            LoadApplications();  // Load applications when the ViewModel is initialized
            //ClearTableAndReloadApplications();
            ClearApplications = new RelayCommand(param => ClearTableAndReloadApplications());
            OpenFolderCommand = new RelayCommand(param => OpenFolderCommandExecute());
            OpenFileCommand = new RelayCommand(param => OpenFileCommandExecute());
            MakeAppsRemoveable = new RelayCommand(param => ToggleRemoveable());
            DeleteSelectedAppsCommand = new RelayCommand(param => DeleteSelectedApps());
        }

        private List<string> OpenDialogAndPaths(Func<bool?> showDialog, Func<string> getSinglePath = null, Func<IEnumerable<string>> getMultiplePaths = null)
        {
            List<string> paths = new List<string>();

            if (showDialog() == true)
            {
                if (getSinglePath != null)
                {
                    var path = getSinglePath();

                    if (!string.IsNullOrEmpty(path))
                    {
                        paths.Add(path);
                    }
                }
                else if (getMultiplePaths != null)
                {
                    foreach (var path in getMultiplePaths())
                    {
                        paths.AddRange(Directory.EnumerateDirectories(path));
                    }
                }
            }
            else
            {
                Console.WriteLine("Failed to load Dialog");
            }

            return paths;
        } 

        private void OpenFileCommandExecute()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Select executable file...",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Filter = "Executable Files|*.exe",
                Multiselect = false
            };

            var exePaths = OpenDialogAndPaths(
                showDialog: () => openFileDialog.ShowDialog(),
                getSinglePath: () => openFileDialog.FileName

                );

            if (exePaths.Count > 0)
            {

                var application = _scannerService.ScanForApplications(exePaths, isSingleExe: true);

                foreach(var app in application)
                {
                    app.LoadIcon();
                    _scannerService.SaveApplication(app);
                    Applications.Add(app);

                    LoadApplications();
                    OnPropertyChanged(nameof(Data));
                }
            }
        }

        private void OpenFolderCommandExecute()
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog()
            {
                Title = "Select folder to open ...",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Multiselect = true
            };

            List<string> myFolderPaths = OpenDialogAndPaths(
                showDialog: () => openFolderDialog.ShowDialog(),
                getMultiplePaths: () => openFolderDialog.FolderNames
                );


            if (myFolderPaths.Count > 0)
            {
                var applications = _scannerService.ScanForApplications(myFolderPaths);

                foreach (var app in applications)
                {
                    app.LoadIcon();
                    _scannerService.SaveApplication(app);
                    Applications.Add(app);
                }
            }

            LoadApplications();
            OnPropertyChanged(nameof(Data));
        }

        public void LoadApplications()
        {
            Console.WriteLine("Loading applications...");
            Applications.Clear();  // Clear the existing items in the collection

            string connectionString = "Data Source=applications.db";
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string selectQuery = "SELECT * FROM Applications";
                SqliteCommand command = new SqliteCommand(selectQuery, connection);
                SqliteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string iconPath = reader["Icon"].ToString();

                    if (string.IsNullOrWhiteSpace(iconPath))
                    {
                        Console.WriteLine("Error: Icon path is empty or null.");
                        continue;
                    }

                    try
                    {
                        // Convert the icon path to a BitmapImage
                        BitmapImage icon = new BitmapImage();
                        icon.BeginInit();
                        icon.UriSource = new Uri(iconPath, UriKind.RelativeOrAbsolute);
                        icon.CacheOption = BitmapCacheOption.OnLoad;
                        icon.EndInit();

                        var app = new ApplicationModel
                        {
                            Name = reader["Name"].ToString(),
                            Icon = icon,
                            Path = reader["Path"].ToString(),
                            IconPath = iconPath
                        };

                        // Subscribes to each app's ApplicationDeleted event with OnApplicationDeleted 
                        app.ApplicationDeleted += OnApplicationDeleted;
                        app.ApplicaitonSelected += UpdateSelectedCount;

                        // Add the new application to the ObservableCollection
                        Applications.Add(app);
                        Applications.OrderBy(app => app.Name);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading icon for path {iconPath}: {ex.Message}");
                    }
                }
            }
            var sortedApplications = Applications.OrderBy(app => app.Name).ToList();
            Applications.Clear();
            foreach (var app in sortedApplications)
            {
                Applications.Add(app);
            }
        }

        private void ToggleRemoveable()
        {

            if (IsRemovingApps)
            {
                IsRemovingApps = false;
            } else
            {
                IsRemovingApps = true;
            }

            foreach (var app in Applications)
            {
                app.IsRemovable = !app.IsRemovable;
                
                if (!app.IsRemovable)
                {
                    app.IsSelected = false;  // Deselect apps if we're turning off removable mode
                }
            }
        }

        private void UpdateSelectedCount()
        {
            Console.WriteLine("HELLO");
            SelectedCount = Applications.Count(app => app.IsSelected);
        }

        private void DeleteSelectedApps()
        {
            var selectedApps = Applications.Where(app => app.IsSelected && app.IsRemovable).ToList();

            foreach (var app in selectedApps)
            {
                app.DeleteApplication();
                Applications.Remove(app);
                IsRemovingApps = false;
                UpdateSelectedCount();
            }
        }


        private void OnApplicationDeleted(ApplicationModel app)
        {
            LoadApplications();
        }

        private void ClearTableAndReloadApplications()
        {
            string connectionString = "Data Source=applications.db";
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string deleteQuery = "DELETE FROM Applications";
                using (SqliteCommand command = new SqliteCommand(deleteQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            LoadApplications();
        }


    }
}
