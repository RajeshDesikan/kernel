using kernel.Entity;
using kernel.Model.ResponseModel;
using kernel.Service.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kernel.Service
{
    public class BankingService : IBankingService
    {
        private readonly List<BankAccount> _bankAccounts;

        public BankingService()
        {
            _bankAccounts = new List<BankAccount>
    {
        new BankAccount
        {
            AccountId = "1001",
            AccountHolderName = "Alice Smith",
            Balance = 10000,
            Transactions = new List<Transaction>
            {
                new Transaction { TransactionId = "T1", Description = "Salary Deposit", Amount = 8000, Date = DateTime.Now.AddMonths(-2) },
                new Transaction { TransactionId = "T2", Description = "Electric Bill", Amount = -2000, Date = DateTime.Now.AddMonths(-1) }
            }
        },
        new BankAccount
        {
            AccountId = "1002",
            AccountHolderName = "Bob Johnson",
            Balance = 7500,
            Transactions = new List<Transaction>
            {
                new Transaction { TransactionId = "T1", Description = "Freelance Payment", Amount = 5000, Date = DateTime.Now.AddMonths(-3) },
                new Transaction { TransactionId = "T2", Description = "Gym Membership", Amount = -1500, Date = DateTime.Now.AddMonths(-1) }
            }
        },
        new BankAccount
        {
            AccountId = "1003",
            AccountHolderName = "Charlie Davis",
            Balance = 12000,
            Transactions = new List<Transaction>
            {
                new Transaction { TransactionId = "T1", Description = "Investment Return", Amount = 7000, Date = DateTime.Now.AddMonths(-4) },
                new Transaction { TransactionId = "T2", Description = "Car Maintenance", Amount = -3000, Date = DateTime.Now.AddMonths(-2) }
            }
        },
        new BankAccount
        {
            AccountId = "1004",
            AccountHolderName = "Diana Lee",
            Balance = 6500,
            Transactions = new List<Transaction>
            {
                new Transaction { TransactionId = "T1", Description = "Bonus", Amount = 4000, Date = DateTime.Now.AddMonths(-1) },
                new Transaction { TransactionId = "T2", Description = "Shopping", Amount = -2000, Date = DateTime.Now.AddMonths(-1) }
            }
        },
        new BankAccount
        {
            AccountId = "1005",
            AccountHolderName = "Evan Wright",
            Balance = 9000,
            Transactions = new List<Transaction>
            {
                new Transaction { TransactionId = "T1", Description = "Consulting Fee", Amount = 9000, Date = DateTime.Now.AddMonths(-2) },
                new Transaction { TransactionId = "T2", Description = "Charity Donation", Amount = -3000, Date = DateTime.Now.AddMonths(-1) }
            }
        }
    };
        }


        public async Task<BankingResponseModel> GetAccountBalanceAsync(string accountId)
        {
            var account = _bankAccounts.FirstOrDefault(a => a.AccountId == accountId);
            if (account == null)
            {
                return new BankingResponseModel { Message = "Account not found." };
            }

            return new BankingResponseModel
            {
                AccountHolderName = account.AccountHolderName,
                Balance = account.Balance,
                Message = "Balance retrieved successfully."
            };
        }

        public async Task<BankingResponseModel> GetAccountStatementAsync(string accountId, int months)
        {
            var account = _bankAccounts.FirstOrDefault(a => a.AccountId == accountId);
            if (account == null)
            {
                return new BankingResponseModel { Message = "Account not found." };
            }

            var fromDate = DateTime.Now.AddMonths(-months);
            var transactions = account.Transactions.Where(t => t.Date >= fromDate).ToList();

            return new BankingResponseModel
            {
                AccountHolderName = account.AccountHolderName,
                Transactions = transactions,
                Message = $"Transactions from the last {months} month(s) retrieved successfully."
            };
        }

        public async Task<IEnumerable<string>> FanOutBalancesAsync(IEnumerable<string> accountIds)
        {
            var tasks = accountIds.Select(async accountId =>
            {
                var balance = await GetAccountBalanceAsync(accountId);
                return $"Account: {accountId}, Balance: {balance.Balance:C}";
            });

            return await Task.WhenAll(tasks);
        }

        public async Task<string> FanInBalancesAsync(IEnumerable<string> accountIds)
        {
            var tasks = accountIds.Select(async accountId =>
            {
                var balance = await GetAccountBalanceAsync(accountId);
                return balance.Balance;
            });

            var balances = await Task.WhenAll(tasks);
            decimal totalBalance = (decimal)balances.Sum();
            return $"Total Balance Across Accounts: {totalBalance:C}";
        }

        public async Task<IEnumerable<string>> MapAccountsAsync(IEnumerable<string> accountIds, int months = 0, bool isBalance = true)
        {
            if (isBalance)
            {
                return await FanOutBalancesAsync(accountIds);
            }
            else
            {
                var tasks = accountIds.Select(async accountId =>
                {
                    var statement = await GetAccountStatementAsync(accountId, months);
                    return $"Account: {accountId}, Transactions: {string.Join(", ", statement.Transactions.Select(t => $"{t.Description} ({t.Amount:C})"))}";
                });

                return await Task.WhenAll(tasks);
            }
        }

        public async Task<string> ReduceBalancesAsync(IEnumerable<string> accountIds)
        {
            var tasks = accountIds.Select(async accountId =>
            {
                var balance = await GetAccountBalanceAsync(accountId);
                return balance.Balance;
            });

            var balances = await Task.WhenAll(tasks);
            decimal totalBalance = (decimal)balances.Sum();
            return $"Total Balance Reduced: {totalBalance:C}";
        }
    }
}
