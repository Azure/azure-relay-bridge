FROM debian:buster AS build
ARG package_name
COPY ./tmp/$package_name .
RUN apt-get update -y
RUN apt-get install -y apt-utils ./$package_name netcat-openbsd findutils
