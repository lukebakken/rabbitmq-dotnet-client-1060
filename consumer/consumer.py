# -*- coding: utf-8 -*-
import datetime
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

i = 0


def on_message(chan, method_frame, _header_frame, body):
    global i
    # LOGGER.info(
    #     "Delivery properties: %s, message metadata: %s", method_frame, header_frame
    # )
    # LOGGER.info("Userdata: %s, message body: %s", userdata, body)
    dt = datetime.datetime.now()
    msg_dt = datetime.datetime.fromtimestamp(pickle.loads(body))
    delta = dt - msg_dt
    i = i + 1
    LOGGER.info("CONSUMER received %s iteration %d at now %s - delay: %s ms", msg_dt, i, dt, delta)
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

    channel.basic_consume(queue=queue_name, on_message_callback=on_message)

    try:
        channel.start_consuming()
    except KeyboardInterrupt:
        channel.stop_consuming()

    connection.close()


if __name__ == "__main__":
    main()
