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
    internal sealed class RemoveRegionsCommand
    {
        public static DTE2 DTE { get; private set; }
        public static RemoveRegionsCommand Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            DTE = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            Assumes.Present(DTE);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new RemoveRegionsCommand(commandService);
        }

        private RemoveRegionsCommand(OleMenuCommandService commandService)
        {
            var menuCommandID = new CommandID(PackageGuids.guidPackageCmdSet, PackageIds.RemoveRegionsCommandId);
            var command = new OleMenuCommand(Callback, menuCommandID);
            commandService.AddCommand(command);
        }

        private void Callback(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;
            Execute(button);
        }

        private void Execute(OleMenuCommand button)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            //IWpfTextView view = ProjectHelpers.GetCurentTextView();
            IComponentModel componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            if (componentModel != null)
            {
                IVsEditorAdaptersFactoryService editorAdapter = componentModel.GetService<IVsEditorAdaptersFactoryService>();
                IVsTextManager textManager = (IVsTextManager)ServiceProvider.GlobalProvider.GetService(typeof(SVsTextManager));
                Assumes.Present(textManager);
                ErrorHandler.ThrowOnFailure(textManager.GetActiveView(1, null, out IVsTextView activeView));
                IWpfTextView view = editorAdapter.GetWpfTextView(activeView);

                try
                {
                    DTE.UndoContext.Open(button.Text);
                    RemoveRegionsFromBuffer(view);
                }
                catch (Exception ex)
                {

                    System.Diagnostics.Debug.Write(ex);
                }
                finally
                {
                    DTE.UndoContext.Close();
                }
            }
        }

        private void RemoveRegionsFromBuffer(IWpfTextView view)
        {
            using (ITextEdit edit = view.TextBuffer.CreateEdit())
            {
                foreach (ITextSnapshotLine line in view.TextBuffer.CurrentSnapshot.Lines.Reverse())
                {
                    string text = line.GetText().TrimStart('/', '*').Replace("<!--", string.Empty).TrimStart().ToLowerInvariant();
                    if (text.StartsWith("#region") || text.StartsWith("#endregion") || text.StartsWith("#end region"))
                    {
                        int lineCount = view.TextBuffer.CurrentSnapshot.LineCount;
                        int nextLine = line.LineNumber + 1;
                        if (lineCount > nextLine)
                        {
                            ITextSnapshotLine next = view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(line.LineNumber + 1);
                            if (IsLineEmpty(next))
                            {
                                edit.Delete(next.Start, next.LengthIncludingLineBreak);
                            }
                        }
                        edit.Delete(line.Start, line.LengthIncludingLineBreak);
                    }
                }
                edit.Apply();
            }
        }

        private bool IsLineEmpty(ITextSnapshotLine line)
        {
            string text = line.GetText().Trim();

            return (string.IsNullOrWhiteSpace(text) || text == "<!--" || text == "-->" || text == "<%%>" || text == "<%" || text == "%>" || Regex.IsMatch(text, @"<!--(\s+)?-->"));
        }
    }
}
