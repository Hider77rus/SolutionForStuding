using System.Threading.Channels;
using System.Threading.Tasks;
using TaskCompletionSource.Interfaces;

namespace TaskCompletionSource
{
    public class CurrRateRepositoryBasedOnChannels: CurrRateRepository, ICurrRateRepository
    {
        private readonly Channel<(string,TaskCompletionSource<decimal>)> _channel;
      
        public async Task<decimal> GetCurrRateAsync(string currency)
        {
            var tcs = new TaskCompletionSource<decimal>();
            await _channel.Writer.WriteAsync((currency,tcs));
            
            return await tcs.Task;
        }

        public CurrRateRepositoryBasedOnChannels(ICurrRateStorage currRateStorage) : base(currRateStorage)
        {
            _channel = Channel.CreateUnbounded<(string,TaskCompletionSource<decimal>)>();
            Task.Run(QueueConsumer);
        }

        //Обработчик очереди
        private async Task QueueConsumer()
        {
            var reader = _channel.Reader;
            await foreach (var (currency, tcs) in reader.ReadAllAsync())
                SetTcsResult(currency,tcs);
        }
    }
}