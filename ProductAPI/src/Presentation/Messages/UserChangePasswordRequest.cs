namespace Presentation.Messages
{
    public class UserChangePasswordRequest
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmationCode { get; set; }
    }
}
