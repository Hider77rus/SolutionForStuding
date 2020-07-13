using System.Threading.Tasks;

namespace TaskCompletionSource.Interfaces
{
    public interface ICurrRateRepository
    {
        public Task<decimal> GetCurrRateAsync(string currency);
    }
}