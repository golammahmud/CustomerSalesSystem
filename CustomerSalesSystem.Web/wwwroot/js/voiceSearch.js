//store selected language in local storage 
document.addEventListener('DOMContentLoaded', () => {
    const langSelect = document.getElementById('languageSelect');
    const savedLang = localStorage.getItem('voiceLang');
    if (langSelect && savedLang) {
        langSelect.value = savedLang;
    }

    langSelect?.addEventListener('change', () => {
        const selectedLang = langSelect.value;
        localStorage.setItem('voiceLang', selectedLang);
    });
});
window.getVoiceLang = function () {
    return localStorage.getItem("voiceLang") || "en-US";
};

//text to spech
if (!'speechSynthesis' in window) {
    alert("Your browser does not support text-to-speech.");
}
window.speak = function (text, lang = 'en-US') {
    if (!('speechSynthesis' in window)) {
        alert("Sorry, speech synthesis is not supported in this browser.");
        return;
    }

    const msg = new SpeechSynthesisUtterance(text);
    msg.lang = lang;

    const voices = window.speechSynthesis.getVoices();

    // Try to get a sweet female voice by name
    const preferredVoice = voices.find(v =>
        v.name === "Google US English" ||
        v.name === "Google UK English Female" ||
        v.name === "Samantha" ||
        v.name.includes("Zira")
    );

    msg.voice = preferredVoice || voices.find(v => v.lang === lang) || voices[0];

    speechSynthesis.cancel();
    speechSynthesis.speak(msg);
    console.log("🎙️ Speaking:", text, "| Voice:", msg.voice?.name);
};

// Fix: Load voices before first use
window.speechSynthesis.onvoiceschanged = () => {
    console.log("✅ Voices loaded:", speechSynthesis.getVoices());
};



// Search + Chat Voice Recognition
let recognition;
let isListening = false;
const language = window.getVoiceLang();

window.initVoiceSearch = function ({
    inputId = "searchQuery",
    onResult = null,
    autoSubmit = false,
    submitFormId = null
} = {}) {
    const inputElement = document.getElementById(inputId);
    const formElement = submitFormId ? document.getElementById(submitFormId) : inputElement?.form;

    if (!('webkitSpeechRecognition' in window || 'SpeechRecognition' in window)) {
        console.warn("Speech recognition not supported in this browser.");
        return;
    }

    recognition = new (window.SpeechRecognition || window.webkitSpeechRecognition)();
    recognition.lang = language;
    recognition.continuous = true;  // keep listening until stopped explicitly
    recognition.interimResults = false;

    recognition.onstart = () => {
        isListening = true;
        console.log("🎤 Listening started...");
        toggleVoiceUI(true);
    };

    recognition.onresult = (event) => {
        // Combine all results since continuous mode might produce multiple results
        let transcript = '';
        for (let i = event.resultIndex; i < event.results.length; i++) {
            transcript += event.results[i][0].transcript;
        }
        transcript = transcript.trim();
        console.log("Result:", transcript);

        if (inputElement) inputElement.value = transcript;

        if (typeof onResult === "function") onResult(transcript);

        // Optional: Auto submit form or trigger processing here
        // if (autoSubmit && formElement) formElement.submit();
       
        handleVoiceTranscript(transcript);

        //const chatStatus = document.getElementById("chatStatus");
        //if (chatStatus) chatStatus.textContent = "🤖 Thinking...";

        //fetch("/Search/GlobalSearch?handler=VoiceSearch", {
        //    method: "POST",
        //    headers: {
        //        "Content-Type": "application/x-www-form-urlencoded"
        //    },
        //    body: `userQuery=${encodeURIComponent(transcript)}`
        //})
        //    .then(res => {
        //        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        //        return res.json();
        //    })
        //    .then(data => {
        //        if (chatStatus) chatStatus.textContent = ""; // clear status

        //        if (data.intent === "Chat") {
        //            // Show chat reply somewhere (replace alert if needed)
        //            speak(data.response);
        //        } else {
        //            const filters = encodeURIComponent(JSON.stringify(data.filters));
        //            window.location.href = `/Search/GlobalSearch?intent=${data.intent}&filters=${filters}`;
        //        }
        //    })
        //    .catch(err => {
        //        if (chatStatus) chatStatus.textContent = "❌ Failed to process query.";
        //        console.error("❌ Error:", err);
        //        speak(" Failed to process query");
        //        setTimeout(() => chatStatus.textContent = "", 3000);
        //    });
    };

    recognition.onerror = (event) => {
        console.error("Speech recognition error:", event.error);
        isListening = false;
        toggleVoiceUI(false);
        alert("Microphone error: " + event.error);
    };

    recognition.onend = () => {
        isListening = false;
        console.log("🛑 Listening stopped.");
        toggleVoiceUI(false);
        // Optionally restart recognition automatically if you want ultra-long listening
        // But be careful: this can cause unexpected loops or bugs
        // recognition.start();
    };
};

