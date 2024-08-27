using DashMaster.MVVM;
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
    public class ApplicationModel
    {
        public string Name { get; set; }
        public string Path {  get; set; }
        public string IconPath { get; set; }
        public BitmapSource Icon { get; set; }

        public ICommand ClickCommand { get; }

        public ApplicationModel()
        {
            Console.WriteLine(IconPath);
            ClickCommand = new RelayCommand(ExecuteApp);
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

        private void ExecuteApp(object parameter)
        {
            Console.WriteLine(parameter);
            if (parameter is string exePath)
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
        }

    }
}
