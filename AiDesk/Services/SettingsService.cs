using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace AiDesk
{
    public class SettingsService
    {
        private readonly string settingsPath;
        private readonly byte[] entropy;

        public SettingsService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appDataPath, "AIDesk");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            settingsPath = Path.Combine(appFolder, "settings.dat");

            string entropySource = Environment.MachineName + Environment.UserName + "AIDesk2025";
            entropy = Encoding.UTF8.GetBytes(entropySource);
        }

        /// <summary>
        /// Ayarları güvenli şekilde kaydeder
        /// </summary>
        public bool SaveSettings(AppSettings settings)
        {
            try
            {
                string jsonData = JsonConvert.SerializeObject(settings, Formatting.Indented);
                byte[] dataBytes = Encoding.UTF8.GetBytes(jsonData);

                if (!string.IsNullOrEmpty(settings.ApiKey))
                {
                    var encryptedSettings = new
                    {
                        ApiKey = ProtectString(settings.ApiKey),
                        ApiUrl = settings.ApiUrl,
                        RememberCredentials = settings.RememberCredentials,
                        Theme = settings.Theme,
                        FontSize = settings.FontSize,
                        LastUpdated = settings.LastUpdated,
                        IsEncrypted = true
                    };

                    jsonData = JsonConvert.SerializeObject(encryptedSettings, Formatting.Indented);
                    dataBytes = Encoding.UTF8.GetBytes(jsonData);
                }

                File.WriteAllBytes(settingsPath, dataBytes);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ayarları güvenli şekilde yükler
        /// </summary>
        public AppSettings LoadSettings()
        {
            try
            {
                if (!File.Exists(settingsPath))
                {
                    return AppSettings.GetDefault();
                }

                byte[] dataBytes = File.ReadAllBytes(settingsPath);
                string jsonData = Encoding.UTF8.GetString(dataBytes);

                dynamic settingsObj = JsonConvert.DeserializeObject(jsonData);

                bool isEncrypted = settingsObj?.IsEncrypted ?? false;

                if (isEncrypted)
                {
                    return new AppSettings
                    {
                        ApiKey = UnprotectString(settingsObj.ApiKey?.ToString()),
                        ApiUrl = settingsObj.ApiUrl?.ToString() ?? AppSettings.GetDefault().ApiUrl,
                        RememberCredentials = settingsObj.RememberCredentials ?? true,
                        Theme = settingsObj.Theme?.ToString() ?? "Dark",
                        FontSize = settingsObj.FontSize ?? 11,
                        LastUpdated = settingsObj.LastUpdated ?? DateTime.Now
                    };
                }
                else
                {
                    var settings = JsonConvert.DeserializeObject<AppSettings>(jsonData);
                    return settings ?? AppSettings.GetDefault();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
                return AppSettings.GetDefault();
            }
        }

        /// <summary>
        /// Ayarları sil
        /// </summary>
        public bool DeleteSettings()
        {
            try
            {
                if (File.Exists(settingsPath))
                {
                    File.Delete(settingsPath);
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error while deleting settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ayarlar dosyasının var olup olmadığını kontrol eder
        /// </summary>
        public bool SettingsExist()
        {
            return File.Exists(settingsPath);
        }

        /// <summary>
        /// String'i şifreler
        /// </summary>
        private string ProtectString(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = ProtectedData.Protect(plainBytes, entropy, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encryptedBytes);
            }
            catch
            {
                return plainText;
            }
        }

        /// <summary>
        /// String'i çözer
        /// </summary>
        private string UnprotectString(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch
            {
                return encryptedText;
            }
        }

        /// <summary>
        /// Ayar dosyasının yolunu döndürür
        /// </summary>
        public string GetSettingsPath()
        {
            return settingsPath;
        }
    }
}
