using System.Threading.Tasks;

namespace NSFWSafeBot.Commands
{
    public class DeleteMessage : ICommand
    {
        public string CommandName => "delete";

        public Task ExecuteAsync(Telegram.Bot.Types.Message message)
        {
            if (message.ReplyToMessage != null)
            {
                return DBController.DeleteMessageByIdAsync(message.Chat.Id, message.ReplyToMessage.MessageId);
            }
            return NsfmBot.BotClient.SendTextMessageAsync(message.Chat.Id, "Send delete as reply to message you want to delete");
        }
    }
}