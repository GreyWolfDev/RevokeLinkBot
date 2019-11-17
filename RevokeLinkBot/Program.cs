using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace RevokeLinkBot
{
    class Program
    {
        public const long LogId = 295152997; //-1001098399855;
        public const string BasePath = @"C:\Olgabrezel\RevokeLinkBot";
        public static TelegramBotClient Bot;
        public static string OwnUsername;

        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += async (sender, e) =>
            {
                await ((Exception)e.ExceptionObject).Log(true);
            };

            Bot = new TelegramBotClient(File.ReadAllText(Path.Combine(BasePath, "Token.txt")));
            OwnUsername = Bot.GetMeAsync().Result.Username.ToLower();
            Bot.OnMessage += Bot_OnMessage;
            Bot.OnCallbackQuery += Bot_OnCallbackQuery;

            try
            {
                var updates = Bot.GetUpdatesAsync(offset: -1).Result;
                if (updates.Any()) Bot.GetUpdatesAsync(offset: updates.Max(x => x.Id) + 1).Wait();
            }
            catch { } // ignored

            Bot.StartReceiving();
            Bot.SendTextMessageAsync(LogId, "Started up!").Wait();
            Thread.Sleep(-1);
        }

        private static void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            new Task(async () => await Handlers.OnMessage(e.Message)).Start();
        }

        private static void Bot_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            new Task(async () => await Handlers.OnCallbackQuery(e.CallbackQuery)).Start();
        }
    }
}
