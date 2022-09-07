FROM debian:buster AS build1
RUN apt-get -qq update -y 
RUN apt-get -qq install -y wget 

FROM build1 AS build2
ARG package_name
COPY ./tmp/$package_name .
RUN apt-get install -y ./$package_name
