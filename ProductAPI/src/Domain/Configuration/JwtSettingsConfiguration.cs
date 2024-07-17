using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Configuration
{
    public class JwtSettingsConfiguration
    {
        public string Issuer { get; set; }
        public bool IsActive { get; set; }
        public string Audience { get; set; }
        public string Key { get; set; }
        public TimeSpan TokenLifetime { get; set; }
        public List<string> Roles { get; set; }
    }
}
