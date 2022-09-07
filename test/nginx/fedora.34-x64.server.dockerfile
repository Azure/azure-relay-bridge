FROM fedora:34 AS build1
RUN yum install -y nginx >/dev/null
COPY index.html /usr/share/nginx/html

FROM build1 AS build2
ARG package_name
COPY ./tmp/$package_name .
RUN yum install -y ./$package_name