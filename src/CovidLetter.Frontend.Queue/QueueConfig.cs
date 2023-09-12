namespace CovidLetter.Frontend.Queue
{
    public class QueueConfig
    {
        public const string Identifier = "QueueConfigStorage";

        public string ConnectionString { get; set; }

        public string QueueName { get; set; }

        public string PdfQueueName { get; set; }

        public string CSBConnectionString { get; set; }
    }
}