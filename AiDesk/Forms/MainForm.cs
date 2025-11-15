using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AiDesk
{
    public partial class MainForm : Form
    {
        private readonly AppSettings _settings;
        private readonly HttpClient _httpClient;

        private readonly Random _random;
        private readonly List<string> _initialPrompts;

        public MainForm(AppSettings settings)
        {
            InitializeComponent();
            _settings = settings;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            _random = new Random();
            _initialPrompts = new List<string>
        {
        "Detail the plan for the first day of a space colony.",
    "Create your own superhero and describe their powers.",
    "Design an alternate world by changing a moment in history.",
    "Design an unusual invention and write a journal explaining how it works.",
    "You spend 24 hours on a mysterious island; describe the events you witness in detail.",
    "Build an imaginary city; describe its culture and technology.",
    "Imagine a robot gaining consciousness; write down its thoughts.",
    "You spend a day with your favorite movie character; write the dialogue.",
    "If you could change the world with a single rule, what would that rule be and what would the consequences be?",
    "Design your own mini-game; describe its rules and characters."
    };
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            string prompt = rtbPrompt.Text.Trim();
            if (string.IsNullOrEmpty(prompt) || _settings == null || !_settings.IsValid())
            {
                MessageBox.Show("Please enter a prompt and ensure API settings are valid.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnSend.Enabled = false;
                rtbPrompt.Clear();
                AppendToChat("You: \n", Color.Green, true);
                AppendToChat(prompt + "\n\n", Color.White, false);
                AppendToChat("AI is thinking...\n", Color.Gray, true);

                string responseText = await SendApiRequestAsync(prompt);

                AppendToChat("AI: \n", Color.Aqua, true);
                AppendFormattedText(responseText);
                AppendToChat("\n\n", Color.FromArgb(248, 250, 252), false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "API Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppendToChat($"Error: {ex.Message}\n", Color.Red, false);
            }
            finally
            {
                btnSend.Enabled = true;
                rtbPrompt.Focus();
            }
        }

        private async Task<string> SendApiRequestAsync(string prompt)
        {
            var requestData = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[] { new { text = prompt } }
                    }
                }
            };

            string jsonContent = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, _settings.ApiUrl);
            request.Content = content;
            request.Headers.Add("x-goog-api-key", _settings.ApiKey);

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();


                dynamic responseObj = JsonConvert.DeserializeObject(responseContent);

                var candidates = responseObj?.candidates;
                if (candidates == null || candidates.Count == 0)
                {
                    throw new Exception("API returned an empty or invalid response.");
                }

                var parts = candidates[0]?.content?.parts;
                if (parts == null || parts.Count == 0)
                {
                    throw new Exception("API response format is invalid (no parts).");
                }

                string resultText = parts[0]?.text;
                if (resultText == null)
                {
                    throw new Exception("API response format is invalid (no text).");
                }

                return resultText;
            }
            else
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error ({response.StatusCode}): {errorContent}");
            }
        }

        private void AppendToChat(string text, Color color, bool isBold)
        {
            if (rtbChat.InvokeRequired)
            {
                rtbChat.Invoke(new Action(() => AppendToChat(text, color, isBold)));
                return;
            }

            rtbChat.SelectionStart = rtbChat.TextLength;
            rtbChat.SelectionLength = 0;

            rtbChat.SelectionColor = color;
            rtbChat.SelectionFont = new Font(rtbChat.Font, isBold ? FontStyle.Bold : FontStyle.Regular);
            rtbChat.AppendText(text);
            rtbChat.SelectionColor = rtbChat.ForeColor;


            rtbChat.ScrollToCaret();
        }


        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _httpClient?.Dispose();
        }

        private void rtbPrompt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                btnSend_Click(sender, e);

                e.SuppressKeyPress = true;
            }
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            rtbPrompt.Focus();
        }

        private void AppendFormattedText(string text)
        {
            if (rtbChat.InvokeRequired)
            {
                rtbChat.Invoke(new Action(() => AppendFormattedText(text)));
                return;
            }

            string[] parts = text.Split(new[] { "**" }, StringSplitOptions.None);

            bool isBold = false;

            foreach (string part in parts)
            {

                if (!string.IsNullOrEmpty(part))
                {
                    AppendToChat(part, Color.FromArgb(248, 250, 252), isBold);
                }

                isBold = !isBold;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            rtbPrompt.Text = _initialPrompts[_random.Next(0, _initialPrompts.Count)];
            AppendToChat("AI: \n", Color.Aqua, true);
            string welcomeMessage = "Hello! I'm the **AiDesk Assistant**. How can I help you?\n\n";
            AppendFormattedText(welcomeMessage);

        }
    }
}
