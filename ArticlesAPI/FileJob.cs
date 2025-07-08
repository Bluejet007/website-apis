using JobLibrary;

namespace WebsiteAPIs
{
    public class FileJob
    {
        public required IFormFile File { get; set; }
        public required JobType JobType { get; set; }
        public int[]? Parameters { get; set; }
    }
}
