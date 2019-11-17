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
    public static class Handlers
    {
        public static async Task OnMessage(Message msg)
        {
            try
            {
                if (msg.Type == MessageType.ChatMembersAdded && msg.NewChatMembers.Any(x => x.Id == Program.Bot.BotId))
                {
                    await msg.Reply($"Hi there! I am @{Program.OwnUsername} " +
                        $"(<a href=\"https://github.com/GreyWolfDev/RevokeLinkBot\">🔗 GitHub</a>) " +
                        $"and I can make your group more secure " +
                        $"by getting rid of the invite link mess that telegram groups tend to have.\n\n" +
                        $"For an explanation of what I do and why, please read this article:\n" +
                        $"https://telegra.ph/RevokeLinkBot---Manual-11-17\n\nThanks for using me!");
                }
                
                if (msg.Type != MessageType.Text) return;
                if (!msg.Entities?.Any(x => x.Type == MessageEntityType.BotCommand && x.Offset == 0) ?? true) return;

                msg.Text = msg.Text.ToLower();
                if (msg.Text.EndsWith($"@{Program.OwnUsername}")) msg.Text = msg.Text.Remove(msg.Text.Length - Program.OwnUsername.Length - 1);
                var args = msg.Text.Split(' ');
                switch (args[0])
                {
                    case "/start": 
                        if (msg.Chat.Type != ChatType.Private) return;
                        if (args.Length > 1)
                        {
                            switch (args[1])
                            {
                                case "donate":
                                    goto donate;
                            }
                        }
                        goto help;

                    case "/help":
                    help:
                        await msg.Reply($"Hi there! I am @{Program.OwnUsername} " +
                            $"(<a href=\"https://github.com/GreyWolfDev/RevokeLinkBot\">🔗 GitHub</a>) " +
                            $"and I can make your group more secure " +
                            $"by getting rid of the invite link mess that telegram groups tend to have.\n\n" +
                            $"For an explanation of what I do and why, please read this article:\n" +
                            $"https://telegra.ph/RevokeLinkBot---Manual-11-17\n\nThanks for using me!");
                        return;

                    case "/donate":
                    donate:
                        if (msg.Chat.Type != ChatType.Private)
                        {
                            await msg.Reply("If you would like to donate me something, please " +
                                $"<a href=\"https://t.me/{Program.OwnUsername}?start=donate\">message me in PM!</a>");
                            return;
                        }
                        await msg.Reply("This bot is under Grey Wolf Development, all donations will go to the same place. Want to keep our bots online? Donate now and gets: Custom gifs and Badges!\n\nClick the button below to donate (you will be redirected to the werewolf bot), and go to @greywolfsupport to claim your reward if you donate via Paypal!\n\nMore Info: https://telegra.ph/Custom-Gif-Packs-and-Donation-Levels-06-27");
                        return;

                    case "/getlink":
                        if (msg.Chat.Type == ChatType.Private)
                        {
                            await msg.Reply("This command can only be used in groups by the group administrators!");
                            return;
                        }
                        if (!await msg.IsFromAdmin())
                        {
                            await msg.Reply("This command can only be used by the group administrators!");
                            return;
                        }
                        if (msg.Chat.Username != null)
                        {
                            await msg.Reply("<b>💡 This group is public</b>, meaning it has a username " +
                                "(@" + msg.Chat.Username + "). " +
                                "Using this bot only makes sense in private groups, as I wouldn't be able to revoke " +
                                "the username in a public group anyway. Consider making this group private by removing " +
                                "its username, then you can use me for higher security.");
                            return;
                        }
                        Chat chat;
                        try
                        {
                            chat = await Program.Bot.GetChatAsync(msg.Chat.Id);
                        }
                        catch (Exception e)
                        {
                            await msg.Reply("An error occured while trying to retrieve this chat:\n" +
                                (e.InnerException ?? e).Message.FormatHTML());
                            return;
                        }
                        IReplyMarkup markup;
                        string text;

                        if (chat.InviteLink != null)
                        {
                            markup = new InlineKeyboardMarkup(
                                new InlineKeyboardButton[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Revoke ↪️", "revokelink"),
                                    InlineKeyboardButton.WithCallbackData("Cancel ❌", "cancel"),
                                }
                            );

                            text = $"<b>Your current invite link:</b>\n" + chat.InviteLink +
                                "\n\nPress \"<b>Revoke</b>\" to revoke this link, or \"<b>Cancel</b>\" to close this message.";
                        }
                        else
                        {
                            markup = new InlineKeyboardMarkup(
                                new InlineKeyboardButton[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Create 💡", "createlink"),
                                    InlineKeyboardButton.WithCallbackData("Cancel ❌", "cancel"),
                                }
                            );

                            text = "<b>You do not currently have an invite link!</b>" +
                                "\n\nPress \"<b>Create</b>\" to create a link, or \"<b>Cancel</b>\" to close this message.";
                        }

                        await msg.Reply(text, markup);

                        if (chat.Permissions?.CanInviteUsers ?? false)
                        {
                            await msg.Reply("<b>💡 By the way:</b> Currently, all members of this group are allowed to add other members. " +
                                "To make your group more secure, go to group permissions and turn that setting off.\n\n" +
                                "Your group members can still invite other people via the group link if you give it to them.",
                                quote: false);
                        }
                        return;
                }
            }
            catch (Exception e)
            {
                await e.Log();
            }
        }

        public static async Task OnCallbackQuery(CallbackQuery call)
        {
            try
            {
                if (!await call.IsFromAdmin())
                {
                    await call.Answer("You are not a group administrator! Only group administrators can do this!\n\n" +
                        "If you are actually an administrator, please try again in one minute.", true);
                    return;
                }
                if (call.Data == "cancel")
                {
                    await call.Message.EditInlineKeyboard(null);
                    await call.Answer("Cancelled.");
                    return;
                }
                if (call.Message.Chat.Username != null)
                {
                    await call.Message.Edit("<b>💡 This group is public</b>, meaning it has a username " +
                        "(@" + call.Message.Chat.Username + "). " +
                        "Using this bot only makes sense in private groups, as I wouldn't be able to revoke " +
                        "the username in a public group anyway. Consider making this group private by removing " +
                        "its username, then you can use me for higher security.");
                    return;
                }

                bool firstTime;

                switch (call.Data)
                {
                    case "revokelink":
                        firstTime = false;
                        goto makelink;

                    case "createlink":
                        firstTime = true;
                        goto makelink;

                    makelink:
                        string answertext, messagetext;
                        if (firstTime)
                        {
                            answertext = "A link has been created by me! Use thee /getlink command again to see it.";
                            messagetext = "<b>✅ A link has been created by me!</b>\n\nThis is your group link:\n";
                        }
                        else
                        {
                            answertext = "Your link has been revoked! Use the /getlink command again to see the new link.\n\nIf your group is just being spammed, I recommend viewing the link later when the attack is over :)";
                            messagetext = "<b>✅ The link has been revoked!</b> The new group link can be viewed by using the <code>/getlink</code> command again.\n\nIf this group is just being spammed, I recommend viewing the link later when the attack is over :)";
                        }

                        string link;
                        try
                        {
                            link = await call.Message.Chat.ExportInviteLink();
                        }
                        catch (Exception e)
                        {
                            if (e.Message == "Bad Request: not enough rights to export chat invite link")
                            {
                                await call.Message.Edit("I cannot do that, I am lacking the right to invite members! " +
                                    "Please give me the administrator right to invite new members and try again!");
                                await call.Answer("I cannot do that, I am lacking the right to invite members! " +
                                    "Please give me the administrator right to invite new members and try again!", true);
                            }
                            else
                            {
                                await call.Message.Edit("An error occured while trying to generate a new link:\n" +
                                    (e.InnerException ?? e).Message.FormatHTML());
                                await call.Answer("An error occured while trying to generate a chat link! " +
                                    "I posted the error details in the group.", true);
                            }
                            return;
                        }
                        await call.Answer(answertext, true);
                        await call.Message.Edit(messagetext + (firstTime ? link : ""));
                        return;
                }
            }
            catch (Exception e)
            {
                await e.Log();
            }
        }
    }
}