window.startVoiceSearch = () => {

    if (recognition && !isListening) {
        window.speak("Hi there, how can I help you?", language);
        recognition.start();
    }
};

window.stopVoiceSearch = () => {
    if (recognition && isListening) {
        recognition.stop();
    }
};

function toggleVoiceUI(listening) {
    const micBtn = document.getElementById("voiceBtn");
    const stopBtn = document.getElementById("stopBtn");
    if (micBtn) micBtn.disabled = listening;
    if (stopBtn) stopBtn.disabled = !listening;
}


// Optional refresh button logic
document.getElementById("refreshBtn")?.addEventListener("click", () => {
    const input = document.getElementById("searchQuery");
    if (input) input.value = "";
    location.reload();
});

// Optional focus on search input when page loads
window.onload = () => {
    document.getElementById("searchQuery")?.focus();
};


function handleVoiceTranscript(transcript) {
    const chatStatus = document.getElementById("chatStatus");
    if (chatStatus) chatStatus.textContent = "🤖 Thinking...";

    fetch("/Search/GlobalSearch?handler=VoiceSearch", {
        method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded"
        },
        body: `userQuery=${encodeURIComponent(transcript)}`
    })
        .then(res => {
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            return res.json();
        })
        .then(data => {
            if (chatStatus) chatStatus.textContent = ""; // clear thinking status

            switch (data.intent?.toLowerCase()) {
                case "chat":
                    
                    const userMsg = transcript;
                    const aiMsg = data.response || "Sorry, I didn't catch that.";
                    // Show modal
                    const chatModal = document.getElementById("chatModal");
                    const chatBox = document.getElementById("chatConversation");

                    if (chatModal && chatBox) {
                        chatModal.style.display = "block";
                        chatBox.innerHTML += `<div class="user"><strong>You:</strong> ${userMsg}</div>`;
                        chatBox.innerHTML += `<div class="ai"><strong>AI:</strong> ${aiMsg}</div>`;
                        chatBox.scrollTop = chatBox.scrollHeight; // auto-scroll
                    }

                    speak(aiMsg);
                    //speak(data.response || "Sorry, I didn't catch that.");
                    break;

                case "navigate":
                    if (data.target) {
                        const targetUrl = data.target.includes("{id}")
                            ? data.target.replace("{id}", data.id || "0")
                            : data.target;
                        window.location.href = targetUrl;
                    } else {
                        speak("Navigation target is missing.");
                    }
                    break;

                case "refresh":
                    window.location.reload();
                    break;

                case "fillfield":
                    const { fieldId, value } = data;
                    if (fieldId && value) {
                        const field = document.getElementById(fieldId);
                        if (field) {
                            field.value = value;
                            speak(`Filled ${fieldId} with ${value}`);
                        } else {
                            speak(`Field ${fieldId} not found.`);
                        }
                    }
                    break;

                case "searchcustomer":
                case "searchproduct":
                case "searchsales":
                    const filters = encodeURIComponent(JSON.stringify(data.filters || []));
                    window.location.href = `/Search/GlobalSearch?intent=${data.intent}&filters=${filters}`;
                    break;

                default:
                    console.warn("⚠️ Unknown intent:", data.intent);
                    speak("Sorry, I didn't understand the command.");
            }
        })
        .catch(err => {
            if (chatStatus) chatStatus.textContent = "❌ Failed to process query.";
            console.error("❌ Error:", err);
            speak("Failed to process your command.");
            setTimeout(() => chatStatus.textContent = "", 3000);
        });
}

