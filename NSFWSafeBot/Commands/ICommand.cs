using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace NSFWSafeBot.Commands
{
    public interface ICommand
    {
        public string CommandName { get; }

        Task ExecuteAsync(Message message);
    }
}