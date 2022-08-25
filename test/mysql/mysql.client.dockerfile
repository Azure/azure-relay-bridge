FROM debian:bullseye AS build
ARG package_name
COPY ./tmp/$package_name .
RUN apt-get update -y
RUN apt-get install -y ./$package_name default-mysql-client

