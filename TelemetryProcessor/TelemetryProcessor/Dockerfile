FROM public.ecr.aws/lambda/dotnet:6

COPY . /var/task

CMD ["TelemetryProcessor::TelemetryProcessor.Function::FunctionHandler"]
