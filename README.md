# 🎫 Proxan Reservation System (Enterprise .NET Core)

Bu proje, yüksek trafikli biletleme platformlarında veri tutarlılığı ve eşzamanlılık güvenliği sağlamak amacıyla geliştirilmiş, konfigüre edilebilir bir backend servisidir.

---

## 🏗️ Teknik Mimari ve Concurrency Yönetimi

Sistem, biletleme dünyasında "Race Condition" riskini sıfıra indirmek için üç aşamalı bir yapı kullanır:

### 1. Pessimistic Locking (SELECT FOR UPDATE)
Rezervasyon (`HOLD`) işlemi başladığında, Entity Framework Core üzerinden bir DB Transaction başlatılır ve ilgili satır kilitlenir. 



### 2. Double-Phase Reservation (Hold & Confirm)
- **Hold:** Bilet 5 dakikalığına geçici olarak rezerve edilir.
- **Confirm:** İşlem onaylandığında rezervasyon kesinleşir.

### 3. Background Clean-up
`.NET BackgroundService` sınıfından türetilen `ExpiredReservationWorker`, süresi dolan "Hold" kayıtlarını her 60 saniyede bir otomatik temizler.



---

## 🔐 Güvenlik ve JWT Yapılandırması

Sistem, kurumsal standartlarda **JWT (JSON Web Token)** tabanlı bir yetkilendirme mimarisine sahiptir. Ancak geliştirme ve test süreçlerini hızlandırmak için `appsettings.json` üzerinden esnek bir kontrol mekanizması sunar:

``` json
"AuthConfig": {
  "EnableAuthorize": true,
}
```

### AuthConfig Parametre Detayları:

* **EnableAuthorize (true):** JWT doğrulaması aktif hale gelir. Tüm kritik endpoint'ler (Hold, Confirm, Event Create) geçerli bir token bekler. Testlerin kesintisiz devam edebilmesi için sisteme **"proxan-admin-secret-2026-token"** adında bir sabit Master Token tanımlanmıştır.
* **EnableAuthorize (false):** JWT katmanı tamamen devre dışı bırakılır. Bu mod, özellikle CI/CD süreçlerinde veya lokal testlerde token üretme/yönetme maliyetine girmeden iş mantığını (Business Logic) hızlıca doğrulamak için kullanılır.

---

## 📂 Modüler Proje Yapısı

``` text
Proxan_dotnet/
├── ProxanReservation/             # Ana Web API Projesi (.NET 10.0)
│   ├── Controllers/               # Events & Reservations Endpointleri
│   ├── Data/                      # AppDbContext (EF Core & Npgsql)
│   ├── Migrations/                # Veritabanı Sürüm Geçmişi
│   ├── Models/                    # Domain Modelleri (Event, Reservation, User, DTOs)
│   ├── Services/                  # Business Logic (ReservationService, Worker)
│   ├── Properties/                # launchSettings.json (Lokal Çalışma Ayarları)
│   ├── appsettings.json           # JWT & DB Bağlantı Ayarları
│   └── Program.cs                 # Uygulama Giriş Noktası & DI Container
├── tests/
│   └── e2e/                       # Python Entegrasyon Testleri
│       ├── conftest.py            # Pytest Fixtures & API Client
│       ├── setup_db.py            # Test Öncesi DB Seed/Cleanup
│       └── test_reservation.py    # Senaryo Testleri
├── Proxan_dotnet.sln              # Visual Studio Solution Dosyası
└── Proxan_Dotnet_Collection.json  # Postman/Insomnia Koleksiyonu
```

---

---

## 🚀 Hızlı Başlangıç

### .NET Uygulamasını Ayağa Kaldır
``` bash
cd ProxanReservation
dotnet restore
dotnet ef database update
dotnet run
```

### Python Testlerini Çalıştırma
``` bash
cd tests/e2e
pip install pytest requests
pytest test_reservation.py -v
```

---

---

## 🛠️ API Dokümantasyonu ve Test Araçları

Proxan API, geliştirici deneyimini (Developer Experience - DX) en üst düzeye çıkarmak için modern araçlarla tam entegre çalışır.

### 1. Swagger (OpenAPI) Entegrasyonu
Proje ayağa kalktığında `/swagger/index.html` adresinde dinamik bir dokümantasyon sunar. 

