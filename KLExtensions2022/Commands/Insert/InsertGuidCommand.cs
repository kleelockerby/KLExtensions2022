using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace KLExtensions2022
{
    internal sealed class InsertGuidCommand
    {
        private static AsyncPackage package;
        public static InsertGuidCommand Instance { get; private set; }
        public static DTE DTE { get; private set; }

        private InsertGuidCommand(OleMenuCommandService commandService)
        {
            var menuCommandID = new CommandID(PackageGuids.guidPackageEditContextCmdSet, PackageIds.InsertGuidCommandId);
            var command = new OleMenuCommand(Execute, menuCommandID);
            commandService.AddCommand(command);
        }

        public static async Task InitializeAsync(AsyncPackage Package)
        {
            package = Package;
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            DTE = KLExtensions2022Package.DTE as DTE;

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new InsertGuidCommand(commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            EnvDTE.TextSelection ts = DTE.ActiveDocument.Selection as EnvDTE.TextSelection;
            ts.Text = System.Guid.NewGuid().ToString();
        }

    }
}
