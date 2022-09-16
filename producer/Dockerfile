# syntax=docker/dockerfile:1
FROM python:3-alpine
WORKDIR /app
RUN python -m pip install --upgrade pip pipenv
COPY Pipfile* .
RUN pipenv install
COPY producer.py .
ENTRYPOINT ["pipenv", "run", "main"]
