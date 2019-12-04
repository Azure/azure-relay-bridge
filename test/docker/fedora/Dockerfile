FROM fedora AS build
ARG rpm_package
WORKDIR /tmp
COPY ./tmp/$rpm_package .
RUN yum install -y $rpm_package
RUN yum install -y nc findutils
USER azbridge