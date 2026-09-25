namespace AdsConnect.api.Utils
{
    /// <summary>
    /// Writes avatars under wwwroot, the same way PMS stores its uploads.
    ///
    /// Files are served straight off disk by UseStaticFiles, so everything that
    /// lands here is world-readable to anyone holding the URL. That is fine for an
    /// avatar and is why the name is a fresh GUID: it makes the URL unguessable,
    /// and it means the client's file name never reaches the file system.
    /// </summary>
    public sealed class ProfileImageStore
    {
        /// <summary>2 MB. Anything larger is a photo nobody needs for a 64px circle.</summary>
        public const long MaxBytes = 2 * 1024 * 1024;

        private const string Folder = "ProfileImages";

        /// <summary>
        /// The formats accepted, and the extension each is stored with. The
        /// extension is derived from the content type rather than taken from the
        /// upload, so a file called "avatar.png.exe" cannot land on disk as one.
        /// </summary>
        private static readonly Dictionary<string, string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["image/png"] = ".png",
            ["image/jpeg"] = ".jpg",
            ["image/webp"] = ".webp",
            ["image/gif"] = ".gif",
        };

        private readonly IWebHostEnvironment _environment;

        public ProfileImageStore(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public static string AllowedTypesHint => "PNG, JPEG, WebP or GIF";

        public static bool IsAllowed(string? contentType) =>
            contentType != null && AllowedTypes.ContainsKey(contentType);

        /// <summary>Stores the upload and returns its relative URL.</summary>
        public async Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            var directory = Path.Combine(WebRoot(), Folder);
            Directory.CreateDirectory(directory);

            var name = $"{Guid.NewGuid():N}{AllowedTypes[file.ContentType]}";

            await using (var stream = new FileStream(Path.Combine(directory, name), FileMode.CreateNew))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            return $"/{Folder}/{name}";
        }

        /// <summary>
        /// Deletes a file this store wrote. Anything that is not a well-formed
        /// <c>/ProfileImages/{name}</c> URL is ignored rather than trusted, so a
        /// tampered column value cannot be turned into an arbitrary delete.
        /// </summary>
        public void Delete(string? relativeUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeUrl)) return;

            var prefix = $"/{Folder}/";
            if (!relativeUrl.StartsWith(prefix, StringComparison.Ordinal)) return;

            var name = relativeUrl[prefix.Length..];
            if (name.Length == 0 || name.Contains('/') || name.Contains('\\') || name.Contains("..")) return;

            var path = Path.Combine(WebRoot(), Folder, name);

            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (IOException)
            {
                // A leftover file is harmless; failing the request over one is not.
            }
        }

        /// <summary>
        /// WebRootPath is null until wwwroot exists on disk, which it does not in a
        /// fresh clone - the folder is empty and git does not carry it.
        /// </summary>
        private string WebRoot()
        {
            var root = _environment.WebRootPath;
            if (!string.IsNullOrEmpty(root)) return root;

            root = Path.Combine(_environment.ContentRootPath, "wwwroot");
            Directory.CreateDirectory(root);
            return root;
        }
    }
}
