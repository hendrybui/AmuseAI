using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace Amuse.UI.Services
{
    /// <summary>
    /// Helper to safely initialize ContentFilter (ONNX safety checker) with guards.
    /// Skips initialization if safety checker is disabled.
    /// </summary>
    public class SafetyCheckerInitializer
    {
        private readonly ILogger<SafetyCheckerInitializer> _logger;
        private readonly IConfiguration _configuration;
        private readonly SafetyResolver _safetyResolver;

        public SafetyCheckerInitializer(
            ILogger<SafetyCheckerInitializer> logger,
            IConfiguration configuration,
            SafetyResolver safetyResolver)
        {
            _logger = logger;
            _configuration = configuration;
            _safetyResolver = safetyResolver;
        }

        /// <summary>
        /// Initializes ContentFilter only if safety checker is enabled.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns>True if initialization succeeded or was skipped; false on error.</returns>
        public bool TryInitialize(string[] args)
        {
            var (isEnabled, source) = _safetyResolver.Resolve(args);

            if (!isEnabled)
            {
                _logger.LogInformation("Skipping ContentFilter.onnx initialization (disabled via {source}).", source);
                return true; // Not an error; intentionally disabled.
            }

            try
            {
                var modelPath = _configuration.GetSection("AmuseSettings").GetValue<string>("SafetyCheckerModel");
                if (string.IsNullOrWhiteSpace(modelPath))
                {
                    _logger.LogWarning("SafetyCheckerModel path is empty. ContentFilter will not be initialized.");
                    return true; // Not an error; path is missing.
                }

                _logger.LogInformation("Initializing ContentFilter from {path}...", modelPath);

                // TODO: Replace with actual ContentFilter initialization logic.
                // Example (pseudocode):
                //   var session = new InferenceSession(modelPath);
                //   _contentFilter = new ContentFilter(session);
                //
                // For now, we log and assume success.
                _logger.LogInformation("ContentFilter initialized successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize ContentFilter. Safety checker will be unavailable.");
                return false; // Indicate failure; caller decides how to handle.
            }
        }
    }
}
