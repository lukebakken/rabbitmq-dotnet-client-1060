using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

bool inContainer = false;
if (bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out inContainer))
{
    if (inContainer)
    {
        Console.WriteLine("CONSUMER: waiting 5 seconds to try initial connection");
        await Task.Delay(TimeSpan.FromSeconds(5));
    }
}

AutoResetEvent latch = new AutoResetEvent(false);

void CancelHandler(object? sender, ConsoleCancelEventArgs e)
{
    Console.WriteLine("CTRL-C pressed, exiting!");
    e.Cancel = true;
    latch.Set();
}

Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelHandler);

var factory = new ConnectionFactory()
{
    UserName = "guest",
    Password = "guest",
    HostName = "localhost",
    Port = 5672
};

bool useQuorumQueues = false;
bool connected = false;

for (ushort iteration = 0; iteration < 2; iteration++)
{
    IConnection? connection = null;
    while (!connected)
    {
        try
        {
            connection = await factory.CreateConnectionAsync();
            connected = true;
        }
        catch (BrokerUnreachableException)
        {
            connected = false;
            Console.WriteLine("CONSUMER: waiting 5 seconds to re-try connection!");
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }

    if (connection == null)
    {
        Console.Error.WriteLine("CONSUMER: unexpected null connection");
    }
    else
    {
        int i = 1;
        IChannel channel = await connection.CreateChannelAsync();
        Dictionary<string, object>? arguments = null;
        if (useQuorumQueues)
        {
            arguments = new Dictionary<string, object> { { "x-queue-type", "quorum" } };
        }
        await channel.QueueDeclareAsync(queue: "hello", durable: true, exclusive: false, autoDelete: false, arguments);

        Console.WriteLine("CONSUMER: waiting for messages...");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            DateTime received = DateTime.Now;
            var body = ea.Body.ToArray();
            string message = Encoding.ASCII.GetString(body);
            DateTime sent = DateTime.ParseExact(message, "MM/dd/yyyy HH:mm:ss.ffffff", null);
            TimeSpan delay = received - sent;
            string receivedText = received.ToString("MM/dd/yyyy HH:mm:ss.ffffff");
            Console.WriteLine($"CONSUMER received at {receivedText}, sent at {message} - iteration: {i++}, delay: {delay}");
        };

        if (iteration == 0)
        {
            /*
            * Note: this is the "warm up" iteration
            */

            BasicGetResult result;
            do
            {
                result = await channel.BasicGetAsync(queue: "hello", autoAck: true);
                if (result == null)
                {
                    Console.WriteLine($"CONSUMER first iteration - waiting for one message");
                    if (latch.WaitOne(TimeSpan.FromSeconds(1)))
                    {
                        Environment.Exit(0);
                    }
                }
                else
                {
                    break;
                }
            }
            while (true);

            var body = result.Body.ToArray();
            string message = Encoding.ASCII.GetString(body);
            Console.WriteLine($"CONSUMER first iteration done (message: {message}), disconnecting and re-connecting...");
            channel.Close();
            connection.Close();
            connected = false;
        }
        else
        {
            await channel.BasicConsumeAsync(queue: "hello", autoAck: true, consumer: consumer);
            latch.WaitOne();
        }
    }
}
