# Build node tool
FROM node:20 AS node-tool
WORKDIR /app

COPY renderer/package.json .
COPY renderer/package-lock.json .
RUN npm install

# Copy the renderer source code
COPY renderer/ .

# Build the renderer
RUN npm run build && rm -rf node_modules

# Build dotnet app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install NSwag CLI globally
RUN dotnet tool install -g NSwag.ConsoleCore --version 14.4.0

# Add dotnet tools to PATH
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy project file and restore dependencies
COPY api/tobeh.Louvre.Server/tobeh.Louvre.Server.csproj ./tobeh.Louvre.Server/
RUN dotnet restore ./tobeh.Louvre.Server/tobeh.Louvre.Server.csproj

# Copy the entire project
COPY api/tobeh.Louvre.Server/ ./tobeh.Louvre.Server/
WORKDIR /src/tobeh.Louvre.Server

# Publish the app
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install gifsicle
RUN apt-get update && \
    apt-get install -y gifsicle && \
    rm -rf /var/lib/apt/lists/*

# Install node
RUN apt-get update && \
    apt-get install -y nodejs npm && \
    rm -rf /var/lib/apt/lists/*

# Copy the node renderer 
COPY --from=node-tool /app/ /renderer/

# Install node dependencies
RUN cd /renderer && npm install --production

# Copy app
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Start app
ENTRYPOINT ["dotnet", "tobeh.Louvre.Server.dll"]
