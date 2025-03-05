using kernel.Plugin;
using kernel.Service;
using kernel.Steps;
using Microsoft.SemanticKernel;


var bankingService = new BankingService(); // Replace with your actual IBankingService implementation
var bankingPlugin = new BankPlugin(bankingService);
var kernel = Kernel.CreateBuilder().Build();



kernel.ImportPluginFromObject(bankingPlugin, "Banking");



// Predefined Account IDs
List<string> accountHeadIds = new List<string>
        {
            "1001",
            "1002",
            "1003",
            "1004",
            "1005"
        };

var processStep = new BankingProcessStep(bankingPlugin);

while (true) 
{
    Console.WriteLine("\nEnter the operation type (e.g., balance, statement, fanout, fanin, map, reduce, or 'exit' to quit):");
    var operationType = Console.ReadLine()?.Trim().ToLower();

    if (operationType == "exit")
    {
        Console.WriteLine("Exiting...");
        break;
    }

    Console.WriteLine("Enter Account IDs (comma-separated, or press Enter to use predefined IDs):");
    var accountIdsInput = Console.ReadLine();
    var accountIds = string.IsNullOrEmpty(accountIdsInput)
        ? accountHeadIds
        : accountIdsInput.Split(",").Select(id => id.Trim()).ToList();

    try
    {
        // Call processStep with the operation type
        var result = await processStep.ExecuteOperationAsync(operationType, accountIds);
        Console.WriteLine("Operation Result:");
        Console.WriteLine(result);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}