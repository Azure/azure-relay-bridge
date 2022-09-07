FROM mysql:5-debian AS build
ARG package_name
COPY ./tmp/$package_name .
RUN apt-get -q update -y
RUN apt-get -q install -y ./$package_name
RUN mv /usr/local/bin/docker-entrypoint.sh /usr/local/bin/mysql-docker-entrypoint.sh
COPY docker-entrypoint.sh /usr/local/bin/
RUN chmod +x /usr/local/bin/docker-entrypoint.sh