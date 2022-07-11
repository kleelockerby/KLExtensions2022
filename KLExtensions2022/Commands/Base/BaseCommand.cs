using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO.Packaging;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;


namespace KLExtensions2022.Commands 
{
    internal abstract class BaseCommand<T> where T : BaseCommand<T>, new()
    {
        public OleMenuCommandService CommandService { get; set; }
        public OleMenuCommand Command { get; private set; }
        public AsyncPackage Package { get; private set; } = null;
        public static DTE2 DTE2 { get; private set; }
        public Guid Guid { get; private set; }
        public int Id { get; private set; }

        public static async Task<T> InitializeAsync(AsyncPackage package)
        {
            BaseCommand<T> instance = (BaseCommand<T>)(object)new T();

            CommandAttribute attr = (CommandAttribute)instance.GetType().GetCustomAttributes(typeof(CommandAttribute), true).FirstOrDefault();
            Guid cmdGuid = attr.Guid == Guid.Empty ? package.GetType().GUID : attr.Guid;
            CommandID cmd = new CommandID(cmdGuid, attr.Id);

            instance.Command = new OleMenuCommand(instance.Execute, changeHandler: null, instance.BeforeQueryStatus, cmd);
            instance.Package = package;

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            DTE2 = KLExtensions2022Package.DTE2 as DTE2;

            IMenuCommandService commandService = (IMenuCommandService)await package.GetServiceAsync(typeof(IMenuCommandService));
            Assumes.Present(commandService);
            commandService.AddCommand(instance.Command);  // Requires main/UI thread

            await instance.InitializeCompletedAsync();
            return (T)(object)instance;
        }

        protected virtual Task InitializeCompletedAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual void Execute(object sender, EventArgs e)
        {
            Package.JoinableTaskFactory.RunAsync(async delegate
            {
                try
                {
                    await ExecuteAsync((OleMenuCmdEventArgs)e);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }).FireAndForget();
        }

        protected virtual Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            return Task.CompletedTask;
        }

        internal virtual void BeforeQueryStatus(object sender, EventArgs e)
        {
            BeforeQueryStatus(e);
        }

        protected virtual void BeforeQueryStatus(EventArgs e)
        {
        }

        public TextDocument GetTextDocument()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return DTE2.ActiveDocument?.Object("TextDocument") as TextDocument;
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

        public IDisposable UndoContext(string name)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DTE2.UndoContext.Open(name);
            return new Disposable(DTE2.UndoContext.Close);
        }

    }
}
