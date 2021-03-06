﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Core.Enums;
using Terminal.Core.Objects;
using Terminal.Core.Commands.Interfaces;
using Terminal.Core.Data.Entities;
using Terminal.Core.Settings;
using System.IO;
using Mono.Options;
using Terminal.Core.ExtensionMethods;
using Terminal.Core.Utilities;
using Terminal.Core.Data;

namespace Terminal.Core.Commands.Objects
{
    public class SETTINGS : ICommand
    {
        private IDataBucket _dataBucket;

        public SETTINGS(IDataBucket dataBucket)
        {
            _dataBucket = dataBucket;
        }

        public CommandResult CommandResult { get; set; }

        public IEnumerable<ICommand> AvailableCommands { get; set; }

        public string[] Roles
        {
            get { return RoleTemplates.AllLoggedIn; }
        }

        public string Name
        {
            get { return "SETTINGS"; }
        }

        public string Parameters
        {
            get { return "[Option(s)]"; }
        }

        public string Description
        {
            get { return "Allows you to set various options."; }
        }

        public bool ShowHelp
        {
            get { return true; }
        }

        public void Invoke(string[] args)
        {
            bool showHelp = false;
            bool? mute = null;
            bool changePassword = false;
            bool setTimeZone = false;
            bool? timeStamps = null;
            bool? autoFollow = null;
            bool? replyNotify = null;
            bool? messageNotify = null;
            string registration = null;

            var options = new OptionSet();
            options.Add(
                "?|help",
                "Show help information.",
                x => showHelp = x != null
            );
            options.Add(
                "m|mute",
                "Mute/unmute the terminal typing sound.",
                x => mute = x != null
            );
            options.Add(
                "changePassword",
                "Change your current password.",
                x => changePassword = x != null
            );
            options.Add(
                "setTimeZone",
                "Set your current time zone.",
                x => setTimeZone = x != null
            );
            options.Add(
                "timeStamps",
                "Turn timestamps on/off.",
                x => timeStamps = x != null
            );
            options.Add(
                "autoFollow",
                "Auto-follow topics you create or reply to.",
                x => autoFollow = x != null
            );
            options.Add(
                "replyNotify",
                "Display notifications about replies to your followed topics.",
                x => replyNotify = x != null
            );
            options.Add(
                "msgNotify",
                "Display notifications for unread messages in your inbox.",
                x => messageNotify = x != null
            );
            if (CommandResult.CurrentUser.IsAdministrator)
            {
                options.Add(
                    "reg|registration=",
                    "1=OPEN, 2=INVITE-ONLY, 3=CLOSED",
                    x => registration = x
                );
            }

            if (args == null)
            {
                CommandResult.WriteLine(DisplayTemplates.InvalidArguments);
            }
            else
            {
                try
                {
                    var parsedArgs = options.Parse(args).ToArray();

                    if (parsedArgs.Length == args.Length)
                    {
                        CommandResult.WriteLine(DisplayTemplates.InvalidArguments);
                    }
                    else
                    {
                        if (showHelp)
                        {
                            HelpUtility.WriteHelpInformation(this, options);
                        }
                        else if (changePassword)
                        {
                            if (CommandResult.CommandContext.PromptData == null)
                            {
                                CommandResult.WriteLine("Type your new password.");
                                CommandResult.PasswordField = true;
                                CommandResult.SetPrompt(Name, args, "NEW PASSWORD");
                            }
                            else if (CommandResult.CommandContext.PromptData.Length == 1)
                            {
                                CommandResult.WriteLine("Confirm your new password.");
                                CommandResult.PasswordField = true;
                                CommandResult.SetPrompt(Name, args, "CONFIRM PASSWORD");
                            }
                            else if (CommandResult.CommandContext.PromptData.Length == 2)
                            {
                                string password = CommandResult.CommandContext.PromptData[0];
                                string confirmPassword = CommandResult.CommandContext.PromptData[1];
                                if (password == confirmPassword)
                                {
                                    CommandResult.CurrentUser.Password = password;
                                    _dataBucket.UserRepository.UpdateUser(CommandResult.CurrentUser);
                                    _dataBucket.SaveChanges();
                                    CommandResult.WriteLine("Password changed successfully.");
                                }
                                else
                                    CommandResult.WriteLine("Passwords did not match.");
                                CommandResult.RestoreContext();
                            }
                        }
                        else if (setTimeZone)
                        {
                            var timeZones = TimeZoneInfo.GetSystemTimeZones();
                            if (CommandResult.CommandContext.PromptData == null)
                            {
                                for (int index = 0; index < timeZones.Count; index++)
                                {
                                    var timeZone = timeZones[index];
                                    CommandResult.WriteLine(DisplayMode.DontType, "{{{0}}} {1}", index, timeZone.DisplayName);
                                }
                                CommandResult.WriteLine();
                                CommandResult.WriteLine("Enter time zone ID.");
                                CommandResult.SetPrompt(Name, args, "CHANGE TIME ZONE");
                            }
                            else if (CommandResult.CommandContext.PromptData.Length == 1)
                            {
                                string promptData = CommandResult.CommandContext.PromptData[0];
                                if (promptData.IsInt())
                                {
                                    int timeZoneId = promptData.ToInt();
                                    if (timeZoneId >= 0 && timeZoneId < timeZones.Count)
                                    {
                                        var timeZone = timeZones[timeZoneId];
                                        CommandResult.CurrentUser.TimeZone = timeZone.Id;
                                        _dataBucket.UserRepository.UpdateUser(CommandResult.CurrentUser);
                                        _dataBucket.SaveChanges();
                                        CommandResult.WriteLine("'{0}' successfully set as your current time zone.", timeZone.Id);
                                        CommandResult.RestoreContext();
                                    }
                                    else
                                    {
                                        CommandResult.WriteLine("'{0}' does not match any available time zone ID.", timeZoneId);
                                        CommandResult.RestoreContext();
                                        CommandResult.WriteLine("Enter time zone ID.");
                                        CommandResult.SetPrompt(Name, args, "CHANGE TIME ZONE");
                                    }
                                }
                                else
                                {
                                    CommandResult.WriteLine("'{0}' is not a valid time zone ID.", promptData);
                                    CommandResult.RestoreContext();
                                    CommandResult.WriteLine("Enter time zone ID.");
                                    CommandResult.SetPrompt(Name, args, "CHANGE TIME ZONE");
                                }
                            }
                        }
                        else
                        {
                            if (mute != null)
                            {
                                CommandResult.CurrentUser.Sound = !(bool)mute;
                                _dataBucket.UserRepository.UpdateUser(CommandResult.CurrentUser);
                                CommandResult.WriteLine("Sound successfully {0}.", (bool)mute ? "muted" : "unmuted");
                            }
                            if (timeStamps != null)
                            {
                                CommandResult.CurrentUser.ShowTimestamps = (bool)timeStamps;
                                _dataBucket.UserRepository.UpdateUser(CommandResult.CurrentUser);
                                CommandResult.WriteLine("Timestamps were successfully {0}.", (bool)timeStamps ? "enabled" : "disabled");
                            }
                            if (autoFollow != null)
                            {
                                CommandResult.CurrentUser.AutoFollow = (bool)autoFollow;
                                _dataBucket.UserRepository.UpdateUser(CommandResult.CurrentUser);
                                CommandResult.WriteLine("Auto-follow {0}.", (bool)autoFollow ? "activated" : "deactivated");
                            }
                            if (replyNotify != null)
                            {
                                CommandResult.CurrentUser.NotifyReplies = (bool)replyNotify;
                                _dataBucket.UserRepository.UpdateUser(CommandResult.CurrentUser);
                                CommandResult.WriteLine("Reply notifications were successfully turned {0}.", (bool)replyNotify ? "on" : "off");
                            }
                            if (messageNotify != null)
                            {
                                CommandResult.CurrentUser.NotifyMessages = (bool)messageNotify;
                                _dataBucket.UserRepository.UpdateUser(CommandResult.CurrentUser);
                                CommandResult.WriteLine("Message notifications were successfully turned {0}.", (bool)messageNotify ? "on" : "off");
                            }
                            if (registration != null)
                            {
                                var registrationStatus = _dataBucket.VariableRepository.GetVariable("Registration");
                                if (registration == "1")
                                {
                                    registrationStatus = "Open";
                                    CommandResult.WriteLine("Registration opened successfully.");
                                }
                                else if (registration == "2")
                                {
                                    registrationStatus = "Invite-Only";
                                    CommandResult.WriteLine("Registration set to invite only.");
                                }
                                else if (registration == "3")
                                {
                                    registrationStatus = "Closed";
                                    CommandResult.WriteLine("Registration closed successfully.");
                                }
                                _dataBucket.VariableRepository.ModifyVariable("Registration", registrationStatus);
                            }
                            _dataBucket.SaveChanges();
                        }
                    }
                }
                catch (OptionException ex)
                {
                    CommandResult.WriteLine(ex.Message);
                }
            }
        }
    }
}
