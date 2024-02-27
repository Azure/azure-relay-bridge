FROM ubuntu:22.04 AS build1
RUN apt-get -qq update -y
RUN apt-get -qq install -y nginx >/dev/null
COPY index.html /var/www/html

FROM build1 AS build2
ARG package_name
COPY ./tmp/$package_name .
RUN apt-get -qq install -y ./$package_name
