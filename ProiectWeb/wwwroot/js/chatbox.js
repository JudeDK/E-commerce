window.addEventListener("DOMContentLoaded", () => {

    const chatBtn = document.getElementById("chat-toggle");
    const chatBox = document.getElementById("chat-box");
    const chatInput = document.getElementById("chat-input");
    const chatSend = document.getElementById("chat-send");
    const chatMessages = document.getElementById("chat-messages");

    if (!chatBtn || !chatBox || !chatMessages) {
        console.warn("⚠️ Chatbox-ul nu este disponibil pe această pagină (poate userul nu e logat).");
        return;
    }

    // 🧠 1️⃣ Determină utilizatorul curent din atributul <body data-user="...">
    let userKey = "chat_history_guest";
    const currentUser = document.body.dataset.user;

    if (currentUser && currentUser.trim() !== "") {
        const safeKey = currentUser.replace(/[^a-zA-Z0-9._-]/g, "_");
        userKey = `chat_history_${safeKey}`;
        console.log(`👤 Istoric chat pentru utilizatorul: ${currentUser}`);
    } else {
        console.log("👥 Utilizator guest — chat comun.");
    }

    // 🧠 2️⃣ Încarcă conversația salvată pentru userul curent (din sessionStorage)
    const savedChat = sessionStorage.getItem(userKey);
    if (savedChat) {
        chatMessages.innerHTML = savedChat;
        chatMessages.scrollTop = chatMessages.scrollHeight;
        console.log("💾 Istoric chat restaurat din sessionStorage pentru acest utilizator.");
    }

    // 🎛️ 3️⃣ Arată / ascunde chatul
    chatBtn.addEventListener("click", () => {
        chatBox.classList.toggle("hidden");
        if (!chatBox.classList.contains("hidden")) {
            chatMessages.scrollTop = chatMessages.scrollHeight;
        }
    });

    // 🚀 4️⃣ Trimite mesajul
    async function sendMessage() {
        const message = chatInput.value.trim();
        if (!message) return;

        const userMsg = `<div class="user-msg"><strong>Tu:</strong> ${message}</div>`;
        chatMessages.insertAdjacentHTML("beforeend", userMsg);
        chatInput.value = "";
        chatMessages.scrollTop = chatMessages.scrollHeight;

        // Salvează conversația curentă în sessionStorage
        sessionStorage.setItem(userKey, chatMessages.innerHTML);

        try {
            const res = await fetch("/api/chat", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ question: message })
            });

            const data = await res.json();
            const botMsg = `<div class="bot-msg"><strong>Asistent:</strong> ${data.answer}</div>`;
            chatMessages.insertAdjacentHTML("beforeend", botMsg);
            chatMessages.scrollTop = chatMessages.scrollHeight;

            // Actualizează istoricul după fiecare răspuns
            sessionStorage.setItem(userKey, chatMessages.innerHTML);
        } catch (err) {
            const errMsg = `<div class="bot-msg error"><strong>Asistent:</strong> Eroare la conectare.</div>`;
            chatMessages.insertAdjacentHTML("beforeend", errMsg);
        }

        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    // ✉️ 5️⃣ Trimite mesajul la click
    chatSend.addEventListener("click", sendMessage);

    // ⌨️ 6️⃣ Trimite mesajul cu Enter (fără Shift)
    chatInput.addEventListener("keydown", (e) => {
        if (e.key === "Enter" && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    });

    // 🧹 7️⃣ Buton pentru ștergerea conversației curente
    const clearBtn = document.createElement("button");
    clearBtn.textContent = "🗑️ Șterge conversația";
    clearBtn.className = "btn btn-sm btn-outline-danger mt-2";
    clearBtn.onclick = () => {
        if (confirm("Sigur vrei să ștergi conversația acestui cont?")) {
            chatMessages.innerHTML = "";
            sessionStorage.removeItem(userKey);
        }
    };
    chatBox.appendChild(clearBtn);
});
