using System.ComponentModel.DataAnnotations;

namespace Be.DTOs.ContentPage
{
    public class ContentPageCreateDto
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public int AccountId { get; set; }
    }

}
