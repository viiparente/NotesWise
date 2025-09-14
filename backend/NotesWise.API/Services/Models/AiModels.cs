using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NotesWise.API.Services.Models
{
    public class OpenAiRequest
    {
        public string model { get; set; }
        public string input { get; set; }
    }

    public class OpenAiResponse
    {
        public List<OpenAiOutput> output { get; set; }
    }

    public class OpenAiOutput
    {
        public List<OpenAiContent> content { get; set; }
    }

    public class OpenAiContent
    {
        public string text { get; set; }
    }

    public class GeminiTextResponse
    {
        [JsonPropertyName("candidates")]
        public Candidate[] Candidates { get; set; }
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content Content { get; set; }
    }

    public class Content
    {
        [JsonPropertyName("parts")]
        public Part[] Parts { get; set; }
    }

    public class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

}