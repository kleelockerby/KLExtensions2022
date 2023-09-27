using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel.Design;
using KLExtensions2022.Commands;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Task = System.Threading.Tasks.Task;

namespace KLExtensions2022
{
    [Command(PackageGuids.guidPackageEditContextCmdSetString, PackageIds.InsertGuidCommandId)]
    internal class InsertGuidCommand : BaseCommand<InsertGuidCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            try
            {
                await KLExtensions2022Package.JoinTaskFactory.SwitchToMainThreadAsync();
                EnvDTE.TextSelection ts = DTE.ActiveDocument.Selection as EnvDTE.TextSelection;
                ts.Text = System.Guid.NewGuid().ToString();
            }
            catch(Exception ex)
            {
                Logger.Log(ex);
            }
        }

    }
}
