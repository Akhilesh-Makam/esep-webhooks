using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook
{
    public class Function
    {
        private static readonly HttpClient client = new HttpClient();

        public class InputModel
        {
            public IssueModel Issue { get; set; }

            public class IssueModel
            {
                public string HtmlUrl { get; set; }
            }
        }

        /// <summary>
        /// A simple function that processes an input and sends a message to Slack
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<string> FunctionHandler(string input, ILambdaContext context)
        {
            try
            {
                var json = JsonConvert.DeserializeObject<InputModel>(input);

                string payload = $"{{\"text\": \"Issue Created: {json.Issue.HtmlUrl}\"}}";
                string slackUrl = Environment.GetEnvironmentVariable("SLACK_URL") ?? throw new InvalidOperationException("SLACK_URL not configured");

                var webRequest = new HttpRequestMessage(HttpMethod.Post, slackUrl)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                };

                var response = await client.SendAsync(webRequest);
                response.EnsureSuccessStatusCode();

                using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
                return await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Error: {ex.Message}");
                throw;
            }
        }
    }
}
