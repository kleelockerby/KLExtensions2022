using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Classification;
using OleInterop = Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using KLExtensions2022.Helpers;
using KLExtensions2022.Extensions;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Task = System.Threading.Tasks.Task;

namespace KLExtensions2022
{
    internal sealed class EditJoinLinesCommand
    {
        private static TextDocument activeTextDocument;

        public static DTE2 DTE { get; private set; }
        public static EditJoinLinesCommand Instance { get; private set; }

        private EditJoinLinesCommand(OleMenuCommandService commandService)
        {
            var menuCommandID = new CommandID(PackageGuids.guidPackageEditContextCmdSet, PackageIds.EditJoinLinesCommandId);
            var command = new OleMenuCommand(Callback, menuCommandID);
            commandService.AddCommand(command);
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            DTE = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            activeTextDocument = DTE.ActiveDocument?.GetTextDocument();

            Assumes.Present(DTE);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new EditJoinLinesCommand(commandService);
        }


        private void Callback(object sender, EventArgs e)
        {
            var button = (OleMenuCommand)sender;
            Execute(button);
        }

        private IEditorOperationsFactoryService GetEditorAdaptersFactoryService()
        {
            IEditorOperationsFactoryService editor = null;
            IComponentModel componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            if (componentModel != null)
            {
                IVsEditorAdaptersFactoryService editorAdapter = componentModel.GetService<IVsEditorAdaptersFactoryService>();
                editor = (IEditorOperationsFactoryService)componentModel.GetService<IEditorOperationsFactoryService>();
            }
            return editor;
        }

        private void Execute(OleMenuCommand button)
        {
            JoinLines();
            //JoinTextLines();
            //JoinSelectedLines();
        }

        private void JoinLines()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IWpfTextView view = ProjectHelpers.GetCurentTextView();

            // 1. If Selection.Length == 0 (i.e. Cursor just placed in text) => iterate through lines until "{"
            // 2. If Selection.Lines.Length >= 2) => Parse Lines

            IEditorOperationsFactoryService editor = GetEditorAdaptersFactoryService();
            IEditorOperations editorOperations = editor.GetEditorOperations(view);

            editorOperations.MoveToEndOfLine(false);
            editorOperations.Delete();
            editorOperations.DeleteHorizontalWhiteSpace();
        }

        private void JoinText(TextSelection textSelection)
        {
            // If the selection has no length, try to pick up the next line.
            if (textSelection.IsEmpty)
            {
                textSelection.LineDown(true);
                textSelection.EndOfLine(true);
            }

            const string pattern = @"[ \t]*\r?\n[ \t]*";
            const string replacement = @" ";

            // Substitute all new lines (and optional surrounding whitespace) with a single space.
            TextDocumentHelper.SubstituteAllStringMatches(textSelection, pattern, replacement);

            // Move the cursor forward, clearing the selection.
            textSelection.CharRight();
        }
    }
}