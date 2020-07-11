using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using NSFWSafeBot.Commands;
using System.Linq;

namespace NSFWSafeBot
{

	class BotClient
	{
		string token;
		public static TelegramBotClient botClient { get; private set; }
		private static List<ICommand> commands = new List<ICommand>();
		public BotClient(string token)
		{
			this.token = token;

			commands.Add(new DeleteMessage());
			commands.Add(new Command() { CommandName = "start", action = (x) => { botClient.SendTextMessageAsync(x.Chat.Id, "Write NSFW message to show in inline mode "); }});

			botClient = new TelegramBotClient(token);
			botClient.StartReceiving();
			botClient.OnMessage += BotClient_OnMessage;
			botClient.OnInlineQuery += BotClient_OnInlineQuery;
			botClient.OnCallbackQuery += BotClient_OnCallbackQuery;
		}

		private async void BotClient_OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
		{
			long.TryParse(e.CallbackQuery.Data.Split('|')[0], out var chatId);
			int.TryParse(e.CallbackQuery.Data.Split('|')[1], out var messageId);
			try
			{
				await botClient.ForwardMessageAsync(new ChatId(e.CallbackQuery.From.Id), new ChatId(chatId), messageId);
			}
			catch (Telegram.Bot.Exceptions.ChatNotInitiatedException)
			{
				await botClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "First write to the bot", true);
			}

		}

		private async void BotClient_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
		{
			if ((!e.Message.Text?.StartsWith("/") ?? true) || string.IsNullOrEmpty(e.Message.Text))
			{
				var msg = new Message
				{
					ChatId = e.Message.Chat.Id,
					MessageId = e.Message.MessageId,
					UserId = e.Message.From.Id,
					message = e.Message
				};
				await DBController.AddMessageAsync(msg);
			}
			else
			{
				var cmd = commands.Where(x => x.CommandName == e.Message.Text.Replace("/", "")).FirstOrDefault();
				if (cmd != null)
					await cmd.Execute(e.Message);
				else
					await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Invalid command");
			}
		}

		private async void BotClient_OnInlineQuery(object sender, Telegram.Bot.Args.InlineQueryEventArgs e)
		{
			List<InlineQueryResultBase> resultList = new List<InlineQueryResultBase>();
			var bot = (TelegramBotClient)sender;
			
			var messageList = await DBController.GetMessagesListAsync(e.InlineQuery.From.Id);
			int id = 0;
			foreach (var message in messageList)
			{
				var msg = message.message;
				var markup = new InlineKeyboardMarkup(new InlineKeyboardButton { Text = "View NSFW content", CallbackData = $"{message.message.Chat.Id}|{message.message.MessageId}" });
				var result = new InlineQueryResultArticle(id.ToString(), msg?.Text , new InputTextMessageContent($"NSFW content sent by{e.InlineQuery.From.Username}")) { ReplyMarkup = markup };
				
				if (result.Title == null)
				{
					result.Title = msg.Caption + $"  ({msg.Type.ToString()})";
				}

				if (msg.Photo != null && msg.Photo.Length > 0)
				{
					var photo = await botClient.GetFileAsync(msg.Photo[0].FileId);
					result.ThumbUrl = "https://api.telegram.org/file/bot" + token + "/" + photo.FilePath;
					result.ThumbWidth = msg.Photo[0].Width;
					result.ThumbHeight = msg.Photo[0].Height;
				}
				resultList.Add(result);
				id++;
			}

			await botClient.AnswerInlineQueryAsync(
			 inlineQueryId: e.InlineQuery.Id,
			 results: resultList,
			 isPersonal: true,
			 cacheTime: 0			
		    );
		}
	}
}
