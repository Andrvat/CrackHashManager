FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source
# copy csproj and restore as distinct layers
COPY *.sln .
COPY *.csproj ./
RUN dotnet restore
COPY /. ./
# copy everything else and build app
COPY /. ./
WORKDIR /source
RUN dotnet publish -c release -o /app 

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=build /app ./
EXPOSE 5180
ENTRYPOINT ["dotnet", "CrackHashManager.dll"]
