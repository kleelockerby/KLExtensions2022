using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel.Design;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System.Linq;
using Microsoft;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using System.CodeDom.Compiler;

namespace KLExtensions2022 
{
    internal class SelectMoveToFunctionCommand
    {
        private readonly AsyncPackage package;
        private IServiceProvider ServiceProvider { get { return this.package; } }
        public static DTE2 DTE2 { get; private set; }
        public static DTE DTE { get; private set; }
        public static SelectMoveToFunctionCommand Instance { get; private set; }

        private SelectMoveToFunctionCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(PackageGuids.guidPackageEditContextCmdSet, PackageIds.SelectMoveToFunctionCommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            DTE2 = KLExtensions2022Package.DTE2 as DTE2;
            DTE = KLExtensions2022Package.DTE as DTE;

            Assumes.Present(DTE2);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new SelectMoveToFunctionCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if ((DTE2 != null) && (DTE2.ActiveDocument != null))
            {
                TextSelection selection = (TextSelection)DTE2.ActiveDocument.Selection;
                try
                {
                    CodeElement codeElement = selection.ActivePoint.CodeElement[vsCMElement.vsCMElementFunction];
                    if (codeElement != null)
                    {
                        selection.MoveToPoint(codeElement.GetStartPoint(vsCMPart.vsCMPartHeader));
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

    }
}
