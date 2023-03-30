using Azure.Storage;
using Azure.Storage.Blobs;
using AzureBlobStorageApi.Dtos;

namespace AzureBlobStorageApi.Services
{
    public class FileService
    {
        private readonly string _storageAccount = "nguyennguyenstorage";
        private readonly string _key = "xU+KKXBNrRl1cBMId9NotMq/4irQCajxZTh+HmgQuVDTNbTz+Ce+ow/mZpWpRXAqjceXEwZjdD7h+AStX8c2Nw==";
        private readonly BlobContainerClient _fileContainer;

        public FileService()
        {
            var credential = new StorageSharedKeyCredential(_storageAccount, _key);
            var blobUri = $"https://{_storageAccount}.blob.core.windows.net";
            var blobServiceClient = new BlobServiceClient(new Uri(blobUri), credential);
            _fileContainer = blobServiceClient.GetBlobContainerClient("nguyencontainer");
        }

        public async Task<List<BlobDto>> ListAsync()
        {
            List<BlobDto> files = new();

            await foreach (var file in _fileContainer.GetBlobsAsync())
            {
                string uri = _fileContainer.Uri.ToString();
                var name = file.Name;
                var fullUri = $"{uri}/{name}";

                files.Add(new BlobDto
                {
                    Uri = fullUri,
                    Name = name,
                    ContentType = file.Properties.ContentType,
                });
            }

            return files;
        }

        public async Task<BlobResponseDto> UploadAsync(IFormFile blob)
        {
            BlobResponseDto response = new();
            BlobClient client = _fileContainer.GetBlobClient(blob.FileName);

            await using (Stream? data = blob.OpenReadStream())
            {
                await client.UploadAsync(data);
            }

            response.Status = $"File {blob.FileName} Uploaded Successfully";
            response.Error = false;
            response.Blob.Uri = client.Uri.AbsoluteUri;
            response.Blob.Name = blob.Name;

            return response;
        }

        public async Task<BlobDto?> DownloadAsync(String blobFileName)
        {
            BlobClient file = _fileContainer.GetBlobClient(blobFileName);

            if(await file.ExistsAsync())
            {
                var data = await file.OpenReadAsync();
                Stream blobContent = data;

                var content = await file.DownloadContentAsync();

                string name = blobFileName;
                string contentType = content.Value.Details.ContentType;

                return new BlobDto { Content = blobContent, Name = name, ContentType = contentType };
            }

            return null;
        }

        public async Task<BlobResponseDto> DeleteAsync(String blobFileName)
        {
            BlobClient file = _fileContainer.GetBlobClient(blobFileName);

            await file.DeleteAsync();

            return new BlobResponseDto { Error = false, Status = $"File: {blobFileName} has been successfully deleted" };
        }
    }
}
