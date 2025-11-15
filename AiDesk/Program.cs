using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AiDesk
{
    internal static class Program
    {
        /// <summary>
        /// Uygulamanın ana girdi noktası.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                var settingsService = new SettingsService();

                var settings = settingsService.LoadSettings();

                if (!settings.IsValid() || !settings.RememberCredentials)
                {
                    using (var configForm = new ApiConfigForm())
                    {
                        if (!string.IsNullOrEmpty(settings.ApiKey))
                        {
                            configForm.LoadApiSettings(settings.ApiKey, settings.ApiUrl);
                        }

                        var result = configForm.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            settings.ApiKey = configForm.ApiKey;
                            settings.ApiUrl = configForm.ApiUrl;
                            settings.RememberCredentials = configForm.RememberCredentials;
                            settings.LastUpdated = DateTime.Now;

                            if (settings.RememberCredentials)
                            {
                                bool saved = settingsService.SaveSettings(settings);
                                if (!saved)
                                {
                                    MessageBox.Show("Settings could not be saved, but the app will work for this session.",
                                        "⚠️ Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                Application.Run(new MainForm(settings));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while starting the application:\n\n{ex.Message}",
                    "❌ Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
