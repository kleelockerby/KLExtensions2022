using System;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using KLExtensions2022.Commands;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace KLExtensions2022 
{
    [Command(PackageGuids.guidPackageCmdSetString, PackageIds.SetAllMethodsBreakpointsCommandId)]
    internal class SetAllMethodsBreakpointsCommand : BaseCommand<SetAllMethodsBreakpointsCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            try
            {
                Document activeDocument = DTE2.ActiveDocument;
                TextSelection textSelection = DTE2.ActiveWindow.Selection as TextSelection;
                CodeClass codeClass = textSelection.ActivePoint.CodeElement[vsCMElement.vsCMElementClass] as EnvDTE.CodeClass;
                
                if ((activeDocument != null) || (textSelection != null) || (codeClass != null))
                {
                    Debugger debugger = DTE.Debugger;
                    EditPoint editPoint = textSelection.ActivePoint.CreateEditPoint();

                    foreach (object targetObject in codeClass.Members)
                    {
                        if (targetObject is CodeFunction)
                        {
                            CodeFunction method = targetObject as CodeFunction;
                            debugger.Breakpoints.Add(String.Empty, activeDocument.FullName, method.GetStartPoint(vsCMPart.vsCMPartBody).Line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}