- **Authorize (Kilit) Butonu:** Sağ üstte bulunan kilit simgesine tıklayarak yetkilendirme yapabilirsiniz.
- **Token Girişi:** Açılan kutuya **sadece** `proxan-admin-secret-2026-token` yazmanız yeterlidir. `bearer` prefix'i sistem tarafından otomatik eklenir.
- **Dinamik Test:** Yetki aldıktan sonra `Hold` ve `Confirm` endpoint'lerini tarayıcı üzerinden doğrudan tetikleyebilirsiniz.



### 2. Postman Koleksiyonu
Kök dizinde yer alan `Proxan_Dotnet_Collection.json` dosyası, tüm API akışını içeren hazır bir test setidir.

- **Token Yapılandırması:** Koleksiyonu import ettikten sonra "Authorization" sekmesinden `Bearer Token` seçilmeli ve değer olarak `proxan-admin-secret-2026-token` girilmelidir.
- **Merkezi Yönetim:** Koleksiyon seviyesinde tanımlanan token, altındaki tüm isteklere otomatik olarak uygulanır.



---

## 🧪 E2E Test ve Güvenlik Doğrulaması (Python Pytest)

Sistemimiz, dış dünyadan gelen isteklere doğru tepki verdiğini kanıtlayan kapsamlı bir **Integration & Security Test** katmanına sahiptir.

### 📄 conftest.py
Bu dosya API istemcisini yönetir ve test oturumu başında veritabanını otomatik olarak sıfırlar.

``` python
import pytest
import requests
import os
import sys

from setup_db import create_test_data

@pytest.fixture(scope="session")
def auto_data():
    """Veritabanını sıfırlar ve başlangıç ID'lerini döner."""
    u_id, e_id = create_test_data()
    return {"user_id": u_id, "event_id": e_id}

@pytest.fixture(scope="session")
def csharp_api():
    """API İstemcisi: Hem Yetkili hem Yetkisiz erişim simülasyonu sağlar."""
    class Client:
        url = "http://localhost:5000/api"
        valid_token = "proxan-admin-secret-2026-token"
        
        def get_headers(self, auth=True):
            token = self.valid_token if auth else "wrong-token"
            return {"Authorization": f"Bearer {token}"}

        def create_event(self, title, capacity, auth=True):
            payload = {"title": title, "capacity": capacity, "isActive": True}
            return requests.post(f"{self.url}/Events", json=payload, headers=self.get_headers(auth))

        def hold(self, event_id, user_id, auth=True):
            params = {"eventId": event_id, "userId": user_id}
            return requests.post(f"{self.url}/Reservations/hold", params=params, headers=self.get_headers(auth))

        def confirm(self, reservation_id, auth=True):
            return requests.post(f"{self.url}/Reservations/confirm/{reservation_id}", headers=self.get_headers(auth))

        def get_details(self, event_id, auth=True):
            return requests.get(f"{self.url}/Events/{event_id}/details", headers=self.get_headers(auth))

    return Client()
```

### 📄 test_reservation.py
Uçtan uca tüm iş akışını ve güvenlik katmanını doğrulayan ana test dosyasıdır.

