namespace Be.Models
{
    public class ContentPage
    {
        // Primary key of the content page
        public int Id { get; set; }

        // Title of the page (displayed on UI and admin panel)
        public string Title { get; set; }

        // SEO-friendly slug used in URLs, e.g., "about", "our-team"
        public string Slug { get; set; } 

        // HTML content or body of the page
        public string Content { get; set; }

        // Author name or admin who last edited the page
        public string Author { get; set; }

        // Date and time when the content was last updated
        public DateTime UpdatedAt { get; set; }
    }
}
