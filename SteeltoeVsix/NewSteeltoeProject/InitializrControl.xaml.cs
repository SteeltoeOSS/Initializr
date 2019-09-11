using EnvDTE;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Linq;
using Microsoft;

namespace NewSteeltoeProject
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class InitializrControl : DiscoveryDialog
    {
        public ObservableCollection<Dependency> Dependencies { get; set; }

      
        public InitializrControl()
        {
            InitializeComponent();
            Dependencies = new ObservableCollection<Dependency>();
            //Dependencies.Add(new Dependency { Name = "Hystrix " });
            //Dependencies.Add(new Dependency { Name = "Actuator " });
            //Dependencies.Add(new Dependency { Name = "SqlServer " });
            //Dependencies.Add(new Dependency { Name = "Dynamic Logging " });

            // TODO: Pull dependencies from pws url
            var deps = GetDependenciesAsync().Result;
            foreach(var dep in deps)
            {
                Dependencies.Add(dep);
            }

            //  this.browser.AllowNavigation = true;
            this.DataContext = this;
        }



        void OnClick2(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            btn2.Foreground = new SolidColorBrush(Colors.Green);


            var stringDependencies = GetSelectedDependencies();
            bool done = false;
            while (!done)
            {
                var proc = new System.Diagnostics.Process();
                var arguments = "new Steeltoe-WebApi " + stringDependencies;
                string workingDir = GetSettingsPath() + Path.DirectorySeparatorChar + txtProjectName.Text;
                if(!Directory.Exists(workingDir))
                {
                    Directory.CreateDirectory(workingDir);
                }
                else
                {
                    throw new Exception("Project exists; select a different name");
                }
                string text = ExecuteProcess(proc, "dotnet", arguments, out var errorText, workingDir);
            
                if (text.Contains("No templates matched the input template name:"))
                {
                     arguments = "new -i steeltoe.templates::2.2.0 --nuget-source https://www.myget.org/F/steeltoedev/api/v3/index.json";
                     ExecuteProcess(proc, "dotnet", arguments, out var errorText2, "");
                     //TODO: check for install failure
                }
                else if (text.Contains("was created successfully."))
                {
                    DTE dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
                    Assumes.Present(dte);
                    dte.ExecuteCommand("File.OpenProject", workingDir + Path.DirectorySeparatorChar + txtProjectName.Text + ".csproj");
                    this.Close();
                    return;
                }
                else if (string.IsNullOrEmpty(text))
                {
                    return;
                }

            }
            

        }

        private string ExecuteProcess(System.Diagnostics.Process proc, string program, string arguments, out string error, string workingdir = "")
        {
            proc.StartInfo.UseShellExecute = false;

            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;

            proc.StartInfo.FileName = program;
            proc.StartInfo.Arguments = arguments;
            if (!string.IsNullOrEmpty(workingdir))
            {
                proc.StartInfo.WorkingDirectory = workingdir;
            }
            proc.Start();
            WaitForExit(proc);

            error = proc.StandardError.ReadToEnd();
            return proc.StandardOutput.ReadToEnd();
        }

        private string GetSelectedDependencies()
        {
            string dependenciesString = "";
            foreach(var item in Dependencies.Where(d=> d.IsSelected))
            {
                dependenciesString += " --" + item.ShortName;
            }
            return dependenciesString;
        }

        private void WaitForExit(System.Diagnostics.Process process)
        {
            while(!process.HasExited)
            {
                System.Threading.Thread.Sleep(100);
            }
        }
        private string GetSettingsPath()
        {
            var settingName = "VisualStudioProjectsLocation";
            SettingsManager sm = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            SettingsStore ss = sm.GetReadOnlySettingsStore(SettingsScope.UserSettings);
            string setting = "";
            try
            {
                setting = ss.GetString("", settingName);
            }
            catch(Exception ex)
            {
                string  collection = "";
                foreach (string s in ss.GetPropertyNames(""))
                {
                    collection += s + "\r\n";
                }
                MessageBox.Show($"Cannot find {settingName} in {collection}");
            }
            return Environment.ExpandEnvironmentVariables(setting);
        }
        private async Task<List<Dependency>> GetDependenciesAsync()
        {
            var url = "https://start.steeltoe.io/api/templates/dependencies";
            try
            {
             
                var request = WebRequest.Create(url);
                var response = request.GetResponse();
                using (Stream dataStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(dataStream);
                     string responseBody = reader.ReadToEnd();
                     return JsonConvert.DeserializeObject<List<Dependency>>(responseBody);
                }
            }

            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
            return null;
        }
       
    }
    public class Dependency
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public bool IsSelected { get; set; } = false;
    }
}
