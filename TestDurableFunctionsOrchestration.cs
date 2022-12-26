using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System;

namespace Company.Function
{
    public static class TestDurableFunctionsOrchestration
    {

        //Orchestrator Function
        [FunctionName("TestDurableFunctionsOrchestrator")]
        public static async Task<List<string>> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>(nameof(TestSayBye.SayBye), "Manas"));


            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Tokyo"));


            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Seattle"));


            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "London"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]

            Uri url = context.GetInput<Uri>();
            // Makes an HTTP GET request to the specified endpoint
            DurableHttpResponse response = await context.CallHttpAsync(HttpMethod.Get, url);
            log.LogInformation($"Response code ==> {response.StatusCode}");
            log.LogInformation($"Got response from URL ==> {response.Content}");

            return outputs;
        }


        //Activity function
        [FunctionName(nameof(SayHello))]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }



        //Starter function
        [FunctionName("TestDurableFunctionsHttpStarter")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            Uri uri1 = new Uri("http://nsocloudmes.centralindia.cloudapp.azure.com/product-manage/get-product/all");
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("TestDurableFunctionsOrchestrator", uri1);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}