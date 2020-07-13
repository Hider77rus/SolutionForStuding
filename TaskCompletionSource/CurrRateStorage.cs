using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskCompletionSource.Interfaces;

namespace TaskCompletionSource
{
    public class CurrRateStorage : ICurrRateStorage
    {
        readonly Dictionary<string, decimal> _currRates = new Dictionary<string, decimal>
        {
            ["RUR/USD"] = 75.0m,
            ["RUR/EUR"] = 85.3m
        };
        public async Task<decimal> GetCurrRateAsync (string currency)
        {
            Console.WriteLine($"Запрос {currency}");
            await Task.Delay(1500);
            
            //throw new Exception("Ошибка в базе");
            return _currRates[currency];
        }
    }
}