using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RevokeLinkBot
{
    public static class Extensions
    {
        public static async Task Log(this Exception e, bool unhandled = false)
        {
            string stacktrace = e.StackTrace;
            string message = "";
            do
            {
                message += e.Message + "\n\n";
                e = e.InnerException;
            }
            while (e != null);
            message += stacktrace;
            await Program.Bot.SendTextMessageAsync(Program.LogId, (unhandled ? "UNHANDLED " : "") + "Exception occured!\n\n" + message);
        }

        public static string FormatHTML(this string str)
        {
            return str.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        public static async Task<string> ExportInviteLink(this Chat chat)
        {
            return await Program.Bot.ExportChatInviteLinkAsync(chat.Id);
        }

        public static async Task<bool> IsFromAdmin(this Message m)
        {
            ChatMember cm = null;
            try
            {
                cm = await Program.Bot.GetChatMemberAsync(m.Chat.Id, m.From.Id);
            }
            catch { }
            if (cm == null) return false;
            return new[] { ChatMemberStatus.Creator, ChatMemberStatus.Administrator }.Contains(cm.Status);
        }

        public static async Task<bool> IsFromAdmin(this CallbackQuery call)
        {
            ChatMember cm = null;
            try
            {
                cm = await Program.Bot.GetChatMemberAsync(call.Message.Chat.Id, call.From.Id);
            }
            catch { };
            if (cm == null) return false;
            return new[] { ChatMemberStatus.Creator, ChatMemberStatus.Administrator }.Contains(cm.Status);
        }

        public static async Task<Message> Reply(this Message m, string text, IReplyMarkup replyMarkup = null, bool quote = true, ParseMode parseMode = ParseMode.Html, bool disableWebPagePreview = true, bool disableNotification = false)
        {
            try
            {
                return await Program.Bot.SendTextMessageAsync(m.Chat.Id, text, parseMode, null, disableWebPagePreview, disableNotification, quote ? m.MessageId : 0, true, replyMarkup);
            }
            catch
            {
                return null; // ignored
            }
        }

        public static async Task<Message> Edit(this Message m, string text, InlineKeyboardMarkup replyMarkup = null, ParseMode parseMode = ParseMode.Html, bool disableWebPagePreview = true)
        {
            try
            {
                return await Program.Bot.EditMessageTextAsync(m.Chat.Id, m.MessageId, text, parseMode, null, disableWebPagePreview, replyMarkup);
            }
            catch
            {
                return null; // ignored
            }
        }

        public static async Task<Message> EditInlineKeyboard(this Message m, InlineKeyboardMarkup newMarkup)
        {
            try
            {
                return await Program.Bot.EditMessageReplyMarkupAsync(m.Chat.Id, m.MessageId, newMarkup);
            }
            catch
            {
                return null; // ignored
            }
        }

        public static async Task Answer(this CallbackQuery call, string text = null, bool alert = false)
        {
            try
            {
                await Program.Bot.AnswerCallbackQueryAsync(call.Id, text, alert, null, 30);
            }
            catch { } // ignored
        }
    }
}
