using DashMaster.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Data.Sqlite;

namespace DashMaster.Services
{
    public class ApplicationScannerService
    {


        // Expanded list of keywords to exclude non-game executables
        private readonly List<string> ExcludedKeywords = new List<string>
        {
            "vconsole", "shader", "convert", "tool", "launcher", "crash", "update", "updater",
            "install", "editor", "maker", "compile", "steamerror", "report", "sender",
            "service", "checker", "debug", "helper", "setup", "demo", "test", "diagnostic",
            "bspzip", "gmad", "gmpublish", "hammer", "hlfaceposer", "hlmv", "studiomdl",
            "vbsp", "vbspinfo", "vpk", "vrad", "vtex", "vvis", "pdfinfo", "pdftopng",
            "w9xpopen", "x64", "redist", "x86", "static", "acs", "statis", "render", "vlang", "play", "eac",
            "client", "win64", "hl2" // Known non-game utility files
        };

        public List<ApplicationModel> ScanForApplications(List<string> folderPaths, bool isSingleExe = false)
        {
            var applications = new List<ApplicationModel>();
            string[] exeFiles;

            foreach (var folder in folderPaths)
            {
                var gameName =  Path.GetFileName(folder);
                if (isSingleExe)
                {
                    exeFiles = new string[] { folder };
                }else
                {
                    exeFiles = Directory.GetFiles(folder, "*.exe", SearchOption.AllDirectories);
                }
                

                foreach (var exePath in exeFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(exePath).ToLower();


                    if (!isSingleExe && ExcludedKeywords.Any(keyword => fileName.Contains(keyword)))
                        continue;


                    // Skip executables found in 'tools', 'sdk', or similar directories
                    if (exePath.ToLower().Contains("tools") || exePath.ToLower().Contains("sdk"))
                        continue;

                    // Ensure no duplicates
                    if (applications.Any(app => app.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    // Get icon path
                    string iconPath = GetIcon(exePath);
                    Console.WriteLine($"Icon path for {fileName}: {iconPath}");

                    // If all checks pass, consider this a valid game executable
                    applications.Add(new ApplicationModel
                    {
                        Name = gameName,
                        Path = exePath,
                        IconPath = iconPath
                    });
                }
            }

            // Log the found applications for debugging
            foreach (var app in applications)
            {
                SaveApplication(app);
            }

            return applications;
        }

        public void SaveApplication(ApplicationModel app)
        {
            string connectionString = "Data Source=applications.db;";

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Check if the application already exists
                string checkQuery = "SELECT COUNT(1) FROM Applications WHERE Name = @Name";
                using (SqliteCommand checkCommand = new SqliteCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@Name", app.Name);
                    long count = (long)checkCommand.ExecuteScalar();

                    if (count > 0)
                    {
                        // Application already exists, so skip the insert
                        Console.WriteLine($"Application with name '{app.Name}' already exists. Skipping insert.");
                        return;
                    }
                }

                // Application does not exist, proceed with the insert
                string insertQuery = "INSERT INTO Applications (Name, Icon, Path) VALUES (@Name, @Icon, @Path)";
                using (SqliteCommand insertCommand = new SqliteCommand(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue("@Name", app.Name);
                    insertCommand.Parameters.AddWithValue("@Icon", app.IconPath);  // Ensure Icon is a string
                    insertCommand.Parameters.AddWithValue("@Path", app.Path);
                    insertCommand.Parameters.AddWithValue("@IconPath", app.IconPath);
                    insertCommand.ExecuteNonQuery();
                }
            }
        }

        private string GetIcon(string exePath)
        {
            try
            {
                // Extract the associated icon
                Icon icon = Icon.ExtractAssociatedIcon(exePath);
                if (icon == null)
                {
                    Console.WriteLine($"No icon found for {exePath}");
                    return ""; // No icon found
                }

                using (Bitmap originalBitmap = icon.ToBitmap())
                {
                    // Resize the bitmap to 32x32 pixels
                    Bitmap resizedBitmap = new Bitmap(originalBitmap, new Size(64, 64));

                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(exePath);

                    // Define the path where the resized icon will be saved
                    string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    // Save the resized bitmap as a PNG file
                    string savePath = Path.Combine(folderPath, $"{fileNameWithoutExtension}_32x32.png");

                    resizedBitmap.Save(savePath, ImageFormat.Png);

                    resizedBitmap.Dispose();

                    return savePath;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("Access to the path is denied: " + ex.Message);
                return ""; // Access denied
            }
            catch (IOException ex)
            {
                Console.WriteLine("An I/O error occurred: " + ex.Message);
                return ""; // I/O error
            }
            catch (Exception ex)
            {
                Console.WriteLine("An unexpected error occurred: " + ex.Message);
                return ""; // Unexpected error
            }
        }
    }
}

