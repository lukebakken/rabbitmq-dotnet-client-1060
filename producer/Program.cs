using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

Console.WriteLine("PRODUCER: waiting 5 seconds to try initial connection");
Thread.Sleep(TimeSpan.FromSeconds(5));

var factory = new ConnectionFactory()
{
    UserName = "zyuser",
    Password = "zypassword",
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
        int messageCounter = 0;
        using var channel = connection.CreateModel();

        Dictionary<string, object>? arguments = null;
        if (useQuorumQueues)
            arguments = new Dictionary<string, object> { { "x-queue-type", "quorum" } };

        channel.QueueDeclare(queue: "hello", durable: useQuorumQueues, exclusive: false, autoDelete: false, arguments);

        Console.WriteLine();
        Console.WriteLine("Press ENTER to pause / resume send loop, or CTRL-C to exit");
        Console.WriteLine();

        while (true)
        {
            int burstSize = randomGenerator.Next(1, 10);

            for (int n = 0; n < burstSize; n++)
            {
                messageCounter++;
                string sendTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff");
                var body = Encoding.ASCII.GetBytes(sendTime);
                channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: null, body: body);
                Console.WriteLine($"PRODUCER sent message {messageCounter} at {sendTime}");
            }

            Console.WriteLine();
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
    }
}
