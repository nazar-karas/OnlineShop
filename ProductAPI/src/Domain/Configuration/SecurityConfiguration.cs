using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Configuration
{
    public class SecurityConfiguration
    {
        public SecurityConfiguration()
        {
            JwtSettings = new JwtSettingsConfiguration();
        }
        public JwtSettingsConfiguration JwtSettings { get; set; }
        public TimeSpan TokenLifetime { get; set; }
    }
}
