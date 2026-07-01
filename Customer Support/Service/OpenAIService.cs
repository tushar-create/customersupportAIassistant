using Microsoft.AspNetCore.Mvc;

namespace Customer_Support.Support
{
    public class OpenAiService
    {
        private readonly HttpClient _httpClient;

        public OpenAiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        //public async Task<string> GetResponseAsync(string message)
        //{
        //    var payload = new
        //    {
        //        //model = "gpt-4o-mini",
        //        model= "gpt-3.5-turbo",
        //        messages = new[]
        //        {
        //        new { role = "system", content = "This is helpful customer support assistant." },
        //        new { role = "user", content = message }
        //    }
        //    };

        //    var response = await _httpClient.PostAsJsonAsync("chat/completions", payload);

        //    var responseBody = await response.Content.ReadAsStringAsync();

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        //throw new Exception($"OpenAI Error: {response.StatusCode} - {responseBody}");

        //            var error = await response.Content.ReadAsStringAsync();

        //            return $"Open AI service unavailable: {error}";

        //    }

        //    using var doc = System.Text.Json.JsonDocument.Parse(responseBody);

        //    return doc.RootElement
        //              .GetProperty("choices")[0]
        //              .GetProperty("message")
        //              .GetProperty("content")
        //              .GetString();
        //}

        public async Task<string> GenerateDraftAsync(string message, string context)
        {
            var payload = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
            new
            {
                role = "system",
                content =
                    "You are a customer support AI assistant. " +
                    "Generate draft responses only. " +
                    "Use provided context to improve accuracy."
            },
            new
            {
                role = "system",
                content = $"Knowledge Base Context: {context}"
            },
            new
            {
                role = "user",
                content = message
            }
        },
                temperature = 0.3
            };

            var response = await _httpClient.PostAsJsonAsync("chat/completions", payload);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return $"AI Error: {responseBody}";
            }

            using var doc = System.Text.Json.JsonDocument.Parse(responseBody);

            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }


    }
}
