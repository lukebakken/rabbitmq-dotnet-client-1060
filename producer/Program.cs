using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

bool inContainer = false;
if (bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out inContainer))
{
    if (inContainer)
    {
        Console.WriteLine("CONSUMER: waiting 5 seconds to try initial connection");
        Thread.Sleep(TimeSpan.FromSeconds(5));
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

IConnection? connection = null;
Random randomGenerator = new Random();

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

using (connection)
{
    if (connection == null)
    {
        Console.Error.WriteLine("PRODUCER: unexpected null connection");
    }
    else
    {
        int messageCounter = 0;
        using IChannel channel = await connection.CreateChannelAsync();

        Dictionary<string, object>? arguments = null;
        if (useQuorumQueues)
        {
            arguments = new Dictionary<string, object> { { "x-queue-type", "quorum" } };
        }

        await channel.QueueDeclareAsync(queue: "hello", durable: true, exclusive: false, autoDelete: false, arguments);

        Console.WriteLine();
        Console.WriteLine("Press ENTER to pause / resume send loop, or CTRL-C to exit");
        Console.WriteLine();

        while (true)
        {
            int burstSize = randomGenerator.Next(1, 10);
            for (int n = 0; n < burstSize; n++)
            {
                messageCounter++;
                string sendTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.ffffff");
                var body = Encoding.ASCII.GetBytes(sendTime);
                await channel.BasicPublishAsync(exchange: "", routingKey: "hello", body: body);
                Console.WriteLine($"PRODUCER sent message {messageCounter} at {sendTime}");
            }

            Console.WriteLine();
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}
