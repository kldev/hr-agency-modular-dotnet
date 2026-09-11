# Stage 1: Build
FROM node:24-alpine AS builder

ARG VITE_API_URL=
ARG VITE_APP_ENV=production
ARG VITE_DEFAULT_EMAIL=j.smith@hr-agency.com
ARG VITE_DEFAULT_PASSWORD=agent999!
ARG VITE_DEFAULT_OWNER=admin@hr-agency.com

WORKDIR /app

# Enable Corepack for Yarn 4
RUN corepack enable

# Copy dependency files first for better Docker layer caching
COPY package.json yarn.lock .yarnrc.yml ./
COPY .yarn/ .yarn/

# Install dependencies
RUN yarn install --immutable

# Copy frontend source
COPY . .

# Vite variables are resolved at build time
ENV VITE_API_URL=${VITE_API_URL}
ENV VITE_APP_ENV=${VITE_APP_ENV}
ENV VITE_DEFAULT_OWNER=${VITE_DEFAULT_OWNER}
ENV VITE_DEFAULT_EMAIL=${VITE_DEFAULT_EMAIL}
ENV VITE_DEFAULT_PASSWORD=${VITE_DEFAULT_PASSWORD}

# Build
RUN yarn build


# Stage 2: Production
FROM nginx:alpine

COPY docker/nginx-front.conf /etc/nginx/conf.d/default.conf

COPY --from=builder /app/dist /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]