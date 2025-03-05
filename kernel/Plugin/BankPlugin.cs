using kernel.Events;
using kernel.Service;
using kernel.Service.IService;
using Microsoft.SemanticKernel;
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
            // Emit "Balance Inquiry Started" event
            await EmitEventAsync(BankingEvents.BalanceInquiryStarted, accountId);

            var response = await _bankingService.GetAccountBalanceAsync(accountId);

            // Emit "Balance Inquiry Completed" event
            await EmitEventAsync(BankingEvents.BalanceInquiryCompleted, accountId);

            return $"Account Holder: {response.AccountHolderName}, Balance: {response.Balance:C}";
        }

        [KernelFunction("GetStatement")]
        public async Task<string> GetStatement(string accountId, int months)
        {
            await EmitEventAsync(BankingEvents.StatementRetrievalStarted, accountId);

         
            var response = await _bankingService.GetAccountStatementAsync(accountId, months);

     
            var cutoffDate = DateTime.Now.AddMonths(-months);
            var filteredTransactions = response.Transactions
                                               .Where(t => t.Date >= cutoffDate)
                                               .Select(t => $"{t.Date.ToShortDateString()}: {t.Description} ({t.Amount:C})")
                                               .ToList();

            await EmitEventAsync(BankingEvents.StatementRetrievalCompleted, accountId);

           
            if (!filteredTransactions.Any())
            {
                return $"No transactions found in the last {months} months for account ID {accountId}.";
            }


            return $"Transactions for the last {months} months:\n{string.Join("\n", filteredTransactions)}";
        }



        [KernelFunction("FanOut")]
        public async Task<List<string>> FanOut(List<string> accountIds)
        {
            await EmitEventAsync(BankingEvents.FanOutStarted, string.Join(", ", accountIds));

            var tasks = accountIds.Select(async accountId =>
            {
                var balance = await _bankingService.GetAccountBalanceAsync(accountId);
                return $"Account: {accountId}, Balance: {balance.Balance:C}";
            });

            var results = await Task.WhenAll(tasks);

            await EmitEventAsync(BankingEvents.FanOutCompleted, string.Join(", ", results));
            return results.ToList();
        }

        // Fan-In: Aggregate results from multiple sources
        [KernelFunction("FanIn")]
        public async Task<decimal?> FanIn(List<string> accountIds)
        {
            await EmitEventAsync(BankingEvents.FanInStarted, string.Join(", ", accountIds));

            var tasks = accountIds.Select(async accountId =>
            {
                var balance = await _bankingService.GetAccountBalanceAsync(accountId);
                return balance.Balance;
            });

            var balances = await Task.WhenAll(tasks);
            var totalBalance = balances.Sum();

            await EmitEventAsync(BankingEvents.FanInCompleted, totalBalance.ToString());
            return totalBalance;
        }

        // Map: Transform a collection of data
        [KernelFunction("Map")]
        public async Task<List<string>> Map(List<string> accountIds)
        {
            await EmitEventAsync(BankingEvents.MapStarted, string.Join(", ", accountIds));

            


            var results = await Task.WhenAll(
        accountIds.Select(async accountId =>
        {
            var response = await _bankingService.GetAccountBalanceAsync(accountId);
            return $"Account ID: {accountId}, Balance: {response.Balance:C}";
        })
    );


            await EmitEventAsync(BankingEvents.MapCompleted, string.Join(", ", results));

         
            return results.ToList();
        }

        // Reduce: Aggregate results into a single value
        [KernelFunction("Reduce")]
        public async Task<string> Reduce(List<string> mappedData)
        {
            await EmitEventAsync(BankingEvents.ReduceStarted, string.Join(", ", mappedData));

          

            // Iterate over account IDs and fetch balances
            var totalBalance = (await Task.WhenAll(
        mappedData.Select(async accountId =>
        {
            var response = await _bankingService.GetAccountBalanceAsync(accountId);
            return (decimal)response.Balance;
        })
    )).Sum();

            // Format the result
            string reducedResult = $"Total Balance: {totalBalance:C}";

            
            await EmitEventAsync(BankingEvents.ReduceCompleted, reducedResult);

       
            return reducedResult;
        }

        private static Task EmitEventAsync(string eventName, string data)
        {
            // Log or handle the emitted event
            Console.WriteLine($"Event: {eventName}, Data: {data}");
            return Task.CompletedTask;
        }
    }
}
