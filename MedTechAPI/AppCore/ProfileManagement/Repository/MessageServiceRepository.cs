using MedTechAPI.Domain.Config;
using MedTechAPI.Domain.DTO;
using MedTechAPI.Domain.Entities.SetupConfigurations;
using MedTechAPI.Persistence;
using Microsoft.Extensions.Options;
using OnaxTools.Dto.Http;
using OnaxTools.Enums.Http;
using System.Net.Mail;

namespace MedTechAPI.AppCore.Repository
{

    public class MessageServiceRepository : IMessageRepository
    {
        private readonly EmailConfig _emailConfig;
        private readonly AppSettings _appsettings;
        private readonly AppDbContext _context;
        private readonly ILogger<MessageServiceRepository> _logger;

        public MessageServiceRepository(IOptions<EmailConfig> emailConfig, ILogger<MessageServiceRepository> logger, IOptions<AppSettings> appsettings
            , AppDbContext context)
        {
            _emailConfig = emailConfig.Value;
            _logger = logger;
            _context = context;
            _appsettings = appsettings.Value;
        }

        public async Task<GenResponse<string>> InsertNewMessageAndSendMail(EmailModelDTO entity, MessageBox msg)
        {
            try
            {
                _context.MessageBox.Add(msg);
                await _context.SaveChangesAsync();
                if (!string.IsNullOrWhiteSpace(msg.Id))
                {
                    if (await this.SendEmail(entity))
                    {
                        _logger.LogInformation($"Email for {entity.ReceiverEmail} with token {msg.MessageData} for operation {msg.Operation} successfully sent.");
                        msg.IsProcessed = true;
                        msg.UpdatedAt = DateTime.Now;
                        _ = await _context.SaveChangesAsync();
                    }
                    else
                    {
                        _logger.LogError($"Email failed to send for {entity.ReceiverEmail} with token {msg.MessageData} for operation {msg.Operation}");
                        return GenResponse<string>.Failed("Message saved to database, but Email not sent.");
                    }
                    return GenResponse<string>.Success(msg.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(message: ex.Message);
                return GenResponse<string>.Failed("Oops, a server error occured. Kindly try again.", StatusCodeEnum.ServerError);
            }
            return GenResponse<string>.Failed("Failed to process request", StatusCodeEnum.NotImplemented);
        }

        public async Task<GenResponse<string>> InsertNewMessage(MessageBox msg)
        {
            try
            {
                _context.MessageBox.Add(msg);
                int objSave = await _context.SaveChangesAsync();
                if (!string.IsNullOrWhiteSpace(msg.Id))
                {
                    return GenResponse<string>.Success(msg.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(message: ex.Message);
                return GenResponse<string>.Failed("Oops, a server error occured. Kindly try again.", StatusCodeEnum.ServerError);
            }
            return GenResponse<string>.Failed("Failed to process request", StatusCodeEnum.NotImplemented);
        }


        public async Task<bool> SendEmail(EmailModelDTO payload)
        {
            try
            {
                MailMessage mailMessage = new()
                {
                    From = new MailAddress(_emailConfig.SenderEmail, _emailConfig.SenderName)
                };
                mailMessage.To.Clear();
                var emails = payload.ReceiverEmail.Split(new char[] { ',', ';', ':', '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var addr in emails)
                {
                    mailMessage.To.Add(new MailAddress(addr));
                }
                mailMessage.Subject = payload.EmailSubject;
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = payload.EmailBody;

                //Configure SMTP Client and send message
                using (var client = new SmtpClient())
                {
                    string envPwd = Environment.GetEnvironmentVariable(_emailConfig.SmtpPassword, EnvironmentVariableTarget.Process) ?? _emailConfig.SmtpPassword;
                    if (_emailConfig.IsDevelopment)
                    {
                        #region TEST
                        // set up the Gmail server
                        //NetworkCredential networkCred = new NetworkCredential(_emailConfig.SenderGMAIL, _emailConfig.Password);
                        client.Host = _emailConfig.SmtpHost;
                        client.EnableSsl = true;
                        client.Port = _emailConfig.SmtpPort;
                        client.UseDefaultCredentials = false;
                        //client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.Credentials = new System.Net.NetworkCredential(_emailConfig.SenderEmail, envPwd);
                        await client.SendMailAsync(mailMessage);
                        client.Dispose();
                        #endregion

                    }
                    else
                    {
                        #region PRODUCTION
                        client.Host = _emailConfig.SmtpHost;
                        client.Port = _emailConfig.SmtpPort;
                        client.UseDefaultCredentials = false;
                        client.Credentials = new System.Net.NetworkCredential(_emailConfig.SenderEmail, envPwd);
                        client.Send(mailMessage);
                        client.Dispose();
                        #endregion
                    }
                }
                //If execution gets here, message has been sent; hence, log and return successful response
                _logger.LogInformation("[MessageServiceRepository][SendEmail] SendEmail Success: Email sent successfully!");
                return true;
            }
            catch (Exception ex)
            {
                //Error occured, log and return failed response
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                OnaxTools.Logger.LogException(ex, $"[MessageServiceRepository][SendEmail] ==> SendMail Exception: {msg}");
                return false;
            }
        }
    }



    public interface IMessageRepository
    {
        Task<GenResponse<string>> InsertNewMessageAndSendMail(EmailModelDTO entity, MessageBox msg);
        Task<GenResponse<string>> InsertNewMessage(MessageBox msg);
        Task<bool> SendEmail(EmailModelDTO payload);
    }

}
