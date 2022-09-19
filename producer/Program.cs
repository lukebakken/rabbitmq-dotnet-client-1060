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

while (!connected)
{
    try
    {
        connection = factory.CreateConnection();
        connected = true;
    }
    catch (BrokerUnreachableException)
    {
        connected = false;
        Console.WriteLine("PRODUCER: waiting 5 seconds to re-try connection!");
        Thread.Sleep(TimeSpan.FromSeconds(5));
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
        int i = 1;
        using var channel = connection.CreateModel();

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
            channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: null, body: body);
            Console.WriteLine($"PRODUCER sent {message} - iteration {i++}");

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
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }
    }
}
