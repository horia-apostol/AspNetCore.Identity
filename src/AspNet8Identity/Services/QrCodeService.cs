using QRCoder;

namespace AspNet8Identity.Services
{
    internal sealed class QrCodeService
    {
        private readonly int pixelsPerModule = 5;
        public string GenerateQrCodeUri(string authenticatorUri)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(authenticatorUri, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(pixelsPerModule);
            return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
        }
    }
}