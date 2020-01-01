using System;
using System.Threading;
using System.Threading.Tasks;

namespace ByteBank.View.Utils
{
    public class ByteBankProgresss<I> : IProgress<I>
    {
        private readonly TaskScheduler _taskScheduler;
        private readonly Action<I> _handler;
        public ByteBankProgresss(Action<I> handler)
        {
            _handler = handler;
            _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }
        public void Report(I value)
        {
            Task.Factory.StartNew(() => _handler(value), CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }
    }
}
