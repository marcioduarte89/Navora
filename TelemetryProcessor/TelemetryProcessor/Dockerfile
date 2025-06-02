# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY TelemetryProcessor.csproj .
RUN dotnet restore TelemetryProcessor.csproj

# Copy the rest and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM public.ecr.aws/lambda/dotnet:6
WORKDIR /var/task
COPY --from=build /app/publish .

CMD ["TelemetryProcessor::TelemetryProcessor.Functions::FunctionHandler"]
