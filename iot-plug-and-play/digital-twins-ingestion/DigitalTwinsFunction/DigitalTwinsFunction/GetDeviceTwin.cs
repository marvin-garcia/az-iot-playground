using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Devices.Shared;

namespace DigitalTwinsFunction
{
    public class GetDeviceTwin
    {
        private static readonly string iotHubConnectionString = Environment.GetEnvironmentVariable("IoTHubConnection");

        [FunctionName("GetDeviceTwin")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                string deviceId = req.Query["deviceId"];

                try
                {
                    RegistryManager registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
                    Twin twin = await registryManager.GetTwinAsync(deviceId);

                    return new OkObjectResult(twin);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unable to retrieve twin for device {deviceId}. Exception: {ex}");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
