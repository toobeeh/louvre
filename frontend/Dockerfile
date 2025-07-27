# Stage 1: Build the Angular app
FROM node:18 AS build

# Install Java (required by openapi-generator-cli)
RUN apt-get update && apt-get install -y default-jre && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copy package files and install dependencies
COPY package.json package-lock.json ./
RUN npm ci

# Copy the rest of the application source code
COPY . .

# Build the Angular app in production mode
RUN npm run generate:api
RUN npm run build -- --configuration production

# Stage 2: Serve the app with NGINX
FROM nginx:alpine

# Remove the default nginx website
RUN rm -rf /usr/share/nginx/html/*

# Copy the Angular build output to NGINX's html directory
COPY --from=build /app/dist/frontend/ /usr/share/nginx/html/

# Copy custom nginx config (optional)
COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
