import os
import traceback
import json
from datetime import datetime

from dotenv import load_dotenv
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from langchain_core.prompts import ChatPromptTemplate
from langchain_core.messages import HumanMessage
from langchain_groq import ChatGroq
from stock_prediction import find_best_order_proposal, get_order_proposal
from typing import Any

load_dotenv()

app = FastAPI(title="E-commerce AI Server", version="1.0.0")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

GROQ_API_KEY = os.getenv("GROQ_API_KEY", "")
llm = None
if GROQ_API_KEY:
    llm = ChatGroq(groq_api_key=GROQ_API_KEY, model="llama-3.1-8b-instant")

user_sessions = {}


class Message(BaseModel):
    question: str
    session_id: str | None = None
    role: str = "User"
    db_context: Any = None


admin_prompt_template = """
Ești un Business Analyst expert. Analizează aceste date: {filtered_data}.
Comanda: {question}.
Răspunde strict sub formă de listă cu puncte: - Produsul [Nume]: [Detalii].
Fără explicații suplimentare sau JSON.
"""

SUPPORT_PROMPT = """Ești asistentul de suport al magazinului online E-commerce. Vorbești în română, prietenos și la obiect.

OBIECTIV: Rezolvă problema utilizatorului cu pași concreți în site. Nu trimite la „contactați suportul” decât după ce ai oferit pași clari de încercat.

INTERZIS:
- Adrese email inventate (folosește DOAR email_contact din context)
- Răspunsuri vagi în 5 puncte generice fără legătură cu site-ul nostru
- Să spui că nu poți ajuta la coș/cont — explică CE să facă utilizatorul

CONTEXT LIVE (sesiune utilizator):
{client_context}

Mesaj utilizator: {question}

Istoric conversație:
{history}

Dacă problema e coș / „nu pot adăuga produs”:
- Verifică din context dacă e autentificat și rol_client; spune explicit ce lipsește.
- Ghidează: Magazin → imagine produs → modal → Adaugă în coș → meniu Coș.
- Dacă produse_in_cos_acum > 0, spune că de fapt are deja articole în coș.
- Max 4-6 propoziții sau pași scurți; fără markdown link-uri fictive."""

CART_KEYWORDS = (
    "cos", "coș", "cart", "adaug", "adauga", "introduc", "nu pot", "nu merge",
    "produs in", "produs în", "basket",
)

ADMIN_COMMANDS = frozenset({
    "admin_stoc_critic",
    "admin_stoc_stagnant_30",
    "admin_analiza_favorite",
    "admin_propunere_comenzi",
})

CRITICAL_STOCK_THRESHOLD = 15


def _format_client_context(db_context: Any) -> str:
    if not db_context:
        return "Niciun context sesiune — presupune utilizator pe site-ul E-commerce."
    if isinstance(db_context, str):
        try:
            db_context = json.loads(db_context)
        except json.JSONDecodeError:
            return db_context
    return json.dumps(db_context, ensure_ascii=False, indent=2)


def _is_admin_request(msg: Message) -> bool:
    return (msg.role or "").strip().lower() == "admin"


def _parse_admin_products(db_context: Any) -> list:
    if db_context is None:
        return []
    if isinstance(db_context, str):
        if not db_context.strip():
            return []
        try:
            data = json.loads(db_context)
        except json.JSONDecodeError:
            return []
        return data if isinstance(data, list) else []
    if isinstance(db_context, list):
        return db_context
    return []


ESCALATION_KEYWORDS = (
    "tot nu merge", "tot nu merge", "nu ma lasa", "nu mă lasă", "nu functioneaza",
    "nu funcționează", "tot nu", "inca nu", "încă nu", "nu merge tot",
)

def _escalation_response(db_context: Any) -> str:
    ctx = db_context if isinstance(db_context, dict) else {}
    if isinstance(db_context, str):
        try:
            ctx = json.loads(db_context)
        except json.JSONDecodeError:
            ctx = {}
    email = ctx.get("email_contact", "raresmarian3344@gmail.com")
    return (
        "Înțeleg că pașii standard nu au rezolvat problema. Hai să investigăm altfel:\n"
        "1. Spune-mi **exact** ce produs încerci să adaugi și **ce mesaj** apare (sau dacă nu apare nimic).\n"
        "2. Încearcă din **alt browser** (Chrome/Edge) sau fereastră incognito, cu ad-blocker dezactivat.\n"
        "3. Verifică în meniul Coș dacă produsul e deja acolo (poate s-a adăugat fără mesaj vizibil).\n"
        f"4. Dacă tot nu merge, scrie-ne la **{email}** cu captură de ecran — răspundem manual."
    )


