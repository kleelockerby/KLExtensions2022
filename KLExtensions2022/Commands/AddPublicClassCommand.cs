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
using KLExtensions2022.Templates;
using Microsoft.VisualStudio.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;
using System.Windows;


namespace KLExtensions2022
{
    internal sealed class AddPublicClassCommand
    {
        private readonly AsyncPackage package;
        private IServiceProvider ServiceProvider { get { return this.package; } }

        public static DTE2 DTE2 { get; private set; }

        public static AddPublicClassCommand Instance { get; private set; }

        private AddPublicClassCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(PackageGuids.guidPackageCmdSet, PackageIds.AddPublicClassCommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            DTE2 = KLExtensions2022Package.DTE2 as DTE2;
            Assumes.Present(DTE2);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new AddPublicClassCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            NewItemTarget target = NewItemTarget.Create(DTE2);
            if (target == null)
            {
                MessageBox.Show(
                        "Could not determine where to create the new file. Select a file or folder in Solution Explorer and try again.",
                        "Add New Class",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                return;
            }

            string fileName = "class1.cs";
            string input = PromptForFileName(fileName);
            if (string.IsNullOrEmpty(input))
            {
                return;
            }

            AddItemAsync(input, target).Forget();
        }

        private async Task AddItemAsync(string fileName, NewItemTarget target)
        {
            await KLExtensions2022Package.JoinTaskFactory.SwitchToMainThreadAsync();
            try
            {
                FileHelper.ValidatePath(fileName);
                string name = fileName;
                if (fileName.IndexOf(".cs") == -1)
                {
                    name = $"{fileName}.cs";
                }

                FileInfo file;
                if (target.IsSolutionFolder && !Directory.Exists(target.Directory))
                {
                    file = new FileInfo(Path.Combine(Path.GetDirectoryName(DTE2.Solution.FullName), Path.GetFileName(name)));
                }
                else
                {
                    file = new FileInfo(Path.Combine(target.Directory, name));
                }

                Directory.CreateDirectory(file.DirectoryName);

                if (!file.Exists)
                {
                    Project project = target.Project;
                    int position = await WriteFileAsync(file.FullName, name, target.NameSpace);

                    if (target.ProjectItem != null && target.ProjectItem.IsKind(EnvDTE.Constants.vsProjectItemKindVirtualFolder))
                    {
                        target.ProjectItem.ProjectItems.AddFromFile(file.FullName);
                    }
                    else
                    {
                        project.AddFileToProject(file);
                    }

                    VsShellUtilities.OpenDocument(ServiceProvider, file.FullName);
                    if (position > 0)
                    {
                        IWpfTextView view = ProjectHelpers.GetCurentTextView();

                        if (view != null)
                        {
                            view.Caret.MoveTo(new SnapshotPoint(view.TextBuffer.CurrentSnapshot, position));
                        }
                    }
                    ExecuteCommand.ExecuteCommandIfAvailable("SolutionExplorer.SyncWithActiveDocument", DTE2);
                    DTE2.ActiveDocument.Activate();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private string PromptForFileName(string fileName)
        {
            var xamlDialog = new SaveFileDialogWindow("Class Name: ", fileName)
            {
                Owner = Application.Current.MainWindow
            };
            xamlDialog.HasMinimizeButton = false;
            xamlDialog.HasMaximizeButton = false;
            xamlDialog.MaxHeight = 180;
            xamlDialog.MinHeight = 140;
            xamlDialog.MaxWidth = 500;
            xamlDialog.MinWidth = 500;
            xamlDialog.Title = "Move To New ViewModel File";
            bool? result = xamlDialog.ShowDialog();
            return (result.HasValue && result.Value) ? xamlDialog.Input : string.Empty;
        }

        private static async Task<int> WriteFileAsync(string fileNameAndPath, string fileName, string nameSpace)
        {
            string template = CreateClassFile(fileName, nameSpace);

            if (!string.IsNullOrEmpty(template))
            {
                int index = template.IndexOf('$');
                if (index > -1)
                {
                    template = template.Remove(index, 1);
                }

                await FileHelper.WriteToDiskAsync(fileNameAndPath, template);
                return index;
            }

            await FileHelper.WriteToDiskAsync(fileNameAndPath, string.Empty);
            return 0;
        }

        internal static string CreateClassFile(string name, string nameSpace)
        {
            string className = name.RemoveFileNameExtension();
            string content = CSharpTemplate.Content;
            content = content.Replace("%NAMESPACE%", nameSpace).Replace("%FILENAME%", className);
            return FileHelper.NormalizeLineEndings(content);
        }

    }
}