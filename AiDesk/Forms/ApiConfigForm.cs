using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AiDesk
{
    public partial class ApiConfigForm : Form
    {
        private readonly Color primaryDark = Color.FromArgb(15, 23, 42);
        private readonly Color secondaryDark = Color.FromArgb(30, 41, 59);
        private readonly Color accentDark = Color.FromArgb(51, 65, 85);
        private readonly Color textLight = Color.FromArgb(248, 250, 252);
        private readonly Color buttonPrimary = Color.FromArgb(59, 130, 246);
        private readonly Color buttonSuccess = Color.FromArgb(34, 197, 94);
        private readonly Color buttonDanger = Color.FromArgb(239, 68, 68);
        private readonly Color inputBg = Color.FromArgb(51, 65, 85);
        private HttpClient httpClient;
        public string ApiKey => txtApiKey.Text.Trim();
        public string ApiUrl => txtApiUrl.Text.Trim();
        public bool RememberCredentials => chbRemember.Checked;

        public ApiConfigForm()
        {
            InitializeComponent();
            httpClient = new HttpClient();
        }

        private void ApiConfigForm_Load(object sender, EventArgs e)
        {
            lblStatus.Text = "";
        }

        private void lblLink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://makersuite.google.com/app/apikey",
                UseShellExecute = true
            });
        }

        private void lblLink_MouseEnter(object sender, EventArgs e)
        {
            lblLink.ForeColor = Color.White;
        }

        private void lblLink_MouseLeave(object sender, EventArgs e)
        {
            lblLink.ForeColor = Color.FromArgb(192, 255, 255);
        }
        private void lblGithub_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/yigityasar",
                UseShellExecute = true
            });
        }

        private void lblGithub_MouseEnter(object sender, EventArgs e)
        {
            lblGithub.ForeColor = Color.White;
        }

        private void lblGithub_MouseLeave(object sender, EventArgs e)
        {
            lblGithub.ForeColor = Color.FromArgb(192, 255, 255);
        }

        private void txtApiKey_TextChanged(object sender, EventArgs e)
        {
            bool hasApiKey = !string.IsNullOrWhiteSpace(txtApiKey.Text);
            btnTest.Enabled = hasApiKey;
            if(hasApiKey)
            {
                lblStatus.Text = "";
                lblStatus.ForeColor = Color.FromArgb(156, 163, 175);
            }
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtApiKey.Text))
            {
                ShowStatus("❌ Please enter your API Key!", buttonDanger);
                return;
            }

            await TestApiConnection();
        }

        private async Task TestApiConnection()
        {
            try
            {
                btnTest.Enabled = false;    btnSave.Enabled = false;
                progressBar.Visible = true;
                ShowStatus("🔄 \r\nTesting the connection..", Color.FromArgb(251, 191, 36));

                var testResult = await SendTestMessage();

                if (testResult)
                {
                    ShowStatus("✅ Connection successful! API is working.", buttonSuccess);
                    btnSave.Enabled = true;
                }
                else
                {
                    ShowStatus("❌ Connection failed! Check your API Key or URL.", buttonDanger);
                }
            }
            catch(Exception ex) 
            {
                ShowStatus($"❌ failed: {ex.Message}", buttonDanger);
            }
            finally
            {
                btnTest.Enabled = true;
                progressBar.Visible = false;
            }
        }

        private async Task<bool> SendTestMessage()
        {
            try
            {
                var requestData = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = "Hello! This is a test message. Could you please give me a brief reply?" }
                            }
                        }
                    }
                };

                string jsonContent = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, txtApiUrl.Text);
                request.Content = content;
                request.Headers.Add("x-goog-api-key", txtApiKey.Text);

                httpClient.Timeout = TimeSpan.FromSeconds(10);

                HttpResponseMessage response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();

                    var responseObj = JsonConvert.DeserializeObject(responseContent);
                    return responseObj != null;
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Error ({response.StatusCode}): {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Internet connection error: " + ex.Message);
            }
            catch (TaskCanceledException)
            {
                throw new Exception("The request timed out. Check your internet connection.");
            }
            catch (JsonException)
            {
                throw new Exception("API response format is invalid. Check the URL.");
            }
        }

        private void ShowStatus(string message, Color color)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtApiKey.Text))
            {
                MessageBox.Show("Please enter your API Key!", "⚠️ Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtApiUrl.Text))
            {
                MessageBox.Show("Please enter API URL!", "⚠️ Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void ApiConfigForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            httpClient?.Dispose();
        }

        public void LoadApiSettings(string apiKey, string apiUrl)
        {
            if (!string.IsNullOrEmpty(apiKey))
            {
                txtApiKey.Text = apiKey;
            }

            if (!string.IsNullOrEmpty(apiUrl))
            {
                txtApiUrl.Text = apiUrl;
            }
        }
    }
}
