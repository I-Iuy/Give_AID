using System.ComponentModel.DataAnnotations;

namespace Be.Models
{
    public class Purpose
    {
        [Key]
        public int PurposeId { get; set; } // PK, auto-increment
        public string Title { get; set; }
        public int AccountId { get; set; } // FK
    }
}
