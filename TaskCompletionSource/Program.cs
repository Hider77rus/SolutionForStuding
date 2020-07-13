using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TaskCompletionSource.Interfaces;

namespace TaskCompletionSource
{
    class Program
    {
        static async Task Main(string[] args)
        {
           
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ICurrRateStorage, CurrRateStorage>()
                //.AddSingleton<ICurrRateRepository, CurrRateRepositoryBasedOnBlockingCollection>()
                //.AddSingleton<ICurrRateRepository, CurrRateRepositoryBasedOnBufferBlock>()
                .AddSingleton<ICurrRateRepository, CurrRateRepositoryBasedOnChannels>()
                .BuildServiceProvider();

            var repository = serviceProvider.GetService<ICurrRateRepository>();
           
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                tasks.Add(repository.GetCurrRateAsync("RUR/USD").ContinueWith(async t => {
                    Console.WriteLine(await t);
                }));
            }
            
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            Console.ReadLine();

            Console.WriteLine(await repository.GetCurrRateAsync("RUR/EUR"));
            
            Console.ReadLine();
        }
    }
}