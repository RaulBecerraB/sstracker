# Use the official .NET SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "sstracker.csproj"
RUN dotnet publish "sstracker.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY Assets ./Assets
EXPOSE 5007
ENV ASPNETCORE_URLS=http://+:5007
ENTRYPOINT ["dotnet", "sstracker.dll"]
