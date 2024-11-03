FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY /home/runner/work/umps/umps/publish/ .
EXPOSE 5120
ENTRYPOINT ["dotnet", "umps.dll"]