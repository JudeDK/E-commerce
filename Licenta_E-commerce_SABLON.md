# LUCRARE DE LICENȚĂ — SABLON (document nou)

> **Instrucțiuni:** Copiază în Word. Completează `[NUME]`, `[COORDONATOR]`, `[an]`.
> **Nu modifica** `Licenta_Tudor_Chitu.pdf` — doar model de structură.
>
> **Structură preluată din `Licenta_Tudor_Chitu.pdf` (Tudor Chițu, iunie 2025):**
> - Pagină titlu → Rezumat → Abstract → Cuprins numerotat
> - **Cap. 1** Introducere (1.1 Preliminarii, 1.2 Aplicații similare, 1.3 Avantaje)
> - **Cap. 2** Tehnologii (2.1–2.4, ultimul subcapitol = AI)
> - **Cap. 3** Componente de inteligență artificială (subcapitole tehnice + figuri 3.x)
> - **Cap. 4** Evaluarea aplicației (**text scurt + multe capturi**, ca la Tudor: 1–2 paragrafe per secțiune, apoi *Figura 4.x* cu legendă italic; poți face **colaje stânga/dreapta** în Word)
> - **Cap. 5** Concluzii (+ limitări + dezvoltări viitoare)
> - **Bibliografie** [1], [2], … format „Autor, Titlu, Accesat: data, an, url: …”
>
> **Ce găsești în acest fișier:**
> - Text licență (cap. 1–5) + **modificări Prophet** detaliate (cap. 3.2)
> - **Bibliografie completă** [1]–[25] (gata de lipit; citări `[n]` imediat după paragrafe, ca la Tudor)
> - **PREGĂTIRE LICENȚĂ** — conturi, pornire proiect, formatare Word
> - **GHID TABELE ȘI DIAGRAME** — pași draw.io / Word, ce box-uri pui
> - **GHID POZE** — pas cu pas 1-1: ce buton apeși, ce rol, ce să apară în screenshot, text de pus în Word
> - **Imagini produse** — convenție fișiere `ProiectWeb/Imagini`
> - Listă **must / recomandat / opțional** + **checklist 36 pași**

---

## PAGINA DE TITLU

```
UNIVERSITATEA DIN BUCUREȘTI
FACULTATEA DE MATEMATICĂ ȘI INFORMATICĂ
SPECIALIZAREA INFORMATICĂ

Lucrare de licență

PLATFORMĂ E-COMMERCE CU RECOMANDĂRI HIBIDE
ȘI ASISTENT INTELIGENT PENTRU ADMINISTRARE

Absolvent
[NUME PRENUME]

Coordonator științific
[GRAD] [NUME COORDONATOR]

București, [lună] [an]
```

*(Opțional: subtitlu mai scurt pe copertă — ex. „Sistem web ASP.NET Core + PostgreSQL + servicii AI”)*

---

## REZUMAT

Comerțul electronic a devenit o componentă esențială a economiei digitale, iar
platformele moderne trebuie să ofere nu doar un catalog de produse, ci și instrumente
care îmbunătățesc experiența cumpărătorului și eficiența operatorului. Gestionarea
stocurilor, analiza vânzărilor și personalizarea recomandărilor sunt procese consumatoare
de timp atunci când sunt realizate manual.

Prezenta lucrare descrie proiectarea și implementarea unei **platforme e-commerce**
complete, destinată atât clienților (navigare, coș, plată, favorite, istoric comenzi), cât
și administratorilor (gestiune produse, comenzi, notificări, panou de Business
Intelligence). Aplicația web este dezvoltată în **ASP.NET Core 8** cu **Razor Pages**,
persistență în **PostgreSQL**, plăți prin **Stripe** și comunicare cu un microserviciu
Python pentru modulele de inteligență artificială.

Componentele de inteligență artificială includ: (1) un sistem de **recomandări hibride**
(ce îmbină similaritate de conținut TF-IDF cu co-ocurența produselor în comenzi reale),
(2) **prognoză a stocului și propuneri de reaprovizionare** cu **Prophet adaptat** (fără
sezonabilități inutile pe termen lung; estimare prin medie pe 3 zile, fallback la media
vânzărilor sau valoare implicită; integrare .NET),
(3) **chatbot** pentru suport client și analiză admin, bazat pe **LLM Groq**,
(4) reguli **prescriptive** pentru produse stagnante și favorite (sugestii de preț).

Rezultatul este o aplicație funcțională, testată pe date reale din baza de date, cu
interfață modernă și fluxuri clare între frontend, backend .NET și microserviciul Python
de AI.

**Cuvinte cheie:** e-commerce, recomandări, Prophet, PostgreSQL, ASP.NET Core, LLM.

---

## ABSTRACT

E-commerce platforms must go beyond product listings by offering personalized
recommendations, automated stock insights, and intelligent admin tools. This thesis
presents the design and implementation of a full-stack **e-commerce web application**
built with **ASP.NET Core 8**, **PostgreSQL**, **Stripe** payments, and a **FastAPI**
Python service for AI features.

AI-driven features include a **hybrid recommendation engine** (TF-IDF content
similarity plus order co-occurrence), **Prophet-based** replenishment proposals for
low-stock products, **Groq-powered** chatbots for customer support and admin analysis,
and **rule-based prescriptive pricing** for stagnant and high-demand items.

The system integrates a .NET web app with a **FastAPI** Python service. Evaluation
sections document the user interface and core workflows with screenshots.

**Keywords:** e-commerce, recommender systems, Prophet, ASP.NET Core, large language models.

---

## CUPRINS *(actualizează numerele de pagină în Word după export)*

```
Rezumat
Abstract
Cuprins
1 Introducere ........................... [pag]
   1.1 Preliminarii .................... [pag]
   1.2 Aplicații similare .............. [pag]
   1.3 Avantaje platformei proiectului . [pag]
2 Tehnologii ............................ [pag]
   2.1 ASP.NET Core ...................... [pag]
   2.2 PostgreSQL ........................ [pag]
   2.3 Stripe, e-mail tranzacțional și sesiune ............................ [pag]
   2.4 Tehnologii, concepte și modele folosite pentru componentele de inteligență artificială .. [pag]
3 Componente de inteligență artificială . [pag]
   3.1 Sistemul de recomandări hibride .. [pag]
   3.2 Prognoză stoc și comenzi AI ...... [pag]
   3.3 Chatbot client și panou BI admin . [pag]
   3.4 Reguli prescriptive de preț ...... [pag]
4 Evaluarea aplicației ................. [pag]
   4.1 Autentificare și profil ......... [pag]
   4.2 Magazin, filtre și recomandări ... [pag]
   4.3 Coș, plată și istoric client ..... [pag]
   4.4 Panou administrator ............. [pag]
   4.5 Business Intelligence și Prophet . [pag]
5 Concluzii ............................. [pag]
Bibliografie ............................ [pag]
Anexe (opțional) ........................ [pag]
```

---

# Capitolul 1 — Introducere

## 1.1 Preliminarii

Platforma proiectului își propune să acopere fluxul complet al unui magazin online,
cu accent pe **automatizare** și **decizii bazate pe date**. Mai jos sunt prezentate
obiectivele funcționale principale (poți extinde fiecare paragraf la ½–1 pagină în Word).

**Catalog și experiență client**

Magazinul permite listarea produselor cu **filtrare** după categorie, interval de preț
(slider dinamic din baza de date) și **căutare** tolerantă la diacritice. Detaliile produsului
sunt afișate într-un modal; utilizatorul poate adăuga în coș, salva la favorite și vizualiza
până la **trei recomandări** per produs, cu prioritate pentru articole din **categorii diferite**
(cross-selling).

**Coș, plată și cont**

