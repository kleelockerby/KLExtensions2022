using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;

namespace KLExtensions2022
{
    internal sealed class SelectionIntStringCommand
    {
        public static DTE2 DTE { get; private set; }
        public static SelectionIntStringCommand Instance { get; private set; }

        private SelectionIntStringCommand(OleMenuCommandService commandService)
        {
            var menuCommandID = new CommandID(PackageGuids.guidPackageSurroundWithCmdSet, PackageIds.SurroundWithIntStringCommandId);
            var command = new OleMenuCommand(Callback, menuCommandID);
            commandService.AddCommand(command);
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            DTE = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            Assumes.Present(DTE);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new SelectionIntStringCommand(commandService);
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
                    string txt = AddInterpolatedString(text);
                    selection.Text = txt;
                }
            }
        }

        private string AddInterpolatedString(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                text = "$\"{" + text.TrimEnd() + "}\"";
            }
            return text;
        }
    }
}
