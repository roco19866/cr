namespace CertificateSystem.Services
{
    public interface IWhatsAppService
    {
        string GenerateWhatsAppLink(string phone, string message);
        Task SendWhatsAppMessageAsync(string phone, string message, string fileUrl);
    }

    public class WhatsAppService : IWhatsAppService
    {
        public string GenerateWhatsAppLink(string phone, string message)
        {
            // Clean phone number (remove +, spaces, etc.)
            var cleanPhone = new string(phone.Where(char.IsDigit).ToArray());
            if (cleanPhone.StartsWith("05")) cleanPhone = "966" + cleanPhone.Substring(1); // Default for Saudi Arabia if starts with 05

            var encodedMessage = Uri.EscapeDataString(message);
            return $"https://wa.me/{cleanPhone}?text={encodedMessage}";
        }

        public async Task SendWhatsAppMessageAsync(string phone, string message, string fileUrl)
        {
            // This would require a real API like Twilio or UltraMsg
            // For now, we log it and provide the link generation as the primary method
            Console.WriteLine($"[WhatsApp Simulation] To: {phone}, Msg: {message}, File: {fileUrl}");
            await Task.CompletedTask;
        }
    }
}
