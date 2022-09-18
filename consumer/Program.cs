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

Console.WriteLine("CONSUMER: waiting 5 seconds to try initial connection");
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
        int i = 1;

        using (var channel = connection.CreateModel())
        {
            Dictionary<string, object>? arguments = null;
            if (useQuorumQueues)
                arguments = new Dictionary<string, object> { { "x-queue-type", "quorum" } };

            channel.QueueDeclare(queue: "hello", durable: useQuorumQueues, exclusive: false, autoDelete: false, arguments);

            Console.WriteLine("CONSUMER: waiting for messages...");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                string message = Encoding.ASCII.GetString(body);
                DateTime received = DateTime.Now;
                DateTime sent = DateTime.ParseExact(message, "MM/dd/yyyy HH:mm:ss.fff", null);
                TimeSpan delay = received - sent;
                string receivedText = received.ToString("MM/dd/yyyy HH:mm:ss.fff");
                Console.WriteLine($"CONSUMER received at {receivedText}, sent at {message} - iteration: {i++}, delay: {delay.TotalMilliseconds} ms");
            };

            channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);

            latch.WaitOne();
        }
    }
}
