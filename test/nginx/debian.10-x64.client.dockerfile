FROM debian:buster AS build1
RUN apt-get update -y
RUN apt-get install -y wget

FROM build1 AS build2
ARG package_name
COPY ./tmp/$package_name .
RUN apt-get update -y
RUN apt-get install -y ./$package_name
