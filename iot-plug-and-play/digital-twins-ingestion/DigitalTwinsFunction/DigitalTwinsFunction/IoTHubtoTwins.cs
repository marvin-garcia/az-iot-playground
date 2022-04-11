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

        [FunctionName("IoTHubtoTwins")]
        public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent, ILogger log)
        {
            if (adtInstanceUrl == null) log.LogError("Application setting \"ADT_SERVICE_URL\" not set");

            try
            {
                // Authenticate with Digital Twins
                var cred = new ManagedIdentityCredential("https://digitaltwins.azure.net");
                var client = new DigitalTwinsClient(
                    new Uri(adtInstanceUrl),
                    cred,
                    new DigitalTwinsClientOptions { Transport = new HttpClientTransport(httpClient) });
                log.LogInformation($"ADT service client connection created.");

                if (eventGridEvent != null && eventGridEvent.Data != null)
                {
                    //log.LogInformation(eventGridEvent.Data.ToString());

                    JObject deviceMessage = (JObject)JsonConvert.DeserializeObject(eventGridEvent.Data.ToString());
                    string deviceId = (string)deviceMessage["systemProperties"]["iothub-connection-device-id"];
                    string deviceBody = (string)deviceMessage["body"];
                    JObject body = JsonConvert.DeserializeObject<JObject>(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(deviceBody)));
                    //log.LogInformation($"DeviceId: {deviceId}");
                    //log.LogInformation($"Encoded Body: {deviceBody}");
                    //log.LogInformation($"Decoded Body: {body}");

                    var updateTwinData = new JsonPatchDocument();

                    // Temperature
                    if (body.ContainsKey("temperature"))
                    {
                        var temperature = body["temperature"];
                        log.LogInformation($"Device:{deviceId} Temperature is:{temperature}");
                        updateTwinData.AppendReplace("/temperature", temperature.Value<double>());

                        await client.UpdateDigitalTwinAsync(deviceId, updateTwinData);
                    }

                    // Humidity
                    if (body.ContainsKey("humidity"))
                    {
                        var humidity = body["humidity"];
                        log.LogInformation($"Device:{deviceId} humidity is:{humidity}");
                        updateTwinData.AppendReplace("/humidity", humidity.Value<double>());

                        await client.UpdateDigitalTwinAsync(deviceId, updateTwinData);
                    }

                    // Pressure
                    if (body.ContainsKey("pressure"))
                    {
                        var pressure = body["pressure"];
                        log.LogInformation($"Device:{deviceId} pressure is:{pressure}");
                        updateTwinData.AppendReplace("/pressure", pressure.Value<double>());

                        await client.UpdateDigitalTwinAsync(deviceId, updateTwinData);
                    }

                    // magnetometerX
                    if (body.ContainsKey("magnetometerX"))
                    {
                        var magnetometerX = body["magnetometerX"];
                        log.LogInformation($"Device:{deviceId} X magnetometer is:{magnetometerX}");
                        updateTwinData.AppendReplace("/magnetometerX", magnetometerX.Value<double>());

                        await client.UpdateDigitalTwinAsync(deviceId, updateTwinData);
                    }

                    // magnetometerY
                    if (body.ContainsKey("magnetometerY"))
                    {
                        var magnetometerY = body["magnetometerY"];
                        log.LogInformation($"Device:{deviceId} Y magnetometer is:{magnetometerY}");
                        updateTwinData.AppendReplace("/magnetometerY", magnetometerY.Value<double>());

                        await client.UpdateDigitalTwinAsync(deviceId, updateTwinData);
                    }

                    // magnetometerZ
                    if (body.ContainsKey("magnetometerZ"))
                    {
                        var magnetometerZ = body["magnetometerZ"];
                        log.LogInformation($"Device:{deviceId} Z magnetometer is:{magnetometerZ}");
                        updateTwinData.AppendReplace("/magnetometerZ", magnetometerZ.Value<double>());

                        await client.UpdateDigitalTwinAsync(deviceId, updateTwinData);
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Error in ingest function: {ex.Message}");
                throw ex;
            }
        }
    }
}
