using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using KLExtensions2022.Commands;
using EnvDTE80;
using Microsoft;
using System.ComponentModel.Design;
using System.Threading.Tasks;

namespace KLExtensions2022 
{
    [Command(PackageGuids.guidPackageEditContextCmdSetString, PackageIds.EditRemoveEmptyLinesCommandId)]
    internal class RemoveEmptyLinesCommand : BaseCommand<RemoveEmptyLinesCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            try
            {
                TextDocument document = GetTextDocument();
                IEnumerable<string> lines = GetSelectedLines(document);

                string result = string.Join(Environment.NewLine, lines.Where(s => !string.IsNullOrWhiteSpace(s)));

                if (result == document.Selection.Text)
                    return;

                using (UndoContext("Remove Empty Lines"))
                {
                    document.Selection.Insert(result, 0);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

    }
}
