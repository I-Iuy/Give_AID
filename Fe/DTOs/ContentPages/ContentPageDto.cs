namespace Fe.DTOs.ContentPages
{
    // Data Transfer Object (DTO) for transferring Content Page data between layers
    public class ContentPageDto
    {
        // Unique identifier for the content page (primary key)
        public int Id { get; set; }

        // Human-readable title of the content page (e.g., "Our Mission")
        public string Title { get; set; }

        // URL-friendly version of the title (e.g., "our-mission")
        // Used for routing or slugs in clean URLs
        public string Slug { get; set; }

        // Main HTML content of the page; rendered on the frontend
        public string Content { get; set; }

        // The person (or system user) who created or last modified the page
        public string Author { get; set; }

        // Timestamp of when the page was last modified or published
        public DateTime UpdatedAt { get; set; }
    }
}
