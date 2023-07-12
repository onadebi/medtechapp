namespace MedTechAPI.Domain.Config
{
    public class AppConstants
    {
        public static string CookieUserId { get; set; } = "appuser_id";
        public static string AppSystem { get; set; } = nameof(AppSystem);
        public static string EmailTemplateConfirmEmail { get; set; } = "ConfirmEmail";
        public static string EmailTemplateForgottenPasswordRequest { get; set; } = "ForgottenPasswordRequest";
    }
}
