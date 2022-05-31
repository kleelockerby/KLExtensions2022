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
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using KLExtensions2022.Helpers;

namespace KLExtensions2022
{
    internal class RemoveCommentCommand
    {
        public static DTE2 DTE { get; private set; }
        public static RemoveCommentCommand Instance { get; private set; }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            DTE = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            Assumes.Present(DTE);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new RemoveCommentCommand(commandService);
        }

        private RemoveCommentCommand(OleMenuCommandService commandService)
        {
            var menuCommandID = new CommandID(PackageGuids.guidPackageCmdSet, PackageIds.RemoveCommentCommandId);
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

            IWpfTextView view = ProjectHelpers.GetCurentTextView();
            IEnumerable<IMappingSpan> mappingSpans = GetClassificationSpans(view, "comment");

            if (!mappingSpans.Any())
                return;

            try
            {
                DTE.UndoContext.Open(button.Text);

                DeleteFromBuffer(view, mappingSpans);
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

        protected static IEnumerable<IMappingSpan> GetClassificationSpans(IWpfTextView view, string classificationName)
        {
            if (view == null)
            {
                return Enumerable.Empty<IMappingSpan>();
            }

            Microsoft.VisualStudio.ComponentModelHost.IComponentModel componentModel = ProjectHelpers.GetComponentModel();
            IBufferTagAggregatorFactoryService service = componentModel.GetService<IBufferTagAggregatorFactoryService>();
            ITagAggregator<IClassificationTag> classifier = service.CreateTagAggregator<IClassificationTag>(view.TextBuffer);
            SnapshotSpan snapshot = new SnapshotSpan(view.TextBuffer.CurrentSnapshot, 0, view.TextBuffer.CurrentSnapshot.Length);

            IEnumerable<IMappingSpan> mapSpan = classifier.GetTags(snapshot).Reverse().Where(cl => cl.Tag.ClassificationType.Classification.IndexOf(classificationName, StringComparison.OrdinalIgnoreCase) > -1).Select(cl2 => cl2.Span);
            return mapSpan;
        }

        private static void DeleteFromBuffer(IWpfTextView view, IEnumerable<IMappingSpan> mappingSpans)
        {
            var affectedLines = new List<int>();

            RemoveCommentSpansFromBuffer(view, mappingSpans, affectedLines);
            RemoveAffectedEmptyLines(view, affectedLines);
        }

        private static void RemoveCommentSpansFromBuffer(IWpfTextView view, IEnumerable<IMappingSpan> mappingSpans, IList<int> affectedLines)
        {
            using (var edit = view.TextBuffer.CreateEdit())
            {
                foreach (var mappingSpan in mappingSpans)
                {
                    var start = mappingSpan.Start.GetPoint(view.TextBuffer, PositionAffinity.Predecessor).Value;
                    var end = mappingSpan.End.GetPoint(view.TextBuffer, PositionAffinity.Successor).Value;

                    var span = new Span(start, end - start);
                    var lines = view.TextBuffer.CurrentSnapshot.Lines.Where(l => l.Extent.IntersectsWith(span));

                    foreach (var line in lines)
                    {
                        if (IsXmlDocComment(line))
                        {
                            edit.Replace(line.Start, line.Length, string.Empty.PadLeft(line.Length));
                        }

                        if (!affectedLines.Contains(line.LineNumber))
                            affectedLines.Add(line.LineNumber);
                    }

                    var mappingText = view.TextBuffer.CurrentSnapshot.GetText(span.Start, span.Length);
                    string empty = Regex.Replace(mappingText, "([\\S]+)", string.Empty);

                    edit.Replace(span.Start, span.Length, empty);
                }

                edit.Apply();
            }
        }

        private static void RemoveAffectedEmptyLines(IWpfTextView view, IList<int> affectedLines)
        {
            if (!affectedLines.Any())
                return;

            using (var edit = view.TextBuffer.CreateEdit())
            {
                foreach (var lineNumber in affectedLines)
                {
                    var line = view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNumber);

                    if (IsLineEmpty(line))
                    {
                        // Strip next line if empty
                        if (view.TextBuffer.CurrentSnapshot.LineCount > line.LineNumber + 1)
                        {
                            var next = view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNumber + 1);

                            if (IsLineEmpty(next))
                                edit.Delete(next.Start, next.LengthIncludingLineBreak);
                        }

                        edit.Delete(line.Start, line.LengthIncludingLineBreak);
                    }
                }

                edit.Apply();
            }
        }

        protected static bool IsLineEmpty(ITextSnapshotLine line)
        {
            var text = line.GetText().Trim();

            return (string.IsNullOrWhiteSpace(text)
                   || text == "<!--"
                   || text == "-->"
                   || text == "<%%>"
                   || text == "<%"
                   || text == "%>"
                   || Regex.IsMatch(text, @"<!--(\s+)?-->"));
        }

        protected static bool IsXmlDocComment(ITextSnapshotLine line)
        {
            var text = line.GetText().Trim();
            Microsoft.VisualStudio.Utilities.IContentType contentType = line.Snapshot.TextBuffer.ContentType;

            if (contentType.IsOfType("CSharp") && text.StartsWith("///"))
            {
                return true;
            }

            if (contentType.IsOfType("FSharp") && text.StartsWith("///"))
            {
                return true;
            }

            if (contentType.IsOfType("Basic") && text.StartsWith("'''"))
            {
                return true;
            }

            return false;
        }
    }
}