﻿using System;
using System.Reflection;
using Loki.Maple.Characters;

namespace Loki.Maple.Commands
{
	static class CommandFactory
	{
		public static Commands Commands { get; private set; }

		public static void Initialize()
		{
			CommandFactory.Commands = new Commands();

			foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
			{
				if (type.IsSubclassOf(typeof(Command)))
				{
					CommandFactory.Commands.Add((Command)Activator.CreateInstance(type));
				}
			}
		}

		public static void Execute(Character caller, string text)
		{
			string[] splitted = text.Split(' ');
			splitted[0] = splitted[0].ToLower();
            string commandName = "";
            if (text.StartsWith(Application.CommandIndicator))
                commandName = splitted[0].TrimStart(Application.CommandIndicator.ToCharArray());
            else if (text.StartsWith(Application.PlayerCommandIndicator))
                commandName = splitted[0].TrimStart(Application.PlayerCommandIndicator.ToCharArray());
            else
                commandName = splitted[0];

			string[] args = new string[splitted.Length - 1];

			for (int i = 1; i < splitted.Length; i++)
			{
				args[i - 1] = splitted[i];
			}

			if (CommandFactory.Commands.Contains(commandName))
			{
				Command command = CommandFactory.Commands[commandName];

                if ((command.IsRestricted && text.StartsWith(Application.CommandIndicator)) || (!command.IsRestricted && text.StartsWith(Application.PlayerCommandIndicator)))
                {
                    if ((command.IsRestricted && caller.IsMaster) || !command.IsRestricted)
                    {
                        try
                        {
                            command.Execute(caller, args);
                        }
                        catch (Exception e)
                        {
                            caller.Notify("[Command] Unknown error: " + e.Message);
                            Log.Error("{0} error by {1}: ", e, command.GetType().Name, caller.Name);
                        }
                    }
                    else
                    {
                        caller.Notify("[Command] Restricted command.");
                    }
                }
                else
                {
                    caller.Notify("[Command] Invalid command.");
                }
			}
			else
			{
				caller.Notify("[Command] Invalid command.");
			}
		}
	}
}