using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TaskCompletionSource.Interfaces;

namespace TaskCompletionSource
{
    public class CurrRateRepositoryBasedOnBufferBlock: CurrRateRepository,  ICurrRateRepository
    {
        BufferBlock<KeyValuePair<string,TaskCompletionSource<decimal>>> _queue = new BufferBlock<KeyValuePair<string,TaskCompletionSource<decimal>>>();

        public async Task<decimal> GetCurrRateAsync(string currency)
        {
            var tcs = new TaskCompletionSource<decimal>();
            var queueItem = new KeyValuePair<string, TaskCompletionSource<decimal>>(currency,tcs);
            _queue.Post(queueItem);
            
            return await tcs.Task;
        }

        public CurrRateRepositoryBasedOnBufferBlock(ICurrRateStorage storage) : base(storage)
        {
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
    }
}