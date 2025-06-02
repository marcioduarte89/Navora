RUN dotnet restore ./TelemetryProcessor/TelemetryProcessor.csproj

# Copy the rest and publish
COPY TelemetryProcessor/ ./TelemetryProcessor/
WORKDIR /src/TelemetryProcessor
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM public.ecr.aws/lambda/dotnet:6
WORKDIR /var/task
COPY --from=build /app/publish .

# Replace with your actual handler
CMD ["TelemetryProcessor::TelemetryProcessor.Functions::FunctionHandler"]
