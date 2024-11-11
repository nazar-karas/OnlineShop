using Application.Common;
using Domain.Documents;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace Application.Requests.Queries
{
    public class GetOrdersQuery : QueryParams, IRequest<IEnumerable<Order>>
    {
        public string SortBy { get; set; } = "firstName";
        public string Order { get; set; } = "asc";
        public string PhoneNumber { get; set; } = null;
        public string FirstName { get; set; } = null;
        public string LastName { get; set; } = null;
        public static bool TryParse(string? value, out GetOrdersQuery? result)
        {
            if (string.IsNullOrEmpty(value))
            {
                result = new GetOrdersQuery()
                {
                    Page = 1,
                    Count = 10
                };

                return true;
            }

            var dictionary = HttpUtility.ParseQueryString(value);
            string json = JsonSerializer.Serialize(dictionary);
            result = JsonSerializer.Deserialize<GetOrdersQuery>(json);
            return true;
        }
    }
}
