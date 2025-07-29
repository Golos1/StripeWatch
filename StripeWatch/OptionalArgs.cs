using System.Linq.Expressions;

namespace StripeWatch;
/// <summary>
/// Utility class handling the parsing of optional command line arguments.
/// </summary>
public static class OptionalArgs
{
    /// <summary>
    /// Parses the --min command line argument for minimum account value if present and makes sure it is a positive double.
    /// </summary>
    /// <param name="logger">Generally the logger of the calling service.</param>
    /// <param name="config">Generally the config of the calling service.</param>
    /// <returns>The desired minimum balance, or double.MinValue if not supplied.</returns>
    /// <exception cref="ArgumentException"> If the minimum is not a positive double. </exception>
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
    /// <summary>
    /// Parses the --minutes command line argument and makes sure it is a positive integer. Defaults to 30 if not present.
    /// </summary>
    /// <param name="logger">Generally the logger of the calling service</param>
    /// <param name="config">Generally the config of the calling service</param>
    /// <returns> 30 if --minutes is not supplied, whatever positive integer was given otherwise.</returns>
    /// <exception cref="ArgumentException">If --minutes can't be parsed as a positive integer.</exception>
    public static int ParseMinutes(ILogger logger, IConfiguration config)
    {
        var minuteString = config.GetValue<string>("minutes");
        if (string.IsNullOrWhiteSpace(minuteString))
        {
            return 30;
        }

        try
        {
            var minutes = int.Parse(minuteString);
            if (minutes < 0)
            {
                throw new ArgumentException();
            }
            return minutes;
        }
        catch(Exception)
        {
            logger.LogError("--minutes must be a positive integer.");
            throw new ArgumentException("--minutes must be a positive integer.");
        }
    }
}