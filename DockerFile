# 1. Temel imajı belirle
FROM python:3.10-slim

# 2. psycopg2 ve curl için gerekli sistem araçlarını kur
RUN apt-get update && apt-get install -y \
    libpq-dev \
    gcc \
    curl \
    && rm -rf /var/lib/apt/lists/*

# 3. Klasörü oluştur ve içine gir
WORKDIR /app

# 4. Bağımlılıkları kopyala ve kur
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# 5. Tüm proje dosyalarını kopyala
COPY . .

# 6. Uygulamanın çalışacağı portu dışarı aç
EXPOSE 5000