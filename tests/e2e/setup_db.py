import psycopg2
from datetime import datetime, timedelta

def create_test_data():
    try:
        conn = psycopg2.connect("dbname=proxan_db user=postgres password=1234 host=localhost")
        cur = conn.cursor()

        # Tabloları sil ve en baştan, yeni şemaya göre oluştur
        cur.execute("DROP TABLE IF EXISTS reservations, events, users CASCADE;")

        # 1. Users Tablosu
        cur.execute("CREATE TABLE users (id SERIAL PRIMARY KEY, username TEXT, email TEXT);")
        
        # 2. Events Tablosu (Yeni kolonlarla)
        cur.execute("""
            CREATE TABLE events (
                id SERIAL PRIMARY KEY, 
                title TEXT, 
                capacity INTEGER, 
                available_capacity INTEGER, 
                start_date TIMESTAMP, 
                end_date TIMESTAMP, 
                is_active BOOLEAN
            );
        """)

        # 3. Reservations Tablosu
        cur.execute("""
            CREATE TABLE reservations (
                id SERIAL PRIMARY KEY,
                event_id INTEGER REFERENCES events(id),
                user_id INTEGER REFERENCES users(id),
                state TEXT,
                created_at TIMESTAMP,
                expires_at TIMESTAMP
            );
        """)

        # Seed Verileri
        cur.execute("INSERT INTO users (username, email) VALUES (%s, %s) RETURNING id;", ("testuser", "test@proxan.com"))
        user_id = cur.fetchone()[0]

        now = datetime.utcnow()
        cur.execute("""
            INSERT INTO events (title, capacity, available_capacity, start_date, end_date, is_active) 
            VALUES (%s, %s, %s, %s, %s, %s) RETURNING id;
        """, ("Konser", 100, 100, now, now + timedelta(hours=2), True))
        event_id = cur.fetchone()[0]

        conn.commit()
        cur.close()
        conn.close()
        print("--- DB TABLOLARI SIFIRLANDI VE YENIDEN OLUSTURULDU ---")
        return user_id, event_id

    except Exception as e:
        print(f"[ERROR] DB Hatası: {e}")
        return None, None