using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace KLExtensions2022
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuids.guidPackageString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.VsEditorFactoryGuid.TextEditor_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class KLExtensions2022Package : AsyncPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await RemoveCommentCommand.InitializeAsync(this);
            await RemoveRegionsCommand.InitializeAsync(this);
            await SelectionBracesCommand.InitializeAsync(this);
            await SelectionParenthesisCommand.InitializeAsync(this);
            await SelectionDoubleQuotesCommand.InitializeAsync(this);
            await EditJoinLinesCommand.InitializeAsync(this);
            await TitleCaseCommand.InitializeAsync(this);
            await LowerTitleCaseCommand.InitializeAsync(this);
        }
    }
}
