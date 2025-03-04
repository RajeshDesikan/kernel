using kernel.Service.IService;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kernel.Plugin
{
    public class BankPlugin
    {
        private readonly IBankingService _bankingService;

        public BankPlugin(IBankingService bankingService)
        {
            _bankingService = bankingService;
        }

        [KernelFunction("GetBalance")]
        public async Task<string> GetBalance(string accountId)
        {
            var response = await _bankingService.GetAccountBalanceAsync(accountId);
            return $"Account Holder: {response.AccountHolderName}, Balance: {response.Balance:C}";
        }

        [KernelFunction("GetStatement")]
        public async Task<string> GetStatement(string accountId, int months)
        {
            var response = await _bankingService.GetAccountStatementAsync(accountId, months);
            var transactions = response.Transactions.Select(t => $"{t.Date.ToShortDateString()}: {t.Description} ({t.Amount:C})");
            return $"Transactions:\n{string.Join("\n", transactions)}";
        }

        [KernelFunction("FanInBalances")]
        public async Task<IEnumerable<decimal?>> FanInBalances(IEnumerable<string> accountIds)
        {
            var tasks = accountIds.Select(async accountId =>
            {
                var response = await _bankingService.GetAccountBalanceAsync(accountId);
                return response.Balance;
            });

            return await Task.WhenAll(tasks);
        }


        [KernelFunction("FanOutBalances")]
        public async Task<IEnumerable<string>> FanOutBalances(IEnumerable<string> accountIds)
        { 
            var tasks = accountIds.Select(accountId => GetBalance(accountId));
            return await Task.WhenAll(tasks);
        }

        [KernelFunction("MapAccounts")]
        public async Task<IEnumerable<string>> MapAccounts(IEnumerable<string> accountIds, int months = 0, bool isBalance = true)
        {
            if (isBalance)
            {
                var tasks = accountIds.Select(accountId => GetBalance(accountId));
                return await Task.WhenAll(tasks);
            }
            else
            {
                var tasks = accountIds.Select(accountId => GetStatement(accountId, months));
                return await Task.WhenAll(tasks);
            }
        }

        [KernelFunction("ReduceBalances")]
        public async Task<IEnumerable<decimal?>> ReduceBalances(IEnumerable<string> accountIds)
        {
            var tasks = accountIds.Select(async accountId =>
            {
                var balance = await _bankingService.GetAccountBalanceAsync(accountId);
                return balance.Balance;
            });

            return await Task.WhenAll(tasks);
        }
    }
}
