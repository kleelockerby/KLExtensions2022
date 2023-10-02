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
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;
using System.Windows;
using System.Collections.Generic;


namespace KLExtensions2022
{
    public class NewItemTarget
    {
        private static DTE2 DTE2;
        public string Directory { get; }
        public Project Project { get; }
        public ProjectItem ProjectItem { get; }
        public string NameSpace { get; set; }
        public string RootFolder { get; set; }
        public string FileFolder { get; set; }
        public NamespaceOptions NamespaceOptions { get; set; }
        public bool UseImplicitUsings { get; set; }
        public bool IsSolutionOrSolutionFolder { get; }
        public bool IsSolution => IsSolutionOrSolutionFolder && Project == null;
        public bool IsSolutionFolder => IsSolutionOrSolutionFolder && Project != null;

        public static NewItemTarget Create(DTE2 dte)
		{
            DTE2 = dte as DTE2;
			NewItemTarget item = CreateFromSolutionExplorerSelection(dte);
			return item;
		}

        private NewItemTarget(string directory, Project project, ProjectItem projectItem, bool isSolutionOrSolutionFolder)
        {
            Directory = directory;
            Project = project;
            ProjectItem = projectItem;
            IsSolutionOrSolutionFolder = isSolutionOrSolutionFolder;

            this.RootFolder = project.GetRootFolder();
            string fileFolder = GetFileFolder(this.RootFolder, Directory);
            this.FileFolder = fileFolder;
            int lastIndex = this.FileFolder.LastIndexOf("\\");
            if (lastIndex > -1)
            {
                this.FileFolder = fileFolder.Remove(lastIndex, 1);
            }
            this.NameSpace = project.Name;

            string rootNameSpace = project.GetRootNamespace();
        }

        private static NewItemTarget CreateFromSolutionExplorerSelection(DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Array items = (Array)dte.ToolWindows.SolutionExplorer.SelectedItems;

            if (items.Length == 1)
            {
                UIHierarchyItem selection = items.Cast<UIHierarchyItem>().First();

                if (selection.Object is Project project)
                {
                    if (project.IsKind(EnvDTE.Constants.vsProjectKindSolutionItems))
                    {
                        return new NewItemTarget(GetSolutionFolderPath(project), project, null, isSolutionOrSolutionFolder: true);
                    }
                    else
                    {
                        return new NewItemTarget(project.GetRootFolder(), project, null, isSolutionOrSolutionFolder: false);
                    }
                }
                else if (selection.Object is ProjectItem projectItem)
                {
                    return CreateFromProjectItem(projectItem);
                }
            }

            return null;
        }

        private static NewItemTarget CreateFromProjectItem(ProjectItem projectItem)
        {
            if (projectItem.IsKind(EnvDTE.Constants.vsProjectItemKindSolutionItems))
            {
                return new NewItemTarget(GetSolutionFolderPath(projectItem.ContainingProject), projectItem.ContainingProject, null, isSolutionOrSolutionFolder: true);
            }
            else
            {
                projectItem = ResolveToPhysicalProjectItem(projectItem);
                string fileName = projectItem?.GetFullPathFileName();

                if (string.IsNullOrEmpty(fileName))
                {
                    return null;
                }

                string directory = File.Exists(fileName) ? Path.GetDirectoryName(fileName) : fileName;
                return new NewItemTarget(directory, projectItem.ContainingProject, projectItem, isSolutionOrSolutionFolder: false);
            }
        }

        private static ProjectItem ResolveToPhysicalProjectItem(ProjectItem projectItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (projectItem.IsKind(EnvDTE.Constants.vsProjectItemKindVirtualFolder))
            {
                return projectItem.ProjectItems
                        .Cast<ProjectItem>()
                        .Select(item => ResolveToPhysicalProjectItem(item))
                        .FirstOrDefault(item => item != null);
            }
            return projectItem;
        }

        private static string GetFileFolder(string rootFolder, string directoryFullPath)
        {
            List<string> rootFolders = GetRootList(rootFolder);
            List<string> filePathFolders = GetFileList(directoryFullPath);
            int total = (filePathFolders.Count - 2);

            for (int i = (filePathFolders.Count - 1) ; i >= 0 ; i--)
            {
                string filePathFolder = filePathFolders[i];
                if (rootFolders.Contains(filePathFolder))
                {
                    filePathFolders.Remove(filePathFolder);
                }
            }
            string retVal = string.Empty;
            for (int i = 0 ; i < filePathFolders.Count ; i++)
            {
                retVal += filePathFolders[i];
                retVal += (i < filePathFolders.Count - 1) ? "\\" : string.Empty;
            }
            return retVal;
        }

        private static List<string> GetRootList(string rootFolder)
        {
            rootFolder = rootFolder.Replace("C:\\", "");
            string[] rootFolders = rootFolder.Split('\\');
            List<string> listRootFolders = new List<string>(rootFolders);
            listRootFolders.RemoveAt(listRootFolders.Count - 1);
            return listRootFolders;
        }

        private static List<string> GetFileList(string filePath)
        {
            filePath = filePath.Replace("C:\\", "");
            string[] filePathFolders = filePath.Split('\\');
            List<string> listFilePathFolders = new List<string>(filePathFolders);
            return listFilePathFolders;
        }

        private static string GetSolutionFolderPath(Project folder)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string solutionDirectory = Path.GetDirectoryName(folder.DTE.Solution.FullName);
            List<string> segments = new List<string>();

            do
            {
                segments.Add(folder.Name);
                folder = folder.ParentProjectItem?.ContainingProject;
            } while (folder != null);

            segments.Reverse();

            return Path.Combine(new[] { solutionDirectory }.Concat(segments).ToArray());
        }
    }
}