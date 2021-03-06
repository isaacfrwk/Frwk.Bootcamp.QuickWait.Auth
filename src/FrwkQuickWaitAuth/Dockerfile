#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .
COPY ["../src/FrwkQuickWait.Infrastructure/FrwkQuickWait.Infrastructure.csproj", "FrwkQuickWait.Data/"]
COPY ["../src/FrwkQuickWait.Service/FrwkQuickWait.Service.csproj", "FrwkQuickWait.Service/"]
COPY ["../src/FrwkQuickWait.Domain/FrwkQuickWait.Domain.csproj", "FrwkQuickWait.Domain/"]
RUN dotnet restore "src/FrwkQuickWaitAuth/FrwkQuickWaitAuth.csproj"
COPY . .
WORKDIR "/src/src/FrwkQuickWaitAuth"
RUN dotnet build "FrwkQuickWaitAuth.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FrwkQuickWaitAuth.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FrwkQuickWaitAuth.dll"]