import math
import pandas as pd
from prophet import Prophet
import logging

# fara logo inutile
logging.getLogger("cmdstanpy").setLevel(logging.WARNING)

def predict_daily_sales_prophet(istoric_vanzari: list, zile_predictie: int = 3) -> float:
    """
    Folosește modelul Meta Prophet pentru a estima media zilnică a vânzărilor.
    istoric_vanzari trebuie să fie o listă de dict: [{'data': '2023-10-01', 'cantitate': 5}, ...]
    """
    if not istoric_vanzari or len(istoric_vanzari) < 5:
        # Dacă nu avem destule date, returnăm o valoare de siguranță
        return 2.0 
        
    # formatam pentru prophet 
    df = pd.DataFrame(istoric_vanzari)
    df = df.rename(columns={'data': 'ds', 'cantitate': 'y'})
    df['ds'] = pd.to_datetime(df['ds'])
    
    # initializam si antrenam modelul
    m = Prophet(yearly_seasonality=False, daily_seasonality=False, weekly_seasonality=True)
    m.fit(df)
    
    # facem pred pentru zilele urmatoare
    future = m.make_future_dataframe(periods=zile_predictie)
    forecast = m.predict(future)
    
    # extragem pred pe zilele urmatoare
    predictii_viitor = forecast.tail(zile_predictie)['yhat'].tolist()
    
    # Media estimată pe zi
    media_zilnica = sum(predictii_viitor) / len(predictii_viitor)
    return max(0.1, media_zilnica) # minim 0.1

def get_order_proposal(product: dict, istoric_vanzari: list, lead_time_zile: int = 3, zile_acoperire: int = 30) -> dict:
    stoc_curent = product.get('stoc', 0)
    nume_produs = product.get('nume', 'Produs Necunoscut')
    
    # estimare produse vandute pe zi
    vanzari_zilnice_est = predict_daily_sales_prophet(istoric_vanzari, zile_predictie=lead_time_zile)
    
    # calculam rop
    stoc_siguranta = math.ceil(vanzari_zilnice_est * lead_time_zile * 0.2)
    rop = math.ceil((vanzari_zilnice_est * lead_time_zile) + stoc_siguranta)
    
    # nu avem nev de comenzi
    if stoc_curent > rop:
        return {"needs_order": False}
        
    # cat comandam
    stoc_tinta = math.ceil((vanzari_zilnice_est * zile_acoperire) + stoc_siguranta)
    
    cantitate_optima = stoc_tinta - stoc_curent
    
    # Măsură de protecție: să comandăm mereu cel puțin o bucată dacă se atinge ROP-ul
    if cantitate_optima <= 0:
        cantitate_optima = 10 
        
    return {
        "needs_order": True,
        "product_name": nume_produs,
        "current_stock": stoc_curent,
        "rop": rop,
        "suggested_quantity": cantitate_optima,
        "daily_sales_prediction": round(vanzari_zilnice_est, 2)
    }