FROM ubuntu:22.04 AS build1
RUN apt-get -qq update -y >/dev/null
RUN apt-get -qq install -y wget >/dev/null

FROM build1 AS build2
ARG package_name
COPY ./tmp/$package_name .
RUN apt-get -qq install -y ./$package_name

