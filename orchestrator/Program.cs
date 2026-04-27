using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

class Program{

    // Update with real ECS application URL
    private static readonly string HealthUrl = "https://cl-efee49abace3432f8da171f88d2d9443.ecs.us-east-2.on.aws/health";

    static async Task Main(string[] args)
    {
        // Starting point for the script
        Console.WriteLine("Starting deployment orchestrator...");

        // Triggering a new ECS deployment so the service pulls the latest image
        RunCommand("aws ecs update-service --cluster default --service cloud-pipeline-da4a --force-new-deployment --query 'service.{name:serviceName,status:status,running:runningCount,pending:pendingCount}' --output table");

        Console.WriteLine("Waiting for ECS service to start updating...");
        await Task.Delay(10000);       

        // Hit the health endpoint to confirm it's running
        Console.WriteLine("Checking service health...");


        try
        {   
            using var client = new HttpClient();
            var response = await client.GetStringAsync(HealthUrl);

            // Check if the API response actually says it's healthy
            if (response.Contains("ok") || response.Contains("healthy"))
            {
                // API returned expected value, then deployment worked
                Console.WriteLine("Deployment successful.");
                Console.WriteLine(response);
            }
            else
            {
                // API responded but not with expected health signal
                Console.WriteLine("Service responded, but something looks off.");
                Console.WriteLine(response);
            }

        }
        catch
        {   
            // If requests fail the service probably didn't start correctly
            Console.WriteLine("Deployment failed. Health check did not respond.");

        }

    }

    static void RunCommand(string command){

        // Runs AWS CLI commands from C# so tool can control deployment steps
        var process = new Process();

        // Use bash so we can run full commands like in the terminal
        process.StartInfo.FileName = "/bin/bash";
        process.StartInfo.Arguments = $"-c \"{command}\"";

        // Capture output so we can print it
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;

        process.Start();
        
        // Read and print whatever the command outputs
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();
        
        // If the command produced normal output then print it so we see what happend.
        if (!string.IsNullOrWhiteSpace(output))
            Console.WriteLine(output);
        
        // If there were any errors, print them too (debug helper)
        if (!string.IsNullOrWhiteSpace(error))
            Console.WriteLine(error);
    }
}