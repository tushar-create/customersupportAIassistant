using Customer_Support.Support;
using Microsoft.AspNetCore.Mvc;

namespace Customer_Support.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly OpenAiService _openAi;
        private readonly ILogger<ChatController> _logger;

        public ChatController(OpenAiService openAi, ILogger<ChatController> logger)
        {
            _openAi = openAi;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message cannot be empty.");
            }

            _logger.LogInformation("Received chat message");

            // 1. Load knowledge base (RAG)
            var context = System.IO.File.ReadAllText("data/knowledgebase.json");
            // 2. Call AI
            var reply = await _openAi.GenerateDraftAsync(request.Message,context);

            return Ok(new ChatResponse(reply));
        }

        [HttpPost("draft")]
        public async Task<IActionResult> CreateDraft([FromBody] string query)
        {
            // 1. Load knowledge base (RAG)
            var context = System.IO.File.ReadAllText("data/knowledgebase.json");

            // 2. Call AI
            var draft = await _openAi.GenerateDraftAsync(query, context);

            var record = new
            {
                Id = Guid.NewGuid(),
                Query = query,
                Draft = draft,
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow
            };

            // 3. Save to file
            var file = "data/drafts.json";
            var json = System.IO.File.Exists(file)
                ? System.IO.File.ReadAllText(file)
                : "[]";

            var list = System.Text.Json.JsonSerializer.Deserialize<List<object>>(json);

            list.Add(record);

            System.IO.File.WriteAllText(file,
                System.Text.Json.JsonSerializer.Serialize(list, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

            return Ok(record);
        }

        [HttpPost("approve/{id}")]
        public IActionResult Approve(string id, [FromBody] string finalResponse)
        {
            var file = "data/approved.json";
            var json = System.IO.File.ReadAllText(file == null ? "[]" : file);

            var list = System.Text.Json.JsonSerializer.Deserialize<List<object>>(json)
                       ?? new List<object>();

            list.Add(new
            {
                Id = id,
                FinalResponse = finalResponse,
                Status = "APPROVED",
                Timestamp = DateTime.UtcNow
            });

            System.IO.File.WriteAllText(file,
                System.Text.Json.JsonSerializer.Serialize(list, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

            return Ok("Approved and saved");
        }

        [HttpPost("reject/{id}")]
        public IActionResult Reject(string id)
        {
            System.IO.File.AppendAllText("data/rejected.json",
                $"{id} rejected at {DateTime.UtcNow}\n");

            return Ok("Rejected and logged");
        }

        [HttpPost("feedback")]
        public IActionResult Feedback([FromBody] string feedback)
        {
            System.IO.File.AppendAllText("data/feedback.json",
                feedback + Environment.NewLine);

            return Ok();
        }
    }

    public record ChatRequest(string Message);
    public record ChatResponse(string Reply);
}