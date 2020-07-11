using Newtonsoft.Json;
using System;
using System.IO;

namespace NSFWSafeBot
{
    internal static class Program
    {
        private const string ConfigFileName = "config.json";

        private static NsfmBot _botClient;

        private static void Main()
        {
            try
            {
                var config = ReadConfigFromJsonFile();
                _botClient = new NsfmBot(config.Token);
                _botClient.StartPolling();

                while (true)
                {
                    if (Console.ReadLine() == "!")
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                _botClient.StopPolling();
            }
        }

        private static Config ReadConfigFromJsonFile()
        {
            string configPath = Path.Combine(Directory.GetCurrentDirectory(), ConfigFileName);
            string configJsonContent = File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<Config>(configJsonContent);
        }
    }
}