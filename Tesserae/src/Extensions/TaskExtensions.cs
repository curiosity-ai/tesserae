﻿using System;
using System.Linq;
using System.Threading.Tasks;
using H5;
using static H5.Core.dom;

namespace Tesserae
{
    [H5.Name("tss.tX")]
    public static class TaskExtensions
    {
        /// <summary>
        /// H5 doesn't support Task.Completed, so we'll fill in something similar
        /// </summary>
        public static Task Completed { get; } = BuildCompletedTask();

        /// <summary>
        /// Sometimes you want to start a Task and not await its results but there is an analyzer that presumes that code that creates Tasks and doesn't await them is incorrect
        /// (and, often, it is right) but sometimes you don't want to await and you don't want the analyzer telling you about it - in that case, use this extensions method
        /// </summary>
        [Name("fireAndForget")] // 2020-06-03 DWR: Since I removed the unnecessary method overload that took a Task<T>, the consuming projects are trying to identify this as "fireAndForget" instead of "FireAndForget" - maybe this will fix it
        public static void FireAndForget(this Task task)
        {
            if (task is null) return;

            Task.Run(async () =>
            {
                try
                {
                    await task;
                }
                catch (OperationCanceledException)
                {
                    //Ignore
                }
                catch (AggregateException age)
                {
                    if (age.InnerExceptions.Count == 1)
                    {
                        var exception = age.InnerExceptions.Single();

                        if (exception.GetType() != typeof(OperationCanceledException))
                        {
                            console.error("Error running FireAndForget Task\n" + exception.ToString());
                            console.error(exception);
                        }
                    }
                    else
                    {
                        bool first = true;

                        foreach (var inner in age.InnerExceptions)
                        {
                            if (inner.GetType() != typeof(OperationCanceledException))
                            {
                                if (first)
                                {
                                    console.error("Multiple errors running FireAndForget Task:");
                                    first = false;
                                }
                                console.error("\t\tInner exception\n" + inner.ToString());
                                console.error(inner);
                            }
                        }
                    }
                }
                catch (Exception E)
                {
                    console.error("Error running FireAndForget Task\n" + E.ToString());
                    console.error(E);
                }
            });
        }

        public static async Task<T> Unwrap<T>(this Task<Task<T>> task)
        {
            // 2020-02-07 DWR: Can't just "return await await task;" because H5 will fail at runtime
            var onceUnwrappedTask = await task;
            return await onceUnwrappedTask;
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

        /// <summary>
        /// Given multiple tasks that return values, wait for them all to complete and then return a tuple that contains the results (in the order in which the tasks are specified as method arguments).
        /// If any of the tasks fail then an exception will be raised.
        /// </summary>
        public async static Task<(T1, T2, T3, T4, T5, T6, T7)> WhenAll<T1, T2, T3, T4, T5, T6, T7>(Task<T1> t1, Task<T2> t2, Task<T3> t3, Task<T4> t4, Task<T5> t5, Task<T6> t6, Task<T7> t7)
        {
            await Task.WhenAll(t1, t2, t3, t4, t5, t6, t7);
            return (t1.Result, t2.Result, t3.Result, t4.Result, t5.Result, t6.Result, t7.Result);
        }

        /// <summary>
        /// Given multiple tasks that return values, wait for them all to complete and then return a tuple that contains the results (in the order in which the tasks are specified as method arguments).
        /// If any of the tasks fail then an exception will be raised.
        /// </summary>
        public async static Task<(T1, T2, T3, T4, T5, T6, T7, T8)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8>(Task<T1> t1, Task<T2> t2, Task<T3> t3, Task<T4> t4, Task<T5> t5, Task<T6> t6, Task<T7> t7, Task<T8> t8)
        {
            await Task.WhenAll(t1, t2, t3, t4, t5, t6, t7, t8);
            return (t1.Result, t2.Result, t3.Result, t4.Result, t5.Result, t6.Result, t7.Result, t8.Result);
        }

        public static Task<IComponent> AsTask(this IComponent component) => Task.FromResult<IComponent>(component);


        private static Task BuildCompletedTask()
        {
            var completionSource = new TaskCompletionSource<object>();
            completionSource.SetResult(new object());
            return completionSource.Task;
        }
    }
}