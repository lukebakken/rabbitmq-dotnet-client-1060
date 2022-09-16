# -*- coding: utf-8 -*-
import logging
import pickle
import pika
import time

LOG_FORMAT = (
    "%(levelname) -10s %(asctime)s %(name) -30s %(funcName) "
    "-35s %(lineno) -5d: %(message)s"
)
LOGGER = logging.getLogger(__name__)

logging.basicConfig(level=logging.INFO, format=LOG_FORMAT)


def main():
    LOGGER.info("PRODUCER waiting 5 seconds to try initial connection")
    time.sleep(5)

    queue_name = "hello"
    credentials = pika.PlainCredentials("guest", "guest")
    parameters = pika.ConnectionParameters("rabbitmq", credentials=credentials)
    connection = pika.BlockingConnection(parameters)
    channel = connection.channel()

    channel.queue_declare(
        queue=queue_name, auto_delete=False, durable=True, exclusive=False
    )

    try:
        props = pika.BasicProperties(content_type="text/plain")
        while True:
            t = time.time()
            msg = str(t)
            tp = pickle.dumps(t)
            channel.basic_publish(
                exchange="", routing_key=queue_name, body=tp, properties=props
            )
            LOGGER.info("PRODUCER sent %s", msg)
            connection.process_data_events(5)
    except KeyboardInterrupt:
        channel.close()
        connection.close()


if __name__ == "__main__":
    main()
