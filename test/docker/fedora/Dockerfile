FROM fedora AS build
ARG rpm_package
WORKDIR /tmp
COPY ./tmp/$rpm_package .
USER root
# temporary workaround for RPM packaging issue
RUN sed -i '/tsflags=nodocs/d' /etc/dnf/dnf.conf
RUN yum update -y
#RUN yum install -y nc libstdc++ libunwind libicu compat-openssl10
RUN useradd -ms /bin/bash azbridge
#RUN rpm -i $rpm_package
RUN yum install -y $rpm_package
RUN yum install -y nc findutils
USER azbridge