FROM tgagor/centos:9 AS build1
RUN yum install -y wget >/dev/null

FROM build1 AS build2
ARG package_name
COPY ./tmp/$package_name .
RUN yum install -y ./$package_name

