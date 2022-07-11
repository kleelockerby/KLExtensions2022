using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KLExtensions2022 
{
    public static class TaskExtensions
    {
        public static void FireAndForget(this System.Threading.Tasks.Task task, bool logOnFailure = true)
        {
            task.ContinueWith(delegate (System.Threading.Tasks.Task antecedent)
            {
                if (logOnFailure)
                {
                    Console.WriteLine("FM");
                }

            }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default).Forget();
        }

        public static void FireAndForget(this JoinableTask joinableTask, bool logOnFailure = true)
        {
            FireAndForget(joinableTask.Task, logOnFailure);
        }
    }
}
