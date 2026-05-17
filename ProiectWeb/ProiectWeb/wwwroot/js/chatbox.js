window.addEventListener("DOMContentLoaded", () => {
    const chatBox = document.getElementById("chat-box");
    const chatInput = document.getElementById("chat-input");
    const chatSend = document.getElementById("chat-send");
    const chatMessages = document.getElementById("chat-messages");

    if (!chatBox || !chatMessages) return;

    const currentUser = document.body.dataset.user || "guest";
    const userKey = `chat_history_${currentUser.replace(/[^a-z0-9]/gi, '_')}`;

    // Restaurare istoric
    const savedChat = sessionStorage.getItem(userKey);
    if (savedChat) {
        chatMessages.innerHTML = savedChat;
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    // Funcție Trimitere Mesaj
    async function sendMessage() {
        const message = chatInput.value.trim();
        if (!message) return;

        chatMessages.insertAdjacentHTML("beforeend", `<div class="user-msg"><strong>Tu:</strong> ${message}</div>`);
        chatInput.value = "";
        chatMessages.scrollTop = chatMessages.scrollHeight;

        try {
            const res = await fetch("/api/chat", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ question: message })
            });

            const data = await res.json();

            let botHtml = `<div class="bot-msg"><strong>Asistent:</strong> ${data.answer}</div>`;

            // Verificare Acțiune (Buton Comandă)
            if (data.action && data.action.type === "execute_order") {
                const btnId = `btn-${Math.random().toString(36).substr(2, 5)}`;
                botHtml += `
                    <div class="p-2 border rounded bg-light mt-2" style="border-left: 4px solid #28a745 !important;">
                        <p class="small mb-2">Comandăm <strong>${data.action.quantity} buc</strong> de ${data.action.product}?</p>
                        <button id="${btnId}" onclick="confirmOrder('${data.action.product}', ${data.action.quantity}, '${btnId}')" 
                                class="btn btn-sm btn-success w-100">📧 Confirmă & Trimite Mail</button>
                    </div>`;
            }

            chatMessages.insertAdjacentHTML("beforeend", botHtml);
            sessionStorage.setItem(userKey, chatMessages.innerHTML);
        } catch (err) {
            chatMessages.insertAdjacentHTML("beforeend", `<div class="text-danger small">Eroare server.</div>`);
        }
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    window.confirmOrder = async function (product, qty, btnId) {
        const btn = document.getElementById(btnId);
        if (!confirm(`Trimite comanda pentru ${product}?`)) return;

        btn.disabled = true;
        btn.innerHTML = "⌛ Se procesează...";

        try {
            const res = await fetch('/api/chat/execute-order', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ productName: product, quantity: qty })
            });
            const result = await res.json();
            if (result.success) {
                btn.className = "btn btn-sm btn-outline-secondary w-100 disabled";
                btn.innerHTML = `✔️ Comandat (Stoc nou: ${result.newQuantity})`;
            }
        } catch (err) {
            btn.innerText = "❌ Eroare";
            btn.disabled = false;
        }
    };

    if (chatSend) chatSend.addEventListener("click", sendMessage);
    chatInput.addEventListener("keydown", (e) => { if (e.key === "Enter") sendMessage(); });

    // Helper Butoane Admin
    window.sendAdminCommand = function (cmd) {
        chatInput.value = cmd;
        sendMessage();
    };
});