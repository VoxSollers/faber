# syntax=docker/dockerfile:1

# Production image for the Faber SPA — replaces src/web/Faber.NgApp/faber-ngapp
# (issue #429). Build context is this directory, NOT the repo root.
#
#   docker build -t faber-web src/web/faber-app

# ---------------------------------------------------------------- build stage
# node:24 satisfies Angular 21's ^20.19 || ^22.12 || ^24 engine floor. The old
# NgApp image used node:20 with `npm install` and a globally installed CLI; this
# one installs from the lockfile and uses the project-local CLI instead, so the
# image is reproducible and matches what CI and developers build.
FROM node:24-bookworm-slim AS build
WORKDIR /app

# Dependencies first, so this layer is cached independently of source edits.
COPY package.json package-lock.json ./
RUN npm ci --no-audit --no-fund

COPY . .
# Emits dist/faber-app/browser (no explicit outputPath in angular.json, so
# @angular/build:application uses its dist/<project>/browser default).
RUN npm run build -- --configuration=production

# -------------------------------------------------------------- runtime stage
# Debian, not Alpine: the maintenance scripts are #!/bin/bash and use GNU
# `stat -c %y`. Pinned rather than :latest so a rebuild is reproducible.
FROM nginx:1.29

# The SPA.
COPY --from=build /app/dist/faber-app/browser /usr/share/nginx/html

# The edge config ships as a template. nginx's own entrypoint renders it with
# envsubst into /etc/nginx/nginx.conf before starting. NGINX_ENVSUBST_FILTER is
# what keeps envsubst from blanking nginx's runtime variables ($host, $uri, ...);
# only FABER_-prefixed names are substituted.
#
# FABER_API_UPSTREAM must carry NO trailing slash: the config appends
# $request_uri to it verbatim, so "http://host:8080/" would proxy to
# "http://host:8080//api/v1/..." and the API would 404 on the double slash.
COPY deploy/nginx/nginx.conf.template /etc/nginx/templates/nginx.conf.template
ENV NGINX_ENVSUBST_OUTPUT_DIR="/etc/nginx" \
    NGINX_ENVSUBST_FILTER="^FABER_" \
    FABER_API_UPSTREAM="http://radju.webapi:8080" \
    FABER_APP_DOMAIN="example.com" \
    FABER_MAINTENANCE_ALLOWED_IP="127.0.0.1"

# Maintenance page plus the three toggle scripts used by the private
# production deployment.
COPY deploy/maintenance/ /usr/share/nginx/html/maintenance/
COPY deploy/scripts/ /usr/local/bin/
RUN chmod +x /usr/local/bin/maintenance-*.sh

EXPOSE 80 443

# ENTRYPOINT (/docker-entrypoint.sh) and CMD (nginx -g "daemon off;") are
# inherited from the base image — the entrypoint is what runs envsubst, so do
# not override it.
