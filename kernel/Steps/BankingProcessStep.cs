using kernel.Events;
using kernel.Plugin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kernel.Steps
{
    public class BankingProcessStep
    {
        private readonly BankPlugin _plugin;

        public BankingProcessStep(BankPlugin plugin)
        {
            _plugin = plugin;
        }

        public async Task<string> ExecuteOperationAsync(string operationType, List<string> accountIds, int months = 0)
        {
            switch (operationType.ToLower())
            {
                case "balance":
                    return await _plugin.GetBalance(accountIds.FirstOrDefault());
                case "statement":
                    return await _plugin.GetStatement(accountIds.FirstOrDefault(), months);
                case "fanout": //A process where a task spawns multiple sub-tasks that can execute concurrently (e.g., fetching data for multiple accounts).
                    var fanOutResults = await _plugin.FanOut(accountIds);
                    return string.Join("\n", fanOutResults);
                case "fanin":   //Collecting and aggregating the results of multiple concurrent tasks into a single result (e.g., combining the results of fetched data).
                    var totalBalance = await _plugin.FanIn(accountIds);
                    return $"Total Balance: {totalBalance:C}";
                case "map":  //Applying a transformation to a collection of data (e.g., invoking a method for each account ID).
                    var mappedData = await _plugin.Map(accountIds);
                    return string.Join("\n", mappedData);
                case "reduce"://Aggregating transformed data into a single result or output (e.g., summarizing balances).
                    var reducedResult = await _plugin.Reduce(accountIds);
                    return reducedResult;
                default:
                    throw new ArgumentException($"Unsupported operation: {operationType}");
            }
        }
    }
}
