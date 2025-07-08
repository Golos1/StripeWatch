using Amqp;

namespace StripeWatch;
/// <summary>
/// Utility class handling the parsing of required command line arguments.
/// </summary>
public static class RequiredArgs
{
    /// <summary>
/// Retreives the stripe API secret key from a filepath supplied as the --stripe-key-file command line arg.
/// </summary>
/// <param name="logger">Generally the logger of the calling service.</param>
/// <param name="config">Generally the config of the calling service.</param>
/// <returns>The Stripe API key.</returns>
/// <exception cref="ArgumentException">If --stripe-key-file is not supplied or not a valid filepath</exception>
    public static string GetStripeKey(ILogger logger,IConfiguration config)
    {
        var keypath = config.GetValue<string>("stripe-key-file");
        if (string.IsNullOrWhiteSpace(keypath))
        {
            logger.LogError("--stripe-key-file is required.");
            throw new ArgumentException("--stripe-key-file is required.");
        }

        try
        {
            var fullKeyPath = Path.GetFullPath(keypath);
        }
        catch (Exception)
        {
            logger.LogError("--stripe-key-file must be a path to a file containing a Stripe API key. Please also check to make sure you have permissions.");
            throw new ArgumentException("--stripe-key-file must be a path to a file containing a Stripe API key. Please also check to make sure you have permissions.");

        }
        var keyFile = File.ReadAllText(keypath);
        return keyFile;
    }

    public static string GetAmqpPath(ILogger logger, IConfiguration config)
    {
        var amqpPath = config.GetValue<string>("amqp");
        if (string.IsNullOrWhiteSpace(amqpPath))
        {
            logger.LogError("--amqp is required.");
            throw new ArgumentException("--amqp is required.");
        }

        try
        {
            Address adress = new Address(amqpPath);
            return amqpPath;
        }
        catch (Exception)
        {
            logger.LogError("--amqp must be a valid amqp address.");
            throw new ArgumentException("--amqp must be a valid amqp address.");
        }
    }
}