using QRCoder; 
 
namespace AkarangaBank.Web.Services; 
 
public class QrCodeService : IQrCodeService 
{ 
    public string GenerateQrCodeBase64(string text) 
    { 
        using var qrGenerator = new QRCodeGenerator(); 
        using var qrCodeData = qrGenerator.CreateQrCode(text, 
QRCodeGenerator.ECCLevel.Q); 
        using var qrCode = new PngByteQRCode(qrCodeData); 
        var bytes = qrCode.GetGraphic(10); 
        return Convert.ToBase64String(bytes); 
    } 
} 
 