using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiDesk
{
    public class AppSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-exp:generateContent";
        public bool RememberCredentials { get; set; } = true;
        public string Theme { get; set; } = "Dark";
        public int FontSize { get; set; } = 11;
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public static AppSettings GetDefault()
        {
            return new AppSettings();
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(ApiKey) &&
                   !string.IsNullOrWhiteSpace(ApiUrl) &&
                   Uri.IsWellFormedUriString(ApiUrl, UriKind.Absolute);
        }
    }
}
