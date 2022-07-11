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
#pragma warning disable VSTHRD010

namespace KLExtensions2022
{
    internal class SortLinesCommand
    {
        private readonly AsyncPackage package;
        private IServiceProvider ServiceProvider { get { return this.package; } }
        private OleMenuCommandService commandService { get; set; }

        private delegate void Replacement(Direction direction);

        public static DTE2 DTE2 { get; private set; }
        public static SortLinesCommand Instance { get; private set; }

        private SortLinesCommand(AsyncPackage package, OleMenuCommandService CommandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = CommandService ?? throw new ArgumentNullException(nameof(commandService));
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            DTE2 = KLExtensions2022Package.DTE2 as DTE2;
            Assumes.Present(DTE2);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new SortLinesCommand(package, commandService);
            Instance.SetupCommands();
        }

        protected void RegisterCommand(CommandID commandId, Action action)
        {
            OleMenuCommand menuCommand = new OleMenuCommand((s, e) => action(), commandId);
            commandService.AddCommand(menuCommand);
        }

        protected void RegisterCommand(Guid commandGuid, int commandId, Action action)
        {
            CommandID cmd = new CommandID(commandGuid, commandId);
            RegisterCommand(cmd, action);
        }

        protected void SetupCommands()
        {
            var cmdAsc = new CommandID(PackageGuids.guidPackageEditContextCmdSet, PackageIds.EditSortLinesAscCommandId);
            RegisterCommand(cmdAsc, () => Execute(Direction.Ascending));

            var cmdDesc = new CommandID(PackageGuids.guidPackageEditContextCmdSet, PackageIds.EditSortLinesDescCommandId);
            RegisterCommand(cmdDesc, () => Execute(Direction.Descending));
        }

        private void Execute(Direction direction)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                TextDocument document = GetTextDocument();
                // SortText(document.Selection);

                IEnumerable<string> lines = GetSelectedLines(document);

                string result = SortLines(direction, lines);

                if (result == document.Selection.Text)
                {
                    return;
                }

                using (UndoContext("Sort Selected Lines"))
                {
                    document.Selection.Insert(result, 0);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public TextDocument GetTextDocument()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return DTE2.ActiveDocument?.Object("TextDocument") as TextDocument;
        }

        public IDisposable UndoContext(string name)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DTE2.UndoContext.Open(name);
            return new Disposable(DTE2.UndoContext.Close);
        }

        public IEnumerable<string> GetSelectedLines(TextDocument document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            int firstLine = Math.Min(document.Selection.TopLine, document.Selection.BottomLine);
            int lineCount = document.Selection.TextRanges.Count;

            document.Selection.MoveToLineAndOffset(firstLine, 1);
            document.Selection.LineDown(true, lineCount);
            document.Selection.CharRight(true, -1);

            for (int i = 1 ; i <= document.Selection.TextRanges.Count ; i++)
            {
                TextRange range = document.Selection.TextRanges.Item(i);
                yield return range.StartPoint.GetText(range.EndPoint).TrimEnd();
            }
        }

        private string SortLines(Direction direction, IEnumerable<string> lines)
        {
            if (direction == Direction.Ascending)
            {
                lines = lines.OrderBy(t => t);
            }
            else
            {
                lines = lines.OrderByDescending(t => t);
            }

            return string.Join(Environment.NewLine, lines);
        }

        private void SortText(TextSelection textSelection)
        {
            if (textSelection.IsEmpty)
            {
                textSelection.LineDown(true);
                textSelection.EndOfLine(true);
            }

            var start = textSelection.TopPoint.CreateEditPoint();
            start.StartOfLine();

            var end = textSelection.BottomPoint.CreateEditPoint();
            if (!end.AtStartOfLine)
            {
                end.EndOfLine();
                end.CharRight();
            }

            var selectedText = start.GetText(end);

            var splitText = selectedText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var orderedText = splitText.OrderBy(x => x);

            var sb = new StringBuilder();
            foreach (var line in orderedText)
            {
                sb.AppendLine(line);
            }

            var sortedText = sb.ToString();

            if (!selectedText.Equals(sortedText, StringComparison.CurrentCulture))
            {
                start.Delete(end);

                var insertCursor = start.CreateEditPoint();
                insertCursor.Insert(sortedText);

                textSelection.MoveToPoint(start, false);
                textSelection.MoveToPoint(insertCursor, true);
            }
        }
    }
}