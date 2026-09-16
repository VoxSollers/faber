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
# Alpine, not Debian: this stage only serves static files now, so the smaller
# base is preferable. Pinned rather than :latest so a rebuild is reproducible.
FROM nginx:1.29-alpine

# The SPA.
COPY --from=build /app/dist/faber-app/browser /usr/share/nginx/html

# A complete, non-templated config — no envsubst, no environment variables.
COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget -q --spider http://127.0.0.1/healthz || exit 1

# ENTRYPOINT (/docker-entrypoint.sh) and CMD (nginx -g "daemon off;") are
# inherited from the base image.
