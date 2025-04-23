#.NET SDK image to build app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj .
RUN dotnet restore 

# Copy everything else and build app
COPY . ./
WORKDIR /app
# Build and publish app
RUN dotnet publish -c release -o /publish --no-restore


# ASP.NET image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
# EXPOSE 8081

# Copy compiled from build into runtime image
COPY --from=build /publish ./

# Start app
ENTRYPOINT [ "dotnet", "quick-share.api.dll" ]