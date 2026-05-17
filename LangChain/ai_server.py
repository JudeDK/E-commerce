import traceback
import json
from datetime import datetime
from fastapi import FastAPI
from pydantic import BaseModel
from langchain_core.prompts import ChatPromptTemplate
from langchain_core.messages import HumanMessage
from langchain_groq import ChatGroq
from stock_prediction import get_order_proposal
from datetime import timedelta
from typing import Any

app = FastAPI()

# Inițializare model AI (Înlocuiește cu cheia ta dacă e diferită)
llm = ChatGroq(
    groq_api_key="API_KEY_HERE",
    model="llama-3.1-8b-instant"
)

user_sessions = {}

class Message(BaseModel):
    question: str
    session_id: str | None = None  
    role: str = "User"
    db_context: Any = None 

# PROMPT-URI
admin_prompt_template = """
Ești un Business Analyst expert. Analizează aceste date: {filtered_data}.
Comanda: {question}. 
Răspunde strict sub formă de listă cu puncte: - Produsul [Nume]: [Detalii].
Fără explicații suplimentare sau JSON.
"""

support_prompt = ChatPromptTemplate.from_template("""
Ești asistentul magazinului online. Ajută utilizatorul cu întrebări despre cont, plăți și funcționare.
Nu inventa stocuri. Dacă întreabă de produse, trimite-l la secțiunea "Produse".
Mesaj: {question}
Istoric: {history}
""")

@app.post("/chat")
async def chat(msg: Message):
    try:
        session_id = msg.session_id or "default_guest"
        if session_id not in user_sessions:
            user_sessions[session_id] = {"history": [], "has_greeted": False}
        
        user_state = user_sessions[session_id]
        now = datetime.now()

        # 1️⃣ LOGICA PENTRU ADMIN
        if msg.role == "Admin" and msg.db_context:
            # Parsare sigură
            raw_data = json.loads(msg.db_context) if isinstance(msg.db_context, str) else msg.db_context
            processed = []
            for p in (raw_data or []):
                p2 = {k.lower(): v for k, v in p.items()}
                p2['stoc'] = int(p2.get('stoc', 0))
                p2['nr_favorite'] = int(p2.get('nr_favorite', 0))
                
                ultima_v = p2.get('ultima_vanzare')
                if ultima_v and str(ultima_v).strip() not in ["", "None"]:
                    try:
                        dt_v = datetime.strptime(str(ultima_v).split(' ')[0], "%Y-%m-%d")
                        p2['zile_fara_vanzare'] = (now - dt_v).days
                    except: p2['zile_fara_vanzare'] = 100
                else: p2['zile_fara_vanzare'] = 100
                processed.append(p2)

            # --- RĂSPUNSURI INSTANT (Opresc execuția aici cu return) ---
            
            if msg.question == "admin_stoc_critic":
                filtered = [p for p in processed if p['stoc'] < 10]
                if not filtered: 
                    # Modificarea cerută de tine
                    return {"answer": "Nu exista niciun produs cu stockul critic"}
                
                res = " **STOC CRITIC:**\n" + "\n".join([f"- {p['nume']}: {p['stoc']} buc" for p in filtered])
                return {"answer": res}

            if msg.question == "admin_stoc_stagnant_30":
                filtered = [p for p in processed if p['zile_fara_vanzare'] >= 30]
                if not filtered: 
                    return {"answer": " Nu există produse stagnante."}
                
                top = sorted(filtered, key=lambda x: x['zile_fara_vanzare'], reverse=True)[:10]
                res = " **TOP 10 STAGNARE:**\n" + "\n".join([f"- {p['nume']}: {p['zile_fara_vanzare']} zile" for p in top])
                return {"answer": res}

            if msg.question == "admin_analiza_favorite":
                filtered = [p for p in processed if p['nr_favorite'] > 0]
                if not filtered: 
                    return {"answer": " Niciun produs la favorite."}
                
                top = sorted(filtered, key=lambda x: x['nr_favorite'], reverse=True)[:5]
                res = "**TOP FAVORITE:**\n" + "\n".join([f"- {p['nume']}: {p['nr_favorite']} salvări" for p in top])
                return {"answer": res}

            if msg.question == "admin_propunere_comenzi":
                critici = [p for p in processed if p['stoc'] < 20]
                if not critici: 
                    # Modificarea cerută de tine
                    return {"answer": "Momentan nu exista propuneri de comenzi"}
                
                # Luăm cel mai critic produs
                p = sorted(critici, key=lambda x: x['stoc'])[0]
                return {
                    "answer": f"**Alertă Stoc:** Produsul **{p['nume']}** mai are doar **{p['stoc']}** unități.",
                    "action": { 
                        "type": "execute_order", 
                        "product": p['nume'], 
                        "quantity": 100 
                    }
                }

            # Dacă nu este un buton, ci o întrebare liberă, abia atunci folosim AI-ul
            prompt_text = admin_prompt_template.format(filtered_data=json.dumps(processed[:10]), question=msg.question)
        
        else:
            # Logica pentru USER normal
            history = user_state["history"]
            prompt_text = support_prompt.format(question=msg.question, history="\n".join(history[-5:]))

        # 3️⃣ APEL CĂTRE AI (Groq) - se execută doar dacă nu s-a dat return mai sus
        response = llm.invoke([HumanMessage(content=prompt_text)])
        ai_text = response.content.strip()
        return {"answer": ai_text}

    except Exception as e:
        traceback.print_exc()
        return {"answer": "Eroare tehnică la procesarea datelor AI."}