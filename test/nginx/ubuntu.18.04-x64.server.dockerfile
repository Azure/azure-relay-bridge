FROM ubuntu:18.04 AS build1
RUN apt-get -q update -y 
RUN apt-get -q install -y nginx 
COPY index.html /var/www/html

FROM build1 AS build2
ARG package_name
COPY ./tmp/$package_name .
RUN apt-get -q install -y ./$package_name

