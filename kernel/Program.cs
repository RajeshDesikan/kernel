using kernel.Plugin;
using kernel.Service;
using kernel.Steps;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// Initialize services
var bankingService = new BankingService();
var bankingPlugin = new BankPlugin(bankingService);
var kernel = Kernel.CreateBuilder().Build();
kernel.ImportPluginFromObject(bankingPlugin, "Banking");

try
{
    // Ensure the appsettings.json file exists
    string configPath = "appsettings.json";
    if (!File.Exists(configPath))
    {
        Console.WriteLine($"Error: Config file not found at {Path.GetFullPath(configPath)}");
        return;
    }

    // Read the JSON file content
    string jsonContent = File.ReadAllText(configPath);

    // Print the exact content for debugging
    Console.WriteLine("Raw config content:");
    Console.WriteLine(jsonContent);

    // Make sure we're getting valid JSON
    if (string.IsNullOrWhiteSpace(jsonContent))
    {
        Console.WriteLine("Error: Config file is empty");
        return;
    }

    // Try to parse the JSON
    JObject config;
    try
    {
        config = JObject.Parse(jsonContent);
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"Error parsing JSON: {ex.Message}");
        return;
    }

    // Validate the expected structure
    if (config["BankingSettings"] == null)
    {
        Console.WriteLine("Error: BankingSettings section is missing");
        return;
    }

    var bankingSettings = config["BankingSettings"];
    var workflowArray = bankingSettings["Workflow"] as JArray;
    var accountIdsArray = bankingSettings["DefaultAccountIds"] as JArray;

    // Validate both required arrays exist
    if (accountIdsArray == null)
    {
        Console.WriteLine("Error: DefaultAccountIds array is missing in config");
        return;
    }

    if (workflowArray == null)
    {
        Console.WriteLine("Error: Workflow array is missing in config");
        return;
    }

    // Convert to strongly typed objects
    List<string> accountIds = accountIdsArray.ToObject<List<string>>();
    Console.WriteLine($"Loaded {accountIds.Count} account IDs: {string.Join(", ", accountIds)}");

    Console.WriteLine($"Loaded {workflowArray.Count} workflow steps");

    // Create the process step handler
    var processStep = new BankingProcessStep(bankingPlugin);

    // Start processing the workflow
    Console.WriteLine("Starting Banking Workflow...");

    if (workflowArray.Count == 0)
    {
        Console.WriteLine("No workflow steps defined");
        return;
    }

    // Get the first step
    var firstStep = workflowArray[0] as JObject;
    if (firstStep == null)
    {
        Console.WriteLine("First workflow step is not a valid object");
        return;
    }

    string currentStepName = firstStep["Step"]?.ToString();
    if (string.IsNullOrEmpty(currentStepName))
    {
        Console.WriteLine("First step has no name");
        return;
    }

    // Execute each step in the workflow
    while (!string.IsNullOrEmpty(currentStepName))
    {
        // Find the current step
        JObject currentStep = null;
        foreach (var item in workflowArray)
        {
            var step = item as JObject;
            if (step != null && step["Step"]?.ToString() == currentStepName)
            {
                currentStep = step;
                break;
            }
        }

        if (currentStep == null)
        {
            Console.WriteLine($"Step '{currentStepName}' not found in workflow");
            break;
        }

        // Get the operation to execute
        string operation = currentStep["Operation"]?.ToString();
        if (string.IsNullOrEmpty(operation))
        {
            Console.WriteLine($"No operation defined for step '{currentStepName}'");
            break;
        }

        Console.WriteLine($"Executing Step: {currentStepName} ({operation})");

        try
        {
            // Execute the operation
            var result = await processStep.ExecuteOperationAsync(
                operation,
                accountIds
            );
            Console.WriteLine($"Result for {currentStepName}: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing step {currentStepName}: {ex.Message}");
            break;
        }

        // Move to the next step
        string nextStep = currentStep["NextStep"]?.ToString();
        if (string.IsNullOrEmpty(nextStep))
        {
            Console.WriteLine("No next step defined. Ending workflow.");
            break;
        }
        else if (nextStep == "Complete")
        {
            Console.WriteLine("Reached workflow completion.");
            break;
        }
        else
        {
            currentStepName = nextStep;
        }
    }

    Console.WriteLine("Workflow complete.");
}
catch (Exception ex)
{
    Console.WriteLine($"Critical error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}