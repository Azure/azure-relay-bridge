FROM ubuntu:xenial AS build
ARG deb_package
WORKDIR /tmp
COPY ./tmp/$deb_package .
USER root
# temporary workaround for RPM packaging issue
# RUN sed -i '/tsflags=nodocs/d' /etc/dnf/dnf.conf
RUN apt-get update -y
#RUN yum install -y nc libstdc++ libunwind libicu compat-openssl10
RUN useradd -ms /bin/bash azbridge
#RUN rpm -i $deb_package
RUN apt-get install -y netcat-openbsd findutils gdebi-core
RUN gdebi -n $deb_package
USER azbridge