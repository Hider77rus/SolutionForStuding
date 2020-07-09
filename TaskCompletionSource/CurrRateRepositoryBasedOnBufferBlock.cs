using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TaskCompletionSource
{
    public class CurrRateRepositoryBasedOnBufferBlock: ICurrRateRepository
    {
        private ConcurrentDictionary<string, decimal> _rates = new ConcurrentDictionary<string, decimal>();
        private readonly Dictionary<string,Task<decimal>> _requests = new Dictionary<string, Task<decimal>>();
        BufferBlock<KeyValuePair<string,TaskCompletionSource<decimal>>> _queue = new BufferBlock<KeyValuePair<string,TaskCompletionSource<decimal>>>();
        private readonly ICurrRateRepository _currRateStorage;
        
        public async Task<decimal> GetCurrRateAsync(string currency)
        {
            var tcs = new TaskCompletionSource<decimal>();
            var queueItem = new KeyValuePair<string, TaskCompletionSource<decimal>>(currency,tcs);
            _queue.Post(queueItem);
            
            return await tcs.Task;
        }


        public CurrRateRepositoryBasedOnBufferBlock(ICurrRateRepository currRateStorage)
        {
            _currRateStorage = currRateStorage;

            Task.Run(QueueConsumer);
        }



        //Обработчик очереди
        private async Task QueueConsumer()
        {
            while (await _queue.OutputAvailableAsync())
            {
                var (currency, tcs) = await _queue.ReceiveAsync();
                
                SetTcsResult(currency,tcs);
            }
        }

        private void SetTcsResult(string currency, TaskCompletionSource<decimal> tcs)
        {
            if (_rates.TryGetValue(currency,out var cacheRate))
                tcs.SetResult(cacheRate);
            else
            {
                Task<decimal> task;
                if (_requests.ContainsKey(currency))
                {
                    Console.WriteLine("Запрос существует");
                    task = _requests[currency];
                }
                else
                {
                    task = Task.Run(async () =>
                    {
                        var currRateAsync = await _currRateStorage.GetCurrRateAsync(currency);
                        _rates.TryAdd(currency, currRateAsync);
                        return currRateAsync;
                    });
                   
                    _requests[currency] = task;
                }

                task.ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        tcs.SetResult(task.Result);
                        _requests.Remove(currency);
                    } else if(t.IsFaulted)
                    {
                        tcs.SetException(t.Exception!);
                    }
                    else
                    {
                        tcs.SetException(new Exception($"unexpected state of DB task: {t.Status}"));
                    }
                });
            }
        }
    }
}