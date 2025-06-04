namespace Be.DTOs.ContentPages
{
    public class ContentPageDto
    {
        // Unique identifier of the content page
        public int Id { get; set; }

        // Title of the page (used in headings and navigation)
        public string Title { get; set; }

        // SEO-friendly slug used in URLs (e.g. "about-us")
        public string Slug { get; set; }

        // HTML content or body of the page
        public string Content { get; set; }

        // Name of the person who created or last updated the page
        public string Author { get; set; }

        // Last time the page was modified
        public DateTime UpdatedAt { get; set; }
    }
}
