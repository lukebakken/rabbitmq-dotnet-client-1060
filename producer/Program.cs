using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

/*
Console.WriteLine("PRODUCER: waiting 5 seconds to try initial connection");
Thread.Sleep(TimeSpan.FromSeconds(5));
*/

var factory = new ConnectionFactory()
{
    UserName = "guest",
    Password = "guest",
    HostName = "localhost",
    Port = 5672
};

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
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

        while (true)
        {
            string message = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt");
            var body = Encoding.ASCII.GetBytes(message);
            channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: null, body: body);
            Console.WriteLine($"PRODUCER sent {message}");
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
    }
}
