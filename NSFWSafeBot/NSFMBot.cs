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
                },
                new Command
                {
                    CommandName = "cleaninline",
                    Action = (msg) =>
                    {
                         DBController.DeleteAllMessagesByUserAsync(msg.From.Id);
                         BotClient.SendTextMessageAsync(msg.Chat.Id, "Inline cleaned");
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

        private async Task TryToForwardMessageAsync(CallbackQueryContent callBackContent)
        {
            try
            {
                await BotClient.ForwardMessageAsync(
                   callBackContent.ChatId,
                   callBackContent.ForwardToChatId,
                   callBackContent.MessageId);
            }
            catch (ChatNotInitiatedException e)
            {
                await BotClient.AnswerCallbackQueryAsync(
                    callBackContent.CallbackId,
                    "First write to the bot.",
                    true);
            }
            catch (Exception e)
            {
                await BotClient.AnswerCallbackQueryAsync(
                   callBackContent.CallbackId,
                   "Unpredicted error happened.",
                   true);
            }
        }

        private async void BotClient_OnMessage(object sender, MessageEventArgs e)
        {

            if (!IsCommand(e.Message) && (e.Message.Type == MessageType.Text || e.Message.Type == MessageType.Photo || e.Message.Type == MessageType.Video || e.Message.Type == MessageType.Audio || e.Message.Type == MessageType.Voice))
            {
                await DBController.AddMessageAsync(new ChatMessage(e.Message));
                await BotClient.SendTextMessageAsync(e.Message.Chat.Id, "Added");
            }
            else if (IsCommand(e.Message))
            {
                var command = _commands.FirstOrDefault(x => x.CommandName == GetCommandName(e.Message));
                if (command is null)
                {
                    await BotClient.SendTextMessageAsync(e.Message.Chat.Id, "Unknown command");
                    return;
                }

                await command.ExecuteAsync(e.Message);
            }
            else
            {
                await BotClient.SendTextMessageAsync(e.Message.Chat.Id, "Send command or image, please");
            }
        }

        private bool IsCommand(Message message)
        {
            return !string.IsNullOrWhiteSpace(message.Text)
                   && message.Text.StartsWith("/");
        }

        private string GetCommandName(Message message)
        {
            // Skip slash character that comes first
            return message.Text.Substring(1);
        }

        private async void BotClient_OnInlineQuery(object sender, InlineQueryEventArgs e)
        {
            IEnumerable<ChatMessage> userMessages =
                await DBController.GetUserMessagesAsync(e.InlineQuery.From.Id, e.InlineQuery.Query);

            var results = new List<InlineQueryResultBase>();
            uint resultId = 0;
            foreach (var userChatMessage in userMessages)
            {
                //var fileId = userChatMessage.Message.Photo is null ? userChatMessage.Message.Document.Thumb.FileId : userChatMessage.Message.Photo?[0].FileId;
                //var photo = await BotClient.GetFileAsync(fileId);
                //var newArticle = new  (resultId.ToString(), photo.FileId);
                dynamic inlineResult = userChatMessage.Message.Type switch
                {
                    MessageType.Text => new InlineQueryResultArticle(resultId.ToString(), "", null),
                    MessageType.Photo => new InlineQueryResultCachedPhoto(resultId.ToString(), userChatMessage.Message.Photo?[0].FileId),
                    MessageType.Video => new InlineQueryResultCachedVideo(resultId.ToString(), userChatMessage.Message.Video.FileId, ""),
                    MessageType.Audio => new InlineQueryResultCachedAudio(resultId.ToString(), userChatMessage.Message.Audio.FileId),
                    MessageType.Voice => new InlineQueryResultCachedVoice(resultId.ToString(), userChatMessage.Message.Voice.FileId, ""),
                    MessageType.Document => new InlineQueryResultCachedDocument(resultId.ToString(), userChatMessage.Message.Document.FileId, ""),
                    _ => null
                };
                if (userChatMessage.Message.Type == MessageType.Document)
                {
                    if ((bool)userChatMessage.Message.Document?.FileName.Contains(".gif"))
                    {
                        inlineResult = new InlineQueryResultCachedGif(resultId.ToString(), userChatMessage.Message.Document.FileId);
                    }
                }
                var markup = new InlineKeyboardMarkup(new InlineKeyboardButton { Text = "View NSFW content", CallbackData = $"{userChatMessage.Message.Chat.Id}|{userChatMessage.Message.MessageId}" });
                inlineResult.ReplyMarkup = markup;
                inlineResult.Title = $"{userChatMessage.Message.Text} ({userChatMessage.Message.Type.ToString()})";
                inlineResult.InputMessageContent = new InputTextMessageContent($"NSFW content sent by {e.InlineQuery.From.Username}");
                results.Add(inlineResult);
                resultId++;
            }

            try
            {
                await BotClient.AnswerInlineQueryAsync(
                    inlineQueryId: e.InlineQuery.Id,
                    results: results,
                    isPersonal: true,
                    cacheTime: 0);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private async Task AddArticleThumb(IThumbnailInlineQueryResult article, PhotoSize messagePhoto)
        {
            var photo = await BotClient.GetFileAsync(messagePhoto.FileId);
            article.ThumbUrl = $"https://api.telegram.org/file/bot{_token}/{photo.FilePath}";
            article.ThumbHeight = 100;
            article.ThumbWidth = 100;
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