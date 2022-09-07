FROM debian:bullseye AS build1
RUN apt-get update -y
RUN apt-get install -y default-mysql-client

FROM build1 AS build2
ARG package_name
COPY ./tmp/$package_name .
RUN apt-get install -y ./$package_name

