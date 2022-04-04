using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;

namespace iot_hub_job_scheduler
{
    internal class Program
    {
        private static JobClient s_jobClient;
        private static string s_connectionString;
        private static string[] s_deviceIds;
        private static string s_directMethodName;

        static async Task Main(string[] args)
        {
            Console.WriteLine("IoT Hub - Simulated device.");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            IConfiguration config = builder.Build();
            s_connectionString = config["IoTHubConnectionString"];
            s_deviceIds = config["DeviceIds"].Split(',');
            s_directMethodName = config["DirectMethodName"];

            Console.WriteLine("Press ENTER to start running jobs.");
            Console.ReadLine();

            s_jobClient = JobClient.CreateFromConnectionString(s_connectionString);

            string methodJobId = Guid.NewGuid().ToString();
            string queryCondition = $"DeviceId IN {JsonConvert.SerializeObject(s_deviceIds)}".Replace('"', '\'');
            await StartMethodJob(s_directMethodName, methodJobId, queryCondition);
            MonitorJob(methodJobId).Wait();
            
            Console.WriteLine("Press ENTER to exit.");
            Console.ReadLine();
        }

        public static async Task MonitorJob(string jobId)
        {
            try
            {
                JobResponse result;
                do
                {
                    result = await s_jobClient.GetJobAsync(jobId);
                    Console.WriteLine("Job Status : " + result.Status.ToString());
                    Thread.Sleep(2000);
                }
                while ((result.Status != JobStatus.Completed) && (result.Status != JobStatus.Failed));

                Console.WriteLine($"Job results: {JsonConvert.SerializeObject(result)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static async Task StartMethodJob(string directMethodName, string jobId, string queryCondition)
        {
            try
            {
                CloudToDeviceMethod directMethod = new CloudToDeviceMethod(
                    directMethodName,
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(5));

                JobResponse result = await s_jobClient.ScheduleDeviceMethodAsync(
                    jobId,
                    queryCondition,
                    directMethod,
                    DateTime.UtcNow,
                    (long)TimeSpan.FromMinutes(2).TotalSeconds);

                Console.WriteLine($"Started Method Job. Status: {result.Status}. Message: {result.StatusMessage}");
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.ToString()); 
            }
        }
    }
}
