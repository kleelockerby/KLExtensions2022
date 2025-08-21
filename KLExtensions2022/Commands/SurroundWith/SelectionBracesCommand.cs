using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace KLExtensions2022
{
    internal sealed class SelectionBracesCommand
    {
        public static DTE2 DTE { get; private set; }
        public static SelectionBracesCommand Instance { get; private set; }

        private SelectionBracesCommand(OleMenuCommandService commandService)
        {
            var menuCommandID = new CommandID(PackageGuids.guidPackageSurroundWithCmdSet, PackageIds.SurroundWithBraceCommandId);
            var command = new OleMenuCommand(Callback, menuCommandID);
            commandService.AddCommand(command);
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            DTE = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            Assumes.Present(DTE);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new SelectionBracesCommand(commandService);
        }

        private void Callback(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;
            Execute(button);
        }

        private void Execute(OleMenuCommand button)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if ((DTE != null) && (DTE.ActiveDocument != null))
            {
                TextSelection selection = (TextSelection)DTE.ActiveDocument.Selection;
                string text = selection.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    string txt = AddBracesAndLines(text);
                    selection.Text = txt.TrimEnd('}');
                }
            }
        }

        private string AddBracesAndLines(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                // text = $"{{{Environment.NewLine}\t{text}{Environment.NewLine}}}";
                text = "{" + Environment.NewLine + text.TrimEnd() + Environment.NewLine + "}";
            }
            return text;
        }
    }
}
