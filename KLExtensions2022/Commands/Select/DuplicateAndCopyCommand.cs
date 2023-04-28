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
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Text.Editor;
using KLExtensions2022.Helpers;
using Microsoft.VisualStudio.Text;

namespace KLExtensions2022
{
    [Command(PackageGuids.guidPackageEditContextCmdSetString, PackageIds.DuplicateAndCommentCommandId)]
    internal class DuplicateAndCopyCommand : BaseCommand<DuplicateAndCopyCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            try
            {
                IWpfTextView textView = ProjectHelpers.GetCurentTextView();
                ITextSnapshot snapshot = textView.TextSnapshot;

                if(snapshot != snapshot.TextBuffer.CurrentSnapshot)
                    return;

                if(!textView.Selection.IsEmpty)
                {
                    VirtualSnapshotPoint selectionStart = textView.Selection.Start;
                    VirtualSnapshotPoint selectionEnd = textView.Selection.End;

                    if(textView.Selection.Start > textView.Selection.End)
                        TextDocumentHelper.Swap(ref selectionStart, ref selectionEnd);

                    ITextSnapshotLine selectionStartLine = selectionStart.Position.GetContainingLine();
                    ITextSnapshotLine selectionEndLine = selectionEnd.Position.GetContainingLine();

                    int blockTextStart = selectionStartLine.Start.Position;
                    string blockText = textView.TextBuffer.CurrentSnapshot.GetText(selectionStartLine.Start.Position, selectionEndLine.End.Position - selectionStartLine.Start.Position);
                    string selectedText = textView.TextBuffer.CurrentSnapshot.GetText(selectionStart.Position, selectionEnd.Position - selectionStart.Position);

                    int caretPositionWithinBlock = textView.GetCaretPosition() - selectionStartLine.Start.Position;
                    int nonSelectedTextLeftOffset = selectionStart.Position - selectionStartLine.Start.Position;

                    string textOffset = new string(' ', nonSelectedTextLeftOffset);
                    string duplicatedText = textOffset + selectedText;

                    duplicatedText = blockText;
                    string commentedText = blockText.Comment();
                    string replacementText = commentedText + Environment.NewLine + duplicatedText;

                    using(ITextEdit edit = textView.TextBuffer.CreateEdit())
                    {
                        edit.Replace(new Span(blockTextStart, blockText.Length), replacementText);
                        edit.Apply();
                    }

                    ITextSnapshotLine firstDuplicatedTextLine = textView.GetLine(selectionEndLine.LineNumber + 1);
                    int newSelectionStart = firstDuplicatedTextLine.Start.Position + nonSelectedTextLeftOffset;
                    int newSelectionLength = Math.Min(selectedText.Length, textView.GetText().Length - newSelectionStart);

                    textView.Selection.Clear();
                    textView.SetSelection(newSelectionStart, newSelectionLength);
                    textView.MoveCaretTo(firstDuplicatedTextLine.Start.Position + caretPositionWithinBlock);
                }
                else
                {
                    int selectionLastLineNumber = textView.Caret.ContainingTextViewLine.End.GetContainingLine().LineNumber;
                    int caretLineOffset = textView.GetCaretPosition() - textView.Caret.ContainingTextViewLine.Start.Position;

                    int areaStart = textView.Caret.ContainingTextViewLine.Start.Position;
                    int areaEnd = textView.Caret.ContainingTextViewLine.End.Position;

                    string text = textView.TextBuffer.CurrentSnapshot.GetText(areaStart, areaEnd - areaStart);
                    string replacementText = text.CommentText(text.IndexOfNonWhitespace(), true) + Environment.NewLine + text;

                    using(ITextEdit edit = textView.TextBuffer.CreateEdit())
                    {
                        edit.Replace(new Span(areaStart, text.Length), replacementText);
                        edit.Apply();
                    }

                    var newSelectedLineNumber = selectionLastLineNumber;
                    newSelectedLineNumber++;

                    var line = textView.GetLine(newSelectedLineNumber);

                    textView.MoveCaretTo(line.Start.Position + caretLineOffset);
                    return;
                }
            }
            catch(Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}
