using System;
using System.Net.Http;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DigitalTwinsFunction
{
    public class IoTHubtoTwins
    {
        private static readonly string adtInstanceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");
        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly string[] deviceProperties = new[] 
        { 
            "Temperature",
            "Pressure",
            "Humidity",
            "magnetometerX",
            "magnetometerY",
            "magnetometerZ",
            "accelerometerX",
            "accelerometerY",
            "accelerometerZ",
            "gyroscopeX",
            "gyroscopeY",
            "gyroscopeZ" 
        };

        [FunctionName("IoTHubtoTwins")]
        public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent, ILogger log)
        {
            if (adtInstanceUrl == null) log.LogError("Application setting \"ADT_SERVICE_URL\" not set");

            try
            {
                var credentials = new DefaultAzureCredential();
                DigitalTwinsClient client = new DigitalTwinsClient(
                    new Uri(adtInstanceUrl),
                    credentials,
                    new DigitalTwinsClientOptions 
                    { 
                        Transport = new HttpClientTransport(httpClient) 
                    });

                if (eventGridEvent != null && eventGridEvent.Data != null)
                {
                    JObject deviceMessage = (JObject)JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());
                    string deviceId = (string)deviceMessage["systemProperties"]["iothub-connection-device-id"];
                    string deviceBody = (string)deviceMessage["body"];
                    JObject body = JsonConvert.DeserializeObject<JObject>(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(deviceBody)));

                    //var updateTwinData = new JsonPatchDocument();
                    foreach (var property in body.Properties())
                    {
                        log.LogInformation($"Device: {deviceId} | {property.Name}: {body.GetValue(property.Name)}");
                        //updateTwinData.AppendReplace($"/{deviceProperty}", propertyValue.Value<double>());

                        await client.PublishTelemetryAsync(deviceId, Guid.NewGuid().ToString(), $"{{ \"{property.Name}\": {body.GetValue(property.Name).Value<double>()} }}");
                    }

                    //await client.UpdateDigitalTwinAsync(deviceId, updateTwinData);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
