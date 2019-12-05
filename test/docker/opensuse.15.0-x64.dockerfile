FROM opensuse/leap:15 AS build
ARG package_name
COPY ./tmp/$package_name .
RUN zypper --no-gpg-checks --non-interactive install  -y ./$package_name
RUN zypper --non-interactive  install -y netcat-openbsd findutils
