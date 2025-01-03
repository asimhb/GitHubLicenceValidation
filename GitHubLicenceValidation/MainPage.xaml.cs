using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Maui.Controls;

namespace GitHubLicenceValidation
{
    public partial class MainPage : ContentPage
    {
        private const string OpenAiApiKey = "your-api-key-here";
        private const string OpenAiApiUrl = "https://api.openai.com/v1/completions";

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnValidateLicenseClicked(object sender, EventArgs e)
        {
            var repositoryUrl = RepositoryUrlInput.Text;

            if (string.IsNullOrWhiteSpace(repositoryUrl))
            {
                ResponseLabel.Text = "Please enter a GitHub repository URL.";
                return;
            }

            ResponseLabel.Text = "Validating...";

            var response = await CheckLicenseWithChatGPT(repositoryUrl);
            ResponseLabel.Text = response ?? "Failed to get a response.";
        }

        private async Task<string> CheckLicenseWithChatGPT(string repositoryUrl)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {OpenAiApiKey}");

            var prompt = $@"
The user provided a GitHub repository URL: {repositoryUrl}.
Check the license of the repository and determine if it allows commercial use. 
If specific restrictions exist, explain them. Return a clear and concise response.";

            var requestBody = new
            {
                model = "text-davinci-003", // Or another model like `gpt-3.5-turbo`
                prompt = prompt,
                max_tokens = 300
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(OpenAiApiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(responseBody);
                var text = json.RootElement.GetProperty("choices")[0].GetProperty("text").GetString();

                return text?.Trim();
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
