using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace KLExtensions2022 
{
    internal static class OutputWindowHelper
    {
        private static IVsOutputWindowPane _outputWindowPane;

        private static IVsOutputWindowPane OutputWindowPane => _outputWindowPane ?? (_outputWindowPane = GetOutputWindowPane());

        internal static void DiagnosticWriteLine(string message, Exception ex = null)
        {
            if (ex != null)
            {
                message += $": {ex}";
            }

            WriteLine("Diagnostic", message);
        }

        internal static void ExceptionWriteLine(string message, Exception ex)
        {
            var exceptionMessage = $"{message}: {ex}";

            WriteLine("Handled Exception", exceptionMessage);
        }

        internal static void WarningWriteLine(string message)
        {
            WriteLine("Warning", message);
        }

        private static IVsOutputWindowPane GetOutputWindowPane()
        {
            var outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            if (outputWindow == null) return null;

            Guid outputPaneGuid = new Guid("4e7ba904-9311-4dd0-abe5-a61c1739780f");
            IVsOutputWindowPane windowPane;

            outputWindow.CreatePane(ref outputPaneGuid, "JoinLines", 1, 1);
            outputWindow.GetPane(ref outputPaneGuid, out windowPane);

            return windowPane;
        }

        private static void WriteLine(string category, string message)
        {
            var outputWindowPane = OutputWindowPane;
            if (outputWindowPane != null)
            {
                string outputMessage = $"[JoinLines {category} {DateTime.Now.ToString("hh:mm:ss tt")}] {message}{Environment.NewLine}";

                outputWindowPane.OutputString(outputMessage);
            }
        }
    }
}
