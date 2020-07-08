using System.Threading.Tasks;

namespace TaskCompletionSource
{
    public interface ICurrRateRepository
    {
        public Task<decimal> GetCurrRateAsync(string currency);
    }
}