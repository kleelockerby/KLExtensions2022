using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Threading;

namespace KLExtensions2022
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuids.guidPackageString)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideOptionPage(typeof(OptionPageGrid), "Add New Class", "General", 0, 0, true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.VsEditorFactoryGuid.TextEditor_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class KLExtensions2022Package : AsyncPackage
    {
        public static DTE DTE;
        public static DTE2 DTE2;
        public static IComponentModel ComponentModel;
        public static JoinableTaskFactory JoinTaskFactory;

        public Document ActiveDocument
        {
            get
            {
                try
                {
                    return DTE2.ActiveDocument;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public bool OptionUsings
        {
            get
            {
                OptionPageGrid optionPageGrid = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
                return optionPageGrid.UsingsOption;
            }
        }

        public NamespaceOptions OptionNamespace
        {
            get
            {
                OptionPageGrid optionPageGrid = (OptionPageGrid)GetDialogPage(typeof(OptionPageGrid));
                return optionPageGrid.KLNamespaceOptions;
            }
        }

        public TextDocument ActiveTextDocument => GetTextDocument(this.ActiveDocument);

        internal TextDocument GetTextDocument(Document document)
        {
            if (document == null)
            {
                return null;
            }
            return document.Object("TextDocument") as TextDocument;
        }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            DTE = await GetServiceAsync(typeof(DTE)) as DTE;
            DTE2 =  (DTE2)await GetServiceAsync(typeof(DTE));
            JoinTaskFactory = this.JoinableTaskFactory;
            ComponentModel = await GetServiceAsync(typeof(SComponentModel)) as IComponentModel;

            await RemoveCommentCommand.InitializeAsync(this);
            await RemoveRegionsCommand.InitializeAsync(this);
            await SelectionBracesCommand.InitializeAsync(this);
            await SelectionParenthesisCommand.InitializeAsync(this);
            await SelectionDoubleQuotesCommand.InitializeAsync(this);
            await EditJoinLinesCommand.InitializeAsync(this);
            await TitleCaseCommand.InitializeAsync(this);
            await LowerTitleCaseCommand.InitializeAsync(this);
            await SentenceCaseCommand.InitializeAsync(this);
            await AddPublicClassCommand.InitializeAsync(this);
            await SelectWholeLineCommand.InitializeAsync(this);
            await SortLinesCommand.InitializeAsync(this);
            await SelectMoveToFunctionCommand.InitializeAsync(this);
            await RemoveEmptyLinesCommand.InitializeAsync(this);
            await SetAllMethodsBreakpointsCommand.InitializeAsync(this);
            await InsertGuidCommand.InitializeAsync(this);
            await DuplicateAndCopyCommand.InitializeAsync(this);
            await CreateObjectInitializerCommand.InitializeAsync(this);
        }
    }
}
