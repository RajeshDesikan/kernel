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
    }
}
