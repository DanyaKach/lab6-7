FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Lab4.Data/Lab4.Data.csproj", "Lab4.Data/"]
RUN dotnet restore "Lab4.Data/Lab4.Data.csproj"
COPY . .
WORKDIR "/src/Lab4.Data"
ARG CACHEBUST=1
RUN dotnet publish "Lab4.Data.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Lab4.Data.dll"]
