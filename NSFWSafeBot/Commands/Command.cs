using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace NSFWSafeBot.Commands
{
	class Command : ICommand
	{
		public string CommandName { get; set; }

		public Action<Telegram.Bot.Types.Message> action;

		public async Task Execute(Telegram.Bot.Types.Message message)
		{
			await Task.Run(() => action(message));
		}
	}
}
