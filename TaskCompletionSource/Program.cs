using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Channels;
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
                //.Decorate<ICurrRateRepository, CurrRateRepositoryBasedOnBlockingCollection>()
                .Decorate<ICurrRateRepository, CurrRateRepositoryBasedOnBufferBlock>()
                .BuildServiceProvider();

            var repository = serviceProvider.GetService<ICurrRateRepository>();
           
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < 1; i++)
            {
                tasks.Add(repository.GetCurrRateAsync("RUR/USD"));
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