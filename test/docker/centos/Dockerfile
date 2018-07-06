FROM centos AS build
ARG rpm_package
WORKDIR /tmp
COPY ./tmp/$rpm_package .
USER root
# temporary workaround for RPM packaging issue
RUN sed -i '/tsflags=nodocs/d' /etc/yum.conf
RUN yum update -y
RUN useradd -ms /bin/bash azbridge
RUN yum install -y $rpm_package
RUN yum install -y nc findutils
USER azbridge