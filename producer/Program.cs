using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory() { HostName = "rabbitmq" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

string message = "Hello World!";
var body = Encoding.UTF8.GetBytes(message);

while(true)
{
    channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: null, body: body);
    Console.WriteLine($" [x] Sent {message}");
    Thread.Sleep(TimeSpan.FromSeconds(5));
}
