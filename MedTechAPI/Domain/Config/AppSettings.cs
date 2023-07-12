namespace MedTechAPI.Domain.Config
{
    public class AppSettings
    {
        public string AppName { get; set; }
        public string AppKey { get; set; }
        public bool LogToAppInsights { get; set; }
        public Encryption Encryption { get; set; }
        public QueueConfig QueueConfig { get; set; }
        public MessageBroker MessageBroker { get; set; }
        public ExternalAPIs ExternalAPIs { get; set; }
        public AzureBlobConfig AzureBlobConfig { get; set; }
        public SpeechSynthesis SpeechSynthesis { get; set; }
        public ServiceBus ServiceBus { get; set; }
        public DatabaseOptions DatabaseOptions { get; set; }
    }
    public class DatabaseOptions
    {
        public bool SeedDatabase { get; set; }
    }
    public class MessageBroker
    {
        public RabbitMq RabbitMq { get; set; }
    }
    public class RabbitMq
    {
        public string ConString { get; set; }
    }
    public class ExternalAPIs
    {
        public string OpenAI { get; set; }
        public string OpenAIKey { get; set; }
        public string AzureSpeechKey { get; set; }
    }

    public class ServiceBus
    {
        public string SvcBusConstring { get; set; }
    }

    public class SpeechSynthesis
    {
        public string SpeechKey { get; set; }
        public string SpeechEndpoint { get; set; }
        public string SpeechLocation { get; set; }
    }

    public class AzureBlobConfig
    {
        public string BlobStorageConstring { get; set; }
        public string BlobReadAccessYear2099 { get; set; }
        public string BlobStoragePath { get; set; }
        public string DefaultContainerName { get; set; }
    }
    public class Encryption
    {
        public string Key { get; set; }
    }

    public class QueueConfig
    {
        public string QueueName { get; set; }
        public string QueueConString { get; set; }
    }
}
