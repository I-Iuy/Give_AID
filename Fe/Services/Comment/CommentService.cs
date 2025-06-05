using Fe.DTOs.Comment;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

namespace Fe.Services.Comment
{
    /// <summary>
    /// Generic API response wrapper class
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }

    /// <summary>
    /// Interface defining comment service operations
    /// </summary>
    public interface ICommentService
    {
        Task<List<CommentDto>> GetByCampaignAsync(int campaignId);
        Task<CommentDto> CreateAsync(CreateCommentDto dto);
        Task<List<CommentDashboardDto>> GetAllForDashboardAsync();
        Task DeleteAsync(int commentId);
        Task ReplyAsync(int commentId, string replyContent);
        Task<CommentDto?> GetByIdAsync(int commentId);
    }

    /// <summary>
    /// Service class for handling comment-related operations
    /// </summary>
    public class CommentService : ICommentService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseApiUrl;
        private readonly ILogger<CommentService> _logger;

        public CommentService(HttpClient httpClient, IConfiguration configuration, ILogger<CommentService> logger)
        {
            _httpClient = httpClient;
            _baseApiUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7255";
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all comments for a specific campaign
        /// </summary>
        /// <param name="campaignId">The ID of the campaign</param>
        /// <returns>List of comments for the campaign</returns>
        public async Task<List<CommentDto>> GetByCampaignAsync(int campaignId)
        {
            try
            {
                _logger.LogInformation($"Getting comments for campaign {campaignId}");
                var response = await _httpClient.GetAsync($"{_baseApiUrl}/api/comment/campaign/{campaignId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to get comments. Status: {response.StatusCode}, Error: {error}");
                    throw new Exception($"Failed to get comments: {error}");
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Raw response content: {content}");

                // Parse response based on its structure (array or object)
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(content);
                List<CommentDto> result;
                
                if (jsonElement.ValueKind == JsonValueKind.Array)
                {
                    // Direct array of comments
                    result = JsonSerializer.Deserialize<List<CommentDto>>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new();
                }
                else if (jsonElement.ValueKind == JsonValueKind.Object)
                {
                    // Check for error response
                    if (jsonElement.TryGetProperty("success", out var successElement) && 
                        successElement.GetBoolean() == false)
                    {
                        var message = jsonElement.TryGetProperty("message", out var messageElement) 
                            ? messageElement.GetString() 
                            : "Failed to get comments";
                        throw new Exception(message);
                    }

                    // Try to get data property if it exists
                    if (jsonElement.TryGetProperty("data", out var dataElement))
                    {
                        result = JsonSerializer.Deserialize<List<CommentDto>>(dataElement.GetRawText(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new();
                    }
                    else
                    {
                        result = JsonSerializer.Deserialize<List<CommentDto>>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new();
                    }
                }
                else
                {
                    result = new();
                }

                // Log deserialized comment details
                _logger.LogInformation($"[FE Service] Deserialized comments count: {result.Count}");
                foreach (var comment in result)
                {
                    _logger.LogInformation($"[FE Service] Deserialized Comment ID: {comment.CommentId}, Content: '{comment.Content}', CommentedAt: {comment.CommentedAt}");
                    if (comment.Replies != null)
                    {
                        _logger.LogInformation($"[FE Service] Deserialized Reply count for comment {comment.CommentId}: {comment.Replies.Count}");
                        foreach (var reply in comment.Replies)
                        {
                             _logger.LogInformation($"[FE Service] Deserialized Reply ID: {reply.CommentId}, Content: '{reply.Content}', CommentedAt: {reply.CommentedAt}");
                        }
                    }
                }

                // Ensure all comments have valid timestamps
                foreach (var comment in result)
                {
                    if (comment.CommentedAt == default)
                    {
                        comment.CommentedAt = DateTime.Now;
                    }
                    if (comment.Replies != null)
                    {
                        foreach (var reply in comment.Replies)
                        {
                            if (reply.CommentedAt == default)
                            {
                                reply.CommentedAt = DateTime.Now;
                            }
                        }
                    }
                }

                _logger.LogInformation($"Retrieved {result.Count} comments for campaign {campaignId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting comments for campaign {campaignId}");
                throw;
            }
        }

        /// <summary>
        /// Creates a new comment
        /// </summary>
        /// <param name="dto">The comment data to create</param>
        /// <returns>The created comment</returns>
        public async Task<CommentDto> CreateAsync(CreateCommentDto dto)
        {
            try
            {
                _logger.LogInformation($"Creating comment for campaign {dto.CampaignId}");
                _logger.LogInformation($"Comment details: Content='{dto.Content}', IsAnonymous={dto.IsAnonymous}, GuestName='{dto.GuestName}'");

                var response = await _httpClient.PostAsJsonAsync($"{_baseApiUrl}/api/comment", dto);
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Create comment response: {responseContent}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to create comment. Status: {response.StatusCode}, Error: {responseContent}");
                    throw new Exception($"Failed to create comment: {responseContent}");
                }

                // Parse response and handle different response formats
                var dynamicResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                if (dynamicResponse.TryGetProperty("success", out var successElement) && 
                    successElement.GetBoolean() == false)
                {
                    var message = dynamicResponse.TryGetProperty("message", out var messageElement) 
                        ? messageElement.GetString() 
                        : "Failed to create comment";
                    throw new Exception(message);
                }

                // Try to get data property if it exists
                if (dynamicResponse.TryGetProperty("data", out var dataElement))
                {
                    var result = JsonSerializer.Deserialize<CommentDto>(dataElement.GetRawText(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    if (result == null)
                    {
                        throw new Exception("Failed to create comment: Empty data response");
                    }
                    _logger.LogInformation($"[FE Service] CreateAsync: Comment created successfully from data property: ID={result.CommentId}, Content='{result.Content}'");
                    return result;
                }

                // Try to parse as CommentDto directly
                var directResult = JsonSerializer.Deserialize<CommentDto>(responseContent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                if (directResult == null)
                {
                    throw new Exception("Failed to create comment: Empty direct response");
                }

                // Set default timestamp if not provided
                if (directResult.CommentedAt == default)
                {
                    directResult.CommentedAt = DateTime.Now;
                }

                _logger.LogInformation($"[FE Service] CreateAsync: Comment created successfully directly: ID={directResult.CommentId}, Content='{directResult.Content}'");
                return directResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment");
                throw;
            }
        }

        /// <summary>
        /// Retrieves all comments for the admin dashboard
        /// </summary>
        /// <returns>List of comments for dashboard display</returns>
        public async Task<List<CommentDashboardDto>> GetAllForDashboardAsync()
        {
            try
            {
                _logger.LogInformation("Getting all comments for dashboard");
                var response = await _httpClient.GetAsync($"{_baseApiUrl}/api/comment/dashboard");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to get dashboard comments. Status: {response.StatusCode}, Error: {error}");
                    throw new Exception($"Failed to get dashboard comments: {error}");
                }

                var result = await response.Content.ReadFromJsonAsync<List<CommentDashboardDto>>(new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                _logger.LogInformation($"[FE Service] GetAllForDashboardAsync: Received {{result?.Count ?? 0}} comments from backend");
                return result ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments for dashboard");
                throw;
            }
        }

        /// <summary>
        /// Deletes a comment by its ID
        /// </summary>
        /// <param name="commentId">The ID of the comment to delete</param>
        public async Task DeleteAsync(int commentId)
        {
            try
            {
                _logger.LogInformation($"Deleting comment {commentId}");
                var response = await _httpClient.DeleteAsync($"{_baseApiUrl}/api/comment/{commentId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to delete comment. Status: {response.StatusCode}, Error: {error}");
                    throw new Exception($"Failed to delete comment: {error}");
                }

                _logger.LogInformation($"Comment {commentId} deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting comment {commentId}");
                throw;
            }
        }

        /// <summary>
        /// Creates a reply to an existing comment
        /// </summary>
        /// <param name="commentId">The ID of the parent comment</param>
        /// <param name="replyContent">The content of the reply</param>
        public async Task ReplyAsync(int commentId, string replyContent)
        {
            try
            {
                _logger.LogInformation($"Replying to comment {commentId}");
                var payload = new
                {
                    commentId = commentId,
                    replyContent = replyContent
                };

                var response = await _httpClient.PostAsJsonAsync($"{_baseApiUrl}/api/comment/reply", payload);
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to reply to comment. Status: {response.StatusCode}, Error: {error}");
                    throw new Exception($"Failed to reply to comment: {error}");
                }

                _logger.LogInformation($"Reply to comment {commentId} created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error replying to comment {commentId}");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a specific comment by its ID
        /// </summary>
        /// <param name="commentId">The ID of the comment to retrieve</param>
        /// <returns>The comment if found, null otherwise</returns>
        public async Task<CommentDto?> GetByIdAsync(int commentId)
        {
            try
            {
                _logger.LogInformation($"Getting comment by ID: {commentId}");
                var response = await _httpClient.GetAsync($"{_baseApiUrl}/api/comment/{commentId}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning($"Comment with ID {commentId} not found.");
                        return null;
                    }
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to get comment by ID {commentId}. Status: {response.StatusCode}, Error: {error}");
                    throw new Exception($"Failed to get comment by ID: {error}");
                }

                var result = await response.Content.ReadFromJsonAsync<CommentDto>(new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                _logger.LogInformation($"[FE Service] GetByIdAsync: Received comment with ID {result?.CommentId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting comment by ID {commentId}");
                throw;
            }
        }
    }
}
