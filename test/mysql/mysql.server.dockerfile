FROM mysql:5 AS build
ARG package_name
COPY ./tmp/$package_name .
RUN apt-get update -y
RUN apt-get install -y ./$package_name
RUN mv /usr/local/bin/docker-entrypoint.sh /usr/local/bin/mysql-docker-entrypoint.sh
COPY docker-entrypoint.sh /usr/local/bin/
RUN chmod +x /usr/local/bin/docker-entrypoint.sh