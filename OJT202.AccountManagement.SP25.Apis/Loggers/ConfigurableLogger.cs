namespace SP25.OJT202.AccountManagement.Presentation.Loggers
{
    public class ConfigurableLogger<T>
    {
        private readonly ILogger<T> _logger;

        public ConfigurableLogger(ILogger<T> logger)
        {
            _logger = logger;
        }

        public void LogInformation()
        {
            _logger.LogInformation("Get accounts from cache");
        }

        public void LogWarning()
        {
            _logger.LogWarning("Accounts not found in cache. Get accounts from database");
        }
    }
}
