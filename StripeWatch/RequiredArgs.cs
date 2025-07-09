using Stripe;
using Address = Amqp.Address;

namespace StripeWatch;
/// <summary>
/// Utility class handling the parsing of required command line arguments.
/// </summary>
public static class RequiredArgs
{
    /// <summary>
/// Retreives the stripe API secret key from a filepath supplied as the --stripe-key command line arg.
/// </summary>
/// <param name="logger">Generally the logger of the calling service.</param>
/// <param name="config">Generally the config of the calling service.</param>
/// <returns>The Stripe API key.</returns>
/// <exception cref="ArgumentException">If --stripe-key is not supplied or not a valid filepath</exception>
    public static string GetStripeKey(ILogger logger,IConfiguration config)
    {
        var key = config.GetValue<string>("stripe-key");
        if (string.IsNullOrWhiteSpace(key))
        {
            logger.LogError("--stripe-key is required.");
            throw new ArgumentException("--stripe-key is required.");
        }
        StripeClient client = new StripeClient(key);
        try
        {
            client.V1.Balance.Get();
        }
        catch (Exception)
        {
            logger.LogError("Stripe key is invalid.");
            throw new ArgumentException("Stripe key is invalid");
        }
        return key;
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