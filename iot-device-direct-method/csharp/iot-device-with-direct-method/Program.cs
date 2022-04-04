using Microsoft.Azure.Devices.Client;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SimulatedDevice
{
    /// <summary>
    /// This sample illustrates the very basics of a device app sending a heartbeat and receiving a command.
    /// For a more comprehensive device app sample, please see
    /// <see href="https://github.com/Azure-Samples/azure-iot-samples-csharp/tree/main/iot-hub/Samples/device/DeviceReconnectionSample"/>.
    /// </summary>
    internal class Program
    {
        private static bool s_resetCounter = false;
        private static double s_counter = 0;
        private static DeviceClient s_deviceClient;
        private static readonly TransportType s_transportType = TransportType.Mqtt;

        // The device connection string to authenticate the device with your IoT hub.
        // Using the Azure CLI:
        // az iot hub device-identity show-connection-string --hub-name {YourIoTHubName} --device-id MyDotnetDevice --output table
        private static string s_connectionString;

        private static TimeSpan s_heartbeatInterval;

        private static async Task Main(string[] args)
        {
            Console.WriteLine("IoT Hub - Simulated device.");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            IConfiguration config = builder.Build();
            s_connectionString = config["DeviceConnectionString"];
            s_heartbeatInterval = TimeSpan.TryParse(config["HeartbeatInterval"], out TimeSpan interval) ? interval : TimeSpan.FromSeconds(5);

            // Connect to the IoT hub using the MQTT protocol
            s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString, s_transportType);

            // Create a handler for the direct method call
            await s_deviceClient.SetMethodHandlerAsync("HeartbeatResponse", HeartbeatResponse, null);
            await s_deviceClient.SetMethodHandlerAsync("ResetCounter", ResetCounter, null);

            // Set up a condition to quit the sample
            Console.WriteLine("Press control-C to exit.");
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            // Run the telemetry loop
            await SendHeartbeat(cts.Token);

            // SendHeartbeat is designed to run until cancellation has been explicitly requested by Console.CancelKeyPress.
            // As a result, by the time the control reaches the call to close the device client, the cancellation token source would
            // have already had cancellation requested.
            // Hence, if you want to pass a cancellation token to any subsequent calls, a new token needs to be generated.
            // For device client APIs, you can also call them without a cancellation token, which will set a default
            // cancellation timeout of 4 minutes: https://github.com/Azure/azure-iot-sdk-csharp/blob/64f6e9f24371bc40ab3ec7a8b8accbfb537f0fe1/iothub/device/src/InternalClient.cs#L1922
            await s_deviceClient.CloseAsync();

            s_deviceClient.Dispose();
            Console.WriteLine("Device simulator finished.");
        }

        // Handle the direct method calls
        private static Task<MethodResponse> HeartbeatResponse(MethodRequest methodRequest, object userContext)
        {
            var data = Encoding.UTF8.GetString(methodRequest.Data);
            Console.WriteLine("Direct method called " + methodRequest.Name + " with data " + data);
            string result = "{ \"result\": \"Hello " + data + "\" }";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }

        private static Task<MethodResponse> ResetCounter(MethodRequest methodRequest, object userContext)
        {
            s_resetCounter = true;
            string result = "{ \"result\": \"Counter reset\" }";
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
        }

        // Async method to send heartbeat
        private static async Task SendHeartbeat(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                if (s_resetCounter)
                {
                    s_counter = 0;
                    s_resetCounter = false;
                }
                else
                {
                    s_counter += 1;
                }
                var utcDate = DateTime.UtcNow;

                // Create JSON message
                string messageBody = JsonSerializer.Serialize(
                    new
                    {
                        timestamp = utcDate.ToString("o"),
                        counter = s_counter,
                    });
                using var message = new Message(Encoding.ASCII.GetBytes(messageBody))
                {
                    ContentType = "application/json",
                    ContentEncoding = "utf-8",
                };

                // Send the telemetry message
                await s_deviceClient.SendEventAsync(message);
                Console.WriteLine($"{DateTime.Now} > Sending message: {messageBody}");

                try
                {
                    await Task.Delay(s_heartbeatInterval, ct);
                }
                catch (TaskCanceledException)
                {
                    // User canceled
                    return;
                }
            }
        }
    }
}