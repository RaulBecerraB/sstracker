# Build stage for React frontend
FROM node:18-alpine AS react-build
WORKDIR /app/clientapp
COPY ClientApp/package*.json ./
RUN npm ci --only=production
COPY ClientApp/ ./
RUN npm run build

# Build stage for .NET backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dotnet-build
WORKDIR /src

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published app
COPY --from=dotnet-build /app/publish .

# Copy React build files to wwwroot
COPY --from=react-build /app/clientapp/build ./wwwroot

# Copy Assets folder
COPY Assets ./Assets

# Expose port
EXPOSE 5007

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5007
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "sstracker.dll"]