``` python
import pytest

def test_comprehensive_flow(csharp_api, auto_data):
    """
    Security + E2E Flow: 
    Negatif Test (401) -> Event Create -> Hold -> Confirm -> Capacity Check
    """
    print("\n" + "="*50)
    print("🚀 PROXAN E2E INTEGRATION TEST")
    print("="*50)

    # 1. GÜVENLİK TESTİ (NEGATİF SENARYO)
    print("\n[STEP 1] Security Check: Hatalı Token")
    fail_res = csharp_api.create_event("Unauthorized Event", 10, auth=False)
    assert fail_res.status_code == 401
    print("✅ [PASS] Yetkisiz erişim engellendi (401).")

    # 2. ETKİNLİK OLUŞTURMA
    print("\n[STEP 2] Event Creation: Doğru Token")
    event_res = csharp_api.create_event("E2E Konser 2026", 100, auth=True)
    assert event_res.status_code == 200
    
    data = event_res.json()
    new_event_id = data.get("id") or data.get("Id")
    print(f"✅ [PASS] Etkinlik oluşturuldu (ID: {new_event_id})")

    # 3. REZERVASYON AKIŞI (HOLD & CONFIRM)
    print(f"\n[STEP 3] Reservation Workflow: Hold & Confirm")
    hold_res = csharp_api.hold(new_event_id, auto_data["user_id"])
    assert hold_res.status_code == 200
    
    res_id = hold_res.json().get("id") or hold_res.json().get("Id")
    print(f"✅ [PASS] Hold başarılı (Res ID: {res_id})")
    
    conf_res = csharp_api.confirm(res_id)
    assert conf_res.status_code == 200
    print("✅ [PASS] Rezervasyon onaylandı.")

    # 4. KAPASİTE VE SON DURUM DOĞRULAMA
    print("\n[STEP 4] Capacity & Data Integrity Check")
    detail_res = csharp_api.get_details(new_event_id)
    detail_data = detail_res.json()
    
    print(f"🔍 [DEBUG] API Yanıt İçeriği: {detail_data}")

    # API'den gelen gerçek anahtar: 'remainingCapacity'
    capacity = detail_data.get("remainingCapacity")
    
    print(f"📊 Tespit Edilen Kapasite: {capacity}")
    assert capacity is not None, "❌ HATA: JSON içinde 'remainingCapacity' bulunamadı!"
    assert int(capacity) == 99, f"❌ HATA: Kapasite 99 olmalıydı, gelen: {capacity}"
    
    # Ek Doğrulama: Onaylanmış bilet sayısını kontrol et
    confirmed = detail_data.get("confirmedCount")
    assert confirmed == 1, f"❌ HATA: Onaylanan bilet 1 olmalıydı, gelen: {confirmed}"

    print("✅ [PASS] Kapasite ve veri tutarlılığı doğrulandı.")
    print("\n" + "="*50)
    print("🏆 TÜM TEST ADIMLARI BAŞARIYLA TAMAMLANDI")
    print("="*50)
```

---

### ✅ Doğrulama ve Çıktı
Testler çalıştırıldığında, terminalde hem güvenlik katmanının hem de veritabanı işlemlerinin başarıyla tamamlandığı yeşil **PASSED** logları ile görülür.
### 📊 Örnek Test Çıktısı (Terminal)
Testler koşturulduğunda alınan başarılı sonuçlar sistemin kararlılığını kanıtlar:

```text
tests/e2e/test_reservation.py::test_comprehensive_flow 
--- DB TABLOLARI SIFIRLANDI VE YENIDEN OLUSTURULDU ---

==================================================
🚀 PROXAN E2E INTEGRATION TEST
==================================================

[STEP 1] Security Check: Hatalı Token
✅ [PASS] Yetkisiz erişim engellendi (401).

[STEP 2] Event Creation: Doğru Token
✅ [PASS] Etkinlik oluşturuldu (ID: 2)

[STEP 3] Reservation Workflow: Hold & Confirm
✅ [PASS] Hold başarılı (Res ID: 1)
✅ [PASS] Rezervasyon onaylandı.

[STEP 4] Capacity & Data Integrity Check
🔍 [DEBUG] API Yanıt İçeriği: {'id': 2, 'title': 'E2E Konser 2026', 'remainingCapacity': 99, ...}
📊 Tespit Edilen Kapasite: 99
✅ [PASS] Kapasite ve veri tutarlılığı doğrulandı.

==================================================
🏆 TÜM TEST ADIMLARI BAŞARIYLA TAMAMLANDI
==================================================
PASSED [100%]
```
---
## 🚀 Continuous Integration (GitHub Actions)

Bu proje, her `push` ve `pull request` işleminde otomatik olarak çalışan kapsamlı bir CI/CD hattına sahiptir. 

### Otomatik Test Süreci
GitHub Actions iş akışımız (`.github/workflows/dotnet.yml`) aşağıdaki adımları sırasıyla gerçekleştirir:

1.  **Environment Setup:** .NET 10 SDK ve Python 3.10 ortamları kurulur.
2.  **Docker Orchestration:** `docker-compose` kullanılarak PostgreSQL ve Redis servisleri ayağa kaldırılır.
3.  **Auto-Migration:** API servisi başladığında veritabanı şemasını otomatik olarak oluşturur.
4.  **E2E Testing:** Pytest, gerçek bir API istemcisi gibi davranarak aşağıdaki senaryoları doğrular:
    * **Security:** JWT/Token tabanlı yetkilendirme kontrolü.
    * **Event Management:** Dinamik etkinlik oluşturma.
    * **Reservation Flow:** Rezervasyonun `Hold` ve `Confirm` aşamaları.
    * **Data Integrity:** İşlem sonrası veritabanı kapasite ve tutarlılık kontrolü.

> **Not:** Test çıktıları detaylı loglama (`pytest -v -s`) ile GitHub Actions konsolunda görüntülenebilir.