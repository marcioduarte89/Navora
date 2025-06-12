# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY TelemetryAdmin.csproj .
RUN dotnet restore TelemetryAdmin.csproj

# Copy the rest and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM public.ecr.aws/lambda/dotnet:8
WORKDIR /var/task
COPY --from=build /app/publish .

CMD ["TelemetryAdmin::TelemetryAdmin.Functions_FunctionHandler::FunctionHandler"]
