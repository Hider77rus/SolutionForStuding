using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TaskCompletionSource
{
    public class CurrRateRepository : ICurrRateRepository
    {
        private ConcurrentDictionary<string, decimal> _rates = new ConcurrentDictionary<string, decimal>();
        private readonly Dictionary<string,Task<decimal>> _requests = new Dictionary<string, Task<decimal>>();
        private readonly BlockingCollection<KeyValuePair<string,TaskCompletionSource<decimal>>> _queue = new BlockingCollection<KeyValuePair<string,TaskCompletionSource<decimal>>>();
        private readonly ICurrRateRepository _currRateStorage;
 
        public CurrRateRepository(ICurrRateRepository currRateStorage)
        {
            _currRateStorage = currRateStorage;
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
                if (_rates.TryGetValue(currency,out var rate))
                    tcs.SetResult(rate);
                else
                {
                    Task<decimal> task;
                    if (_requests.ContainsKey(currency))
                    {
                        task = _requests[currency];
                    }
                    else
                    {
                        task = Task.Run(async () =>
                        {
                            var rate = await _currRateStorage.GetCurrRateAsync(currency);
                            _rates.TryAdd(currency, rate);
                            return rate;
                        });
                        _requests[currency] = task;
                    }

                    Task.Run(async () =>
                    {
                        tcs.SetResult(await task);
                        _requests.Remove(currency);
                    });
                }
            }
        }
    }
}