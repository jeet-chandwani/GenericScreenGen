# Stage 1: Build Angular app
FROM node:20-alpine AS build

WORKDIR /app

# Copy package files first for layer caching on npm install
COPY package.json package-lock.json ./

RUN npm ci

# Copy source and build
COPY . .

RUN npm run build

# Stage 2: Serve with Nginx
FROM nginx:alpine AS runtime

# Remove default Nginx static assets
RUN rm -rf /usr/share/nginx/html/*

# Copy Angular build output
COPY --from=build /app/dist/clientapp /usr/share/nginx/html

# Copy custom Nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
