ARG NODE_VERSION=current-alpine
ARG NGINX_VERSION=stable-alpine

FROM node:${NODE_VERSION} AS builder
WORKDIR /app
COPY ["package.json", "package-lock.json", "./"]
RUN --mount=type=cache,target=/root/.npm npm ci
COPY . .
RUN npm run build

FROM nginx:${NGINX_VERSION} AS runner
COPY --chown=nginx:nginx --from=builder /app/dist /usr/share/nginx/html
USER nginx
EXPOSE 443
ENTRYPOINT ["nginx", "-c", "/etc/nginx/nginx.conf"]
CMD ["-g", "daemon off;"]