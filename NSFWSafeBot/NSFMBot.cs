using NSFWSafeBot.Commands;
using NSFWSafeBot.Commands.Data;
using NSFWSafeBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InlineQueryResults.Abstractions;
using Telegram.Bot.Types.ReplyMarkups;

namespace NSFWSafeBot
{
    public sealed class NsfmBot
    {
        private readonly string _token;
        private readonly List<ICommand> _commands;

        public static TelegramBotClient BotClient { get; private set; }

        public NsfmBot(string token)
        {
            _token = token;
            InitializeBotCommands(out _commands);
            InitializeBotClient();
        }

        private void InitializeBotCommands(out List<ICommand> commands)
        {
            commands = new List<ICommand>
            {
                new DeleteMessage(),
                new Command
                {
                    CommandName = "start",
                    Action = (msg) =>
                    {
                        BotClient.SendTextMessageAsync(msg.Chat.Id, "Write NSFW message to show in inline mode ");
                    }
                }
            };
        }

        private void InitializeBotClient()
        {
            BotClient = new TelegramBotClient(_token);
            BotClient.OnMessage += BotClient_OnMessage;
            BotClient.OnInlineQuery += BotClient_OnInlineQuery;
            BotClient.OnCallbackQuery += BotClient_OnCallbackQuery;
        }

        private async void BotClient_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            var callBackContent = new CallbackQueryContent(e.CallbackQuery);
            await TryToForwardMessageAsync(callBackContent);
        }

        private Task TryToForwardMessageAsync(CallbackQueryContent callBackContent)
        {
            try
            {
                return BotClient.ForwardMessageAsync(
                    callBackContent.ChatId,
                    callBackContent.ForwardToChatId,
                    callBackContent.MessageId);
            }
            catch (ChatNotInitiatedException)
            {
                return BotClient.AnswerCallbackQueryAsync(
                    callBackContent.CallbackId,
                    "First write to the bot.",
                    true);
            }
            catch (Exception)
            {
                return BotClient.AnswerCallbackQueryAsync(
                    callBackContent.CallbackId,
                    "Unpredicted error happened.",
                    true);
            }
        }

        private async void BotClient_OnMessage(object sender, MessageEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Message.Text))
            {
                return;
            }

            if (!IsCommand(e.Message))
            {
                await DBController.AddMessageAsync(new ChatMessage
                {
                    ChatId = e.Message.Chat.Id,
                    MessageId = e.Message.MessageId,
                    UserId = e.Message.From.Id,
                    Message = e.Message
                });
            }
            else
            {
                var command = _commands.FirstOrDefault(x => x.CommandName == GetCommandName(e.Message));
                if (command is null)
                {
                    await BotClient.SendTextMessageAsync(e.Message.Chat.Id, "Unknown command");
                    return;
                }

                await command.ExecuteAsync(e.Message);
            }
        }

        private bool IsCommand(Message message)
        {
            return message.Text.StartsWith("/");
        }

        private string GetCommandName(Message message)
        {
            // Skip slash character that comes first
            return message.Text.Substring(1);
        }

        private async void BotClient_OnInlineQuery(object sender, InlineQueryEventArgs e)
        {
            var articles = new List<InlineQueryResultBase>();

            var userMessages = await DBController.GetUserMessagesAsync(e.InlineQuery.From.Id);
            uint resultArticleId = 0;
            foreach (var userChatMessage in userMessages)
            {
                var message = userChatMessage.Message;
                var markup = GetArticleMarkup(message);
                var newArticle = new InlineQueryResultArticle(
                    resultArticleId.ToString(),
                    GetArticleTitle(message),
                    new InputTextMessageContent($"NSFW content sent by {e.InlineQuery.From.Username}"))
                {
                    ReplyMarkup = markup
                };

                if (userChatMessage.Message.Type == MessageType.Photo)
                {
                    await AddArticleThumb(newArticle, message.Photo[0]);
                }
                articles.Add(newArticle);
                resultArticleId++;
            }

            await BotClient.AnswerInlineQueryAsync(
             inlineQueryId: e.InlineQuery.Id,
             results: articles,
             isPersonal: true,
             cacheTime: 0
            );
        }

        private string GetArticleTitle(Message message)
        {
            return string.IsNullOrEmpty(message.Text)
                ? $"{message.Caption} ({message.Type})"
                : message.Text;
        }

        private InlineKeyboardMarkup GetArticleMarkup(Message message)
        {
            return new InlineKeyboardMarkup(
                new InlineKeyboardButton
                {
                    Text = "View NSFW content",
                    CallbackData = $"{message.Chat.Id}|{message.MessageId}"
                });
        }

        private async Task AddArticleThumb(IThumbnailInlineQueryResult article, PhotoSize messagePhoto)
        {
            var photo = await BotClient.GetFileAsync(messagePhoto.FileId);
            article.ThumbUrl = "https://api.telegram.org/file/bot" + _token + "/" + photo.FilePath;
            article.ThumbWidth = messagePhoto.Width;
            article.ThumbHeight = messagePhoto.Height;
        }

        public void StartPolling()
        {
            BotClient.StartReceiving();
        }

        public void StopPolling()
        {
            BotClient.StopReceiving();
        }
    }
}