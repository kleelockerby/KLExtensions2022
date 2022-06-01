using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace KLExtensions2022
{
	internal static class ExecuteCommand
	{
		internal static void ExecuteCommandIfAvailable(string commandName, DTE2 dte2)
		{
			ThreadHelper.ThrowIfNotOnUIThread();
			Command command;

			try
			{
				command = dte2.Commands.Item(commandName);
			}
			catch (ArgumentException)
			{
				// The command does not exist, so we can't execute it.
				return;
			}

			if (command.IsAvailable)
			{
				dte2.ExecuteCommand(commandName);
			}
		}
	}
}
