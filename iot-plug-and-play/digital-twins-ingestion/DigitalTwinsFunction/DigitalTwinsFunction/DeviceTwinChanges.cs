using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core.Pipeline;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DigitalTwinsFunction
{
    public class DeviceTwinChanges
    {
        private static readonly string adtInstanceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");
        private static readonly HttpClient httpClient = new HttpClient();

        [FunctionName("DeviceTwinChanges")]
        public async Task Run([EventHubTrigger("%DeviceTwinHub%", Connection = "EventHubConnection")] EventData[] events, ILogger log)
        {
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

                var exceptions = new List<Exception>();

                foreach (EventData eventData in events)
                {
                    try
                    {
                        string deviceId = (string)eventData.SystemProperties["iothub-connection-device-id"];
                        string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                        JObject deviceTwinChange = JsonConvert.DeserializeObject<JObject>(messageBody);

                        var updateTwinData = new JsonPatchDocument();
                        foreach (var property in (JObject)deviceTwinChange["properties"]["reported"])
                        {
                            if (property.Key == "ledState")
                            {
                                log.LogInformation($"Device: {deviceId} | {property.Key}: {property.Value.Value<string>()}");
                                updateTwinData.AppendReplace($"/{property.Key}", property.Value.Value<Boolean>());
                            }
                        }

                        await client.UpdateDigitalTwinAsync(deviceId, updateTwinData);
                    }
                    catch (Exception e)
                    {
                        // We need to keep processing the rest of the batch - capture this exception and continue.
                        // Also, consider capturing details of the message that failed processing so it can be processed again later.
                        exceptions.Add(e);
                    }
                }

                // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

                if (exceptions.Count > 1)
                    throw new AggregateException(exceptions);

                if (exceptions.Count == 1)
                    throw exceptions.Single();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
