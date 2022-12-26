using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host;


namespace Company.Function
{
    public static class TestSayBye
 {

 [FunctionName(nameof(SayBye))]
        public static string SayBye([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying bye to {name}.");
            return $"Bye {name}!";
        }
 }
    
}
