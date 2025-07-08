namespace StripeWatch;
/// <summary>
/// Utility class handling the parsing of optional command line arguments.
/// </summary>
public static class OptionalArgs
{
    /// <summary>
    /// Parses the --min command line argument if present and makes sure it is a positive double.
    /// </summary>
    /// <param name="logger">Generally the logger of the calling service.</param>
    /// <param name="config">Generally the config of the calling service.</param>
    /// <returns>The desired minimum balance, or double.MinValue if not supplied.</returns>
    /// <exception cref="ArgumentException"> If the minimum is either not present or not a positive double. </exception>
    public static double ParseMin(ILogger logger, IConfiguration config)
    {
        var minString = config.GetValue<string>("min");
        if (string.IsNullOrWhiteSpace(minString))
        {
            return double.MinValue;
        }

        try
        {
            var min = double.Parse(minString);
            if (min < 0)
            {
                throw new ArgumentException();
            }
            return min;
        }
        catch (Exception)
        {
            logger.LogError("--min must be a positive double.");
            throw new ArgumentException("--min must be a positive double."); 
        }
    }

    /// <summary>
    /// Gets the file stream that the service will write events to.
    /// </summary>
    /// <param name="logger">Generally the logger of the calling service.</param>
    /// <param name="config">Generally the config of the calling service.</param>
    /// <returns>The file stream to write events to.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static string GetLogFile(ILogger logger, IConfiguration config)
    {
        var path = config.GetValue<string>("log-file");
        if (string.IsNullOrWhiteSpace(path))
        {
            return "";
        }

        try
        {
            File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
        }
        catch (Exception)
        {
            logger.LogError("Could not create or open log file. Please check permissions.");
            throw new ArgumentException("Could not create or open log file. Please check permissions.");
        }
        return path;
    }
}