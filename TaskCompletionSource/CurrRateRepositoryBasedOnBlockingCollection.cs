using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskCompletionSource.Interfaces;

namespace TaskCompletionSource
{
    public class CurrRateRepositoryBasedOnBlockingCollection : CurrRateRepository, ICurrRateRepository
    {
        private readonly BlockingCollection<KeyValuePair<string,TaskCompletionSource<decimal>>> _queue = new BlockingCollection<KeyValuePair<string,TaskCompletionSource<decimal>>>();

 
        public CurrRateRepositoryBasedOnBlockingCollection(ICurrRateStorage storage) : base(storage)
        {
            Task.Run(QueueConsumer);
        }

        public async Task<decimal> GetCurrRateAsync(string currency)
        {
            var tcs = new TaskCompletionSource<decimal>();
            var queueItem = new KeyValuePair<string, TaskCompletionSource<decimal>>(currency,tcs);
            _queue.Add(queueItem);
            
            return await tcs.Task;
        }
        
        //Обработчик очереди
        private void QueueConsumer()
        {
            foreach (var (currency, tcs) in _queue.GetConsumingEnumerable(CancellationToken.None))
            {
                SetTcsResult(currency,tcs);
            }
        }
    }
}