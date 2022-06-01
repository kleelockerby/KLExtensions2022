using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using KLExtensions2022.Helpers;

namespace KLExtensions2022
{
	public static class ProjectExtensions
	{
		private static readonly DTE2 Dte2 = KLExtensions2022Package.DTE2 as DTE2;

		public static string GetRootFolder(this Project project)
		{
			if (project == null)
			{
				return null;
			}

			if (project.IsKind(ProjectTypes.SOLUTION_FOLDER))
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

		public static string GetRootNamespace(this Project project)
		{
			if (project == null)
			{
				return null;
			}

			string ns = project.Name ?? string.Empty;

			try
			{
				Property prop = project.Properties.Item("RootNamespace");

				if (prop != null && prop.Value != null && !string.IsNullOrEmpty(prop.Value.ToString()))
				{
					ns = prop.Value.ToString();
				}
			}
			catch { /* Project doesn't have a root namespace */ }

			return ProjectHelpers.CleanNameSpace(ns, stripPeriods: false);
		}

		public static bool IsKind(this Project project, params string[] kindGuids)
		{
			foreach (string guid in kindGuids)
			{
				if (project.Kind.Equals(guid, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		private static IEnumerable<Project> GetChildProjects(Project parent)
		{
			try
			{
				if (!parent.IsKind("{66A26720-8FB5-11D2-AA7E-00C04F688DDE}") && parent.Collection == null)   
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
			return parent.ProjectItems.Cast<ProjectItem>().Where(p => p.SubProject != null).SelectMany(p => GetChildProjects(p.SubProject));
		}

	}
}
