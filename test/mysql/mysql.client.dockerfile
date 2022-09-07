FROM debian:bullseye AS build1
RUN apt-get -qq update -y
RUN apt-get -qq install -y default-mysql-client

FROM build1 AS build2
ARG package_name
COPY ./tmp/$package_name .
RUN apt-get -qq install -y ./$package_name

