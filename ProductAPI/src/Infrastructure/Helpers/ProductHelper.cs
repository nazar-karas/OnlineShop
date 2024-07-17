using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    public class ProductHelper
    {
        public static string Convert<T>(List<T> items)
        {
            return items != null ? JsonConvert.SerializeObject(items) : null;
        }

        public static List<T> Convert<T>(string items)
        {
            return !items.IsNullOrEmpty() ? JsonConvert.DeserializeObject<List<T>>(items) : null;
        }
    }
}
