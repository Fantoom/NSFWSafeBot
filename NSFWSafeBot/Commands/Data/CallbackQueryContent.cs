using System;
using Telegram.Bot.Types;

namespace NSFWSafeBot.Commands.Data
{
    public sealed class CallbackQueryContent
    {
        private const int ChatIdPartOrderNumber = 0;
        private const int MessageIdPartOrderNumber = 1;

        public string CallbackId;
        public long ForwardToChatId;
        public int MessageId;
        public int ChatId;

        public CallbackQueryContent(CallbackQuery callbackQuery)
        {
            CallbackId = callbackQuery.Id;
            ChatId = callbackQuery.From.Id;
            ParseCallbackDataContent(callbackQuery.Data);
        }

        private void ParseCallbackDataContent(string callbackData)
        {
            string[] callbackDataParts = callbackData.Split('|', StringSplitOptions.RemoveEmptyEntries);
            ForwardToChatId = long.Parse(callbackDataParts[ChatIdPartOrderNumber]);
            MessageId = int.Parse(callbackDataParts[MessageIdPartOrderNumber]);
        }
    }
}