def _cart_troubleshooting_hint(question: str, db_context: Any, history: list | None = None) -> str | None:
    q = (question or "").strip()
    if q in ADMIN_COMMANDS or q.startswith("admin_"):
        return None

    q_lower = q.lower()
    if any(k in q_lower for k in ESCALATION_KEYWORDS):
        return _escalation_response(db_context)

    # Dacă am dat deja pași similari în istoric, escaladăm
    if history:
        recent_bot = " ".join(h for h in history[-4:] if h.startswith("Bot:")).lower()
        if any(k in q_lower for k in CART_KEYWORDS) and ("magazin" in recent_bot or "coș" in recent_bot or "cos" in recent_bot):
            return _escalation_response(db_context)

    if not any(k in q_lower for k in CART_KEYWORDS):
        return None

    ctx = db_context if isinstance(db_context, dict) else {}
    if isinstance(db_context, str):
        try:
            ctx = json.loads(db_context)
        except json.JSONDecodeError:
            ctx = {}

    if not ctx.get("utilizator_autentificat"):
        return (
            "Se pare că nu ești conectat cu un cont de client. Autentifică-te (înregistrare + login), "
            "apoi mergi la Magazin, apasă pe poza produsului și folosește „Adaugă în coș” din fereastra care se deschide."
        )

    if not ctx.get("rol_client"):
        return (
            "Ești conectat, dar coșul funcționează doar pentru conturi de tip Client. "
            "Folosește un cont User obișnuit (nu panoul de admin), apoi Magazin → click pe produs → Adaugă în coș."
        )

    in_cart = int(ctx.get("produse_in_cos_acum") or 0)
    if in_cart > 0:
        return (
            f"În sesiunea ta sunt deja {in_cart} produs(e) în coș — deschide meniul „Coș” din navbar ca să le vezi. "
            "Dacă tocmai ai adăugat ceva și nu apare, dă refresh la pagină (Ctrl+F5) sau încearcă din nou din Magazin."
        )

    return (
        "Încearcă așa: 1) Mergi la Magazin. 2) Click pe imaginea produsului (nu doar pe inimioară la favorite). "
        "3) În fereastra deschisă, apasă „Adaugă în coș”. 4) Verifică în meniul Coș. "
        "Dacă tot nu merge, reîncarcă pagina sau încearcă alt browser; spune-mi ce mesaj vezi după click."
    )


def _format_order_proposal_answer(product_name: str, current_stock: int) -> str:
    return f"**Propunere:** {product_name} — stoc actual **{current_stock}** bucăți"


def _process_admin_products(raw_data: list, now: datetime) -> list[dict]:
    processed = []
    for p in raw_data or []:
        p2 = {k.lower(): v for k, v in p.items()}
        stoc_raw = p2.get("stoc", p2.get("quantity"))
        p2["stoc"] = int(stoc_raw or 0)
        p2["nume"] = p2.get("nume") or p2.get("name") or "Produs"
        p2["nr_favorite"] = int(p2.get("nr_favorite", 0) or 0)

        ultima_v = p2.get("ultima_vanzare")
        if ultima_v and str(ultima_v).strip() not in ["", "None"]:
            try:
                dt_v = datetime.strptime(str(ultima_v).split(" ")[0], "%Y-%m-%d")
                p2["zile_fara_vanzare"] = (now - dt_v).days
            except ValueError:
                p2["zile_fara_vanzare"] = 100
        else:
            p2["zile_fara_vanzare"] = 100
        processed.append(p2)
    return processed


@app.get("/health")
async def health():
    return {
        "status": "ok",
        "groq_configured": bool(GROQ_API_KEY),
        "prophet": "ready",
    }


