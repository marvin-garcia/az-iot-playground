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
    //public class IoTHubtoTwins
    //{
    //    private static readonly string adtInstanceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");
    //    private static readonly HttpClient httpClient = new HttpClient();
    //    private static readonly string[] simDevices = Environment.GetEnvironmentVariable("SimulatedDeviceIds").Split(',');
    //    private static readonly int minSimThreshold = 3;
    //    private static readonly int maxSimThreshold = 15;
    //    private static readonly Random random = new Random();

    //    [FunctionName("IoTHubtoTwins")]
    //    public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent, ILogger log)
    //    {
    //        if (adtInstanceUrl == null) log.LogError("Application setting \"ADT_SERVICE_URL\" not set");

    //        try
    //        {
    //            var credentials = new DefaultAzureCredential();
    //            DigitalTwinsClient client = new DigitalTwinsClient(
    //                new Uri(adtInstanceUrl),
    //                credentials,
    //                new DigitalTwinsClientOptions 
    //                { 
    //                    Transport = new HttpClientTransport(httpClient) 
    //                });

    //            if (eventGridEvent != null && eventGridEvent.Data != null)
    //            {
    //                JObject deviceMessage = (JObject)JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());
    //                string deviceId = (string)deviceMessage["systemProperties"]["iothub-connection-device-id"];
    //                DateTime enqueuedTime = (DateTime)deviceMessage["systemProperties"]["iothub-enqueuedtime"];
    //                if (enqueuedTime < DateTime.UtcNow.Subtract(new TimeSpan(0, 30, 0)))
    //                {
    //                    log.LogWarning("Finishing execution because message is too old");
    //                    return;
    //                }


    //                string deviceBody = JsonConvert.SerializeObject(deviceMessage["body"]);
    //                JObject body = JsonConvert.DeserializeObject<JObject>(deviceBody);
    //                log.LogInformation($"device body: {JsonConvert.SerializeObject(body)}");

    //                #region update device twin properties
    //                var updateTwinData = new JsonPatchDocument();
    //                foreach (var property in body.Properties())
    //                {
    //                    log.LogInformation($"Device: {deviceId}, {property.Name}: {JsonConvert.SerializeObject(body.GetValue(property.Name).Value<object>())}");

    //                    // handle location telemetry
    //                    if (property.Name == "location")
    //                    {
    //                        JObject coordinatesObject = body.GetValue(property.Name).Value<JObject>();
    //                        double lat = coordinatesObject.GetValue("lat").Value<double>();
    //                        double lon = coordinatesObject.GetValue("lon").Value<double>();
    //                        updateTwinData.AppendReplace($"/latitude", lat);
    //                        updateTwinData.AppendReplace($"/longitude", lon);
    //                    }
    //                    else if (property.Name == "state")
    //                        updateTwinData.AppendReplace($"/{property.Name}", body.GetValue(property.Name).Value<string>());
    //                    else
    //                        updateTwinData.AppendReplace($"/{property.Name}", body.GetValue(property.Name).Value<double>());
    //                }
                    
    //                await client.UpdateDigitalTwinAsync(deviceId, updateTwinData);
    //                #endregion
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            throw ex;
    //        }
    //    }
    //}
}
