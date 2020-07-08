using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace TaskCompletionSource
{
    class Program
    {
        static async Task Main(string[] args)
        {
            
            
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ICurrRateRepository, CurrRateStorage>()
                .Decorate<ICurrRateRepository, CurrRateRepository>()
                .BuildServiceProvider();

            var repository = serviceProvider.GetService<ICurrRateRepository>();
           
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var t = await repository.GetCurrRateAsync("RUR/USD");
                    Console.WriteLine(t);
                }));
            }
            
            await Task.WhenAll(tasks);
            Console.ReadLine();
        }
    }
}