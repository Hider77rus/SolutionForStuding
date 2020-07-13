using System.Threading.Tasks;

namespace TaskCompletionSource.Interfaces
{
    public interface ICurrRateStorage
    {
        public Task<decimal> GetCurrRateAsync(string currency);
    }
}