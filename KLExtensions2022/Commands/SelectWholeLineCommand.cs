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
    internal sealed class SelectWholeLineCommand
    {
        private static AsyncPackage package;

        public static SelectWholeLineCommand Instance { get; private set; }

        private SelectWholeLineCommand(OleMenuCommandService commandService)
        {
            var menuCommandID = new CommandID(PackageGuids.guidPackageEditContextCmdSet, PackageIds.EditSelectWholeLineCommandId);
            var command = new OleMenuCommand(Execute, menuCommandID);
            commandService.AddCommand(command);
        }

        public static async Task InitializeAsync(AsyncPackage Package)
        {
            package = Package;
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new SelectWholeLineCommand(commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            IWpfTextView view = GetTextView();

            if (view == null)
            {
                return;
            }

            Microsoft.VisualStudio.Text.SnapshotPoint position = view.Selection.Start.Position;
            Microsoft.VisualStudio.Text.Formatting.IWpfTextViewLine line = view.GetTextViewLineContainingBufferPosition(position);
            Microsoft.VisualStudio.Text.SnapshotSpan span = line.Extent;

            view.Selection.Select(span, false);
        }

        public static IWpfTextView GetTextView()
        {
            IComponentModel compService = package.GetService<SComponentModel, IComponentModel>();
            Assumes.Present(compService);

            IVsEditorAdaptersFactoryService editorAdapter = compService.GetService<IVsEditorAdaptersFactoryService>();
            return editorAdapter.GetWpfTextView(GetCurrentNativeTextView());
        }

        public static IVsTextView GetCurrentNativeTextView()
        {
            IVsTextManager textManager = package.GetService<SVsTextManager, IVsTextManager>();
            Assumes.Present(textManager);

            ErrorHandler.ThrowOnFailure(textManager.GetActiveView(1, null, out IVsTextView activeView));
            return activeView;
        }
    }
}
