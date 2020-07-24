FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

COPY ./PostMonitor/bin/publish /App
WORKDIR /App
ENTRYPOINT ["dotnet", "PostMonitor.dll"]