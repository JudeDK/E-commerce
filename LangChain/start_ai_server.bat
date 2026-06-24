@echo off
cd /d "%~dp0"

if not exist "venv\Scripts\python.exe" (
    echo Creaza mediul virtual:
    python -m venv venv
)

set PYTHONNOUSERSITE=1
call venv\Scripts\activate.bat

echo Instalare dependinte (numpy ^< 2 pentru Prophet)...
venv\Scripts\python.exe -m pip install --upgrade pip
venv\Scripts\python.exe -m pip install -r requirements.txt

if not exist ".env" (
    echo Copiaza .env.example in .env si adauga GROQ_API_KEY
    copy .env.example .env
    pause
    exit /b 1
)

set AI_SERVER_PORT=8001
set MPLBACKEND=Agg
echo Pornire server AI pe http://127.0.0.1:%AI_SERVER_PORT%
venv\Scripts\python.exe ai_server.py
