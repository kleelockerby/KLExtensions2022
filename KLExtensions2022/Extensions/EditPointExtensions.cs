using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLExtensions2022.Extensions
{
    public static class EditPointExtensions
    {
        internal static string GetLine(this EditPoint editPoint)
        {
            return editPoint.GetLines(editPoint.Line, editPoint.Line + 1);
        }
    }
}
