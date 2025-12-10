using ModelBaseAPI.Models.DTO;

namespace ModelBaseAPI.Models.Response
{
    public class BlobResponse
    {
        public string? Status { get; set; }
        public bool? Error { get; set; }

        public BlobDTO? Blob { get; set; }
    }
}
