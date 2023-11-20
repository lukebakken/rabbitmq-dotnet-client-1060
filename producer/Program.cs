using System.Text;
using RabbitMQ.Client;
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
    IChannel? channel = null;
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
            Console.WriteLine("PRODUCER: waiting 5 seconds to re-try connection!");
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }

    if (connection == null)
    {
        Console.Error.WriteLine("PRODUCER: unexpected null connection");
    }
    else
    {
        int i = 1;
        channel = await connection.CreateChannelAsync();
        Dictionary<string, object>? arguments = null;
        if (useQuorumQueues)
        {
            arguments = new Dictionary<string, object> { { "x-queue-type", "quorum" } };
        }

        channel.QueueDeclare(queue: "hello", durable: true, exclusive: false, autoDelete: false, arguments);

        Console.WriteLine();
        Console.WriteLine("Press ENTER to pause / resume send loop, or CTRL-C to exit");
        Console.WriteLine();

        while (true)
        {
            string message = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.ffffff");
            var body = Encoding.ASCII.GetBytes(message);
            await channel.BasicPublishAsync(exchange: "", routingKey: "hello", body: body);
            Console.WriteLine($"PRODUCER sent {message} - iteration {i++}");

            if (iteration == 0)
            {
                /*
                * Note: this is the "warm up" iteration
                */
                Console.WriteLine($"PRODUCER first iteration done, disconnecting");
                channel?.Close();
                connection?.Close();
                connected = false;
                channel = null;
                connection = null;
                Console.WriteLine($"PRODUCER re-connecting in 10 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(10));
                break;
            }
            else
            {
                if (Console.KeyAvailable) 
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine("Send loop paused. Press any key to resume or CTRL-C to exit");
                        Console.ReadKey(true);
                    }
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(3));
                }
            }
        }
    }
}
