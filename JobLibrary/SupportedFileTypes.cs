namespace JobLibrary
{
    public class SupportedFileTypes
    {
        public static readonly HashSet<string> fileTypes = [".png", ".jpg", ".jpeg"];
        public static readonly HashSet<string> mimeTypes = ["image/png", "image/jpg", "image/jpeg"];

        public static readonly Dictionary<string, string> filesToMimes = new()
        {
            [".png"] = "image/png",
            [".jpg"] = "image/jpg",
            [".jpeg"] = "image/jpeg",
        };

        public static bool Contains(string type)
        {
            return fileTypes.Contains(type) || mimeTypes.Contains(type);
        }
    }
}
