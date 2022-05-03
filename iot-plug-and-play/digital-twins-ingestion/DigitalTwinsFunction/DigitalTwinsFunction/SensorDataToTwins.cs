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
        private static readonly string[] simDevices = Environment.GetEnvironmentVariable("SimulatedDeviceIds").Split(',');
        private static readonly int minSimThreshold = 7;
        private static readonly int maxSimThreshold = 13;
        private static readonly Random random = new Random();

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
                    DateTime enqueuedTime = (DateTime)deviceMessage["systemProperties"]["iothub-enqueuedtime"];
                    if (enqueuedTime < DateTime.UtcNow.Subtract(new TimeSpan(0, 30, 0)))
                    {
                        log.LogWarning("Finishing execution because message is too old");
                        return;
                    }
                    
                    string deviceBody = (string)deviceMessage["body"];
                    JObject body = JsonConvert.DeserializeObject<JObject>(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(deviceBody)));

                    #region update device twin properties
                    // Comment if PnP uses telemetry
                    var updateTwinData = new JsonPatchDocument();
                    foreach (var property in body.Properties())
                    {
                        log.LogInformation($"Device: {deviceId} | {property.Name}: {body.GetValue(property.Name)}");

                        // Comment if PnP uses telemetry
                        updateTwinData.AppendReplace($"/{property.Name}", body.GetValue(property.Name).Value<double>());

                        // Uncomment if PnP uses telemetry
                        //await client.PublishTelemetryAsync(deviceId, Guid.NewGuid().ToString(), $"{{ \"{property.Name}\": {body.GetValue(property.Name).Value<double>()} }}");
                    }

                    // Comment if PnP uses telemetry
                    await client.UpdateDigitalTwinAsync(deviceId, updateTwinData);
                    #endregion

                    #region update properties for simulated devices (if any)
                    foreach (var device in simDevices)
                    {
                        updateTwinData = new JsonPatchDocument();
                        foreach (var property in body.Properties())
                        {
                            log.LogInformation($"Sim device: {device} | {property.Name}: {body.GetValue(property.Name)}");

                            // Comment if PnP uses telemetry
                            double r = random.Next(minSimThreshold, maxSimThreshold);
                            double propertyValue = body.GetValue(property.Name).Value<double>() * r / 10;
                            updateTwinData.AppendReplace($"/{property.Name}", propertyValue);

                            // Uncomment if PnP uses telemetry
                            //await client.PublishTelemetryAsync(deviceId, Guid.NewGuid().ToString(), $"{{ \"{property.Name}\": {propertyValue} }}");
                        }

                        // Comment if PnP uses telemetry
                        var response = await client.UpdateDigitalTwinAsync(device, updateTwinData);
                        log.LogInformation($"Response: {response.Status}. {response.Content}");
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}