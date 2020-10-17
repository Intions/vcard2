using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using Microsoft.Azure;
using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Linq;

namespace Intions.vCard2
{
    public static class QueryAPI
    {
        [FunctionName("QueryAPI")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string querystring = req.Query["Query"];

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;
            /*
            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
            */

            var storageCredentials = new StorageCredentials("spintegratorstorage01", "N /ig5UBFyaWny2ofxIOO1MS7LGWktqJGFMlJUcfWg9zpZFiMw0yexfX/2x1G8bJT4FJXgH0GveCiCFMhF34SwQ==");

            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            var cloudTable = tableClient.GetTableReference("Submission");

            try
            {
                TableQuery<DynamicTableEntity> DataTableQuery = new TableQuery<DynamicTableEntity>();
                if (!string.IsNullOrEmpty(querystring))
                {
                    DataTableQuery = new TableQuery<DynamicTableEntity>().Where(querystring);
                }
                IEnumerable<DynamicTableEntity> IDataList = cloudTable.ExecuteQuery(DataTableQuery);
                List<DynamicTableEntity> DataList =  IDataList.ToList<DynamicTableEntity>();

                List<IDictionary<string, object>> expando = new List<IDictionary<string, object>>();

                DataList.ForEach((item) =>
                {
                    Dictionary<string, object> output = new Dictionary<string, object>();
                    List<string> keys = item.Properties.Keys.ToList<string>();
                    foreach (var key in keys)
                    {
                        output[key] = item.Properties[key].ToString();
                    }
                    expando.Add(output);
                });

                return  new OkObjectResult(expando);
            }
            catch (Exception ExceptionObj)
            {
                return new BadRequestObjectResult(ExceptionObj.Message);
                
            }
        }
 
    }
}
