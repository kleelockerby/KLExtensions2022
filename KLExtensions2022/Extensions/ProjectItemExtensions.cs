using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using System;
using System.IO;
using System.Linq;
using KLExtensions2022.Helpers;

namespace KLExtensions2022
{
    public static class ProjectItemExtensions
    {
        private static readonly DTE2 Dte2 = KLExtensions2022Package.DTE2 as DTE2;

        public static string GetFullPathFileName(this ProjectItem item)
        {
            try
            {
                return item?.Properties?.Item("FullPath").Value?.ToString();
            }
            catch (ArgumentException)
            {
                // The property does not exist.
                return null;
            }
        }

        public static string GetFileName(this ProjectItem projectItem)
        {
            if (!IsPhysicalFile(projectItem))
                return null;

            return projectItem.Name;
        }

        public static string GetFileNamePath(this ProjectItem item)
        {
            try
            {
                return item?.Properties?.Item("FullPath").Value?.ToString();
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public static string GetFolderName(this ProjectItem projectItem)
        {
            if (IsPhysicalFile(projectItem))
            {
                string fileName = GetFileName(projectItem);
                string path = Path.GetDirectoryName(fileName);
                return path;
            }

            if (!IsPhysicalFolder(projectItem))
            {
                return null;
            }

            return projectItem.FileNames[1].TrimEnd('\\');
        }

        public static ProjectItem AddFileToProject(this Project project, FileInfo file, string itemType = null)
        {
            if (project.IsKind(ProjectTypes.ASPNET_5, ProjectTypes.SSDT))
            {
                return Dte2.Solution.FindProjectItem(file.FullName);
            }

            string root = project.GetRootFolder();

            if (string.IsNullOrEmpty(root) || !file.FullName.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            ProjectItem item = project.ProjectItems.AddFromFile(file.FullName);
            item.SetItemType(itemType);
            return item;
        }

        public static ProjectItem GetParentProjectItem(this ProjectItem projectItem)
        {
            try
            {
                var parentProjectItem = projectItem.Collection?.Parent as ProjectItem;
                return parentProjectItem;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static bool IsExternal(this ProjectItem projectItem)
        {
            try
            {
                if (projectItem.Collection == null || !projectItem.IsPhysicalFile())
                {
                    return true;
                }

                return projectItem.Collection.OfType<ProjectItem>().All(x => x.Object != projectItem.Object);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static void SetItemType(this ProjectItem item, string itemType)
        {
            try
            {
                if (item == null || item.ContainingProject == null)
                {
                    return;
                }

                if (string.IsNullOrEmpty(itemType) || item.ContainingProject.IsKind(ProjectTypes.WEBSITE_PROJECT) || item.ContainingProject.IsKind(ProjectTypes.UNIVERSAL_APP))
                {
                    return;
                }

                item.Properties.Item("ItemType").Value = itemType;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static bool IsKind(this ProjectItem projectItem, params string[] kindGuids)
        {
            foreach (string guid in kindGuids)
            {
                if (projectItem.Kind.Equals(guid, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static ProjectItem ResolveToPhysicalProjectItem(this ProjectItem projectItem)
        {
            if (projectItem.IsKind(Constants.vsProjectItemKindVirtualFolder))
            {
                return projectItem.ProjectItems
                        .Cast<ProjectItem>()
                        .Select(item => ResolveToPhysicalProjectItem(item))
                        .FirstOrDefault(item => item != null);
            }
            return projectItem;
        }

        private static bool IsPhysicalFile(this ProjectItem projectItem)
        {
            try
            {
                return string.Equals(projectItem.Kind, EnvDTE.Constants.vsProjectItemKindPhysicalFile, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static bool IsPhysicalFolder(ProjectItem projectItem)
        {
            try
            {
                return String.Equals(projectItem.Kind, VSConstants.GUID_ItemType_PhysicalFolder.ToString("B"), StringComparison.InvariantCultureIgnoreCase);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
