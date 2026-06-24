window.addEventListener("DOMContentLoaded", () => {
    const chatContainer = document.getElementById("chat-container");
    const chatBox = document.getElementById("chat-box");
    const chatInput = document.getElementById("chat-input");
    const chatSend = document.getElementById("chat-send");
    const chatMessages = document.getElementById("chat-messages");
    const chatToggle = document.getElementById("chat-toggle");

    const isAdminPanel = chatBox?.classList.contains("admin-chat-sidebar");
    const isUserChat = Boolean(chatContainer && chatBox && !isAdminPanel);

    // Buton flotant dreapta-jos — suport client
    if (isUserChat && chatToggle && chatBox) {
        chatToggle.addEventListener("click", (e) => {
            e.preventDefault();
            e.stopPropagation();
            chatBox.classList.toggle("hidden");
            if (!chatBox.classList.contains("hidden")) {
                chatInput?.focus();
            }
        });
    }

    if (!chatBox || !chatMessages) return;

    function scrollChatToBottom() {
        chatMessages.scrollTop = chatMessages.scrollHeight;
        const last = chatMessages.lastElementChild;
        if (last) last.scrollIntoView({ block: "end", behavior: "smooth" });
    }

    const currentUser = document.body.dataset.user || "guest";
    const userKey = `chat_history_${currentUser.replace(/[^a-z0-9]/gi, "_")}`;

    const savedChat = sessionStorage.getItem(userKey);
    if (savedChat) {
        chatMessages.innerHTML = savedChat;
        scrollChatToBottom();
    }

    async function postChatMessage(message, showUserBubble) {
        if (showUserBubble) {
            const label = isAdminPanel ? message.replace("admin_", "").replace(/_/g, " ") : message;
            chatMessages.insertAdjacentHTML(
                "beforeend",
                `<div class="user-msg"><strong>Tu:</strong> ${label}</div>`
            );
            scrollChatToBottom();
        }

        try {
            const res = await fetch("/api/chat", {
                method: "POST",
                credentials: "same-origin",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ question: message }),
            });

            const data = await res.json();
            let botHtml = `<div class="bot-msg"><strong>Asistent:</strong> ${data.answer}</div>`;

            if (data.action && data.action.type === "execute_order") {
                const btnId = `btn-${Math.random().toString(36).substr(2, 5)}`;
                const product = data.action.product.replace(/'/g, "\\'");
                botHtml += `
                    <div class="p-2 border rounded bg-light mt-2">
                        <p class="small mb-2">Comandăm <strong>${data.action.quantity} buc</strong> de ${data.action.product}?</p>
                        <button id="${btnId}" type="button" onclick="confirmOrder('${product}', ${data.action.quantity}, '${btnId}')"
                            class="btn btn-sm btn-success w-100">Confirmă</button>
                    </div>`;
            }

            chatMessages.insertAdjacentHTML("beforeend", botHtml);
            sessionStorage.setItem(userKey, chatMessages.innerHTML);
        } catch {
            chatMessages.insertAdjacentHTML("beforeend", `<div class="text-danger small">Eroare server.</div>`);
        }
        scrollChatToBottom();
    }

    async function sendMessage() {
        if (isAdminPanel) return;
        if (!chatInput) return;
        const message = chatInput.value.trim();
        if (!message) return;
        chatInput.value = "";
        await postChatMessage(message, true);
    }

    window.confirmOrder = async function (product, qty, btnId) {
        const btn = document.getElementById(btnId);
        if (!btn || !confirm(`Trimite comanda pentru ${product}?`)) return;

        btn.disabled = true;
        btn.innerHTML = "Se procesează...";

        try {
            const res = await fetch("/api/chat/execute-order", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ productName: product, quantity: qty }),
            });
            const result = await res.json();
            if (result.success) {
                btn.className = "btn btn-sm btn-outline-secondary w-100 disabled";
                btn.innerHTML = `Comandat (Stoc: ${result.newQuantity})`;
            }
        } catch {
            btn.innerText = "Eroare";
            btn.disabled = false;
        }
    };

    if (!isAdminPanel) {
        if (chatSend) chatSend.addEventListener("click", sendMessage);
        if (chatInput) {
            chatInput.addEventListener("keydown", (e) => {
                if (e.key === "Enter") sendMessage();
            });
        }
    }

    window.sendAdminCommand = function (cmd) {
        if (isAdminPanel) {
            postChatMessage(cmd, true);
            return;
        }
        if (chatInput && chatSend) {
            chatInput.value = cmd;
            chatSend.click();
        }
    };
});
