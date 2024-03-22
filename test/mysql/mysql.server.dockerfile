# Pinning to 8.0.35 to get around libssl and libicu dependency errors
FROM mysql:8.0.35-debian AS build
ARG package_name
COPY ./tmp/$package_name .
RUN apt-get -qq update -y
RUN apt-get -qq install -y ./$package_name
RUN mv /usr/local/bin/docker-entrypoint.sh /usr/local/bin/mysql-docker-entrypoint.sh
COPY docker-entrypoint.sh /usr/local/bin/
RUN chmod +x /usr/local/bin/docker-entrypoint.sh