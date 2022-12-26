using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading;
using Azure.Messaging.EventGrid;
using Azure;
using Azure.Core.Serialization;
using System.Text.Json;

namespace Company.Function
{
    public static class DurableFunctionChainingPattern
    {

        //Starter function
        [FunctionName("FunctionChainingHttpStarter")]
        public static async Task<IActionResult> HttpStart([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "greetings")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter, ILogger log)
        {

            // string name = req.Query["name"];

            // log.LogInformation($" Input value ===>{name}");

            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("FunctionChainingOrchestrator");

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);

        }


        //Orchestrator Function
        [FunctionName("FunctionChainingOrchestrator")]
        public static async Task<string> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {

            // string name1 = context.GetInput<string>();

            // log.LogInformation($" Name in orchestrator ===>{name1}");

            EventGridPublisherClient client = new EventGridPublisherClient(
                new Uri("https://nso-cloudmes-event-grid-topic.centralindia-1.eventgrid.azure.net/api/events"), // topic endpoint
                new AzureKeyCredential("Z7oqJbz1b9hcYwRLZhKNMv7YW612yjAo7Sqkb0xsK0U=")); // access key

            // Example of a custom ObjectSerializer used to serialize the event payload to JSON
            var myCustomDataSerializer = new JsonObjectSerializer(
                new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            // Add EventGridEvents to a list to publish to the topic
            List<EventGridEvent> eventsList = new List<EventGridEvent>
{
    // EventGridEvent with custom model serialized to JSON
    new EventGridEvent(
        "ExampleEventSubject",
        "Example.EventType",
        "1.0",
        //new CustomModel() { A = 5, B = true }
        "This my event data 1."
        ),

    // EventGridEvent with custom model serialized to JSON using a custom serializer
    new EventGridEvent(
        "ExampleEventSubject",
        "Example.EventType",
        "1.0",
        //myCustomDataSerializer.Serialize(new CustomModel() { A = 5, B = true })
        "This my event data 2."
        ),
};

            // Send the events
            await client.SendEventsAsync(eventsList);





            string name = "Manas";

            string responseFromF1 = await context.CallActivityAsync<string>("Function1", name);

            string responseFromF2 = await context.CallActivityAsync<string>("Function2", responseFromF1);

            string responseFromF3 = await context.CallActivityAsync<string>("Function3", responseFromF2);

            string responseFromF4 = await context.CallActivityAsync<string>("Function4", responseFromF3);

            log.LogInformation($"Final output message from function 4 ===> {responseFromF4}.");

            return responseFromF4;
        }


        //Activity function
        [FunctionName(nameof(Function1))]
        public static string Function1([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            Thread.Sleep(15000);
            return $"Hello {name}!";
        }

        [FunctionName(nameof(Function2))]
        public static string Function2([ActivityTrigger] string inputMsg, ILogger log)
        {
            log.LogInformation($"Input Message from function 2 ===> {inputMsg}.");
            Thread.Sleep(20000);
            return $"{inputMsg} Good morning.";
        }

        [FunctionName(nameof(Function3))]
        public static string Function3([ActivityTrigger] string inputMsg, ILogger log)
        {
            log.LogInformation($"Input Message from function 3 ===> {inputMsg}.");
            Thread.Sleep(15000);
            return $"{inputMsg} how are you?";
        }

        [FunctionName(nameof(Function4))]
        public static string Function4([ActivityTrigger] string inputMsg, ILogger log)
        {
            log.LogInformation($"Input Message from function 4 ===> {inputMsg}.");
            Thread.Sleep(10000);
            return $"{inputMsg} Congratulations!!!";
        }

    }
}