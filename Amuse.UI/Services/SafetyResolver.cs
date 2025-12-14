using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Amuse.UI.Services
{
    /// <summary>
    /// Resolves safety checker toggle from CLI args, environment variable, or configuration.
    /// Priority: CLI flag > env var > config.
    /// </summary>
    public class SafetyResolver
    {
        private readonly ILogger<SafetyResolver> _logger;
        private readonly IConfiguration _configuration;

        public SafetyResolver(ILogger<SafetyResolver> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Determines if the safety checker is enabled and the source of the decision.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns>A tuple: (isEnabled, source)</returns>
        public (bool IsEnabled, string Source) Resolve(string[] args)
        {
            // Priority 1: CLI flag --disable-safety
            if (args != null && args.Contains("--disable-safety", StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Safety checker disabled (source: CLI flag --disable-safety).");
                return (false, "CLI");
            }

            // Priority 2: Environment variable AMUSE_DISABLE_SAFETY
            var envDisable = Environment.GetEnvironmentVariable("AMUSE_DISABLE_SAFETY");
            if (!string.IsNullOrEmpty(envDisable) && envDisable != "0")
            {
                _logger.LogInformation("Safety checker disabled (source: environment variable AMUSE_DISABLE_SAFETY={0}).", envDisable);
                return (false, "Environment");
            }

            // Priority 3: Config setting AmuseSettings.EnableSafetyChecker
            bool configValue = _configuration.GetSection("AmuseSettings").GetValue<bool>("EnableSafetyChecker", true);
            if (!configValue)
            {
                _logger.LogInformation("Safety checker disabled (source: config AmuseSettings.EnableSafetyChecker=false).");
                return (false, "Config");
            }

            // Default: enabled
            _logger.LogInformation("Safety checker enabled. ContentFilter will be initialized.");
            return (true, "Default");
        }
    }
}
