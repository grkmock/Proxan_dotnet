import pytest
import requests
import sys
import os

# Veritabanı scriptini (setup_db.py) dahil et
current_dir = os.path.dirname(__file__)
sys.path.append(current_dir)
from setup_db import create_test_data

@pytest.fixture(scope="session")
def auto_data():
    """DB'yi her test oturumu başında sıfırlar ve test verisi oluşturur."""
    u_id, e_id = create_test_data()
    return {"user_id": u_id, "event_id": e_id}

@pytest.fixture(scope="session")
def csharp_api():
    """
    API İstemcisi: Yetkili (Valid) ve Yetkisiz (Invalid) token desteği sağlar.
    """
    class Client:
        url = "http://localhost:5000/api"
        valid_token = "proxan-admin-secret-2026-token"
        invalid_token = "wrong-token-123"

        def get_headers(self, authorized=True):
            """Header yönetimini merkezileştirir."""
            token = self.valid_token if authorized else self.invalid_token
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