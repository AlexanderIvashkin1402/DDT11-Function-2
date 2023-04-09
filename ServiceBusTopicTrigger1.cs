using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Company.Function;

public class ServiceBusTopicTrigger1
{
    private readonly ILogger<ServiceBusTopicTrigger1> _logger;
    private static readonly HttpClient httpClient = new HttpClient();

    public ServiceBusTopicTrigger1(ILogger<ServiceBusTopicTrigger1> log)
    {
        _logger = log;
    }

    [FunctionName("ServiceBusTopicTrigger1")]
    public async Task Run([ServiceBusTrigger("oddeven", "S1", Connection = "ddt11sb1_SERVICEBUS")]string mySbMsg)
    {
        _logger.LogInformation($"C# ServiceBus topic trigger function processed message: {mySbMsg}");
        Console.WriteLine($"C# ServiceBus topic trigger function processed message: {mySbMsg}");

        try
        {
            var data = JsonConvert.DeserializeObject<dynamic>(mySbMsg);
            int number = data!.numberItem;
            Console.WriteLine($"---> The number is: {number.ToString()}");

            if (number % 2 == 0)
            {
                Console.WriteLine("---> The number is EVEN");
                await PostToEndPoint(number, "https://ddt11aivapiapp.azurewebsites.net/api/numbers");
            }
            else
            {
                Console.WriteLine("---> The number is ODD");
                await PostToEndPoint(number, "http://aivddt11numberapi.fsggdrfweve7gme2.eastus.azurecontainer.io/api/numbers");
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Could not post to endpoints: {ex.Message}");
        }
    }

    async Task PostToEndPoint(int num, string url)
    {
        string json = "{\"numberItem\": " + num.ToString() + "}";

        Console.WriteLine($"---> The json to send is: {json}");

        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(url, content);

        var responseMessage = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"----> Response message: {responseMessage}");
    }
}
