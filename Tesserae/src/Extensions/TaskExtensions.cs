using System;
using System.Threading.Tasks;
using static Retyped.dom;

namespace Tesserae.Components
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Bridge doesn't support Task.Completed, so we'll fill in something similar
        /// </summary>
        public static Task Completed { get; } = BuildCompletedTask();

        /// <summary>
        /// Sometimes you want to start a Task and not await its results but there is an analyzer that presumes that code that creates Tasks and doesn't await them is incorrect
        /// (and, often, it is right) but sometimes you don't want to await and you don't want the analyzer telling you about it - in that case, use this extensions method
        /// </summary>
        public static void FireAndForget(this Task task)
        {
            if (task is null) return;
            Task.Run(async () =>
            {
                try
                {
                    await task;
                }
                catch (Exception E)
                {
                    console.log("Error running FireAndForget Task: " + E.ToString());
                }
            });
        }

        /// <summary>
        /// Sometimes you want to start a Task and not await its results but there is an analyzer that presumes that code that creates Tasks and doesn't await them is incorrect
        /// (and, often, it is right) but sometimes you don't want to await and you don't want the analyzer telling you about it - in that case, use this extensions method
        /// </summary>
        public static void FireAndForget<T>(this Task<T> task)
        {
            if (task is null) return;
            Task.Run(async () =>
            {
                try
                {
                    await task;
                }
                catch (Exception E)
                {
                    console.log("Error running FireAndForget Task: " + E.ToString());
                }
            });
        }

        private static Task BuildCompletedTask()
        {
            var completionSource = new TaskCompletionSource<object>();
            completionSource.SetResult(new object());
            return completionSource.Task;
        }

        /// <summary>
        /// Given multiple tasks that return values, wait for them all to complete and then return a tuple that contains the results (in the order in which the tasks are specified as method arguments).
        /// If any of the tasks fail then an exception will be raised.
        /// </summary>
        public async static Task<(T1, T2)> WhenAll<T1, T2>(Task<T1> t1, Task<T2> t2)
        {
            await Task.WhenAll(t1, t2);
            return (t1.Result, t2.Result);
        }

        /// <summary>
        /// Given multiple tasks that return values, wait for them all to complete and then return a tuple that contains the results (in the order in which the tasks are specified as method arguments).
        /// If any of the tasks fail then an exception will be raised.
        /// </summary>
        public async static Task<(T1, T2, T3)> WhenAll<T1, T2, T3>(Task<T1> t1, Task<T2> t2, Task<T3> t3)
        {
            await Task.WhenAll(t1, t2, t3);
            return (t1.Result, t2.Result, t3.Result);
        }

        /// <summary>
        /// Given multiple tasks that return values, wait for them all to complete and then return a tuple that contains the results (in the order in which the tasks are specified as method arguments).
        /// If any of the tasks fail then an exception will be raised.
        /// </summary>
        public async static Task<(T1, T2, T3, T4)> WhenAll<T1, T2, T3, T4>(Task<T1> t1, Task<T2> t2, Task<T3> t3, Task<T4> t4)
        {
            await Task.WhenAll(t1, t2, t3, t4);
            return (t1.Result, t2.Result, t3.Result, t4.Result);
        }

        /// <summary>
        /// Given multiple tasks that return values, wait for them all to complete and then return a tuple that contains the results (in the order in which the tasks are specified as method arguments).
        /// If any of the tasks fail then an exception will be raised.
        /// </summary>
        public async static Task<(T1, T2, T3, T4, T5)> WhenAll<T1, T2, T3, T4, T5>(Task<T1> t1, Task<T2> t2, Task<T3> t3, Task<T4> t4, Task<T5> t5)
        {
            await Task.WhenAll(t1, t2, t3, t4, t5);
            return (t1.Result, t2.Result, t3.Result, t4.Result, t5.Result);
        }

        /// <summary>
        /// Given multiple tasks that return values, wait for them all to complete and then return a tuple that contains the results (in the order in which the tasks are specified as method arguments).
        /// If any of the tasks fail then an exception will be raised.
        /// </summary>
        public async static Task<(T1, T2, T3, T4, T5, T6)> WhenAll<T1, T2, T3, T4, T5, T6>(Task<T1> t1, Task<T2> t2, Task<T3> t3, Task<T4> t4, Task<T5> t5, Task<T6> t6)
        {
            await Task.WhenAll(t1, t2, t3, t4, t5, t6);
            return (t1.Result, t2.Result, t3.Result, t4.Result, t5.Result, t6.Result);
        }
    }
}
