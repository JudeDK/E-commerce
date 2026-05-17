import pandas as pd
import numpy as np
import json
import psycopg2
import os
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.metrics.pairwise import cosine_similarity

# ===============================
# 1. Configurare Cale Export și Conexiune
# ===============================

# Calea furnizată de tine pentru proiectul .NET
# Folosim 'r' în fața ghilimelelor pentru a trata corect caracterele '\' din Windows
DESTINATION_PATH = r"C:\Users\User\Desktop\ProiectWeb\ProiectWeb\hybrid_model.json"

db_config = {
    "dbname": "E-commerce",
    "user": "postgres",
    "password": "mPyJjJeM",
    "host": "127.0.0.1",
    "port": "5433"
}

def load_data():
    try:
        conn = psycopg2.connect(**db_config)
        
        # Citire Produse
        query_products = 'SELECT "Id" as "productId", "Name" as name, "Category" as category, "Description" as description FROM "Products"'
        products_df = pd.read_sql_query(query_products, conn)

        # Citire Comenzi (Join între OrderItems și Orders)
        query_orders = """
            SELECT o."UserId" as "userId", i."ProductId" as "productId", 1 as rating, o."Id" as "orderId"
            FROM "OrderItems" i
            JOIN "Orders" o ON i."OrderId" = o."Id"
        """
        orders_df = pd.read_sql_query(query_orders, conn)

        conn.close()
        return products_df, orders_df
    except Exception as e:
        print(f"Eroare la încărcarea datelor din baza de date: {e}")
        return None, None

# ===============================
# 2. Procesare și Antrenare
# ===============================
print("Pornire proces de antrenare...")
products_df, orders_df = load_data()

if products_df is None:
    print("Eroare critică: Nu s-au putut prelua produsele.")
    exit()

if orders_df is None or orders_df.empty:
    print("Info: Nu există comenzi. Modelul se va baza exclusiv pe conținutul text.")
    orders_df = pd.DataFrame(columns=["userId", "productId", "rating", "orderId"])

# Mapări ID-uri
product_ids = products_df["productId"].unique()
product_id_to_index = {str(pid): i for i, pid in enumerate(product_ids)}
num_products = len(product_ids)

# --- Similaritate Conținut (TF-IDF) ---
print("Calculare similaritate bazată pe descriere (TF-IDF)...")
products_df["text"] = (
    products_df["name"].astype(str).fillna("") + " " +
    products_df["category"].astype(str).fillna("") + " " +
    products_df["description"].astype(str).fillna("")
)

texts = ["" for _ in range(num_products)]
for pid_str, idx in product_id_to_index.items():
    row = products_df[products_df["productId"].astype(str) == pid_str]
    if not row.empty:
        texts[idx] = row["text"].values[0]

vectorizer = TfidfVectorizer(stop_words="english")
tfidf_matrix = vectorizer.fit_transform(texts)
content_sim_matrix = cosine_similarity(tfidf_matrix)

# --- Co-occurrence (Cumpărate împreună) ---
co_occurrence = np.zeros((num_products, num_products))
if not orders_df.empty:
    print("Calculare corelații bazate pe comenzi reale...")
    for _, group in orders_df.groupby("orderId"):
        p_in_order = [product_id_to_index[str(p)] for p in group["productId"] if str(p) in product_id_to_index]
        for i in p_in_order:
            for j in p_in_order:
                if i != j: 
                    co_occurrence[i][j] += 1
    
    max_co = np.max(co_occurrence)
    if max_co > 0: 
        co_occurrence = co_occurrence / max_co

# ===============================
# 3. Export JSON DIRECT în proiectul .NET
# ===============================
export_data = {
    "product_id_to_index": product_id_to_index,
    "content_similarity": content_sim_matrix.tolist(),
    "conditional_popularity": co_occurrence.tolist()
}

try:
    with open(DESTINATION_PATH, "w", encoding="utf-8") as f:
        json.dump(export_data, f)
    print("--------------------------------------------------")
    print(f"SUCCES!")
    print(f"Fișierul a fost salvat direct în: {DESTINATION_PATH}")
    print("Acum poți porni aplicația .NET și recomandările vor fi actualizate.")
    print("--------------------------------------------------")
except Exception as e:
    print(f"Eroare la salvarea fișierului în folderul .NET: {e}")
    # Backup salvare locală în caz de eroare de permisiuni
    with open("hybrid_model.json", "w", encoding="utf-8") as f:
        json.dump(export_data, f)
    print("Fișierul a fost salvat local în folderul curent ca backup.")