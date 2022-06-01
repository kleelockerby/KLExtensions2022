using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;

namespace KLExtensions2022.Helpers
{
    public static class ProjectHelpers
    {
        private static readonly DTE2 Dte2 = KLExtensions2022Package.DTE2 as DTE2;

        public static Project GetActiveProject()
        {
            try
            {
                if (Dte2.ActiveSolutionProjects is Array activeSolutionProjects && activeSolutionProjects.Length > 0)
                {
                    return activeSolutionProjects.GetValue(0) as Project;
                }

                Document doc = Dte2.ActiveDocument;

                if (doc != null && !string.IsNullOrEmpty(doc.FullName))
                {
                    ProjectItem item = Dte2.Solution?.FindProjectItem(doc.FullName);

                    if (item != null)
                    {
                        return item.ContainingProject;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting the active project" + ex);
            }
            return null;
        }

        public static string CleanNameSpace(string ns, bool stripPeriods = true)
        {
            if (stripPeriods)
            {
                ns = ns.Replace(".", "");
            }
            ns = ns.Replace(" ", "").Replace("-", "").Replace("\\", ".");
            return ns;
        }

        public static IWpfTextView GetCurentTextView()
        {
            IComponentModel componentModel = GetComponentModel();
            if (componentModel == null)
            {
                return null;
            }

            IVsEditorAdaptersFactoryService editorAdapter = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            return editorAdapter.GetWpfTextView(GetCurrentNativeTextView());
        }

        public static IVsTextView GetCurrentNativeTextView()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var textManager = (IVsTextManager)ServiceProvider.GlobalProvider.GetService(typeof(SVsTextManager));
            Assumes.Present(textManager);

            ErrorHandler.ThrowOnFailure(textManager.GetActiveView(1, null, out IVsTextView activeView));
            return activeView;
        }

        public static IComponentModel GetComponentModel()
        {
            return (IComponentModel)KLExtensions2022Package.ComponentModel;
        }

        public static bool IsKind(this Project project, string kindGuid)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return project.Kind.Equals(kindGuid, StringComparison.OrdinalIgnoreCase);
        }

        private static IEnumerable<Project> GetChildProjects(Project parent)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                if (!parent.IsKind(ProjectKinds.vsProjectKindSolutionFolder) && parent.Collection == null)   
                {
                    return Enumerable.Empty<Project>();
                }

                if (!string.IsNullOrEmpty(parent.FullName))
                {
                    return new[] { parent };
                }
            }
            catch (COMException)
            {
                return Enumerable.Empty<Project>();
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            return parent.ProjectItems
                    .Cast<ProjectItem>()
                    .Where(p => p.SubProject != null)
                    .SelectMany(p => GetChildProjects(p.SubProject));
        }

        public static string GetRootFolder(this Project project)
        {
            if (project == null)
            {
                return null;
            }

            if (project.IsKind("{66A26720-8FB5-11D2-AA7E-00C04F688DDE}")) 
            {
                return Path.GetDirectoryName(Dte2.Solution.FullName);
            }

            if (string.IsNullOrEmpty(project.FullName))
            {
                return null;
            }

            string fullPath;

            try
            {
                fullPath = project.Properties.Item("FullPath").Value as string;
            }
            catch (ArgumentException)
            {
                try
                {
                    fullPath = project.Properties.Item("ProjectDirectory").Value as string;
                }
                catch (ArgumentException)
                {
                    fullPath = project.Properties.Item("ProjectPath").Value as string;
                }
            }

            if (string.IsNullOrEmpty(fullPath))
            {
                return File.Exists(project.FullName) ? Path.GetDirectoryName(project.FullName) : null;
            }

            if (Directory.Exists(fullPath))
            {
                return fullPath;
            }

            if (File.Exists(fullPath))
            {
                return Path.GetDirectoryName(fullPath);
            }

            return null;
        }

        public static string GetFileName(this ProjectItem item)
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
    }
}
