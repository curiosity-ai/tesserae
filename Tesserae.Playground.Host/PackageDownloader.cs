
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace Tesserae.Playground.Host
{
    public class PackageDownloader
    {
        private readonly string _packagesDirectory;
        private readonly HttpClient _httpClient;

        public PackageDownloader(string packagesDirectory)
        {
            _packagesDirectory = packagesDirectory;
            _httpClient = new HttpClient();
        }

        public async Task EnsurePackageAsync(string packageId, string version)
        {
            var packageDir = Path.Combine(_packagesDirectory, packageId.ToLower(), version);
            if (Directory.Exists(packageDir))
            {
                return;
            }

            Console.WriteLine($"Downloading {packageId} {version}...");

            try
            {
                var url = $"https://api.nuget.org/v3-flatcontainer/{packageId.ToLower()}/{version}/{packageId.ToLower()}.{version}.nupkg";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var tempFile = Path.GetTempFileName();
                using (var fs = File.Create(tempFile))
                {
                    await response.Content.CopyToAsync(fs);
                }

                Directory.CreateDirectory(packageDir);
                ZipFile.ExtractToDirectory(tempFile, packageDir);
                File.Delete(tempFile);

                Console.WriteLine($"Downloaded and extracted {packageId} {version} to {packageDir}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to download {packageId} {version}: {ex.Message}");
                // Clean up
                if (Directory.Exists(packageDir)) Directory.Delete(packageDir, true);
                throw;
            }
        }
    }
}