Coșul este gestionat în sesiune; la checkout se creează o comandă și utilizatorul este
redirecționat către **Stripe** pentru plată. După confirmare, stocul scade, se generează
notificare pentru admin și se trimite **email** de confirmare cu detalii pe linii de comandă.
Clientul vede **istoricul comenzilor** numerotate per cont (#1, #2, …) și statistici agregate
pe luni.

**Administrare**

Administratorul gestionează produse (CRUD, imagini, preț, stoc, cost achiziție),
comenzi (vizualizare globală cronologică), notificări și top favorite. Panoul **„Asistent
Analiză”** oferă comenzi rapide: stoc critic, produse stagnante, analiză favorite, propuneri
de reaprovizionare cu cantitate estimată (Prophet sau fallback .NET).

**Suport și contact**

Un chatbot flotant răspunde clienților autentificați (context coș + pași de depanare).
Formularul de contact trimite mesaje pe email.

**→ Figura 1.1:** vezi secțiunea **„Diagramă 1.1 — Arhitectura sistemului”** (draw.io, nu screenshot).

---

## 1.2 Aplicații similare

**eMAG**

eMAG este un marketplace de referință în România, cu catalog extins, recomandări
„Cumpărate împreună”, campanii promoționale și logistică integrată. Platforma este
orientată spre scară națională și volume mari de trafic; un magazin mic nu dispune,
în mod nativ, de un panou BI simplu cu prognoză Prophet și comenzi rapide de reaprovizionare
ca în proiectul de față. [12]

**Shopify**

Shopify permite crearea rapidă a magazinelor online prin teme și aplicații din ecosistemul
App Store. Funcționalitățile avansate de recomandare, prognoză stoc sau chat AI depind
adesea de extensii plătite sau integrări terțe, spre deosebire de soluția implementată
end-to-end în acest proiect (.NET + PostgreSQL + serviciu Python propriu). [13]

**WooCommerce**

WooCommerce extinde WordPress într-un magazin online flexibil, popular la IMM-uri.
Este open-source și extensibil prin plugin-uri, însă configurarea multi-plugin pentru
AI, plăți și analitică poate deveni complexă; proiectul curent unifică aceste module
într-un singur codbase controlat. [14]

**Adobe Commerce (Magento)**

Magento / Adobe Commerce este o platformă enterprise, matură, cu funcții bogate de catalog
și B2B. Costul și complexitatea de operare sunt ridicate; proiectul de licență prioritizează
un stack mai ușor de întreținut pentru un singur operator (ASP.NET Core 8 + un microserviciu
FastAPI). [15]

**Tabel comparativ (recomandat — Figura 1.2)**

Inserează în Word un tabel cu coloanele: *Platformă | Recomandări | Prognoză stoc |
Chat / asistent AI | Stack principal*. Ultimul rând: **Proiectul de față** — toate „Da”.
Legendă: *Figura 1.2: Comparație între platforme comerciale și proiectul implementat*

---

## 1.3 Avantaje platformei proiectului

Spre deosebire de soluțiile care tratează separat magazinul și analitica, aplicația
integrează într-un singur produs:

- recomandări antrenate pe **comenzi reale** din propria bază de date;
- **stoc critic** și **reaprovizionare** cu model de serii temporale (Prophet);
- **chat** contextual pentru client și comenzi rapide pentru admin (fără dependență totală de LLM pentru liste);
- **sugestii de preț** explicabile (reguli + marjă minimă), nu doar text generat aleator.

[Extinde cu 1–2 paragrafe despre contribuția personală: design UI, integrare Stripe, etc.]

---

# Capitolul 2 — Tehnologii

*(Paralel cu **Licenta_Tudor_Chitu.pdf**, cap. 2: aici descrii **doar tehnologii, biblioteci și concepte**
— ce sunt, la ce se folosesc, cu citări `[n]` la finalul paragrafului. **Nu** descrii modulele tale
(`Chatbox.cs`, `TextNormalization`, fluxul coșului etc.); implementarea lor este în **capitolul 3**
și **capitolul 4**.)*

## 2.1 ASP.NET Core

ASP.NET Core este un framework web open-source, multiplatformă, dezvoltat de Microsoft
pe baza runtime-ului .NET. Oferă un pipeline de middleware configurabil (rutare, fișiere
statice, autentificare, autorizare), hosting pe Kestrel și suport pentru API-uri HTTP
și pagini server-rendered. Este folosit în producție pentru aplicații de la scară mică
la enterprise, cu accent pe performanță și configurabilitate. [1]

Framework-ul include **dependency injection** nativ: serviciile sunt înregistrate la pornire
și injectate în pagini sau endpoint-uri, ceea ce separă logica de acces la date de stratul
de prezentare. Versiunea **ASP.NET Core 8** este cea pe care se bazează aplicația din
această lucrare. [1]

### Razor Pages

**Razor Pages** este un model de programare care combină markup HTML cu cod C# în fișiere
`.cshtml`, cu un code-behind `.cshtml.cs` pentru evenimente `OnGet` / `OnPost`. Este o
alternativă la arhitectura MVC clasică, potrivită pentru aplicații cu multe formulare și
pagini tradiționale, fără un frontend SPA separat. [23]

### Bootstrap

**Bootstrap** este un framework CSS open-source pentru layout responsive (grid pe 12 coloane),
componente UI (navbar, modal, card, formulare) și utilitare de spacing/typography. Versiunea
5 este folosită în proiect ca bază vizuală, peste care se aplică stiluri proprii în CSS. [20]

### noUiSlider

**noUiSlider** este o bibliotecă JavaScript pentru controale tip slider (unul sau două capete),
folosită frecvent la filtre de preț în magazine online. Emite evenimente la schimbarea
intervalului și se integrează ușor cu câmpuri de input sau cu logica de filtrare din backend. [21]

**→ Figura 2.1:** vezi **Ghid Figura 2.1** (screenshot Solution Explorer: `Pages`, `Services`, `API`).

---

## 2.2 PostgreSQL

PostgreSQL este un sistem de gestionare a bazelor de date relaționale (SGBDR) open-source,
respectat pentru conformitatea SQL, tranzacțiile **ACID**, extensibilitatea tipurilor de date
și suportul pentru JSON/JSONB. Permite indexare avansată, constrângeri referențiale și
funcții definite de utilizator, fiind potrivit atât pentru tranzacții OLTP (comenzi, stoc),
cât și pentru interogări analitice pe volume moderate. [3]

### Entity Framework Core

**Entity Framework Core** este ORM-ul (Object-Relational Mapping) de la Microsoft pentru
.NET: mapează clase C# la tabele, traduce LINQ în SQL și gestionează migrări de schemă.
Reduce codul SQL repetitiv și oferă tracking al entităților, relații one-to-many și
transacții integrate cu `DbContext`. [2]

### Npgsql

**Npgsql** este driver-ul .NET pentru PostgreSQL și provider-ul EF Core asociat. Permite
conexiuni parametrizate, maparea tipurilor PostgreSQL (inclusiv `timestamp`, `numeric`) și
executarea interogărilor generate de EF sau SQL brut. Este distribuit ca pachet NuGet și
menținut activ pentru .NET 8. [4]

**→ Figura 2.2:** vezi **„Diagramă 2.2 — Model entitate-relație”** (draw.io).

---

## 2.3 Stripe, e-mail tranzacțional și sesiune

Într-un magazin online, finalizarea comenzii implică mai mult decât încasarea banilor:
starea temporară a coșului trebuie păstrată între cereri HTTP, iar după plată clientul
și administratorul trebuie informați prin e-mail. Această secțiune prezintă tehnologiile
folosite pentru plăți (Stripe), notificări (SMTP) și persistența tranzitorie a datelor
(sesiune server-side și serializare JSON). [5]

### Stripe

**Stripe** este o platformă de plăți online care oferă API-uri și componente gata făcute
(checkout hosted, elemente de card, webhooks) pentru comerț electronic. **Stripe Checkout**
găzduiește pagina de plată: comerciantul creează o sesiune server-side cu suma și produsele,
iar clientul finalizează plata pe infrastructura Stripe (PCI-DSS). Confirmarea plății este
raportată aplicației prin redirect sau webhook, fără ca datele cardului să tranziteze
serverul magazinului. Stripe oferă și mod **test** cu chei separate, permițând simularea
tranzacțiilor în dezvoltare fără mișcări reale de fonduri. [5]

### SMTP și trimiterea e-mailurilor

**SMTP** (Simple Mail Transfer Protocol) este protocolul standard pentru transmiterea
mesajelor e-mail între servere. În ecosistemul .NET, clasa **SmtpClient** din
`System.Net.Mail` permite autentificarea la un relay (de exemplu Gmail pe portul 587
cu TLS), construirea mesajelor HTML și trimiterea către destinatari. În comerțul electronic,
e-mailul tranzacțional confirmă comanda, livrarea sau răspunsul la formulare de contact,
fără a fi nevoie de un client de poștă pe partea utilizatorului. [26]

### Newtonsoft.Json

**Newtonsoft.Json** (Json.NET) este o bibliotecă .NET larg răspândită pentru serializarea
și deserializarea obiectelor C# în format JSON și invers. Oferă atribute de mapare,
gestionarea referințelor circulare și compatibilitate cu structuri complexe, fiind o
alternativă matură la **System.Text.Json** introdus în .NET Core. În aplicații web,
JSON este formatul uzual pentru stocarea temporară a listelor de obiecte (de exemplu
articole de coș) în sesiune sau pentru schimb de date cu API-uri externe. [27]

### Sesiune în ASP.NET Core

**Sesiunea** în ASP.NET Core păstrează starea utilizatorului între cereri HTTP fără a
expune date sensibile în cookie-ul clientului: browserul primește doar un identificator
de sesiune, iar perechea cheie–valoare este stocată server-side (în memorie sau cache
distribuit). Middleware-ul de sesiune se configurează în pipeline-ul aplicației și se
folosește împreună cu serializarea JSON pentru a menține coșul de cumpărături până la
checkout. [25]

---

## 2.4 Tehnologii, concepte și modele folosite pentru componentele de inteligență artificială

### scikit-learn și TF-IDF

**scikit-learn** este o bibliotecă Python pentru machine learning clasic: clasificare,
regresie, clustering și preprocesare text. **TfidfVectorizer** transformă corpusuri de
documente în vectori TF-IDF (Term Frequency – Inverse Document Frequency), iar metrici
precum **similaritatea cosinus** măsoară proximitatea între produse descrise textual. [9]

### Sisteme de recomandare și filtrare colaborativă

Sistemele de recomandare pot combina **filtrare pe conținut** (similitudine între atribute
sau descrieri) cu **filtrare colaborativă** (pattern-uri din comportamentul utilizatorilor,
ex. produse cumpărate împreună). Literatura de specialitate descrie hibridizarea acestor
abordări pentru a depăși limitările fiecăreia în parte (cold start, diversitate). [16] [17]

### Prophet

**Prophet** este un model open-source de prognoză pentru serii temporale, dezvoltat de
Meta, conceput pentru date cu tendință și componente sezoniere. Acceptă regresori exogeni
și este robust la date lipsă sau outlier-i; este folosit frecvent pentru estimarea cererii
și a vânzărilor pe intervale regulate. [6] [7]

### Transformer

Arhitectura **Transformer**, introdusă în lucrarea „Attention Is All You Need”, se bazează
pe mecanismul de **self-attention** și permite procesarea paralelă a secvențelor, capturând
dependențe pe termen lung. A stat la baza modelelor moderne de limbaj (GPT, BERT) și a
multor sisteme de NLP și viziune. [18]

### Large Language Model (LLM)

Un **Large Language Model** este un model neural de mari dimensiuni, antrenat pe text masiv,
capabil să genereze și să interpreteze limbaj natural. LLM-urile sunt folosite în chatboți,
sumarizare, asistență la cod și răspunsuri contextuale; calitatea depinde de datele de
antrenare, de prompt și de validarea răspunsurilor în aplicație. [19]

### Groq

**Groq** oferă infrastructură de inferență pentru modele de limbaj, cu API compatibil cu
fluxuri tip OpenAI. Platforma pune accent pe latență redusă la generare, fiind potrivită
pentru asistenți conversaționali în timp real. În ecosistemul open-source sunt disponibile
modele precum familiile LLaMA, accesibile prin API-ul Groq. [8]

### FastAPI

**FastAPI** este un framework Python pentru construirea API-urilor REST, bazat pe type
hints Pydantic și pe ASGI (Uvicorn). Generează documentație OpenAPI automată, suportă
validarea cererilor și este folosit frecvent ca strat subțire între modele ML/LLM și
clienții HTTP (inclusiv aplicații .NET). [10]

### LangChain

**LangChain** este un framework Python pentru aplicații bazate pe modele de limbaj: lanțuri
de prompturi, agenți, memorie conversație și integrare cu API-uri externe. Simplifică
legătura dintre LLM și date structurate sau baze de cunoștințe. [11]

### pandas

**pandas** este biblioteca standard Python pentru tabele (`DataFrame`) și serii temporale:
agregări, grupări pe dată, alinierea istoricului de vânzări — operații necesare înainte
de prognoză Prophet sau de exportul datelor către alte unelte. [22]

**→ Figura 2.3:** vezi **„Diagramă 2.3 — Flux recomandări”** (draw.io) — diagramă de concepte, nu cod aplicație.

---

# Capitolul 3 — Componente de inteligență artificială

*(Capitolul cel mai tehnic — echivalentul cap. 3 din licența Tudor Chițu.)*

## 3.1 Sistemul de recomandări hibride

### Prezentare generală

La vizualizarea unui produs, sistemul returnează trei recomandări. Prioritate:
(1) scor din `hybrid_model.json`, (2) fallback „cumpărate împreună” din SQL live,
(3) popularitate cross-categorie, (4) produse alternative cu stoc > 0.

### Antrenare offline

Scriptul `Antrenare/model.py` citește produse și `OrderItems` din PostgreSQL,
construiește TF-IDF pe nume+categorie+descriere și matricea de co-ocurență per `orderId`.
Rezultatul este serializat JSON consumat de .NET (cache la `LastWriteTime`).

**→ Figura 3.1 (opțional):** captură din `hybrid_model.json` (primele 10 linii) sau schema scor în text.

### Inferență în aplicație

Descrie funcția `TryGetHybridProductIds`, boost pentru categorii diferite, `GetBoughtTogetherAsync`.

**→ Figura 3.2:** vezi **Ghid Figura 3.2** (modal magazin, 3 recomandări).

### Evaluare calitativă (opțional)

2–3 exemple: produs A → recomandările B, C din altă categorie datorită co-ocurenței în comenzi test.

---

## 3.2 Prognoză stoc și comenzi AI (Prophet adaptat)

### Context business

Administratorul apasă **„Comenzi AI”** în panoul BI. Aplicația .NET (`Chatbox.cs`):

1. Alege din PostgreSQL produsul cu **cel mai mic stoc**, dar doar dacă stocul este **sub 20 buc.**.
2. Încarcă **istoricul vânzărilor din ultimele 14 zile**, grupat **pe zile** (nu pe comandă individuală).
3. Calculează **cantitatea de comandat**:
   - dacă serverul Python răspunde (`GET /health` + `POST /chat`) → **Prophet**;
   - altfel → **aceeași formulă în C#** (`CalculeazaCantitateReaprovizionare`), fără ML.
4. Afișează în chat doar: *„Propunere: [Nume] — stoc actual [X] bucăți”* (fără ROP, fără „Prophet”, fără buc./zi în mesajul principal).
5. Butonul **Confirmă** trimite cantitatea la `POST /api/chat/execute-order` (stoc + email furnizor).

Listele **Stoc critic**, **Stagnant**, **Favorite** sunt calculate **direct în .NET** din SQL, nu în Python — pentru răspuns rapid și predictibil.

---

### Adaptarea modelului Prophet la magazinul online

În literatură și în exemplele oficiale, Prophet este prezentat ca un model de prognoză
pe termen mediu-lung: se activează adesea **sezonabilitatea anuală și zilnică**, se pot
modela **sărbători**, iar prognoza este citită pe **sute de zile** în viitor. În paralel,
formulele clasice de gestiune a stocurilor (Reorder Point, **stoc de siguranță**, capacitate
de depozit, lot economic de comandă) presupun un istoric bogat și parametri de depozitare
care, în cazul unui magazin nou sau al unui produs cu puține comenzi, **nu se pot estima
fiabil** și nu reflectă nevoia reală din proiect.

Pornind de la aceste observații, am **renunțat la componentele care nu mă ajutau** în
contextul e-commerce-ului implementat și le-am înlocuit cu o abordare centrată pe **vânzările
reale din ultimele 14 zile**, agregate **pe zi** în baza de date.

**Ce nu am păstrat din configurarea „clasică” Prophet**

Nu folosesc sezonabilitatea anuală (`yearly_seasonality=False`) nici pe cea zilnică
(`daily_seasonality=False`), deoarece istoricul disponibil acoperă doar câteva săptămâni
sau luni de comenzi; activarea lor produce fluctuații artificiale. Nu construiesc prognoze
lungi „pentru depozit” pe 30–90 de zile înainte, ci extrag din Prophet doar o **medie a
vânzărilor estimate pe următoarele 3 zile** (perioada de lead time aprovizionare), pe care
o tratez ca **vânzări zilnice estimate** în formula de reaprovizionare.

**Cu ce am înlocuit estimarea vânzărilor**

Funcția `predict_daily_sales_prophet` lucrează astfel:

- dacă există **cel puțin 5 zile** cu vânzări în istoric → antrenez Prophet doar cu
  **sezonabilitate săptămânală** (`weekly_seasonality=True`), apoi media valorilor `yhat`
  din cele 3 zile viitoare devine `vanzari_zilnice_est`;
- dacă sunt **mai puțin de 5 zile** → **nu mai rulez Prophet** și folosesc **media aritmetică**
  a cantităților vândute pe zilele înregistrate;
- dacă **nu există deloc vânzări** în interval → folosesc o valoare implicită de **2,0 bucăți/zi**,
  astfel încât propunerea de comandă să rămână utilizabilă și pentru produse noi.

Această ierarhie (Prophet → medie simplă → valoare implicită) este o **înlocuire explicită**
a prognozei „pure” Prophet acolo unde modelul nu are suficiente date.

**Legătura cu gestiunea stocului din proiect**

După estimarea vânzărilor zilnice, cantitatea de comandat nu vine direct din graficul
Prophet, ci din formulele adaptate magazinului:

- **stoc de siguranță** = vânzări zilnice estimate × timp aprovizionare (3 zile) × 20%;
- **punct de recomandare (ROP)** = vânzări zilnice estimate × 3 zile + stoc de siguranță;
- **stoc țintă** = vânzări zilnice estimate × 30 zile + stoc de siguranță;
- **cantitate propusă** = stoc țintă − stoc curent.

Pentru produsele cu **stoc critic în magazin (≤ 15 bucăți)**, am introdus o regulă de
business care **forțează reaprovizionarea** chiar dacă ROP-ul teoretic ar sugera că mai
este stoc „suficient” — situație întâlnită în practică la produse cu vânzări mici dar
vizibilitate mare. Prioritatea între mai multe produse cu stoc sub 20 bucăți este dată de
funcția `find_best_order_proposal`, care favorizează cazurile de stoc critic.

**Integrare cu aplicația .NET**

Istoricul trimis din C# poate avea câmpuri în PascalCase sau camelCase; funcția
`normalize_sales_history` unifică formatul înainte de antrenare. Listele „Stoc critic”,
„Stagnant” și „Favorite” sunt calculate în **Chatbox.cs** direct din PostgreSQL, iar
Prophet este apelat **doar pentru cifra cantității** la „Comenzi AI”. Dacă serverul Python
nu răspunde, **aceeași logică** (medie / 2,0 buc./zi + formule ROP) rulează în
`CalculeazaCantitateReaprovizionare`, astfel încât administratorul să nu depindă obligatoriu
de microserviciul AI.

Din punct de vedere tehnic, la rularea pe Windows am setat backend-ul matplotlib la **Agg**
(fără fereastră grafică) și am redus zgomotul din logurile motorului Stan, pentru stabilitate
în `start_ai_server.bat`.

**Formule de rezumat (pentru licență):**

- vânzări_zilnice_est = media Prophet(3 zile) **sau** media istoricului **sau** 2,0  
- stoc_siguranta = ⌈vânzări_zilnice_est × 3 × 0,2⌉  
- stoc_țintă = ⌈vânzări_zilnice_est × 30 + stoc_siguranta⌉  
- cantitate = max(0, stoc_țintă − stoc_curent), cu prioritate pentru stoc ≤ 15 buc.  

`[FIGURA 3.3]` — vezi **Ghid Figura 3.3** (diagramă flux + opțional grafic Prophet).

`[FIGURA 3.4]` — vezi **Ghid Figura 3.4** (panou Comenzi AI + Confirmă).

### Executare comandă furnizor

`POST /api/chat/execute-order` — doar rol Admin: crește `Quantity` la produs, notificare, email către furnizor.

---

## 3.3 Chatbot client și panou Business Intelligence admin

### API Chatbox și legătura .NET ↔ Python

Modulul `Chatbox.cs` expune în ASP.NET Core endpoint-urile `POST /api/chat` și
`POST /api/chat/execute-order`. Acestea formează singurul punct de legătură HTTP între
interfața web și microserviciul **FastAPI** (port 8001). Pentru administrator, comenzile
text `admin_stoc_critic`, `admin_stoc_stagnant_30`, `admin_analiza_favorite` și
`admin_propunere_comenzi` sunt rezolvate în C# direct din PostgreSQL sau prin apel
`HttpClient` către Python (Prophet), fără LLM. Pentru client, întrebarea și un context
JSON (produse din coș, pași de depanare) sunt trimise către `/chat` pe serverul Groq.
Confirmarea reaprovizionării (`execute-order`) actualizează stocul, creează notificare
și poate trimite email către furnizor.

### Client

Context: produse în coș, pași depanare. Cuvinte cheie → hint escaladare
(„tot nu merge”). Răspunsul vine din FastAPI + Groq prin proxy-ul de mai sus.

**→ Figura 3.5:** vezi **Ghid Figura 3.5** (chat client).

### Admin

Comenzi rapide procesate în **.NET** (`Chatbox.cs`): `admin_stoc_critic`, `admin_stoc_stagnant_30`,
`admin_analiza_favorite`, `admin_propunere_comenzi`. Input text dezactivat — doar butoane.

**→ Figura 3.6:** vezi **Ghid Figura 3.6** (stoc critic în panou BI).

*(Figura 3.7 — opțional: diagramă LLM pentru mesaje libere admin; poți omite.)*

---

## 3.4 Reguli prescriptive de preț (stagnant / favorite)

**Stagnant:** fără vânzări ≥30 zile → −5%; ≥60 zile → −10%, respectând marja minimă
(`AcquisitionCost` + 15%).

**Favorite + top 20% vânzări:** sugestie +5% … +10% cu motivație în textul returnat adminului.

**→ Figura 3.8:** vezi **Ghid Figura 3.8** (stagnant/favorite cu preț sugerat).

---

# Capitolul 4 — Evaluarea aplicației

*(Capitol demonstrativ — **ca la licența Tudor Chițu**: pentru fiecare subsecțiune scrii **1–3 paragrafe scurte** care descriu ce face ecranul, apoi inserezi **Figura 4.x** cu legendă italic sub poză. Poți combina **2 capturi alăturate** (stânga/dreapta) într-un singur rând Word, exact ca Fig. 4.1 din model.)*

**Text tip pentru Word (adaptat):**

> *„La acces, utilizatorul neautentificat vede pagina principală, având opțiunea de a naviga la înregistrare sau autentificare. După crearea contului, informațiile de profil pot fi modificate ulterior, cu excepția adresei de email.”*

**→ Toate pașii de captură:** secțiunea **„GHID POZE”** de mai jos.

## 4.1 Paginile de autentificare, înregistrare și profil

La deschiderea aplicației, vizitatorul vede homepage-ul cu butoane **„Înregistrează-te”** și **„Conectează-te”**.
După autentificare, clientul accesează profilul din meniul inițialei din navbar (nickname, telefon)
și poate schimba parola fără a reintroduce parola veche.

**→ Figura 4.1:** colaj — (stânga) hero Acasă + login; (dreapta) register + profil/parolă.  
**→ Figura 4.2:** profil detaliat (dacă nu e inclus în 4.1).

---

## 4.2 Magazin, filtre, imagini produse și recomandări

Pagina **Magazin** (`/Client/Products`) afișează produsele cu imagini din folderul
`ProiectWeb/Imagini`, filtre (categorie, preț, căutare), sortare și paginare (15/pagină).

**Căutare fără diacritice:** serviciul `TextNormalization` normalizează textul în forma
Unicode NFD, elimină diacriticele și compară șirurile fără sensibilitate la majuscule,
astfel încât „sampon” găsește și „șampon”. Potrivirea se face în memorie după încărcarea
numelor din PostgreSQL, păstrând coerența cu filtrul AJAX și paginarea. [24]

**Imagini produse:** `ProductImageService` mapează numele produsului la fișierul JPG
(fără caractere interzise în Windows, ex. `LG OLED 55"` → `LG OLED 55.jpg`), servește
URL-uri `/Imagini/…` și folosește `Imagine negasita.jpg` ca fallback.

Click pe imagine deschide modalul cu detalii, cantitate și recomandări.

**→ Figura 4.3:** grid magazin — filtre active + sortare + paginare + **poze produse vizibile**.  
**→ Figura 4.4:** modal produs + **Adaugă în coș** + 3 recomandări.  
**→ Figura 4.5 (recomandat):** pagina **Favorite** cu buton **Salvat**.

---

## 4.3 Coș, plată, istoric și statistici client

Coșul este ținut în **sesiunea ASP.NET Core** (listă `Cart` serializată JSON, timeout 30 min);
la adăugare se verifică stocul din PostgreSQL. Checkout-ul deschide **Stripe Checkout**;
după plată, comanda este salvată, stocul scade și administratorul primește notificare. [5] [25]

Clientul vizualizează **Istoric comenzi** (#1, #2, …) cu paginare 10/pagină.

**→ Figuri 4.6 – 4.10:** Ghid poze (4.8 email opțional).

---

## 4.4 Panou administrator

Administratorul gestionează produse (CRUD, upload imagine în `Imagini`), comenzi (sortare +
paginare), notificări și top favorite.

**→ Figuri 4.11 – 4.15:** Ghid poze (4.12 formular produs obligatoriu pentru CRUD).

---

## 4.5 Business Intelligence, Prophet, pagini publice și contact

Panoul **„Analiză stoc”** (dreapta-jos, doar Admin) oferă acțiuni rapide: stoc critic,
stagnant, favorite, comenzi AI (Prophet). Paginile **Acasă**, **Despre** și **Contact**
prezintă magazinul public.

**→ Figuri 4.16 – 4.18:** Ghid poze.

---

# Capitolul 5 — Concluzii

În cadrul acestei lucrări am proiectat și implementat o platformă e-commerce care
integrează funcționalități clasice de magazin online cu module de inteligență artificială
utile în practică: recomandări hibride, prognoză de stoc, asistent conversațional și
reguli prescriptive de preț.

**Rezultate obținute**

- Aplicație web stabilă ASP.NET Core + PostgreSQL cu roluri Admin/Client.
- Plată Stripe și flux complet comandă → stoc → notificare → email.
- Recomandări personalizate pe baza istoricului de comenzi (reantrenare `model.py`).
- Panou BI pentru decizii rapide de stoc și merchandising.

**Limitări**

- Prophet necesită istoric suficient; altfel se folosește media sau valori implicite.
- LLM-ul poate genera răspunsuri incorecte fără validare strictă (mitigare: comenzi admin în .NET).
- `hybrid_model.json` trebuie regenerat manual după schimbări majore de catalog.

**Dezvoltări viitoare**

- A/B testing pentru recomandări; metrici CTR.
- Retraining automat la program (job nocturn).
- Dashboard grafic vânzări în admin (Chart.js).
- Deploy containerizat (Docker Compose: web + db + ai).

---

# Bibliografie

*(Format ca la licența Tudor Chițu: [n] Autor, Titlu, Accesat: data, an, url: … — citează în text cu [n].)*

[1] Microsoft Corporation, ASP.NET Core documentation, Accesat: 2026-06-04, 2025, url: https://learn.microsoft.com/en-us/aspnet/core/.

[2] Microsoft Corporation, Entity Framework Core documentation, Accesat: 2026-06-04, 2025, url: https://learn.microsoft.com/en-us/ef/core/.

[3] PostgreSQL Global Development Group, About PostgreSQL, Accesat: 2026-06-04, 2025, url: https://www.postgresql.org/about/.

[4] Npgsql Development Team, Npgsql documentation, Accesat: 2026-06-04, 2025, url: https://www.npgsql.org/doc/.

[5] Stripe, Inc., Stripe Checkout documentation, Accesat: 2026-06-04, 2025, url: https://stripe.com/docs/payments/checkout.

[6] Sean J. Taylor și Ben Letham, Forecasting at scale, Prophet, Accesat: 2026-06-04, 2018, url: https://facebook.github.io/prophet/.

[7] Meta Platforms, Inc., Prophet Python package, Accesat: 2026-06-04, 2024, url: https://github.com/facebook/prophet.

[8] Groq, Inc., Groq API documentation, Accesat: 2026-06-04, 2025, url: https://console.groq.com/docs.

[9] scikit-learn developers, TfidfVectorizer and cosine_similarity, Accesat: 2026-06-04, 2025, url: https://scikit-learn.org/stable/modules/generated/sklearn.feature_extraction.text.TfidfVectorizer.html.

[10] Sebastian Ramírez, FastAPI documentation, Accesat: 2026-06-04, 2025, url: https://fastapi.tiangolo.com/.

[11] LangChain, Inc., LangChain documentation, Accesat: 2026-06-04, 2025, url: https://python.langchain.com/.

[12] eMAG, eMAG — marketplace România, Accesat: 2026-06-04, 2025, url: https://www.emag.ro/.

[13] Shopify Inc., Shopify documentation, Accesat: 2026-06-04, 2025, url: https://help.shopify.com/.

[14] WooCommerce, WooCommerce documentation, Accesat: 2026-06-04, 2025, url: https://woocommerce.com/documentation/.

[15] Magento, Adobe Commerce documentation, Accesat: 2026-06-04, 2025, url: https://experienceleague.adobe.com/docs/commerce.html.

[16] Francesco Ricci, Lior Roach și Bracha Shapira, Recommender Systems Handbook, Springer, 2015, url: https://link.springer.com/book/10.1007/978-1-4899-7637-6.

[17] Jure Leskovec, Anand Rajaraman și Jeffrey D. Ullman, Mining of Massive Datasets, Cambridge University Press, 2014, url: http://www.mmds.org/.

[18] Ashish Vaswani și et al., Attention Is All You Need, Accesat: 2026-06-04, 2017, url: https://arxiv.org/abs/1706.03762.

[19] Michael McDonough, Large language model, Accesat: 2026-06-04, 2025, url: https://www.britannica.com/topic/large-language-model.

[20] Bootstrap Team, Bootstrap 5 documentation, Accesat: 2026-06-04, 2025, url: https://getbootstrap.com/docs/5.3/.

[21] noUiSlider, noUiSlider documentation, Accesat: 2026-06-04, 2025, url: https://refreshless.com/nouislider/.

[22] Pandas Development Team, pandas documentation, Accesat: 2026-06-04, 2025, url: https://pandas.pydata.org/docs/.

[23] Microsoft Corporation, Introduction to Razor Pages in ASP.NET Core, Accesat: 2026-06-04, 2025, url: https://learn.microsoft.com/en-us/aspnet/core/razor-pages/.

[24] Microsoft Corporation, NormalizationForm Enum, Accesat: 2026-06-04, 2025, url: https://learn.microsoft.com/en-us/dotnet/api/system.text.normalizationform.

[25] Microsoft Corporation, Session and state management in ASP.NET Core, Accesat: 2026-06-04, 2025, url: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app/state.

[26] Microsoft Corporation, Send an email using SMTP, Accesat: 2026-06-04, 2025, url: https://learn.microsoft.com/en-us/dotnet/framework/network-programming/how-to-send-an-email-using-smtp.

[27] James Newton-King, Json.NET (Newtonsoft.Json), Accesat: 2026-06-04, 2025, url: https://www.newtonsoft.com/json.

**Cum citezi în text (ca la Tudor Chițu):** pui `[n]` **la finalul propoziției sau paragrafului**, imediat înainte de punct — ex. „PostgreSQL oferă tranzacții ACID [3].” / „Căutarea ignoră diacriticele prin normalizare Unicode [24].” / „Razor Pages combină markup-ul cu code-behind [23].”

---

# Anexe (opțional)

**Anexa A** — Endpoint-uri: `GET /health`, `POST /api/chat`, `POST /api/chat/execute-order`

**Anexa B** — Rulare: `dotnet run`, `LangChain\start_ai_server.bat`, `cd Antrenare && python model.py`

**Anexa C** — Imagini produse: folder `ProiectWeb/Imagini`, sincronizare `dotnet run -- --sync-product-images`

---

# PREGĂTIRE LICENȚĂ — ÎNAINTE DE SCRIS ȘI POZE

## Formatare Word (ca `Licenta_Tudor_Chitu.pdf`)

| Element | Setare |
|---------|--------|
| Font | Times New Roman, **12 pt** (titluri capitol: 14 pt bold) |
| Interlinie | **1,5** |
| Margini | **2,5 cm** stânga/dreapta/sus/jos |
| Aliniere | Justified pentru paragrafe |
| Legendă figură | *Italic*, Times 12, centrat sub poză: *Figura X.Y: Descriere* |
| Numerotare capitole | 1, 1.1, 1.2 … (Word: Multilevel list) |
| Lungime țintă | **25–35 pagini** (fără anexe) |
| Cuprins | Generat automat din titluri (Referințe → Cuprins) |

## Pornire proiect (obligatoriu înainte de capturi)

1. **PostgreSQL** pornit (port **5433**, baza `E-commerce` — vezi `appsettings.json`).
2. Terminal 1:
   ```text
   cd ProiectWeb\ProiectWeb
   dotnet run
   ```
   URL tipic: `https://localhost:7xxx` sau `http://localhost:5000` (vezi consola).
3. Terminal 2 (pentru Comenzi AI / Prophet / chat Groq):
   ```text
   cd LangChain
   start_ai_server.bat
   ```
4. Verificare AI: browser → `http://127.0.0.1:8001/health` → `"status":"ok"`.

## Conturi de test

| Rol | Email | Parolă | Unde te loghezi |
|-----|-------|--------|-----------------|
| **Admin** | `admin@stock.com` | `Admin123!` | Login → redirect admin |
| **Client** | cont creat de tine | (alegi tu) | Register → confirmare email |

**Plată test Stripe:** card `4242 4242 4242 4242`, dată viitoare, CVC oarecare.

## Reguli captură ecran

- **Windows + Shift + S** → regiune sau fereastră → lipești în Word (Ctrl+V).
- Rezoluție: **1920×1080** recomandat; fereastra browser **maximizată**.
- **Ascunde** parole, token-uri, chei API (blur în Paint dacă apar).
- Fiecare figură: **un singur scop** (nu amesteca login + magazin în aceeași poză, decât dacă e colaj intenționat stânga/dreapta ca la Tudor).
- Sub fiecare poză în Word: legendă *Figura X.Y: …* (italic).
- Denumiri butoane exacte din UI: **Înregistrează-te**, **Conectează-te**, **Salvează** / **Salvat**, **Plătește**, **Analiză stoc**, **Comenzi**, **Stoc critic**, **Stagnant**, **Favorite**.

## Imagini produse — ce trebuie să știi pentru licență

| Regulă | Detaliu |
|--------|---------|
| Folder | `E-commerce\ProiectWeb\Imagini\` |
| Format | **JPG** |
| Denumire | `{NumeProdus}.jpg` — același text ca în DB câmpul `Name`, **fără** caractere interzise Windows |
| Ghilimele `"` (inch) | În DB: `Sony Bravia 43"` → fișier: `Sony Bravia 43.jpg` |
| Fallback | `Imagine negasita.jpg` (există deja în folder) |
| În DB | Câmp `Imagine` = doar numele fișierului, ex. `Samsung Galaxy A54.jpg` |
| Admin upload | Create/Edit produs → imagine salvată automat ca `{Nume}.jpg` în `Imagini` |
| Resincronizare | `dotnet run -- --sync-product-images` |

**Pentru licență:** la Fig. 4.3 arată cel puțin un produs cu poză reală + (opțional) un produs fără poză cu placeholder „negăsită”.

---

# GHID TABELE ȘI DIAGRAME — PAS CU PAS

## Tabel 1.2 — Comparație aplicații similare (Word)

**Unde în licență:** Capitolul 1, după paragraful eMAG/Shopify.

**Pași:**

1. Deschide **Word** → Insert → Table → **5 coloane × 5 rânduri**.
2. Prima linie (header, bold):

   | Platformă | Recomandări personalizate | Prognoză stoc / reaprovizionare | Chat / asistent AI | Stack principal |

3. Rândurile 2–4: **eMAG**, **Shopify**, **WooCommerce** — pune **Da/Parțial/Nu** sincer.
4. Rândul 5: **Proiectul de față** — toate **Da** (sau „Da (Prophet + reguli)” la prognoză).
5. Centrează tabelul, borduri simple. Fă screenshot **sau** lasă tabelul în Word (nu trebuie poză dacă e tabel nativ).
6. Legendă: *Figura 1.2: Comparație între platforme comerciale și proiectul implementat*

**Ce scrii la „Nu” la competitori:** ex. Shopify — prognoză doar cu app plătit; chat = plugin.

---

## Diagramă 1.1 — Arhitectura sistemului (draw.io)

**Unde:** Introducere, după preliminarii.

**Pași draw.io (diagrams.net):**

1. Intră pe https://app.diagrams.net/ → Blank diagram.
2. Trage **3 dreptunghiuri** mari, de sus în jos sau stânga-dreapta:
   - **Browser** (Chrome) — text: „Utilizator: Magazin, Coș, Admin, Chat”
   - **ASP.NET Core 8** — text: „Razor Pages, Stripe, API Chatbox”
   - **PostgreSQL** — text: „Produse, Comenzi, Favorite, Notificări”
3. Sub dreptunghiul ASP.NET, alt dreptunghi: **Python FastAPI :8001** — text: „Prophet, Groq LLM”.
4. Săgeți:
   - Browser ↔ ASP.NET (HTTP)
   - ASP.NET ↔ PostgreSQL (EF Core)
   - ASP.NET ↔ Python (JSON /api/chat, health)
5. Export **PNG** → Insert în Word.
6. Legendă: *Figura 1.1: Arhitectura generală a platformei*

---

## Diagramă 2.2 — Model entitate-relație (draw.io)

**Pași:**

1. 6 casete: **Utilizator**, **Product**, **Order**, **OrderItem**, **Favorite**, **Notification**.
2. Linii între ele:
   - User **1 — n** Order
   - Order **1 — n** OrderItem
   - Product **1 — n** OrderItem
   - User **1 — n** Favorite → Product
3. În casete notează câmpuri cheie: Product (Name, Price, Quantity, Category), Order (OrderDate, IsPaid).
4. Export PNG. Legendă: *Figura 2.2: Diagramă entitate-relație simplificată*

---

## Diagramă 2.3 — Flux recomandări (draw.io)

**Cutii în ordine:**

`PostgreSQL (comenzi)` → `Antrenare/model.py` → `hybrid_model.json` → `RecommendationService` → `Modal produs (3 recomandări)`

Sub ultima cutie, săgeată întoarsă: „Fallback SQL: cumpărate împreună”.

Legendă: *Figura 2.3: Fluxul de generare a recomandărilor hibride*

---

## Diagramă 3.3 — Flux Prophet + fallback (OBLIGATORIU pentru cap. 3.2)

**Cutii:**

1. Admin apasă **Comenzi AI**
2. .NET: produs stoc &lt; 20, istoric 14 zile
3. Diamond: **Python /health OK?**
   - Da → `stock_prediction.get_order_proposal` → cantitate Prophet
   - Nu → `CalculeazaCantitateReaprovizionare` (C#)
4. Chat: „Propunere: Nume — stoc X buc.”
5. Confirmă → stoc + email

**Opțional alături:** screenshot matplotlib cu `yhat` (vezi Ghid Figura 3.3b).

Legendă: *Figura 3.3: Fluxul decizional pentru propunerea de reaprovizionare*

---

*(Opțional în cap. 3.2: un paragraf scurt care compară „Prophet implicit” vs „varianta din proiect” —
fără tabel obligatoriu; ideea principală este renunțarea la sezonabilități/an și folosirea mediei
când datele sunt insuficiente.)*

---

# GHID POZE (SCREENSHOT-URI) — PAS CU PAS (36 PAȘI)

> **Reguli generale:**  
> - Rezoluție: tot ecranul sau fereastra browser, **Windows + Shift + S** → lipești în Word.  
> - **Loghează-te** cu rolul potrivit (Client vs Admin).  
> - **Ascunde** parola/email real (blur în Paint) dacă vrei.  
> - Sub fiecare poză în Word: *Figura X.Y: …* (Times 12, italic) — **exact ca în `Licenta_Tudor_Chitu.pdf`**.  
> - Pornește înainte: `dotnet run` + `LangChain\start_ai_server.bat` pentru Comenzi AI / chat.  
> - **Text scurt în Word** înainte de fiecare figură (1–2 paragrafe) — vezi modelele din Capitolul 4 de mai sus.

**Legendă coloane din tabelul de mai jos:**

- **Capitol** = unde inserezi în licență  
- **Text Word** = propoziție scurtă de pus deasupra figurii  
- **Pași** = ce faci concret  
- **Trebuie vizibil** = elemente obligatorii în captură  

---

## PARTEA A — Diagrame și tabele (draw.io / Word, NU screenshot site)

| # | Figură | Capitol | Text Word (exemplu) | Pași | Trebuie vizibil |
|---|--------|---------|-------------------|------|-----------------|
| 1 | **1.1** Arhitectură | 1.1 | „Arhitectura generală este prezentată în figura de mai jos.” | draw.io: Browser ↔ ASP.NET ↔ PostgreSQL + Python FastAPI :8001. Export PNG. | 4 box-uri + săgeți |
| 2 | **1.2** Tabel comparativ | 1.2 | „Tabelul compară platforme similare cu proiectul implementat.” | Word: 5 col × 5 rând. Header: Platformă, Recomandări, Prognoză, Chat AI, Stack. Rând final: **Proiectul de față**. | Toate Da la proiect |
| 3 | **2.1** Arbore proiect | 2.1 | „Organizarea codului sursă.” | VS / VS Code → `ProiectWeb/ProiectWeb` → expand `Pages`, `Services`, `API`, `Data`, `wwwroot`. | Arborele fișierelor |
| 4 | **2.2** Diagramă ER | 2.2 | „Modelul entitate-relație simplificat.” | draw.io: User, Product, Order, OrderItem, Favorite, Notification + relații 1-n. | Câmpuri cheie pe entități |
| 5 | **2.3** Flux recomandări (diagramă) | 2.4 / 3.1 | „Fluxul de generare a recomandărilor.” | draw.io: PostgreSQL → `model.py` → `hybrid_model.json` → inferență → modal. | Săgeată fallback SQL |
| 6 | **3.3** Flux Prophet | 3.2 | „Fluxul decizional pentru reaprovizionare.” | draw.io: Comenzi AI → stoc&lt;20 → health Python? → cantitate → Confirmă → stoc+email. | Diamond „Python OK?” |

---

## PARTEA B — Capturi aplicație (screenshot)

| # | Figură | Capitol | Text Word (exemplu) | Pași detaliați | Trebuie vizibil |
|---|--------|---------|-------------------|----------------|-----------------|
| 7 | **3.2** Recomandări | 3.1 | „Sistemul afișează trei recomandări în modalul produsului.” | Client → **Magazin** → click pe **imaginea** unui produs → modal → panoul **Recomandări** stânga. | 3 carduri cu poză + preț |
| 8 | **3.4** Comenzi AI | 3.2 | „Modulul Comenzi AI propune reaprovizionarea.” | Admin → tab jos-dreapta **Analiză stoc** → **Comenzi** → mesaj *Propunere: [Nume] — stoc actual X bucăți* → dialog Confirmă. | Buton Comenzi + mesaj propunere |
| 9 | **3.5** Chat client | 3.3 | „Asistentul răspunde întrebărilor clienților.” | Client → buton flotant **Suport** (jos-dreapta) → întrebare → Enter → răspuns. | Întrebare + răspuns Groq |
| 10 | **3.6** Stoc critic | 3.3 | „Listarea produselor cu stoc critic.” | Admin → **Analiză stoc** → **Stoc critic**. | Text STOC CRITIC + produse ≤15 |
| 11 | **3.8** Stagnant/Favorite | 3.4 | „Analiza prescriptivă propune ajustări de preț.” | Admin → **Stagnant** sau **Favorite** → scroll la preț vechi → sugerat. | Preț vechi → preț sugerat |
| 12 | **4.1** Login/Register | 4.1 | „Paginile de autentificare și înregistrare.” | Incognito → **Acasă** (hero) → **Conectează-te** → login → link Register. **Colaj** 2 poze stânga/dreapta (ca Tudor Fig. 4.1). | Butoane Înregistrează-te / Conectează-te |
| 13 | **4.2** Profil | 4.1 | „Profilul și schimbarea parolei.” | Client → inițială navbar → **Profil** → apoi **Schimbă parola**. | Nickname, telefon, formular parolă |
| 14 | **4.3** Magazin | 4.2 | „Interfața magazinului cu filtre active.” | Client → **Magazin** → categorie + slider preț + căutare → sortare **Preț crescător** (buton activ terracotta) → scroll jos paginare. | Filtre + ≥4 carduri **cu imagini** + paginare |
| 15 | **4.4** Modal produs | 4.2 | „Detaliile produsului și acțiunile de cumpărare.” | Click produs → modal dreapta: nume, preț, descriere, cantitate, **Adaugă în coș**. | Buton Adaugă în coș + recomandări |
| 16 | **4.5** Favorite | 4.2 | „Produsele salvate de client.” | Client → **Favorite** → cel puțin 1 produs cu **Salvat**. | Grid favorite + poze |
| 17 | **4.6** Coș | 4.3 | „Coșul de cumpărături.” | Adaugă 2 produse → **Coș** → cantități + total + **Plătește**. | 2+ linii + total RON |
| 18 | **4.7** Plată | 4.3 | „Fluxul de plată online.” | **Plătește** → Stripe test 4242… → pagina succes. | Stripe SAU „Plată confirmată” |
| 19 | **4.8** Email | 4.3 | „Confirmarea prin email.” | Gmail cont din `EmailSettings` → mail **Confirmare Comandă**. | Subiect + linii produse |
| 20 | **4.9** Istoric | 4.3 | „Istoricul comenzilor utilizatorului.” | Client → inițială → **Istoric comenzi** → #1, #2 + paginare dacă >10. | Comandă #n + dată + total |
| 21 | **4.10** Statistici | 4.3 | „Statistici agregate pe luni.” | Client → **Statistici** (din meniu profil dacă există). | Grafic linie / categorii |
| 22 | **4.11** Admin produse | 4.4 | „Panoul de administrare a produselor.” | Admin → **Produse** → tabel Nume, Preț, Stoc, Categorie + **Produs nou**. | Tabel + badge stoc |
| 23 | **4.12** Formular produs | 4.4 | „Adăugarea și editarea produselor.” | Admin → **Produs nou** SAU **Editează** → formular + câmp **Imagine produs** + preview la Edit. **Colaj** Create stânga / Edit dreapta. | Toate câmpurile + upload imagine |
| 24 | **4.13** Comenzi admin | 4.4 | „Vizualizarea comenzilor în panoul administrator.” | Admin → **Comenzi** → sortare **Cele mai noi** (activ) → deschide accordion → paginare dacă >10. | Sort bar + accordion deschis |
| 25 | **4.14** Notificări | 4.4 | „Notificările generate de sistem.” | Admin → **Notificări** → mesaje stoc / comandă AI + paginare. | Tabel + date |
| 26 | **4.15** Top favorite admin | 4.4 | „Clasamentul produselor favorite.” | Admin → **Favorite** (navbar) → top produse + count. | Listă + număr favorite |
| 27 | **4.16** Confirmare AI | 4.5 / 3.2 | „Efectul confirmării reaprovizionării.” | Confirmă Comenzi AI → **Notificări** „Comandă AI acceptată” → **Produse** stoc crescut. | Notificare + stoc actualizat |
| 28 | **4.17** Pagini publice | 4.5 | „Paginile principale ale magazinului.” | **Acasă** (hero + bento) + **Despre** + opțional **Contact**. Colaj 2–3 capturi. | Hero + secțiune Despre |
| 29 | **4.18** Imagini produse | 2.3 / 4.2 | „Gestionarea imaginilor produselor.” | Explorer: folder `ProiectWeb/Imagini` + browser magazin același produs (ex. televizor cu `"` în nume). | Fișier fără `"` + poză corectă în site |
| 30 | **3.3b** Grafic Prophet | 3.2 | „Exemplu prognoză Prophet.” | Opțional: output matplotlib din `stock_prediction.py`. | Grafic yhat |
| 31 | **A.1** Health AI | Anexă | „Verificarea serverului AI.” | `http://127.0.0.1:8001/health` → JSON ok. | status + prophet ready |

---

## Pași extinși (duplicate pentru claritate — cele mai importante)

### Figura 1.1 — Arhitectură

→ Fă **diagrama** draw.io (nu screenshot), vezi secțiunea GHID TABELE.

### Figura 4.18 — Folder imagini (recomandat, cap. 4)

1. Deschide **Explorer** → `Desktop\E-commerce\ProiectWeb\Imagini`.
2. Screenshot cu ~10 fișiere `.jpg` vizibile + `Imagine negasita.jpg`.
3. Legendă: *Figura 4.18: Stocarea locală a imaginilor produselor*

### Figura 4.3 — Magazin cu filtre și sortare

1. Client → **Magazin** (`/Client/Products`).
2. Panoul din stânga: categorie, slider preț, căutare — activează cel puțin un filtru.
3. Click **Preț crescător** — butonul activ devine terracotta.
4. Asigură-te că se văd **cel puțin 4–6 carduri cu imagini reale** + **paginarea** jos (dacă >15 produse).
5. Legendă: *Figura 4.3: Interfața magazinului cu filtre, sortare și imagini produse*

### Figura 4.12 — Formular produs admin

1. Admin → **Produse** → **Produs nou**.
2. Screenshot formular: Nume, Categorie, Preț, Stoc, **Imagine produs**, Descriere, **Salvează**.
3. Deschide **Editează** la un produs existent → screenshot cu **preview imagine** + upload.
4. Colaj stânga/dreapta (ca Tudor).
5. Legendă: *Figura 4.12: Formularul de adăugare și editare produse*

### Figura 4.18 — Potrivire nume produs ↔ fișier imagine

1. Găsește un produs cu `"` în nume (ex. `LG OLED 55"`).
2. Screenshot Explorer: fișier `LG OLED 55.jpg` (fără ghilimele).
3. Screenshot magazin: același produs cu **poză corectă** (nu placeholder).
4. Legendă: *Figura 4.18: Corelarea numelui produsului cu fișierul imagine*

---

### Figura 2.1 — Structură proiect

1. Deschide **Visual Studio** sau **VS Code** cu folderul `ProiectWeb/ProiectWeb`.
2. Extinde folderele: `Pages`, `Services`, `API`, `Data`, `wwwroot`.
3. Screenshot la panoul din stânga (arborele de fișiere vizibil).
4. Legendă: *Figura 2.1: Organizarea directoarelor proiectului web*

---

### Figura 3.2 — Recomandări în modal

1. Loghează-te ca **client**.
2. Mergi la **Magazin** (`/Client/Products`).
3. Click pe **orice produs** → se deschide modalul.
4. Scroll puțin până vezi titlul **recomandări** și **exact 3 carduri** cu imagini + nume + preț.
5. Screenshot cu modalul deschis (nu doar fundalul paginii).
6. Legendă: *Figura 3.2: Recomandări de produse în fereastra de detaliu*

---

### Figura 3.3b — Grafic Prophet (opțional, bonus)

1. În `LangChain`, script scurt sau notebook: apelează `predict_daily_sales_prophet` cu un istoric real exportat din DB.
2. Sau salvează `forecast.plot()` din Prophet dacă adaugi 2 linii în `stock_prediction.py` temporar.
3. Legendă: *Figura 3.3b: Exemplu de prognoză Prophet pentru vânzări zilnice*

*(Dacă e prea greu, folosește doar diagrama 3.3 din draw.io.)*

---

### Figura 3.4 — Comenzi AI + Confirmă

1. Loghează-te ca **Admin**.
2. Click pe tab-ul vertical **„Analiză stoc”** (dreapta ecranului).
3. Click butonul **„Comenzi”** din panoul de acțiuni rapide.
4. Așteaptă mesajul: **Propunere: [nume produs] — stoc actual [X] bucăți**.
5. Apare întrebare tip **„Comandăm … buc?”** → fă screenshot **înainte** de Confirmă (sau și cu dialogul vizibil).
6. **Pornește** `start_ai_server.bat` înainte ca să poți menționa în text că cantitatea vine din Prophet.
7. Legendă: *Figura 3.4: Propunere de reaprovizionare generată de modulul Comenzi AI*

---

### Figura 3.5 — Chat client

1. Loghează-te **client**.
2. Jos-dreapta: butonul **„Suport”** (text, nu iconiță) → click.
3. Scrie: „Nu pot adăuga produsul în coș” sau „Ce produse am în coș?”.
4. Enter → așteaptă răspunsul Groq.
5. Screenshot cu **întrebarea ta + răspunsul** vizibile.
6. Legendă: *Figura 3.5: Asistentul de suport pentru clienți*

---

### Figura 3.6 — Stoc critic în panou admin

1. Admin → panou **Analiză stoc** deschis.
2. Click **Stoc critic**.
3. Screenshot cu lista **STOC CRITIC:** și produse ≤ 15 buc.
4. Legendă: *Figura 3.6: Listarea produselor cu stoc critic în panoul de analiză*

---

### Figura 3.8 — Stagnant sau Favorite

1. În același panou, click **Stagnant (30 zile)** SAU **Favorite**.
2. Screenshot cu textul care include **preț vechi → preț sugerat** (la stagnant/favorite cu AI prescriptiv).
3. Legendă: *Figura 3.8: Analiză prescriptivă pentru produse stagnante*

---

### Figura 4.1 — Login și Register

1. **Deloghează-te** (sau browser incognito).
2. Mergi la **Acasă** → screenshot cu butoanele **„Înregistrează-te”** și **„Conectează-te”** (hero).
3. Click **Conectează-te** (navbar sau hero) → screenshot pagină de autentificare.
4. Click link **Înregistrare** → screenshot pagină register.
5. Poți pune **2 poze una sub alta** sau **un colaj** (login + register).
6. Legendă: *Figura 4.1: Paginile de autentificare și înregistrare*

---

### Figura 4.2 — Profil

1. Loghează-te client.
2. Click **inițiala** din navbar (dreapta) → meniu dropdown.
3. Click **Profil** → screenshot nickname + telefon.
4. Deschide **Parolă** din meniul profil → screenshot formular (fără parole vizibile).
5. Legendă: *Figura 4.2: Pagina de profil și schimbare parolă*

---

### Figura 4.3 — Magazin cu filtre și sortare

1. Client → **Magazin** (`/Client/Products`).
2. Panoul din stânga: categorie, slider preț, căutare — mișcă sliderul sau scrie la căutare.
3. Click **Preț crescător** (sau **Nume A → Z**) — butonul activ devine terracotta.
4. Asigură-te că se văd **cel puțin 4–6 carduri** + **paginarea jos** (dacă ai >15 produse).
5. Legendă: *Figura 4.3: Interfața magazinului cu filtre și sortare active*

---

### Figura 4.4 — Modal produs (evaluare)

La fel ca 3.2, dar în capitolul 4 poți include și butonul **Adaugă în coș**.

Legendă: *Figura 4.4: Detalii produs și acțiuni de cumpărare*

---

### Figura 4.6 — Coș

1. Adaugă **2 produse** în coș.
2. Mergi la pagina **Coș**.
3. Screenshot: listă produse, cantități, total.
4. Legendă: *Figura 4.6: Coșul de cumpărături*

---

### Figura 4.7 — Plată / succes

1. Din coș → **Finalizează** → Stripe test (card 4242…).
2. Screenshot fie pagina **Stripe**, fie **Order Success** după plată.
3. Legendă: *Figura 4.7: Fluxul de plată online*

---

### Figura 4.8 — Email confirmare

1. Deschide Gmail la `raresmarian3344@gmail.com` (sau contul din EmailSettings).
2. Găsește mailul **Confirmare Comandă**.
3. Screenshot cu subiect + 1–2 linii de produse (blur adresa dacă vrei).
4. Legendă: *Figura 4.8: Email de confirmare a plății*

---

### Figura 4.9 — Istoric client

1. Client → click inițială navbar → **Istoric comenzi** (sau din meniu profil).
2. Screenshot: **Comandă #1**, **#2**, date și total.
3. Dacă ai >10 comenzi: scroll jos → include **paginarea** (1, 2, 3…).
4. Legendă: *Figura 4.9: Istoricul comenzilor pentru utilizator*

---

### Figura 4.10 — Statistici

1. Client → **Statistici** (dacă există pagina).
2. Screenshot cu graficul pe luni.
3. Legendă: *Figura 4.10: Statistici de cumpărare pe luni*

---

### Figura 4.11 — Admin produse

1. Admin → **Produse**.
2. Screenshot tabel cu coloane Nume, Preț, Stoc, Categorie.
3. Legendă: *Figura 4.11: Panoul de administrare a produselor*

---

### Figura 4.13 — Comenzi admin

1. Admin → **Comenzi** (navbar).
2. Screenshot bara de sortare: **Cele mai noi / vechi / mari / mici** (un buton activ).
3. Screenshot accordion cu **Comandă #1** deschisă (produse + total).
4. Dacă ai >10 comenzi: include **paginarea** jos.
5. Legendă: *Figura 4.13: Vizualizarea comenzilor în panoul administrator*

---

### Figura 4.14 — Notificări

1. Admin → **Notificări**.
2. Screenshot tabel cu mesaje („Stoc scăzut…”, „Comandă AI acceptată”) și date.
3. Dacă ai >10 notificări: include **paginarea** jos.
4. Legendă: *Figura 4.14: Notificări generate la scăderea stocului*

---

### Figura 4.16 — După confirmare Comenzi AI

1. Confirmă o comandă AI.
2. Mergi la **Notificări** → screenshot „Comandă AI acceptată”.
3. Opțional: **Produse** → stoc crescut la produsul respectiv.
4. Legendă: *Figura 4.16: Efectul confirmării propunerii de reaprovizionare*

---

### Figura 4.17 — Homepage și Despre

1. **Delogat** sau client: screenshot **Acasă** — hero întunecat + secțiune bento + categorii (dacă există).
2. Click **Despre** → screenshot poveste echipă + valori (01/02/03) + categorii din DB.
3. Opțional: **Contact** — formular + date email.
4. Legendă: *Figura 4.17: Paginile publice principale*

---

### Figura A.1 — Health server AI (anexă)

1. Pornește `start_ai_server.bat`.
2. Browser: `http://127.0.0.1:8001/health`
3. Screenshot JSON: `"status":"ok"`, `"prophet":"ready"`.
4. Legendă: *Figura A.1: Verificarea disponibilității serverului AI*

---

## Listă rapidă — ce poze sunt obligatorii (minimum pentru notă bună)

| Prioritate | Figuri | Note |
|------------|--------|------|
| **Must** | 1.1, 1.2, 2.2, 3.3, 3.4, 3.6, 4.3, 4.4, 4.6, 4.9, 4.11, 4.12, 4.13, 4.14 | Minimum ~14 figuri + diagrame |
| **Recomandat** | 2.1, 2.3, 2.4, 3.2, 3.5, 3.8, 4.1, 4.2, 4.5, 4.7, 4.16, 4.17, 4.18 | Colaje stânga/dreapta ca Tudor |
| **Opțional** | 3.3b, 4.8, 4.10, 4.15, A.1 | Bonus / anexă |

**Checklist complet (31 pași numerotați):** tabelul **PARTEA A + PARTEA B** de mai sus + `RAPORT_MODIFICARI.txt` → **GHID POZE LICENȚĂ**.

**Ordine recomandată de lucru:**

1. Scrii cap. 1–3 (text + diagrame draw.io).
2. Faci toate capturile (PARTEA B) într-o sesiune cu app pornită.
3. Scrii cap. 4 — lipești poze + legende + 1 paragraf per subsecțiune.
4. Concluzii + Bibliografie → Cuprins automat → verificare pagini.

**Formatare Word:** vezi secțiunea **PREGĂTIRE LICENȚĂ** (Times 12, 1,5, margini 2,5 cm, ~25–35 pagini).

---

*Document: `Licenta_E-commerce_SABLON.md` — sablon separat. Nu modifica `Licenta_Tudor_Chitu.pdf`.*
