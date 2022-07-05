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
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.VsEditorFactoryGuid.TextEditor_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class KLExtensions2022Package : AsyncPackage
    {
        public static DTE DTE;
        public static DTE2 DTE2;
        public static IComponentModel ComponentModel;
        public static JoinableTaskFactory JoinTaskFactory;

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
        }
    }
}
