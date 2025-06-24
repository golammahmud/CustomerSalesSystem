// Voice Assistant JavaScript Module

// ================================
// 🔁 Language Persistence
// ================================

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

// ================================
// 🔊 Text-to-Speech
// ================================

window.speak = function (text, lang = null) {
    if (!('speechSynthesis' in window)) {
        alert("Sorry, your browser doesn't support speech synthesis.");
        return;
    }

    // Step 1: Get preferred language from dropdown or override
    const selectedLang = lang || window.getVoiceLang();
    const availableVoices = speechSynthesis.getVoices();

    // Step 2: Preferred voice names to try in order
    const preferredVoiceNames = [
        "Google US English",
        "Google UK English Female",
        "Samantha",
        // Add more preferred voice names here if needed
        "Zira"
    ];

    // Step 3: Try to find a preferred voice matching the selected language first
    let voiceToUse = null;

    // Find preferred voice by name AND language match
    voiceToUse = availableVoices.find(v =>
        preferredVoiceNames.includes(v.name) &&
        v.lang.toLowerCase().startsWith(selectedLang.toLowerCase().split('-')[0])
    );

    // If no preferred voice with lang match, try any preferred voice ignoring language
    if (!voiceToUse) {
        voiceToUse = availableVoices.find(v => preferredVoiceNames.includes(v.name));
    }

    // If still no preferred voice, try to find any voice that matches selected language
    if (!voiceToUse) {
        voiceToUse = availableVoices.find(v => v.lang === selectedLang);
    }

    // Finally fallback to English US or first available voice
    if (!voiceToUse) {
        voiceToUse = availableVoices.find(v => v.lang === 'en-US') || availableVoices[0];
    }

    // Step 4: Clean up emoji and other unsupported symbols
    const cleanText = (text || "").replace(/[\u{1F600}-\u{1F64F}]/gu, '').trim(); // remove emoji

    const msg = new SpeechSynthesisUtterance(cleanText || "Sorry, nothing to speak.");
    msg.lang = voiceToUse.lang || selectedLang;
    msg.voice = voiceToUse;

    speechSynthesis.cancel(); // cancel any existing speech
    speechSynthesis.speak(msg);

    console.log(`🗣️ Speaking in [${msg.lang}]:`, cleanText);
};


window.speechSynthesis.onvoiceschanged = () => {
    console.log("✅ Voices loaded:", speechSynthesis.getVoices());
};

// ================================
// 🎙️ Voice Search Initialization
// ================================

let recognition;
let isListening = false;
const language = window.getVoiceLang();

/**
 * Initializes voice recognition for search/chat input
 */
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
    recognition.continuous = true;
    recognition.interimResults = false;

    recognition.onstart = () => {
        isListening = true;
        console.log("🎤 Listening started...");
        toggleVoiceUI(true);
    };

    recognition.onresult = (event) => {
        let transcript = '';
        for (let i = event.resultIndex; i < event.results.length; i++) {
            transcript += event.results[i][0].transcript;
        }
        transcript = transcript.trim();
        console.log("Result:", transcript);

        if (inputElement) inputElement.value = transcript;
        if (typeof onResult === "function") onResult(transcript);

        handleVoiceTranscript(transcript);
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
    };
};

/**
 * Starts voice recognition
 */
window.startVoiceSearch = () => {
    if (recognition && !isListening) {
        const namePart = window.userName ? ' ' + window.userName : ' you';
        window.speak(`Sensa here. How can I assist${namePart}?`, language);
        recognition.start();
    }
};

/**
 * Stops voice recognition
 */
window.stopVoiceSearch = () => {
    if (recognition && isListening) {
        recognition.stop();
    }
};

/**
 * Enables/disables mic and stop button UI
 */
function toggleVoiceUI(listening) {
    const micBtn = document.getElementById("voiceBtn");
    const stopBtn = document.getElementById("stopBtn");
    if (micBtn) micBtn.disabled = listening;
    if (stopBtn) stopBtn.disabled = !listening;
}

// ================================
// 🔁 Refresh Button & Autofocus
// ================================

document.getElementById("refreshBtn")?.addEventListener("click", () => {
    const input = document.getElementById("searchQuery");
    if (input) input.value = "";
    location.reload();
});

window.onload = () => {
    document.getElementById("searchQuery")?.focus();
};

// ================================
// 🔍 Form Submit Event
// ================================

$(document).ready(function () {
    $('#globalSearchForm').on('submit', function (e) {
        e.preventDefault(); // Prevent default form submission

        let query = $('#searchQuery').val();
        if (!query || query.trim() === "") return;

        handleVoiceTranscript(query); // Just reuse your existing AI logic
    });
});



// ================================
// 🧠 Handle Transcription Logic
// ================================

/**
 * Processes the transcribed voice input
 */
function handleVoiceTranscript(transcript) {
    const chatStatus = document.getElementById("chatStatus");
    if (chatStatus) chatStatus.textContent = "🤖 Sensa is Thinking...";

    fetch("/Search/GlobalSearch?handler=VoiceSearch", {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: `userQuery=${encodeURIComponent(transcript)}&pagePath=${encodeURIComponent(currentPageContext.path)}`
    })
        .then(res => {
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            return res.json();
        })
        .then(data => {
            if (chatStatus) chatStatus.textContent = "";
            console.log("Intent received hfghfg:", data.intent + "entities" + data.entities);

            switch (data.intent?.toLowerCase()) {

                case "chat":
                    displayChatResponse(transcript, data.response);
                    break;
                case "navigate":
                    const targetUrl = data.target.includes("{id}")
                        ? data.target.replace("{id}", data.id || "0")
                        : data.target;
                    window.location.href = targetUrl;
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

                case "setusername":
                    
                    if (Array.isArray(data.entities) && data.entities.length > 0) {
                        const nameEntity = data.entities.find(e => e.field?.toLowerCase() === "name");
                        if (nameEntity?.value) {
                            const userName = nameEntity.value;
                            localStorage.setItem("userName", userName);
                            window.userName = userName;
                            speak(data.message || `Nice to meet you, ${userName}`);
                        } else {
                            speak("I couldn't find your name in the response.");
                        }
                    } else {
                        speak(data.message || "Sorry, I couldn't catch your name.");
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

/**
 * Display chat conversation modal
 */
function displayChatResponse(userText, responseText) {
    const chatModal = document.getElementById("chatModal");
    const chatBox = document.getElementById("chatConversation");

    if (chatModal && chatBox) {
        chatModal.style.display = "block";
        chatBox.innerHTML += `<div class="user"><strong><span style="font-size: 1.2em;">🧑‍💻</span> You:</strong> ${userText}</div>`;
        chatBox.innerHTML += `<div class="ai"><strong><span style="font-size: 1.2em;">👩‍🎤</span> Sensa:</strong> ${responseText || "Sorry, I didn't catch that."}</div>`;
        chatBox.scrollTop = chatBox.scrollHeight;
    }

    speak(responseText);
}

