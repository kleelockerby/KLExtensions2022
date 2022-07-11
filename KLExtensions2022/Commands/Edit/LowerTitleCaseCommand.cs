using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using System.Globalization;
using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Task = System.Threading.Tasks.Task;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Collections;
using System.Runtime.InteropServices;
using KLExtensions2022.Helpers;

namespace KLExtensions2022
{
    internal class LowerTitleCaseCommand
    {
        public static DTE2 DTE { get; private set; }
        public static LowerTitleCaseCommand Instance { get; private set; }
        private OleMenuCommandService CommandService { get; set; }

        private delegate string Replacement(string original);

        private LowerTitleCaseCommand(OleMenuCommandService commandService)
        {
            CommandService = commandService;
            SetupCommands();
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            DTE = await package.GetServiceAsync(typeof(DTE)) as DTE2;

            Assumes.Present(DTE);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new LowerTitleCaseCommand(commandService);
        }

        protected void SetupCommands()
        {
            SetupCommand(PackageIds.EditLowerTitleCaseCommandId, new Replacement(x => GetLowerTitleCase(x)));
        }

        private void SetupCommand(int commandId, Replacement callback)
        {
            RegisterCommand(PackageGuids.guidPackageEditContextCmdSet, commandId, () => Execute(callback));
        }

        protected void RegisterCommand(CommandID commandId, Action action)
        {
            var menuCommand = new OleMenuCommand((s, e) => action(), commandId);
            CommandService.AddCommand(menuCommand);
        }

        protected void RegisterCommand(Guid commandGuid, int commandId, Action action)
        {
            var cmd = new CommandID(commandGuid, commandId);
            RegisterCommand(cmd, action);
        }

        private void Execute(Replacement callback)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                TextDocument document = GetTextDocument();
                
                string result = callback(document.Selection.Text);

                if (result == document.Selection.Text)
                    return;

                using (UndoContext(callback.Method.Name))
                {
                    document.Selection.Insert(result, 0);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public TextDocument GetTextDocument()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return DTE.ActiveDocument?.Object("TextDocument") as TextDocument;
        }


        public string GetLowerTitleCase(string textOriginal)
        {
            string firstCharLower = textOriginal.Substring(0, 1).ToLower();
            string textPart = textOriginal.Substring(1, textOriginal.Length-1);
            string text = string.Concat(firstCharLower + textPart);
            return text;
        }

        public IDisposable UndoContext(string name)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DTE.UndoContext.Open(name);
            return new Disposable(DTE.UndoContext.Close);
        }

    }
}
