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
using Microsoft.VisualStudio.Text.Classification;
using OleInterop = Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using KLExtensions2022.Helpers;
using KLExtensions2022.Extensions;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Task = System.Threading.Tasks.Task;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using static Microsoft.ServiceHub.Framework.ServiceBrokerClient;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace KLExtensions2022
{
    internal sealed class EditJoinLinesCommand
    {
        private static TextDocument activeTextDocument;
        private IVsTextManager textManager;

        public static DTE2 DTE2 { get; private set; }
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

            DTE2 = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            activeTextDocument = DTE2.ActiveDocument?.GetTextDocument();

            Assumes.Present(DTE2);

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
            //ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                DTE2.UndoContext.Open(button.Text);
                textManager = (IVsTextManager)ServiceProvider.GlobalProvider.GetService(typeof(SVsTextManager));
                JoinLines();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            finally
            {
                DTE2.UndoContext.Close();
            }
        }

        private void JoinLines()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IWpfTextView textView = ProjectHelpers.GetCurentTextView();
            EnvDTE.TextSelection selection = (EnvDTE.TextSelection)activeTextDocument.Selection;
            string input = selection.Text;

            if (selection.IsEmpty)
            {
                input = ExpandSelection(textView, selection);
            }
            string txt = RemoveSpacesAndReturns(input);
            EditPoint startPoint = selection.TopPoint.CreateEditPoint();
            EditPoint endPoint = selection.BottomPoint.CreateEditPoint();
            endPoint.ReplaceText(startPoint, txt, (int)vsEPReplaceTextOptions.vsEPReplaceTextAutoformat);
            selection.CharRight();
        }

        private string RemoveSpacesAndReturns(string input)
        {
            string input2 = input;
            string pattern = "(\\r?\\n[\\s\\t]+)";
            string replace = "";
            input = Regex.Replace(input, pattern, replace);
            return input;
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

        public IDisposable UndoContext(string name)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DTE2.UndoContext.Open(name);
            return new Disposable(DTE2.UndoContext.Close);
        }
    }
}



/*
private string LoopLines(ITextSnapshot snapshot, int startLineNumber, out int joinedLength)
{
    string joinedText = string.Empty;
    string totalText = string.Empty;

    int currentLineNumber = startLineNumber;
    string lastChar = "";
    while (lastChar != ";" && lastChar != "{")
    {
        string lineText = snapshot.GetLineFromLineNumber(currentLineNumber).GetText();
        lastChar = lineText.TrimEnd().Substring(lineText.Length - 1);
        if (lastChar != ";" && lastChar != "{")
        {
            //totalText += (currentLineNumber == startLineNumber) ? $"{lineText} " : $"{lineText} ";
            totalText += $"{lineText} ";
            joinedText += (currentLineNumber == startLineNumber) ? $"{lineText} " : $"{lineText.TrimStart()} ";
        }
        currentLineNumber++;
    }
    joinedLength = totalText.Length + 2;
    return joinedText.TrimEnd();
}



*/