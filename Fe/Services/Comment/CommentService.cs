using Fe.DTOs.Comment;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Fe.Services.Comment
{
    public interface ICommentService
    {
        Task<List<CommentDto>> GetByCampaignAsync(int campaignId);
        Task CreateAsync(CreateCommentDto dto);
        Task<List<CommentDashboardDto>> GetAllForDashboardAsync();
        Task DeleteAsync(int commentId);
        Task ReplyAsync(int commentId, string replyContent);
    }

    public class CommentService : ICommentService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseApiUrl;

        public CommentService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseApiUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7255";
        }

        public async Task<List<CommentDto>> GetByCampaignAsync(int campaignId)
        {
            var result = await _httpClient.GetFromJsonAsync<List<CommentDto>>(
                $"{_baseApiUrl}/api/comment/campaign/{campaignId}");

            return result ?? new();
        }

        public async Task CreateAsync(CreateCommentDto dto)
        {
            await _httpClient.PostAsJsonAsync($"{_baseApiUrl}/api/comment", dto);
        }

        public async Task<List<CommentDashboardDto>> GetAllForDashboardAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<List<CommentDashboardDto>>(
                $"{_baseApiUrl}/api/comment/dashboard");

            return result ?? new();
        }

        public async Task DeleteAsync(int commentId)
        {
            await _httpClient.DeleteAsync($"{_baseApiUrl}/api/comment/{commentId}");
        }

        public async Task ReplyAsync(int commentId, string replyContent)
        {
            var payload = new
            {
                commentId = commentId,
                replyContent = replyContent
            };

            await _httpClient.PostAsJsonAsync($"{_baseApiUrl}/api/comment/reply", payload);
        }
    }
}
