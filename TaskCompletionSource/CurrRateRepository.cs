using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskCompletionSource.Interfaces;

namespace TaskCompletionSource
{
    public abstract class CurrRateRepository
    {
        private readonly ConcurrentDictionary<string, decimal> _rates = new ConcurrentDictionary<string, decimal>();
        private readonly Dictionary<string,Task<decimal>> _requests = new Dictionary<string, Task<decimal>>();
        private readonly ICurrRateStorage _currRateStorage;

        protected CurrRateRepository(ICurrRateStorage currRateStorage)
        {
            _currRateStorage = currRateStorage;
        }
        
        protected void SetTcsResult(string currency, TaskCompletionSource<decimal> tcs)
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