using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Be.Models
{
    public class ContentPage
    {
        [Key]
        public int PageId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [ForeignKey("Account")]
        public int AccountId { get; set; }
        public Account? Account { get; set; }
    }

}
