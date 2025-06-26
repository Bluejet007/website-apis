namespace WebsiteAPIs
{
    public class FileJob
    {

        public string? Id { get; set; }
        public IFormFile? File {  get; set; }
        public string JobType { get; set; } = String.Empty;
    }
}
