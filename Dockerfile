FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# .NET Bağımlılıklarını kur ve yayınla
COPY ProxanReservation/*.csproj ./ProxanReservation/
RUN dotnet restore ProxanReservation/ProxanReservation.csproj

COPY . .
RUN dotnet publish ProxanReservation/ProxanReservation.csproj -c Release -o out

# Python ve Test araçlarını ekle
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

RUN apt-get update && apt-get install -y python3 python3-pip
COPY requirements.txt .
RUN pip3 install --no-cache-dir -r requirements.txt --break-system-packages

EXPOSE 5000
ENTRYPOINT ["dotnet", "ProxanReservation.dll"]