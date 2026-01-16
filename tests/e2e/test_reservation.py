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
    
    # Ek Doğrulama: Onaylanmış bilet sayısını da kontrol edelim
    confirmed = detail_data.get("confirmedCount")
    assert confirmed == 1, f"❌ HATA: Onaylanan bilet 1 olmalıydı, gelen: {confirmed}"

    print("✅ [PASS] Kapasite ve veri tutarlılığı doğrulandı.")
    print("\n" + "="*50)
    print("🏆 TÜM TEST ADIMLARI BAŞARIYLA TAMAMLANDI")
    print("="*50)