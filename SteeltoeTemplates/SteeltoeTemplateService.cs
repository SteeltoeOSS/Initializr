using System.Collections.Generic;
using System.IO;
using System.Linq;
using InitializrApi.Models;
using Newtonsoft.Json;
using Stubble.Core;
using Stubble.Core.Builders;
using Stubble.Extensions.JsonNet;
using System.IO.Compression;
using System.Text;
using Microsoft.Extensions.Logging;

namespace InitializrApi.Services
{
    public class SteeltoeTemplateService : ISteeltoeTemplateService
    {
        StubbleVisitorRenderer _stubble;
        ILogger<SteeltoeTemplateService> _logger;

        public SteeltoeTemplateService(ILogger<SteeltoeTemplateService> logger)
        {
            _stubble = new StubbleBuilder()
                .Configure(settings => settings.AddJsonNet())
                .Build();
            _logger = logger;
        }
        public byte[] GenerateProject(GeneratorModel model)
        {
            // Set a variable to the My Documents path.

            var name = "WebApi-CSharp";
            string current = Directory.GetCurrentDirectory();
            var templatePath = Path.Combine(current, "SteeltoeTemplates", "templates", name);
            var json = File.ReadAllText(Path.Combine(templatePath, "mustache.json"));
            var dataView = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            foreach(var dependency in model.dependencies)
            {
                if(dataView.ContainsKey(dependency))
                {
                    dataView[dependency] = "true";
                }
            }
            var listoffiles = new List<KeyValuePair<string, string>>();
            byte[] archiveBytes;

            foreach (var file in Directory.EnumerateFiles(templatePath,"*", SearchOption.AllDirectories))
            {
                if (file.EndsWith("mustache.json")) continue;
                var pathPrefix = file.Replace(Path.GetFullPath(templatePath), "").TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                string fileText = File.ReadAllText(file);

                if (file.EndsWith(".csproj"))
                {
                    pathPrefix = pathPrefix.Replace("ReplaceMe", "SteeltoeExample"); // get from model
                    var output = _stubble.Render(fileText, dataView);
                    listoffiles.Add(new KeyValuePair<string, string>(pathPrefix, output));
                }
                else if (file.EndsWith(".cs"))
                {
                    var output = _stubble.Render(fileText, dataView);
                    listoffiles.Add(new KeyValuePair<string, string>(pathPrefix, output));
                }
                else
                {
                    listoffiles.Add(new KeyValuePair<string, string>(pathPrefix, fileText));
                }
            }


            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {

                    foreach (var entry in listoffiles)
                    {
                        _logger.LogDebug(entry.Key);
                        var ef = archive.CreateEntry(entry.Key, CompressionLevel.Optimal);
                        using (var entryStream = ef.Open())
                        using (var fileToCompress = new MemoryStream(Encoding.UTF8.GetBytes(entry.Value)))
                        {
                            fileToCompress.CopyTo(entryStream);
                        }
                    }
                }
                archiveBytes = memoryStream.ToArray();

                //using (var fileStream = new FileStream(@"C:\Temp\test.zip", FileMode.Create))
                //{
                //    memoryStream.Seek(0, SeekOrigin.Begin);
                //    memoryStream.CopyTo(fileStream);
                //}

                //  ZipFile.CreateFromDirectory(outFolder, zipfile);
                //  return zipfile;
                
            }
            return archiveBytes;
        }
        public List<string> GetAvailableTemplates()
        {
            string current = Directory.GetCurrentDirectory();
            var templatesPath =Path.Combine(current, "SteeltoeTemplates", "templates");
            return Directory.GetDirectories(templatesPath).Select(path => new DirectoryInfo(path).Name).ToList();
        }

          

    }
}
