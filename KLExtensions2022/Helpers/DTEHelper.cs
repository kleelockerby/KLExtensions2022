using System;
using System.IO;
using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Concurrent;
using System.Text;
using VSLangProj;
using System.Collections;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.VisualStudio.OLE.Interop;
using System.ComponentModel.Design;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Modeling;

namespace KLExtensions2022.Helpers
{
	public static class DTEHelper
	{
		private const uint ROOT = 0xFFFFFFFE;

		public static DTE DTE { get; set; }
		public static DTE2 DTE2 { get; set; }

		public static void Initialize(EnvDTE.DTE dte, EnvDTE80.DTE2 dte2)
		{
			DTE = dte;
			DTE2 = dte2 as DTE2;
		}

		public static Project Project
		{
			get
			{
				object[] projects = ((object[])DTE.ActiveSolutionProjects);
				if (projects.Length > 0)
				{
					return ((object[])DTE.ActiveSolutionProjects)[0] as Project;
				}
				else return null;
			}
		}

		public static ProjectItem ProjectItem
		{
			get
			{
				Array items = (Array)DTE2.ToolWindows.SolutionExplorer.SelectedItems;
				if (items.Length == 1)
				{
					UIHierarchyItem selection = items.Cast<UIHierarchyItem>().First();
					if (selection.Object is ProjectItem projectItem)
					{
						return projectItem;
					}
					else
					{
						return null;
					}
				}
				return null;
			}
		}

		public static IVsHierarchy GetVsHierarchy(System.IServiceProvider provider, EnvDTE.Project project)
		{
			IVsSolution solution = (IVsSolution)provider.GetService(typeof(SVsSolution));
			Debug.Assert(solution != null, "couldn't get the solution service");
			if (solution != null)
			{
				if (project != null)
				{
					IVsHierarchy vsHierarchy = null;
					solution.GetProjectOfUniqueName(project.UniqueName, out vsHierarchy);
					return vsHierarchy;
				}
			}
			return null;
		}

		internal static Guid GetProjectGuid(System.IServiceProvider serviceProvider, Project project)
		{
			if (project != null)
			{
				IVsHierarchy vsHier = DTEHelper.GetVsHierarchy(serviceProvider, project);
				Guid projectGuid = Guid.Empty;
				vsHier.GetGuidProperty(ROOT, (int)__VSHPROPID.VSHPROPID_ProjectIDGuid, out projectGuid);
				return projectGuid;
			}
			return Guid.Empty;
		}
	}
}
