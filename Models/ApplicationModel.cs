﻿using DashMaster.MVVM;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DashMaster.Models
{
    public class ApplicationModel : ViewModelBase
    {
        private bool _isRemovable { get; set; }
        private bool _isSelected { get; set; }

        public string Name { get; set; }
        public string Path {  get; set; }
        public string IconPath { get; set; }
        public BitmapSource Icon { get; set; }
        
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsRemovable
        {
            get => _isRemovable;
            set
            {
                if (_isRemovable != value) { }
                _isRemovable = value;
                OnPropertyChanged();
            }
        }

        public ICommand ExecuteAppCommand { get; }

        public ApplicationModel()
        {
            Console.WriteLine(IconPath);
            ExecuteAppCommand = new RelayCommand(ExecuteApp);
        }

        public void LoadIcon()
        {
            if (!string.IsNullOrEmpty(IconPath) && File.Exists(IconPath))
            {
                using (FileStream stream = new FileStream(IconPath, FileMode.Open, FileAccess.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = stream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    Icon = bitmapImage;
                }
            }
        }

        public event Action<ApplicationModel> ApplicationDeleted;
        public event Action ApplicaitonSelected;

        // Handles the app state by checking if user wants to execute the app or execute a selection for deletion
        private void ExecuteApp(object parameter)
        {
            Console.WriteLine(parameter);
            if (!IsRemovable && parameter is string exePath)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception e)
                {
                    // Handle exceptions here
                    Console.WriteLine("Error: " + e.Message);
                }
            }
            else
            {
                SelectApplication();
                ApplicaitonSelected?.Invoke();
            }
        }

        private void SelectApplication()
        {
            IsSelected = !IsSelected;
        }



        public void DeleteApplication()
        {
            string connectionString = "Data Source=applications.db";
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string deleteQuery = "DELETE FROM Applications WHERE Name = @Name";

                using (SqliteCommand command = new SqliteCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@Name", Name);
                    command.ExecuteNonQuery();

                    if (!string.IsNullOrEmpty(IconPath) && File.Exists(IconPath))
                    {
                        File.Delete(IconPath);
                    }
                }
            }

            ApplicationDeleted?.Invoke(this);
        }

    }
}
