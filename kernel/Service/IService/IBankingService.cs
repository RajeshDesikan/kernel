using kernel.Model.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kernel.Service.IService
{
    public  interface IBankingService
    {
        Task<BankingResponseModel> GetAccountBalanceAsync(string accountId);
        Task<BankingResponseModel> GetAccountStatementAsync(string accountId, int months);
        Task<IEnumerable<string>> FanOutBalancesAsync(IEnumerable<string> accountIds);
        Task<string> FanInBalancesAsync(IEnumerable<string> accountIds);
        Task<IEnumerable<string>> MapAccountsAsync(IEnumerable<string> accountIds, int months = 0, bool isBalance = true);
        Task<string> ReduceBalancesAsync(IEnumerable<string> accountIds);


    }
}
