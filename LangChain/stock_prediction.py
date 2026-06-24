import math
import logging
import os
from typing import Any

# Prophet trage matplotlib; backend Agg evita GUI si reduce conflicte
os.environ.setdefault("MPLBACKEND", "Agg")

import pandas as pd
from prophet import Prophet

logging.getLogger("cmdstanpy").setLevel(logging.WARNING)

# Sub acest prag — reaprovizionare obligatorie (indiferent de ROP Prophet)
CRITICAL_STOCK_LEVEL = 15
MIN_REORDER_QTY = 10


def normalize_sales_history(istoric_vanzari: Any) -> list[dict]:
    """Normalizează istoricul vânzărilor trimis de .NET (PascalCase sau camelCase)."""
    if not istoric_vanzari:
        return []

    normalized = []
    for entry in istoric_vanzari:
        if not isinstance(entry, dict):
            continue
        keys = {str(k).lower(): v for k, v in entry.items()}
        data_val = keys.get("data") or keys.get("ds")
        qty_val = keys.get("cantitate") or keys.get("y") or keys.get("quantity")
        if data_val is None or qty_val is None:
            continue
        try:
            qty = int(qty_val)
        except (TypeError, ValueError):
            continue
        normalized.append({"data": str(data_val).split(" ")[0], "cantitate": max(0, qty)})

    return sorted(normalized, key=lambda x: x["data"])


def predict_daily_sales_prophet(istoric_vanzari: list, zile_predictie: int = 3) -> float:
    """
    Estimează media zilnică a vânzărilor cu Meta Prophet.
    istoric_vanzari: [{'data': '2023-10-01', 'cantitate': 5}, ...] (format .NET)
    """
    istoric = normalize_sales_history(istoric_vanzari)

    if len(istoric) < 5:
        if istoric:
            return max(0.1, sum(x["cantitate"] for x in istoric) / len(istoric))
        return 2.0

    df = pd.DataFrame(istoric)
    df = df.rename(columns={"data": "ds", "cantitate": "y"})
    df["ds"] = pd.to_datetime(df["ds"])
    df["y"] = pd.to_numeric(df["y"], errors="coerce").fillna(0)

    m = Prophet(yearly_seasonality=False, daily_seasonality=False, weekly_seasonality=True)
    m.fit(df)

    future = m.make_future_dataframe(periods=zile_predictie)
    forecast = m.predict(future)
    predictii_viitor = forecast.tail(zile_predictie)["yhat"].tolist()
    media_zilnica = sum(predictii_viitor) / len(predictii_viitor)
    return max(0.1, float(media_zilnica))


def get_order_proposal(
    product: dict,
    istoric_vanzari: list,
    lead_time_zile: int = 3,
    zile_acoperire: int = 30,
) -> dict:
    keys = {str(k).lower(): v for k, v in product.items()}
    stoc_curent = int(keys.get("stoc", 0) or 0)
    nume_produs = keys.get("nume") or keys.get("name") or "Produs Necunoscut"
    istoric = normalize_sales_history(
        istoric_vanzari or keys.get("istoric_vanzari") or keys.get("istoricvanzari")
    )

    vanzari_zilnice_est = predict_daily_sales_prophet(istoric, zile_predictie=lead_time_zile)
    stoc_siguranta = math.ceil(vanzari_zilnice_est * lead_time_zile * 0.2)
    rop = math.ceil((vanzari_zilnice_est * lead_time_zile) + stoc_siguranta)

    stoc_tinta = math.ceil((vanzari_zilnice_est * zile_acoperire) + stoc_siguranta)
    cantitate_optima = max(0, stoc_tinta - stoc_curent)

    # Stoc critic în magazin: propunere chiar dacă ROP teoretic e sub stoc actual
    if stoc_curent <= CRITICAL_STOCK_LEVEL:
        cantitate_optima = max(
            cantitate_optima,
            MIN_REORDER_QTY,
            (20 - stoc_curent) if stoc_curent < 20 else MIN_REORDER_QTY,
        )
        return {
            "needs_order": True,
            "product_name": nume_produs,
            "current_stock": stoc_curent,
            "rop": rop,
            "suggested_quantity": int(cantitate_optima),
            "daily_sales_prediction": round(vanzari_zilnice_est, 2),
            "reason": "stoc_critic",
        }

    if stoc_curent > rop:
        return {"needs_order": False, "product_name": nume_produs, "current_stock": stoc_curent, "rop": rop}

    if cantitate_optima <= 0:
        cantitate_optima = max(MIN_REORDER_QTY, rop - stoc_curent)

    return {
        "needs_order": True,
        "product_name": nume_produs,
        "current_stock": stoc_curent,
        "rop": rop,
        "suggested_quantity": int(cantitate_optima),
        "daily_sales_prediction": round(vanzari_zilnice_est, 2),
    }


def find_best_order_proposal(products: list[dict]) -> dict | None:
    """Alege produsul cu cea mai urgentă nevoie de reaprovizionare (stoc vs ROP)."""
    candidates = []
    for p in products or []:
        keys = {str(k).lower(): v for k, v in p.items()} if isinstance(p, dict) else {}
        stoc = int(keys.get("stoc", 0) or 0)
        istoric = p.get("istoric_vanzari") if isinstance(p, dict) else None
        proposal = get_order_proposal(p, istoric or [])
        if proposal.get("needs_order"):
            gap = proposal["rop"] - proposal["current_stock"]
            if proposal.get("reason") == "stoc_critic":
                gap += 1000
            candidates.append((gap, proposal))

    if not candidates:
        return None

    candidates.sort(key=lambda x: x[0], reverse=True)
    return candidates[0][1]
