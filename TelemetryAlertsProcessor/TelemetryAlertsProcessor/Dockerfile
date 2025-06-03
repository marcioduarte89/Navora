# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY TelemetryAlertsProcessor.csproj .
RUN dotnet restore TelemetryAlertsProcessor.csproj

# Copy the rest and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM public.ecr.aws/lambda/dotnet:8
WORKDIR /var/task
COPY --from=build /app/publish .

CMD ["TelemetryAlertsProcessor::AlertsProcessor.Function_FunctionHandler_Generated::FunctionHandler"]
