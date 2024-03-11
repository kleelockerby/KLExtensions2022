using KLExtensions2022.Commands;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft;
using EnvDTE90;
using KLExtensions2022.Helpers;

namespace KLExtensions2022
{
    [Command(PackageGuids.guidPackageEditContextCmdSetString, PackageIds.InsertNullabilityCommandId)]
    internal class InsertRemoveNullabilityCommand : BaseCommand<InsertRemoveNullabilityCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            try
            {
                await KLExtensions2022Package.JoinTaskFactory.SwitchToMainThreadAsync();
                IServiceProvider serviceProvideer = Package as System.IServiceProvider;
                IWpfTextView textView = GetTextView();
                SnapshotPoint caretPosition = textView.Caret.Position.BufferPosition;
                Document document = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();

                //CompilationUnitSyntax root = (CompilationUnitSyntax)document.GetSyntaxRootAsync(new CancellationTokenSource().Token).Result;
                CompilationUnitSyntax root = (CompilationUnitSyntax)await document.GetSyntaxRootAsync(new CancellationTokenSource().Token);
                SyntaxTree tree = root.SyntaxTree;
                SourceText text = await tree.GetTextAsync();
                string treeText = $"#nullable disable warnings\r\n{text}";
                await FileHelper.WriteToDiskAsync(document.FilePath, treeText);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private IWpfTextView GetTextView()
        {
            IComponentModel compService = Package.GetService<SComponentModel, IComponentModel>();
            Assumes.Present(compService);

            IVsTextManager textManager = Package.GetService<SVsTextManager, IVsTextManager>();
            IVsTextView textView;
            textManager.GetActiveView(1, null, out textView);
            IVsEditorAdaptersFactoryService editorAdapter = compService.GetService<IVsEditorAdaptersFactoryService>();
            return editorAdapter.GetWpfTextView(textView);
        }
    }
}
