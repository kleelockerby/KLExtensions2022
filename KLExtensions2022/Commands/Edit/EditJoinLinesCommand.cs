using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using KLExtensions2022.Helpers;
using KLExtensions2022.Extensions;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Task = System.Threading.Tasks.Task;
using System.Text.RegularExpressions;

namespace KLExtensions2022
{
    internal sealed class EditJoinLinesCommand
    {
        private readonly AsyncPackage package;
        private IServiceProvider ServiceProvider { get { return this.package; } }

        public static EditJoinLinesCommand Instance { get; private set; }
        public static DTE2 DTE2 { get; private set; }

        private EditJoinLinesCommand(AsyncPackage package)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                CommandID menuCommandID = new CommandID(PackageGuids.guidPackageEditContextCmdSet, PackageIds.EditJoinLinesCommandId);
                MenuCommand menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            Instance = new EditJoinLinesCommand(package);
        }


        private void MenuItemCallback(object sender, EventArgs e)
        {
            var undoTransaction = new UndoTransactionHelper(ServiceProvider as KLExtensions2022Package, "JoinLines");
            TextDocument activeTextDocument = (ServiceProvider as KLExtensions2022Package).ActiveTextDocument;
            
            if (activeTextDocument != null)
            {
                TextSelection textSelection = activeTextDocument.Selection;
                if (textSelection != null)
                {
                    undoTransaction.Run(() => JoinLine(textSelection));
                }
            }
        }

        private void JoinLine(TextSelection textSelection)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string input = textSelection.Text;
            IWpfTextView textView = ProjectHelpers.GetCurentTextView();

            if (textSelection.IsEmpty)
            {
                input = ExpandSelection(textView, textSelection);
            }
            string txt = RemoveSpacesAndReturns(input);
            EditPoint startPoint = textSelection.TopPoint.CreateEditPoint();
            EditPoint endPoint = textSelection.BottomPoint.CreateEditPoint();
            endPoint.ReplaceText(startPoint, txt, (int)vsEPReplaceTextOptions.vsEPReplaceTextAutoformat);
            textSelection.CharRight();
        }

        private string ExpandSelection(IWpfTextView textView, EnvDTE.TextSelection selection)
        {
            IEditorOperationsFactoryService editorOperationsFactoryService = GetEditorAdaptersFactoryService();
            IEditorOperations editorOperations = editorOperationsFactoryService.GetEditorOperations(textView);
            editorOperations.MoveToStartOfLine(false);

            bool isValidEndingChar = false;
            while (!isValidEndingChar)
            {
                editorOperations.MoveToEndOfLine(true);
                string selectedText = selection.Text;
                string lastChar = selectedText.TrimEnd().Substring(selectedText.Length - 1);

                if (lastChar != ";" && lastChar != "{")
                {
                    editorOperations.MoveToStartOfNextLineAfterWhiteSpace(true);
                    editorOperations.MoveToNextCharacter(true);
                    selectedText = selection.Text;
                    lastChar = selectedText.TrimEnd().Substring(selectedText.Length - 1);

                    if (lastChar == ";")
                    {
                        editorOperations.MoveToStartOfLine(true);
                        editorOperations.MoveToStartOfPreviousLineAfterWhiteSpace(true);
                        editorOperations.MoveToEndOfLine(true);
                        isValidEndingChar = true;
                    }
                }
                else
                {
                    isValidEndingChar = true;
                }
            }
            return selection.Text;
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

        private string RemoveSpacesAndReturns(string input)
        {
            string input2 = input;
            string pattern = "(\\r?\\n[\\s\\t]+)";
            string replace = "";
            input = Regex.Replace(input, pattern, replace);
            return input;
        }

    }
}
