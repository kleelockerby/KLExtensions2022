using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;

namespace KLExtensions2022 
{
    public class UndoTransactionHelper
    {
        private readonly KLExtensions2022Package package;
        private readonly string transactionName;

        public static DTE2 DTE2 { get; private set; }

        public UndoTransactionHelper(KLExtensions2022Package _package, string _transactionName)
        {
            package = _package;
            transactionName = _transactionName;
        }

        public void Run(Action tryAction, Action<Exception> catchAction = null)
        {
            bool shouldCloseUndoContext = false;
            DTE2 = KLExtensions2022Package.DTE2 as DTE2;

            if (DTE2.UndoContext.IsOpen)
            {
                DTE2.UndoContext.Open(transactionName);
                shouldCloseUndoContext = true;
            }

            try
            {
                tryAction();
            }
            catch (Exception ex)
            {
                var message = $"{transactionName} was stopped";
                OutputWindowHelper.ExceptionWriteLine(message, ex);
                DTE2.StatusBar.Text = $"{message}.  See output window for more details.";

                catchAction?.Invoke(ex);

                if (shouldCloseUndoContext)
                {
                    DTE2.UndoContext.SetAborted();
                    shouldCloseUndoContext = false;
                }
            }
            finally
            {
                if (shouldCloseUndoContext)
                {
                    DTE2.UndoContext.Close();
                }
            }
        }
    }
}
