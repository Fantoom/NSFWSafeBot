using System;
using System.IO;
using Newtonsoft.Json;
namespace NSFWSafeBot
{
	class Program
	{

		static BotClient botClient;
		static void Main(string[] args)
		{

			try
			{
				var conf = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "config.json")));
				DBController.Init();
				botClient = new BotClient(conf.token);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
			while (true)
			{
				if (Console.ReadLine() == "!")
					break;
			}
		}
	}
}
