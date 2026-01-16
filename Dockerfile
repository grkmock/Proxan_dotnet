# .NET 10 SDK imajını kullanıyoruz
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /app

# Bağımlılıkları kopyala ve geri yükle
COPY ProxanReservation/*.csproj ./ProxanReservation/
RUN dotnet restore ProxanReservation/ProxanReservation.csproj

# Tüm dosyaları kopyala ve projeyi yayınla
COPY . .
RUN dotnet publish ProxanReservation/ProxanReservation.csproj -c Release -o out

# Çalışma ortamı olarak da .NET 10 ASP.NET imajını kullanıyoruz
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview
WORKDIR /app
COPY --from=build /app/out .

# Testler için Python kurulumu
RUN apt-get update && apt-get install -y python3 python3-pip
COPY requirements.txt .
RUN pip3 install --no-cache-dir -r requirements.txt --break-system-packages

EXPOSE 8080
ENTRYPOINT ["dotnet", "ProxanReservation.dll"]