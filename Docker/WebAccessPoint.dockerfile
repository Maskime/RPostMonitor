FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

COPY ./NewPostPoller/bin/Release/netcoreapp3.0/publish App/
WORKDIR /App
VOLUME /RedditDownloads
ENTRYPOINT ["dotnet", "NewPostPoller.dll"]