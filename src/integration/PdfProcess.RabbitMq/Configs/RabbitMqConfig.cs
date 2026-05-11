using System.ComponentModel.DataAnnotations;

namespace PdfProcess.Data.Configs;

public class RabbitMqConfig
{
    [Required]
    public string HostName { get; set; } = "localhost";

    public int Port { get; set; } = 5672;

    [Required]
    public string UserName { get; set; } = "guest";

    [Required]
    public string Password { get; set; } = "guest";

    public string VirtualHost { get; set; } = "/";

    public string QueueName { get; set; } = "pdf_process_queue";

    public int PrefetchCount { get; set; } = 1;

    public int RetryCount { get; set; } = 3;

    public int RetryDelaySeconds { get; set; } = 5;

    public bool AutomaticRecoveryEnabled { get; set; } = true;

    public int NetworkRecoveryIntervalSeconds { get; set; } = 10;
}
