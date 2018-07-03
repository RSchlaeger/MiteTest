using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace MiteTest
{
    public static class ProjectTimes
    {
        [FunctionName("ProjectTimes")]
        public static async Task Run([TimerTrigger("0 * * * * *", RunOnStartup = true)]TimerInfo myTimer,
                               [Blob("cache/projects.json", FileAccess.Write)] CloudBlockBlob output,
                               TraceWriter log,
                               ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
                            .SetBasePath(context.FunctionAppDirectory)
                            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables()
                            .Build();

            var httpClient = new HttpClient();
            var tenant = config["Tenant"];
            var apiKey = config["ApiKey"];
            var baseUrl = config["BaseUrl"];
            var requestUrl = string.Format(baseUrl, tenant, "projects.json?", apiKey);
            var response = await httpClient.GetStringAsync(requestUrl);
            var projects = (JsonConvert.DeserializeObject<IEnumerable<ProjectWrapper>>(response)).Select(wrapper => wrapper.Project);

            var entryTasks = new List<Task<IEnumerable<TimeEntry>>>();
            foreach (var project in projects)
            {
                entryTasks.Add(GetTimeEntriesForProject(httpClient, tenant, apiKey, baseUrl, project.Id));
            }

            await Task.WhenAll(entryTasks);
            foreach (var task in entryTasks)
            {
                projects.Single(p => p.Id == task.Result.First().ProjectId).ConsumedBudget = task.Result.Sum(e => e.Minutes);
            }

            var outputJsonString = JsonConvert.SerializeObject(projects);
            output.Properties.ContentType = "application/json";
            await output.UploadTextAsync(outputJsonString);
        }

        private static async Task<IEnumerable<TimeEntry>> GetTimeEntriesForProject(HttpClient httpClient, string tenant, string apiKey, string baseUrl, int projectId)
        {
            var requestUrl = string.Format(baseUrl, tenant, $"time_entries.json?project_id={projectId}&", apiKey);
            var response = await httpClient.GetStringAsync(requestUrl);
            return (JsonConvert.DeserializeObject<IEnumerable<TimeEntryWrapper>>(response)).Select(wrapper => wrapper.TimeEntry);
        }
    }
}
