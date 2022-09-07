FROM ubuntu:20.04 AS build1
RUN apt-get -q update -y >/dev/null
RUN apt-get -q install -y wget >/dev/null

FROM build1 AS build2
ARG package_name
COPY ./tmp/$package_name .
RUN apt-get -q install -y ./$package_name

