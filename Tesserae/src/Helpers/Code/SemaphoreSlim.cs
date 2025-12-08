using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tesserae
{
    [H5.Name("tss.SingleSemaphoreSlim")]
    public class SingleSemaphoreSlim
    {
        public SingleSemaphoreSlim()
        {

        }

        private readonly Queue<TaskCompletionSource<bool>> _queue = new Queue<TaskCompletionSource<bool>>();

        public Task WaitAsync()
        {
            var completion = new TaskCompletionSource<bool>();

            _queue.Enqueue(completion);

            if (_queue.Count == 1)
            {
                completion.SetResult(true);
            }

            return completion.Task;
        }


        public bool IsPending => _queue.Count > 0;
        public void Release()
        {
            if (_queue.Count == 0)
            {
                throw new InvalidOperationException("Nothing to release");
            }

            var completion = _queue.Dequeue();

            if (!completion.Task.IsCompleted && !completion.Task.IsCanceled && !completion.Task.IsFaulted)
            {
                throw new InvalidOperationException("Released wrong semaphore");
            }

            if (_queue.Count > 0)
            {
                _queue.Peek().SetResult(true);
            }
        }
    }


    [H5.Name("tss.Semaphore")]
    public class Semaphore
    {
        private          int                               currentCount;
        private readonly Queue<TaskCompletionSource<bool>> _queue;

        public Semaphore(int maxConcurrency = 1)
        {
            currentCount = maxConcurrency;
            _queue       = new Queue<TaskCompletionSource<bool>>();
        }

        public Task WaitAsync()
        {
            if (this.currentCount > 0)
            {
                this.currentCount--;
                return Task.CompletedTask;
            }

            var completion = new TaskCompletionSource<bool>();

            _queue.Enqueue(completion);

            return completion.Task.ContinueWith(t =>
            {
                this.currentCount--;
            });
        }

        public bool IsPending => _queue.Count > 0;
        public void Release()
        {
            this.currentCount++;

            if (_queue.Count > 0)
            {
                _queue.Dequeue().SetResult(true);
            }
        }
    }

}