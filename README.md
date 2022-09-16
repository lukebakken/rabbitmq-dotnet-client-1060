Run RabbitMQ in docker on local PC with this command

    docker run -d -p 5672:5672 -e RABBITMQ_DEFAULT_USER=zyuser -e RABBITMQ_DEFAULT_PASS=zypassword --rm rabbitmq

1. Build the solution
2. Run consumer.exe
3. Run producer.exe

Output will look like this:

```
CONSUMER: waiting 5 seconds to try initial connection
CONSUMER: waiting for messages...
CONSUMER received 09-16-2022 12:50:59.937  iteration 1 at 09-16-2022 12:50:59.975  - delay: 38,8116 ms
CONSUMER received 09-16-2022 12:51:04.977  iteration 2 at 09-16-2022 12:51:04.984  - delay: 7,5664 ms
CONSUMER received 09-16-2022 12:51:09.983  iteration 3 at 09-16-2022 12:51:09.985  - delay: 2,696 ms
CONSUMER received 09-16-2022 12:51:14.996  iteration 4 at 09-16-2022 12:51:15.000  - delay: 4,8325 ms
CONSUMER received 09-16-2022 12:51:20.013  iteration 5 at 09-16-2022 12:51:20.015  - delay: 2,8959 ms
CONSUMER received 09-16-2022 12:51:25.014  iteration 6 at 09-16-2022 12:51:25.016  - delay: 2,616 ms
```

Delivery of first message is 4 - 5 times as slow as the subsequent messages.
