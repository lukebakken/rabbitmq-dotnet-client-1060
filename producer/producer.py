# -*- coding: utf-8 -*-
import logging
import pika

LOG_FORMAT = (
    "%(levelname) -10s %(asctime)s %(name) -30s %(funcName) "
    "-35s %(lineno) -5d: %(message)s"
)
LOGGER = logging.getLogger(__name__)

logging.basicConfig(level=logging.INFO, format=LOG_FORMAT)


def main():
    queue_name = "hello"
    credentials = pika.PlainCredentials("guest", "guest")
    parameters = pika.ConnectionParameters("localhost", credentials=credentials)
    connection = pika.BlockingConnection(parameters)
    channel = connection.channel()

    channel.queue_declare(
        queue=queue_name, auto_delete=False, durable=True, exclusive=False
    )

    try:
        props = pika.BasicProperties(content_type='text/plain')
        while (True):
            msg = "FOOBAR"
            channel.basic_publish(exchange='', routing_key=queue_name, body=msg, properties=props)
            LOGGER.info('sent msg: %s', msg)
            connection.process_data_events(5)
    except KeyboardInterrupt:
        channel.close()
        connection.close()


if __name__ == "__main__":
    main()
