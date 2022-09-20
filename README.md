Run RabbitMQ in docker on local PC with this command

    docker run -d -p 5672:5672 -e RABBITMQ_DEFAULT_USER=zyuser -e RABBITMQ_DEFAULT_PASS=zypassword --rm rabbitmq

1. Build the solution
2. Run consumer.exe
3. Run producer.exe

The producer sends a burst of 1 to 9 messages to the consumer.

Output will look like this:

```
CONSUMER: waiting 5 seconds to try initial connection
CONSUMER: waiting for messages...
CONSUMER received message 1 at 09-20-2022 09:23:52.473, delay: 30,5195 ms
CONSUMER received message 2 at 09-20-2022 09:23:52.510, delay: 49,3794 ms
CONSUMER received message 3 at 09-20-2022 09:23:52.510, delay: 49,8232 ms
CONSUMER received message 4 at 09-20-2022 09:23:57.469, delay: 5,5229 ms
CONSUMER received message 5 at 09-20-2022 09:23:57.470, delay: 5,2937 ms
CONSUMER received message 6 at 09-20-2022 09:24:02.497, delay: 12,1088 ms
CONSUMER received message 7 at 09-20-2022 09:24:02.502, delay: 9,989 ms
CONSUMER received message 8 at 09-20-2022 09:24:02.505, delay: 8,349 ms
CONSUMER received message 9 at 09-20-2022 09:24:02.512, delay: 15,5252 ms
CONSUMER received message 10 at 09-20-2022 09:24:02.521, delay: 23,2979 ms
CONSUMER received message 11 at 09-20-2022 09:24:02.521, delay: 22,9266 ms
CONSUMER received message 12 at 09-20-2022 09:24:02.522, delay: 22,1546 ms
CONSUMER received message 13 at 09-20-2022 09:24:02.522, delay: 20,4214 ms
CONSUMER received message 14 at 09-20-2022 09:24:07.517, delay: 3,8164 ms
CONSUMER received message 15 at 09-20-2022 09:24:07.518, delay: 2,6562 ms
CONSUMER received message 16 at 09-20-2022 09:24:07.518, delay: 2,971 ms
CONSUMER received message 17 at 09-20-2022 09:24:07.519, delay: 3,1154 ms
CONSUMER received message 18 at 09-20-2022 09:24:12.535, delay: 13,0199 ms
CONSUMER received message 19 at 09-20-2022 09:24:12.541, delay: 12,4516 ms
CONSUMER received message 20 at 09-20-2022 09:24:12.543, delay: 8,7846 ms
CONSUMER received message 21 at 09-20-2022 09:24:12.545, delay: 4,7128 ms
CONSUMER received message 22 at 09-20-2022 09:24:12.549, delay: 3,7252 ms
CONSUMER received message 23 at 09-20-2022 09:24:12.560, delay: 6,016 ms
CONSUMER received message 24 at 09-20-2022 09:24:12.564, delay: 3,1196 ms
CONSUMER received message 25 at 09-20-2022 09:24:12.567, delay: 3,7524 ms
```

## Observations

Delivery of the first burst of messages is 4 - 5 times as slow as the subsequent messages.

From then on the delivery times vary a lot.

The times above was using RabbitMQ running in a docker container on the same PC.

Related RabbitMQ .NET Client issue<br />
https://github.com/rabbitmq/rabbitmq-dotnet-client/issues/1252