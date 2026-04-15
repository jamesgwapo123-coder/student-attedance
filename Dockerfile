# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AttendanceSystem/AttendanceSystem.csproj", "AttendanceSystem/"]
RUN dotnet restore "AttendanceSystem/AttendanceSystem.csproj"
COPY . .
WORKDIR "/src/AttendanceSystem"
RUN dotnet build "AttendanceSystem.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AttendanceSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV PORT=8080
ENTRYPOINT ["dotnet", "AttendanceSystem.dll"]
