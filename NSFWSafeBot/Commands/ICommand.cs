using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace NSFWSafeBot.Commands
{
	public interface ICommand
	{
		public string CommandName { get; }
		public Task Execute(Telegram.Bot.Types.Message message);
	}
}
