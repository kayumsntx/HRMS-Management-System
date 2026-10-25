ARG DOTNET_VERSION=9.0
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

COPY *.sln ./
COPY HRMS.Domain/HRMS.Domain.csproj HRMS.Domain/
COPY HRMS.Application/HRMS.Application.csproj HRMS.Application/
COPY HRMS.Infrastructure/HRMS.Infrastructure.csproj HRMS.Infrastructure/
COPY HRMS.API/HRMS.API.csproj HRMS.API/
RUN dotnet restore HRMS.API/HRMS.API.csproj

COPY . .
RUN dotnet publish HRMS.API/HRMS.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final
WORKDIR /app
COPY --from=build /app/publish .

# uploaded files and logs live on a volume, owned by the non-root user
RUN mkdir -p /data/storage /data/logs && chown -R app /data
USER app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "HRMS.API.dll"]
