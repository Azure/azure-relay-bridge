FROM opensuse/leap:15.4  AS build1
RUN zypper --non-interactive  install -y wget 

FROM build1 AS build2
ARG package_name
COPY ./tmp/$package_name .
RUN zypper --no-gpg-checks --non-interactive install  -y ./$package_name