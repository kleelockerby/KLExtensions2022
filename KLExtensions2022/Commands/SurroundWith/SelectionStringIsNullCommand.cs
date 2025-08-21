using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;

namespace KLExtensions2022
{
    internal sealed class SelectionStringIsNullCommand
    {
        public static DTE2 DTE { get; private set; }
        public static SelectionStringIsNullCommand Instance { get; private set; }

        private SelectionStringIsNullCommand(OleMenuCommandService commandService)
        {
            var menuCommandID = new CommandID(PackageGuids.guidPackageSurroundWithCmdSet, PackageIds.SurroundWithStringIsNullCommandId);
            var command = new OleMenuCommand(Callback, menuCommandID);
            commandService.AddCommand(command);
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            DTE = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            Assumes.Present(DTE);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new SelectionStringIsNullCommand(commandService);
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
                    string txt = AddIsNullOrEmpty(text);
                    selection.Text = txt;
                }
            }
        }

        private string AddIsNullOrEmpty(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                text = $"if(!string.IsNullOrEmpty({text}))" + Environment.NewLine + "{" + Environment.NewLine + Environment.NewLine + "}";
            }
            return text;
        }
    }
}
