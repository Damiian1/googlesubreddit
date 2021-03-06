﻿using Discord;
using Discord.Addons.EmojiTools;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using RoleButtonsBot.DTO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestEasyBot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("Ping")]
        public async Task Ping()
        {
            await Context.Channel.SendMessageAsync("Pong!");
        }
        [Command("help")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task help()
        {
            await Context.Channel.SendMessageAsync(
                "**.Assign** [MessageID] [Emoji] [RoleID] (Creates a link between a reaction and role on a message)" + Environment.NewLine +
                "**.RemoveEmojiLink** [LinkId] (Removes the spesified emojo link)" + Environment.NewLine +
                "**.RemoveAllEmojiLinks**(Removes all emoji links)" + Environment.NewLine +
                "**.ShowEmojiLinks**(Shows all the emoji links)" + Environment.NewLine +
                "**.ShowRoles** (Shows all the roles with their ID for setting up links)" + Environment.NewLine +
                "**.FindMessage [MessageID]** (tells you where to find the message by ID)" + Environment.NewLine +
                "**.Info** (Shows bot info)" + Environment.NewLine 
                );
        }
        [Command("Removeemojilink")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task delete(int LinkID)
        {
            foreach (var item in CommandHandler.buttonlinks.FindAll(x => x.GuildID == Context.Guild.Id && x.ID == LinkID))
            {
                CommandHandler.buttonlinks.Remove(item);
            }
            await Context.Channel.SendMessageAsync("Emoji link "+ LinkID+" have been removed!");

            File.WriteAllText("ButtonLinks.json", JsonConvert.SerializeObject(CommandHandler.buttonlinks));
        }
        [Command("RemoveAllemojilinks")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task deletealllinks()
        {
            foreach (var item in CommandHandler.buttonlinks.FindAll(x => x.GuildID == Context.Guild.Id))
            {
                CommandHandler.buttonlinks.Remove(item);
            }
            await Context.Channel.SendMessageAsync("All emoji links have been removed!");

            File.WriteAllText("ButtonLinks.json", JsonConvert.SerializeObject(CommandHandler.buttonlinks));
        }
        [Command("showemojilinks")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task list()
        {
            string msg = "";
            if (CommandHandler.buttonlinks.Count != 0)
            {


                foreach (var item in CommandHandler.buttonlinks.FindAll(x => x.GuildID == Context.Guild.Id))
                {

                    if (msg.Length > 1700)
                    {
                        Thread.Sleep(1000);
                        await Context.Channel.SendMessageAsync(msg);
                        msg = "";
                    }
                    else
                    {
                        SocketRole role = null;
                        SocketGuildUser user = null;
                        try
                        {

                            user = Context.Guild.GetUser(item.CreatedByID);
                            role = Context.Guild.GetRole(item.RoleID);
                        }
                        catch (Exception)
                        {

                        }
                        msg = msg + "( Id: " + item.ID + ")** Message : " + item.MessageID + "** -  Reaction : " + item.Emoji + " - Role : " + role.Name + "(" + role.Id + ")" + " - Created by : " + user + " on " + item.Created + Environment.NewLine;
                    }



                }
                await Context.Channel.SendMessageAsync(msg);
            }
            else
            {
                await Context.Channel.SendMessageAsync("There are no links!");
            }
    }
        [Command("assign")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task assign(ulong MessageID, string Icon,ulong RoleID)
        {
            if (CommandHandler.buttonlinks.FindAll(x => x.MessageID == MessageID && x.RoleID == RoleID && x.Emoji == Icon).Count != 0)
            {
                await Context.Channel.SendMessageAsync("Link cant create link cause it already exist!");
            }
            else
            {

                var link = new ButtonLinks();
                link.GuildID = Context.Guild.Id;
                link.MessageID = MessageID;
                link.RoleID = RoleID;
                link.Emoji = Icon;
                link.CreatedByID = Context.User.Id;
                link.Created = DateTime.Now;
                int i = 0;
                while (CommandHandler.buttonlinks.Exists(x => x.ID == i))
                {
                    i++;
                }
                link.ID = i;


                CommandHandler.buttonlinks.Add(link);
                await Context.Channel.SendMessageAsync("Link have been saved!");

                File.WriteAllText("ButtonLinks.json", JsonConvert.SerializeObject(CommandHandler.buttonlinks));
            }
        }
        [Command("showRoles")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task listRoles()
        {
            string msg = "";
            foreach (var role in Context.Guild.Roles)
            {
                if (!role.IsEveryone)
                {


                    if (msg.Length > 1700)
                    {
                        Thread.Sleep(1000);
                        await Context.Channel.SendMessageAsync(msg);
                        msg = "";
                    }
                    else
                    {
                        msg = msg + "**" + role.Name + "**(" + role.Id + ")" + Environment.NewLine;
                    }
                }
                
            }
            await Context.Channel.SendMessageAsync(msg);
        }
        [Command("info")]
        public async Task Info()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            await ReplyAsync(
                $"{Format.Bold("Info")}\n" +
                $"- Author: SindreMA#9630\n" +
                $"- Library: Discord.Net ({DiscordConfig.Version})\n" +
                $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n" +
                $"- Uptime: {GetUptime()}\n\n" +

                $"{Format.Bold("Stats")}\n" +
                $"- Heap Size: {GetHeapSize()} MB\n" +
                $"- Guilds: {(Context.Client as DiscordSocketClient).Guilds.Count}\n" +
                $"- Channels: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count)}\n" +
                $"- Users: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Users.Count)}"
            );
        }
        [Command("Findmessage")]
        public async Task findmsg(ulong MessageID)
        {
            foreach (var item in Context.Guild.TextChannels)
            {
                var be = item.GetMessageAsync(MessageID).Result;
            
                if (be != null)
                {
                    var content = be.Content;


                    const int MaxLength = 200;
                    if (content.Length > MaxLength)
                        content = content.Substring(0, MaxLength);

                    await Context.Channel.SendMessageAsync(
                        "The message was sent by " + be.Author.Username + " in " + be.Channel.Name + " at " + be.Timestamp.DateTime.AddHours(2) + Environment.NewLine + Environment.NewLine +
                        "Here is a sample of the message, so you get some keywords that you can search with : " + Environment.NewLine + Environment.NewLine + content + "..."
                        
                        );
                }
            }
           
        }
        [Command("setgame")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetGame([Remainder]string text)
        {
            if (Context.User.Id == 170605899189190656)
            {
                await Context.Client.SetGameAsync(text);
            }

        }
        private static string GetUptime()
           => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();
    }
}
