using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

class Program{

    static async Task Main(string[] args)
    {
        // Starting point for the script
        Console.WriteLine("Starting deployment orchestrator...");

        // Clean up any previous run so the port is free and then hide it if there is no previous one
        RunCommand("docker rm -f cloud-pipeline-api 2>/dev/null || true");

        // Start the Python API container in the background
        RunCommand("docker run -d --name cloud-pipeline-api -p 5000:5000 cloud-pipeline");

        // Give the container a few seconds to boot up before checking it 
        await Task.Delay(3000);

        // Hit the health endpoint to confirm it's running
        Console.WriteLine("Checking service health...");


        try
        {   
            using var client = new HttpClient();
            var response = await client.GetStringAsync("http://localhost:5000/health");

            // Check if the API response actually says it's healthy
            if (response.Contains("ok"))
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

        // Runs shell commands (like docker) from C#
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