using Application.Requests.Queries;
using static MassTransit.ValidationResultExtensions;
using System.Text.Json;
using System.Web;

namespace Presentation.Utilities
{
    public class RequestUtility
    {
        public static T ParseQuery<T>(string queryString)
        {
            if (string.IsNullOrEmpty(queryString))
            {
                return default(T);
            }

            var input = HttpUtility.ParseQueryString(queryString);

            var dictionary = input.Cast<string>().ToDictionary(x => x, y => input[y]);

            string json = JsonSerializer.Serialize(dictionary);

            var result = JsonSerializer.Deserialize<T>(json);

            return result;
        }
    }
}
