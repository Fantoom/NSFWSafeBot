using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace NSFWSafeBot.Commands
{
    public sealed class Command : ICommand
    {
        public string CommandName { get; set; }

        public Action<Message> Action;

        public Task ExecuteAsync(Message message)
        {
            return Task.Run(() => Action(message));
        }
    }
}