@app.post("/chat")
async def chat(msg: Message):
    try:
        session_id = msg.session_id or "default_guest"
        if session_id not in user_sessions:
            user_sessions[session_id] = {"history": [], "has_greeted": False}

        user_state = user_sessions[session_id]
        now = datetime.now()
        prompt_text = None

        if _is_admin_request(msg):
            raw_data = _parse_admin_products(msg.db_context)
            processed = _process_admin_products(raw_data, now)

            if msg.question == "admin_stoc_critic":
                filtered = [p for p in processed if p["stoc"] <= CRITICAL_STOCK_THRESHOLD]
                if not filtered:
                    if not processed:
                        return {
                            "answer": (
                                "Nu am primit date despre produse. Repornește aplicația .NET "
                                "(dotnet run) și serverul AI, apoi încearcă din nou."
                            ),
                        }
                    lowest = sorted(processed, key=lambda x: x["stoc"])[:3]
                    hint = ", ".join(f"{p['nume']} ({p['stoc']} buc.)" for p in lowest)
                    return {
                        "answer": (
                            f"Nu există produse cu stoc critic (≤{CRITICAL_STOCK_THRESHOLD} buc.). "
                            f"Cele mai mici stocuri primite: {hint}."
                        ),
                    }
                res = "**STOC CRITIC:**\n" + "\n".join(
                    [f"- {p['nume']}: {p['stoc']} buc" for p in sorted(filtered, key=lambda x: x["stoc"])]
                )
                return {"answer": res}

            if msg.question == "admin_stoc_stagnant_30":
                filtered = [p for p in processed if p["zile_fara_vanzare"] >= 30]
                if not filtered:
                    return {"answer": "Nu există produse stagnante."}
                top = sorted(filtered, key=lambda x: x["zile_fara_vanzare"], reverse=True)[:10]
                res = "**TOP 10 STAGNARE:**\n" + "\n".join(
                    [f"- {p['nume']}: {p['zile_fara_vanzare']} zile" for p in top]
                )
                return {"answer": res}

            if msg.question == "admin_analiza_favorite":
                filtered = [p for p in processed if p["nr_favorite"] > 0]
                if not filtered:
                    return {"answer": "Niciun produs la favorite."}
                top = sorted(filtered, key=lambda x: x["nr_favorite"], reverse=True)[:5]
                res = "**TOP FAVORITE:**\n" + "\n".join(
                    [f"- {p['nume']}: {p['nr_favorite']} salvări" for p in top]
                )
                return {"answer": res}

            if msg.question == "admin_propunere_comenzi":
                critici = [p for p in processed if p["stoc"] < 20]
                if not critici:
                    return {"answer": "Momentan nu există propuneri de comenzi."}

                proposal = find_best_order_proposal(critici)
                if not proposal:
                    p = sorted(critici, key=lambda x: x["stoc"])[0]
                    fallback = get_order_proposal(
                        {"nume": p["nume"], "stoc": p["stoc"]},
                        p.get("istoric_vanzari") or [],
                    )
                    if fallback.get("needs_order"):
                        proposal = fallback
                    else:
                        qty = max(10, 25 - int(p["stoc"]))
                        return {
                            "answer": _format_order_proposal_answer(p["nume"], int(p["stoc"])),
                            "action": {
                                "type": "execute_order",
                                "product": p["nume"],
                                "quantity": qty,
                            },
                        }

                return {
                    "answer": _format_order_proposal_answer(
                        proposal["product_name"],
                        int(proposal["current_stock"]),
                    ),
                    "action": {
                        "type": "execute_order",
                        "product": proposal["product_name"],
                        "quantity": proposal["suggested_quantity"],
                    },
                }

            prompt_text = admin_prompt_template.format(
                filtered_data=json.dumps(processed[:10], ensure_ascii=False),
                question=msg.question,
            )
        else:
            if (msg.question or "").strip() in ADMIN_COMMANDS or (msg.question or "").startswith("admin_"):
                return {
                    "answer": (
                        "Comandă de administrare, dar serverul nu a primit context Admin. "
                        "Autentifică-te ca Admin și repornește serverul AI (LangChain)."
                    ),
                }

            history = user_state["history"]
            client_ctx = _format_client_context(msg.db_context)

            hint = _cart_troubleshooting_hint(msg.question, msg.db_context, user_state["history"])
            if hint:
                user_state["history"].append(f"User: {msg.question}")
                user_state["history"].append(f"Bot: {hint}")
                return {"answer": hint}

            prompt_text = SUPPORT_PROMPT.format(
                question=msg.question,
                history="\n".join(history[-5:]),
                client_context=client_ctx,
            )

        if not llm:
            return {"answer": "Serverul AI nu are cheia GROQ configurată. Setează GROQ_API_KEY în .env."}

        response = llm.invoke([HumanMessage(content=prompt_text)])
        ai_text = response.content.strip()
        user_state["history"].append(f"User: {msg.question}")
        user_state["history"].append(f"Bot: {ai_text}")
        return {"answer": ai_text}

    except Exception:
        traceback.print_exc()
        return {"answer": "Eroare tehnică la procesarea datelor AI."}


if __name__ == "__main__":
    import uvicorn

    port = int(os.getenv("AI_SERVER_PORT", "8001"))
    # reload=False — evita proces copil care crapa cu Prophet/NumPy pe Windows
    uvicorn.run("ai_server:app", host="127.0.0.1", port=port, reload=False)
