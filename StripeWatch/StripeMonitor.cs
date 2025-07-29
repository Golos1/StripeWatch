using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using Stripe;
using Amqp;
using Address = Amqp.Address;
namespace StripeWatch;

/// <summary>
/// A background service that polls the stripe API for events. 
/// </summary>
public class StripeMonitor : BackgroundService
{
    private readonly ILogger<StripeMonitor> _logger;
    private  BalanceService _service;
    private  string _address;
    private  string _logFilePath;
    private double _min;
    private readonly IConfiguration _configuration;
    private StripeClient _stripeClient;
    private readonly ConcurrentDictionary<string,Stripe.Event> _eventMap = new ConcurrentDictionary<string, Stripe.Event>();


    public StripeMonitor(ILogger<StripeMonitor> logger,IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
    
/// <summary>
/// Sends an AMQP message detailing the minimum and current balance.
/// </summary>
/// <param name="amount"></param>
    private void BalanceMessage(long amount)
    {
        var warning = new {description="WARNING: Balance Below minimum.",minimum=_min.ToString(CultureInfo.InvariantCulture),balance=amount.ToString(CultureInfo.InvariantCulture),time=DateTime.Now.ToString("yyyy-MM-dd HH:mm")};

        Message message = new Amqp.Message(JsonSerializer.Serialize(warning));
        Connection connection = new Connection(new Address(_address));
        Session session = new Session(connection);
        SenderLink sender = new SenderLink(session, "stripe-watch", "stripe-events");
        sender.Send(message); 
        sender.Close();
        session.Close();
        connection.Close();
    }
/// <summary>
/// Sends new Events as AMQP messages.
/// </summary>
    private void EventsMessages()
    {
        var currentEvents = _stripeClient.V1.Events.ListAsync().Result.ToList();
        Connection connection = new Connection(new Address(_address));
        Session session = new Session(connection);
        SenderLink sender = new SenderLink(session, "stripe-watch", "stripe-events");
        Parallel.ForEach(currentEvents, stripeEvent =>
        {
            if (!_eventMap.ContainsKey(stripeEvent.Id))
            {
                _eventMap.TryAdd(stripeEvent.Id, stripeEvent);
                Message message = new Amqp.Message( stripeEvent.ToJson());
                sender.Send(message);
            }
        });
        sender.Close();
        session.Close();
        connection.Close();
    }
/// <summary>
/// Checks if Stripe balance is under minimum.
/// </summary>
/// <param name="logFilePath">The path to a log file.</param>
    private void CheckBalance(string logFilePath)
    {
        var balances = _service.Get();
        var amounts = new List<long>();
        Parallel.ForEach(balances.Available, balance => { amounts.Add(balance.Amount); });
        Parallel.ForEach(amounts, amount =>
        {
            if (amount < _min)
            {
                using (FileStream logFile = new FileStream(logFilePath, FileMode.Append, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(logFile))
                {
                    sw.WriteLine("WARNING: Balance Below minimum. MIN: " +
                                 _min.ToString(CultureInfo.InvariantCulture) + " BALANCE: " +
                                 amount.ToString(CultureInfo.InvariantCulture) + " at TIME " +
                                 DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                }
                BalanceMessage(amount);
            }
        });
    }
/// <summary>
/// Checks if Stripe balance is under minimum.
/// </summary>
    private void CheckBalance()
    {
        var balances = _service.Get();
        var amounts = new List<long>();
        Parallel.ForEach(balances.Available, balance => { amounts.Add(balance.Amount); });
        Parallel.ForEach(amounts, amount =>
        {
            if (amount < _min)
            {
                 BalanceMessage(amount);
            }
        });
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var minIsPresent = true;
        var logToFile = true;
         _min = OptionalArgs.ParseMin(_logger, _configuration);
        if (_min == double.MinValue)
        {
            minIsPresent = false;
        }
        StripeConfiguration.ApiKey = RequiredArgs.GetStripeKey(_logger, _configuration);
         _stripeClient = new StripeClient(StripeConfiguration.ApiKey);
        var eventList = _stripeClient.V1.Events.ListAsync().Result.ToList();
        Parallel.ForEach(eventList, stripeEvent =>
        {
            if (!_eventMap.ContainsKey(stripeEvent.Id))
            {
                _eventMap.TryAdd(stripeEvent.Id, stripeEvent);
            }
        });
        _service = new BalanceService();
         _logFilePath = OptionalArgs.GetLogFile(_logger, _configuration);
        _address = RequiredArgs.GetAmqpPath(_logger, _configuration);
         if (string.IsNullOrWhiteSpace(_logFilePath))
        {
            logToFile = false;
        }
        var minutes = OptionalArgs.ParseMinutes(_logger, _configuration);
        while (!stoppingToken.IsCancellationRequested)
        {
            if (minIsPresent)
            {
                if (logToFile)
                {
                    CheckBalance(_logFilePath);
                }
                else
                {
                    CheckBalance();
                }
            }
            EventsMessages();
            await Task.Delay(minutes * 60 *1000, stoppingToken);
        }
    }
}