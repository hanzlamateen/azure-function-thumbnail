using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FunctionThumbnail
{
    public static class FunctionThumbnail
    {
        [FunctionName("FunctionThumbnail")]
        public static async Task Run([BlobTrigger("function/{name}", Connection = "")]Stream myBlob, string name, ILogger log)
        {
            try
            {
                if (myBlob != null)
                {
                    var extension = Path.GetExtension(name);
                    var encoder = GetEncoder(extension);

                    if (encoder != null)
                    {
                        var thumbnailWidth = Convert.ToInt32(Environment.GetEnvironmentVariable("THUMBNAIL_WIDTH"));
                        var miniThumbnailWidth = Convert.ToInt32(Environment.GetEnvironmentVariable("MINITHUMBNAIL_WIDTH"));

                        var thumbContainerName = Environment.GetEnvironmentVariable("THUMBNAIL_CONTAINER_NAME");
                        var connectionString = Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING");

                        var storageAccount = CloudStorageAccount.Parse(connectionString);
                        var blobClient = storageAccount.CreateCloudBlobClient();
                        var container = blobClient.GetContainerReference(thumbContainerName);
                        await container.CreateIfNotExistsAsync();

                        using (Image<Rgba32> image = Image.Load(myBlob))
                        {
                            // Thumbnail
                            var divisor = image.Width / thumbnailWidth;
                            var height = Convert.ToInt32(Math.Round((decimal)(image.Height / divisor)));

                            image.Mutate(x => x.Resize(thumbnailWidth, height));
                            using (var output = new MemoryStream())
                            {
                                image.Save(output, encoder);
                                output.Position = 0;

                                var blobName = string.Format("{0}-{1}px{2}",
                                                             Path.GetFileNameWithoutExtension(name),
                                                             thumbnailWidth,
                                                             Path.GetExtension(name));
                                var blockBlob = container.GetBlockBlobReference(blobName);

                                await blockBlob.UploadFromStreamAsync(output);
                            }

                            // Mini Thumbnail
                            divisor = image.Width / miniThumbnailWidth;
                            height = Convert.ToInt32(Math.Round((decimal)(image.Height / divisor)));

                            image.Mutate(x => x.Resize(miniThumbnailWidth, height));
                            using (var output = new MemoryStream())
                            {
                                image.Save(output, encoder);
                                output.Position = 0;

                                var blobName = string.Format("{0}-{1}px.{2}",
                                                             Path.GetFileNameWithoutExtension(name),
                                                             miniThumbnailWidth,
                                                             Path.GetExtension(name));
                                var blockBlob = container.GetBlockBlobReference(blobName);

                                await blockBlob.UploadFromStreamAsync(output);
                            }
                        }
                    }
                    else
                    {
                        log.LogInformation($"No encoder support for: {name}");
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
                throw;
            }
        }

        private static IImageEncoder GetEncoder(string extension)
        {
            IImageEncoder encoder = null;

            extension = extension.Replace(".", "");

            var isSupported = Regex.IsMatch(extension, "gif|png|jpe?g", RegexOptions.IgnoreCase);

            if (isSupported)
            {
                switch (extension)
                {
                    case "png":
                        encoder = new PngEncoder();
                        break;
                    case "jpg":
                        encoder = new JpegEncoder();
                        break;
                    case "jpeg":
                        encoder = new JpegEncoder();
                        break;
                    case "gif":
                        encoder = new GifEncoder();
                        break;
                    default:
                        break;
                }
            }

            return encoder;
        }
    }
}
