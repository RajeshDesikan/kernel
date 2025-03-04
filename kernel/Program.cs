using kernel.Plugin;
using kernel.Service;
using Microsoft.SemanticKernel;

var bankingService = new BankingService();
var bankingPlugin = new BankPlugin(bankingService);
var kernel = Kernel.CreateBuilder().Build();
kernel.ImportPluginFromObject(bankingPlugin, "Banking");

List<string> accountHeadIds = new List<string>
{
    "1001",
    "1002",
    "1003",
    "1004",
    "1005"
};

while (true)
{
    Console.WriteLine("\nDo you want to fetch balance, statement, fanout, fanin, map, or reduce? (Enter 'exit' to quit):");
    string choice = Console.ReadLine()?.Trim().ToLower();

    if (choice == "exit")
    {
        Console.WriteLine("Exiting...");
        break;
    }

    switch (choice)
    {
        case "balance":
        case "statement":
            Console.WriteLine("Enter the account head ID:");
            string accountHeadId = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(accountHeadId))
            {
                Console.WriteLine("Account head ID cannot be empty. Please try again.");
                continue;
            }

            if (choice == "balance")
            {
                var arguments = new KernelArguments
                {
                    ["accountId"] = accountHeadId
                };

                var balanceResult = await kernel.InvokeAsync(kernel.Plugins["Banking"]["GetBalance"], arguments);
                Console.WriteLine($"Balance for Account ID {accountHeadId}: {balanceResult}");
            }
            else
            {
                Console.WriteLine("Enter the number of months for the statement:");
                if (!int.TryParse(Console.ReadLine(), out int months) || months <= 0)
                {
                    Console.WriteLine("Invalid input. Please enter a positive integer for months.");
                    continue;
                }

              
                var arguments = new KernelArguments
                {
                    ["accountId"] = accountHeadId,
                    ["months"] = months
                };

                var statementResult = await kernel.InvokeAsync(kernel.Plugins["Banking"]["GetStatement"], arguments);
                Console.WriteLine($"Statement for Account ID {accountHeadId}:\n{statementResult}");
            }
            break;

        case "fanout":                       //A process where a task spawns multiple sub-tasks that can execute concurrently (e.g., fetching data for multiple accounts).

            var fanOutArgs = new KernelArguments
            {
                ["accountIds"] = accountHeadIds
            };

            var fanOutResult = await kernel.InvokeAsync(kernel.Plugins["Banking"]["FanOutBalances"], fanOutArgs);
            Console.WriteLine("Fan-Out Results:\n" + string.Join("\n", fanOutResult.GetValue<IEnumerable<string>>()));
            break;

        case "fanin":
           
            var fanInArgs = new KernelArguments
            {
                ["accountIds"] = accountHeadIds
            };

            var fanInResult = await kernel.InvokeAsync(kernel.Plugins["Banking"]["FanInBalances"], fanInArgs);
            var balances = fanInResult.GetValue<IEnumerable<decimal?>>();
            decimal totalBalance = 0;
            foreach (var balance in balances)
            {
                totalBalance += balance ?? 0; 
            }
            Console.WriteLine($"Total Fan-In Balance: {totalBalance:C}");
            break;

        case "map":
            Console.WriteLine("Do you want to map balances or statements? (Enter 'balance' or 'statement'):");
            string mapChoice = Console.ReadLine()?.Trim().ToLower();

            bool isBalance = mapChoice == "balance";
            int monthsForMap = 0;

            if (!isBalance)
            {
                Console.WriteLine("Enter the number of months for the statement:");
                if (!int.TryParse(Console.ReadLine(), out monthsForMap) || monthsForMap <= 0)
                {
                    Console.WriteLine("Invalid input. Please enter a positive integer for months.");
                    continue;
                }
            }

           
            var mapArgs = new KernelArguments
            {
                ["accountIds"] = accountHeadIds,
                ["months"] = monthsForMap,
                ["isBalance"] = isBalance
            };

            var mapResult = await kernel.InvokeAsync(kernel.Plugins["Banking"]["MapAccounts"], mapArgs);
            Console.WriteLine("Map Results:\n" + string.Join("\n", mapResult.GetValue<IEnumerable<string>>()));
            break;

        case "reduce":
            // Create arguments for the function call
            var reduceArgs = new KernelArguments
            {
                ["accountIds"] = accountHeadIds
            };

            var reduceResult = await kernel.InvokeAsync(kernel.Plugins["Banking"]["ReduceBalances"], reduceArgs);
            var reducedBalances = reduceResult.GetValue<IEnumerable<decimal?>>();
            decimal reducedBalance = 0;
            foreach (var balance in reducedBalances)
            {
                reducedBalance += balance ?? 0; // Handle null balances
            }
            Console.WriteLine($"Reduced Total Balance: {reducedBalance:C}");
            break;

        default:
            Console.WriteLine("Invalid choice. Please enter 'balance', 'statement', 'fanout', 'fanin', 'map', 'reduce', or 'exit'.");
            break;
    }
}