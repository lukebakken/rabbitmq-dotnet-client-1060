using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

AutoResetEvent latch = new AutoResetEvent(false);

void CancelHandler(object? sender, ConsoleCancelEventArgs e)
{
    Console.WriteLine("CTRL-C pressed, exiting!");
    e.Cancel = true;
    latch.Set();
}

Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelHandler);

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
        Console.WriteLine("CONSUMER: waiting 5 seconds to re-try connection!");
        Thread.Sleep(TimeSpan.FromSeconds(5));
    }
}

using (connection)
{
    if (connection == null)
    {
        Console.Error.WriteLine("CONSUMER: unexpected null connection");
    }
    else
    {
        int messageCounter = 0;

        using (var channel = connection.CreateModel())
        {
            Dictionary<string, object>? arguments = null;
            if (useQuorumQueues)
            {
                arguments = new Dictionary<string, object> { { "x-queue-type", "quorum" } };
            }
            channel.QueueDeclare(queue: "hello", durable: true, exclusive: false, autoDelete: false, arguments);

            Console.WriteLine("CONSUMER: waiting for messages...");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                DateTime received = DateTime.Now;
                messageCounter++;
                var body = ea.Body.ToArray();
                string message = Encoding.ASCII.GetString(body);
                DateTime sent = DateTime.ParseExact(message, "MM/dd/yyyy HH:mm:ss.ffffff", null);
                TimeSpan delay = received - sent;
                string receivedText = received.ToString("MM/dd/yyyy HH:mm:ss.ffffff");
                Console.WriteLine($"CONSUMER received at {receivedText}, sent at {message} - iteration: {messageCounter}, delay: {delay}");
            };

            channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);

            latch.WaitOne();
        }
    }
}
