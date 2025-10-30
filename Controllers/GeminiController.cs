using DebtManagerApp.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes; // لإضافة هذا

namespace DebtManagerApp.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class GeminiController : ControllerBase
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;
		private readonly ILogger<GeminiController> _logger;
		private readonly string? _geminiApiKey;
		private readonly string? _geminiModelName;

		public GeminiController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<GeminiController> logger)
		{
			_httpClientFactory = httpClientFactory;
			_configuration = configuration;
			_logger = logger;

			// قراءة الإعدادات الجديدة من appsettings.json
			_geminiApiKey = _configuration["ApiKeys:GeminiApiKey"];
			_geminiModelName = _configuration["ApiKeys:GeminiModelName"];
		}

		/// <summary>
		/// (يعمل) - يستخدم Google Gemini Standard API
		/// </summary>
		[HttpPost("generate")]
		public async Task<IActionResult> GenerateContent([FromBody] GeminiRequestDto request)
		{
			if (string.IsNullOrEmpty(_geminiApiKey) || string.IsNullOrEmpty(_geminiModelName))
			{
				_logger.LogError("Gemini API Key or Model Name is not configured in server settings (ApiKeys section).");
				return StatusCode(500, new { message = "The AI service is not configured on the server. Missing API Key or Model Name." });
			}

			if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
			{
				return BadRequest(new { message = "Request body is missing or the 'prompt' field is empty." });
			}

			// استخدام v1beta وهو الأكثر شيوعاً
			var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{_geminiModelName}:generateContent?key={_geminiApiKey}";

			// بناء الجسم (Payload) الصحيح للـ API القياسي
			var payload = new
			{
				contents = new[]
				{
					new
					{
						parts = new[]
						{
							new { text = request.Prompt }
						}
					}
				}
			};

			var jsonPayload = JsonSerializer.Serialize(payload);
			var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

			try
			{
				var client = _httpClientFactory.CreateClient();
				_logger.LogInformation("Sending request to Google Gemini Standard API (from GenerateContent)...");

				var response = await client.PostAsync(apiUrl, httpContent);

				if (response.IsSuccessStatusCode)
				{
					var responseBody = await response.Content.ReadAsStringAsync();
					_logger.LogInformation("Received successful response from Gemini.");

					// استخراج النص من الرد (مختلف عن Vertex AI)
					try
					{
						var jsonNode = JsonNode.Parse(responseBody);
						var generatedText = jsonNode?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

						if (string.IsNullOrEmpty(generatedText))
						{
							_logger.LogWarning("Could not parse text from Gemini response structure. Response: {ResponseBody}", responseBody);
							return Ok(new { text = "[لم يتم العثور على نص في رد Google]" });
						}

						return Ok(new { text = generatedText });
					}
					catch (JsonException jsonEx)
					{
						_logger.LogError(jsonEx, "Failed to parse JSON response from Gemini. ResponseBody: {ResponseBody}", responseBody);
						return StatusCode(500, new { message = "Failed to parse AI response.", details = jsonEx.Message });
					}
				}
				else
				{
					// إذا فشل (مثل 404 أو 400)، اقرأ الخطأ من Google
					var errorBody = await response.Content.ReadAsStringAsync();
					_logger.LogError("Google API returned an error. Status: {StatusCode}, Body: {ErrorBody}", response.StatusCode, errorBody);
					return StatusCode((int)response.StatusCode, new { message = "Error from Google Gemini API.", details = errorBody });
				}
			}
			catch (HttpRequestException httpEx)
			{
				_logger.LogError(httpEx, "HTTP Request Exception caught in GenerateContent.");
				return StatusCode(500, new { message = $"An internal server error occurred (HTTP): {httpEx.Message}" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Generic Exception caught in GenerateContent.");
				return StatusCode(500, new { message = $"An unexpected internal server error occurred: {ex.Message}" });
			}
		}


		/// <summary>
		/// (تم تفعيله) - يستخدم نفس منطق "generate"
		/// </summary>
		[HttpPost("analyze-receipt")]
		public async Task<IActionResult> AnalyzeReceipt([FromBody] GeminiRequestDto request)
		{
			if (string.IsNullOrEmpty(_geminiApiKey) || string.IsNullOrEmpty(_geminiModelName))
			{
				_logger.LogError("Gemini API Key or Model Name is not configured in server settings (ApiKeys section).");
				return StatusCode(500, new { message = "The AI service is not configured on the server. Missing API Key or Model Name." });
			}

			if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
			{
				return BadRequest(new { message = "Request body is missing or the 'prompt' field is empty." });
			}

			var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{_geminiModelName}:generateContent?key={_geminiApiKey}";

			var payload = new
			{
				contents = new[]
				{
					new
					{
						parts = new[]
						{
							new { text = request.Prompt }
						}
					}
				}
			};

			var jsonPayload = JsonSerializer.Serialize(payload);
			var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

			try
			{
				var client = _httpClientFactory.CreateClient();
				_logger.LogInformation("Sending request to Google Gemini Standard API (from AnalyzeReceipt)...");

				var response = await client.PostAsync(apiUrl, httpContent);

				if (response.IsSuccessStatusCode)
				{
					var responseBody = await response.Content.ReadAsStringAsync();
					_logger.LogInformation("Received successful response from Gemini.");

					try
					{
						var jsonNode = JsonNode.Parse(responseBody);
						var generatedText = jsonNode?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

						if (string.IsNullOrEmpty(generatedText))
						{
							_logger.LogWarning("Could not parse text from Gemini response structure. Response: {ResponseBody}", responseBody);
							return Ok(new { text = "[لم يتم العثور على نص في رد Google]" });
						}

						return Ok(new { text = generatedText });
					}
					catch (JsonException jsonEx)
					{
						_logger.LogError(jsonEx, "Failed to parse JSON response from Gemini. ResponseBody: {ResponseBody}", responseBody);
						return StatusCode(500, new { message = "Failed to parse AI response.", details = jsonEx.Message });
					}
				}
				else
				{
					var errorBody = await response.Content.ReadAsStringAsync();
					_logger.LogError("Google API returned an error. Status: {StatusCode}, Body: {ErrorBody}", response.StatusCode, errorBody);
					return StatusCode((int)response.StatusCode, new { message = "Error from Google Gemini API.", details = errorBody });
				}
			}
			catch (HttpRequestException httpEx)
			{
				_logger.LogError(httpEx, "HTTP Request Exception caught in AnalyzeReceipt.");
				return StatusCode(500, new { message = $"An internal server error occurred (HTTP): {httpEx.Message}" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Generic Exception caught in AnalyzeReceipt.");
				return StatusCode(500, new { message = $"An unexpected internal server error occurred: {ex.Message}" });
			}
		}

		/// <summary>
		/// (تم تفعيله) - يستخدم نفس منطق "generate"
		/// </summary>
		[HttpPost("bulk-add")]
		public async Task<IActionResult> AnalyzeBulkAdd([FromBody] GeminiRequestDto request)
		{
			if (string.IsNullOrEmpty(_geminiApiKey) || string.IsNullOrEmpty(_geminiModelName))
			{
				_logger.LogError("Gemini API Key or Model Name is not configured in server settings (ApiKeys section).");
				return StatusCode(500, new { message = "The AI service is not configured on the server. Missing API Key or Model Name." });
			}

			if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
			{
				return BadRequest(new { message = "Request body is missing or the 'prompt' field is empty." });
			}

			var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{_geminiModelName}:generateContent?key={_geminiApiKey}";

			var payload = new
			{
				contents = new[]
				{
					new
					{
						parts = new[]
						{
							new { text = request.Prompt }
						}
					}
				}
			};

			var jsonPayload = JsonSerializer.Serialize(payload);
			var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

			try
			{
				var client = _httpClientFactory.CreateClient();
				_logger.LogInformation("Sending request to Google Gemini Standard API (from AnalyzeBulkAdd)...");

				var response = await client.PostAsync(apiUrl, httpContent);

				if (response.IsSuccessStatusCode)
				{
					var responseBody = await response.Content.ReadAsStringAsync();
					_logger.LogInformation("Received successful response from Gemini.");

					try
					{
						var jsonNode = JsonNode.Parse(responseBody);
						var generatedText = jsonNode?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

						if (string.IsNullOrEmpty(generatedText))
						{
							_logger.LogWarning("Could not parse text from Gemini response structure. Response: {ResponseBody}", responseBody);
							return Ok(new { text = "[لم يتم العثور على نص في رد Google]" });
						}

						return Ok(new { text = generatedText });
					}
					catch (JsonException jsonEx)
					{
						_logger.LogError(jsonEx, "Failed to parse JSON response from Gemini. ResponseBody: {ResponseBody}", responseBody);
						return StatusCode(500, new { message = "Failed to parse AI response.", details = jsonEx.Message });
					}
				}
				else
				{
					var errorBody = await response.Content.ReadAsStringAsync();
					_logger.LogError("Google API returned an error. Status: {StatusCode}, Body: {ErrorBody}", response.StatusCode, errorBody);
					return StatusCode((int)response.StatusCode, new { message = "Error from Google Gemini API.", details = errorBody });
				}
			}
			catch (HttpRequestException httpEx)
			{
				_logger.LogError(httpEx, "HTTP Request Exception caught in AnalyzeBulkAdd.");
				return StatusCode(500, new { message = $"An internal server error occurred (HTTP): {httpEx.Message}" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Generic Exception caught in AnalyzeBulkAdd.");
				return StatusCode(500, new { message = $"An unexpected internal server error occurred: {ex.Message}" });
			}
		}
	}
}

