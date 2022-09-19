# -*- coding: utf-8 -*-
import logging
import pika
import datetime
import time

LOG_FORMAT = (
    "%(levelname) -10s %(asctime)s %(name) -30s %(funcName) "
    "-35s %(lineno) -5d: %(message)s"
)
LOGGER = logging.getLogger(__name__)

logging.basicConfig(level=logging.INFO, format=LOG_FORMAT)


def maybe_delay():
    import ast
    import os

    env_container = os.getenv("PYTHON_RUNNING_IN_CONTAINER")
    if env_container:
        try:
            in_container = ast.literal_eval(env_container.title())
            if in_container:
                LOGGER.info("CONSUMER waiting 5 seconds to try initial connection")
                time.sleep(5)
        except ValueError:
            pass


def main():
    maybe_delay()

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
            # DateTime sent = DateTime.ParseExact(message, "MM/dd/yyyy HH:mm:ss.fff", null);
            dt = datetime.datetime.now().strftime("%m/%d/%Y %H:%M:%S.%f")
            channel.basic_publish(
                exchange="", routing_key=queue_name, body=dt, properties=props
            )
            LOGGER.info("PRODUCER sent %s", dt)
            connection.process_data_events(5)
    except KeyboardInterrupt:
        channel.close()
        connection.close()


if __name__ == "__main__":
    main()
