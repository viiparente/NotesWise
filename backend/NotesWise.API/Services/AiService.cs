using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NotesWise.API.Services.Models;

namespace NotesWise.API.Services
{
    public class AiService : IAiService
    {
        readonly HttpClient _httpClient;
        readonly string _openAiKey;
        readonly string _geminiApiKey;
        // readonly JsonSerializerOptions _jsonOptions;
        public AiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _openAiKey = configuration["OpenAi:ApiKey"] ?? "";
            _geminiApiKey = configuration["Gemini:ApiKey"] ?? "";
        }

        public async Task<string> GenerateSummaryAsync(string content)
        {
            try
            {
                if (!string.IsNullOrEmpty(_geminiApiKey) || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GOOGLE_API_KEY")))
                {
                    return await GenerateSummaryWithGeminiAsync(content);
                }
                else if (!string.IsNullOrEmpty(_openAiKey) || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENAI_API_KEY")))
                {
                    return await GenerateSummaryWithOpenAIAsync(content);
                }
                else
                {
                    throw new InvalidOperationException("No AI provider configured.");
                }
            }
            catch (Exception ex)
            {
                return $"Error generating summary: {ex.Message}";
            }
        }

        public async Task<string> GenerateSummaryWithGeminiAsync(string content)
        { 
            var request = new
            {
                contents = new[] {
                    new {
                        parts = new[]{
                            new {
                                text = $"Resuma este texto de forma concisa: {content}"
                            }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(request);

            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent")
            {
                Content = httpContent
            };

            httpRequest.Headers.Add("x-goog-api-key", $"{_geminiApiKey}");

            var response = await _httpClient.SendAsync(httpRequest);

            var responseText = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseText);

            var openAiResponse = JsonSerializer.Deserialize<GeminiTextResponse>(responseText);

            var summary = openAiResponse.Candidates.First().Content.Parts.First().Text;

            return summary;
        } 

        public async Task<string> GenerateSummaryWithOpenAIAsync(string content)
        {

            var request = new OpenAiRequest
            {
                model = "gpt-4o-mini",
                input = $"Resuma este texto de forma concisa: {content}"
            };

            var json = JsonSerializer.Serialize(request);

            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses")
            {
                Content = httpContent
            };

            httpRequest.Headers.Add("Authorization", $"Bearer {_openAiKey}");

            var response = await _httpClient.SendAsync(httpRequest);

            var responseText = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseText);

            var openAiResponse = JsonSerializer.Deserialize<OpenAiResponse>(responseText);

            var summary = openAiResponse.output.First().content.First().text;

            return summary;
        }
    }
}