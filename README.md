```
git clone https://github.com/lukebakken/rabbitmq-dotnet-client-1060.git
cd rabbitmq-dotnet-client-1060
git submodule update --init
make certs
docker compose build
docker compose up
```

If your build is sucessful, output will look like this:

```
rabbitmq-dotnet-client-1060-rmq-1       | 2022-09-15 22:23:06.428334+00:00 [info] <0.692.0> connection <0.692.0> (172.19.0.4:40866 -> 172.19.0.2:5671): user 'guest' authenticated and granted access to vhost '/'
rabbitmq-dotnet-client-1060-consumer-1  | CONSUMER: waiting for messages...
rabbitmq-dotnet-client-1060-consumer-1  | CONSUMER received 09/15/2022 10:23:06.282 PM at 09/15/2022 10:23:06.437 PM
rabbitmq-dotnet-client-1060-producer-1  | PRODUCER sent 09/15/2022 10:23:11.285 PM
```
