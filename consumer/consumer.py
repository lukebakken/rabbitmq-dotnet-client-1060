# -*- coding: utf-8 -*-
import functools
import logging
import pika

LOG_FORMAT = (
    "%(levelname) -10s %(asctime)s %(name) -30s %(funcName) "
    "-35s %(lineno) -5d: %(message)s"
)
LOGGER = logging.getLogger(__name__)

logging.basicConfig(level=logging.INFO, format=LOG_FORMAT)


def on_message(chan, method_frame, header_frame, body, userdata=None):
    LOGGER.info(
        "Delivery properties: %s, message metadata: %s", method_frame, header_frame
    )
    LOGGER.info("Userdata: %s, message body: %s", userdata, body)
    chan.basic_ack(delivery_tag=method_frame.delivery_tag)


def main():
    queue_name = "hello"
    credentials = pika.PlainCredentials("guest", "guest")
    parameters = pika.ConnectionParameters("localhost", credentials=credentials)
    connection = pika.BlockingConnection(parameters)
    channel = connection.channel()

    channel.queue_declare(
        queue=queue_name, auto_delete=False, durable=True, exclusive=False
    )

    on_message_callback = functools.partial(on_message, userdata="on_message_userdata")
    channel.basic_consume(queue=queue_name, on_message_callback=on_message_callback)

    try:
        channel.start_consuming()
    except KeyboardInterrupt:
        channel.stop_consuming()

    connection.close()


if __name__ == "__main__":
    main()
