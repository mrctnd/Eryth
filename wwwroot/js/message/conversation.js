document.addEventListener("DOMContentLoaded", function () {
  lucide.createIcons();

  // Auto-scroll to bottom of messages
  const messagesContainer = document.getElementById("messages-container");
  if (messagesContainer) {
    messagesContainer.scrollTop = messagesContainer.scrollHeight;
  }

  // Setup reply form
  setupReplyForm();

  // Handle escape key to close
  document.addEventListener("keydown", function (e) {
    if (e.key === "Escape") {
      closeConversation();
    }
  });
});

function setupReplyForm() {
  const form = document.getElementById("reply-form");
  const contentTextarea = document.getElementById("reply-content");
  const errorDiv = document.getElementById("reply-error");

  if (!form || !contentTextarea || !errorDiv) return;

  form.addEventListener("submit", function (e) {
    e.preventDefault();

    const content = contentTextarea.value.trim();
    const conversationId = document.getElementById("conversationId").value;

    // Clear previous error
    errorDiv.classList.add("hidden");    if (!content) {
      errorDiv.textContent = "Lütfen bir mesaj girin.";
      errorDiv.classList.remove("hidden");
      return;
    }

    // Send reply
    const formData = new FormData();
    formData.append("conversationId", conversationId);
    formData.append("content", content);
    formData.append(
      "__RequestVerificationToken",
      document.querySelector('input[name="__RequestVerificationToken"]').value
    );

    fetch("/Message/SendReply", {
      method: "POST",
      body: formData,
    })
      .then((response) => response.json())
      .then((data) => {
        if (data.success) {
          // Add the new message to the conversation
          addMessageToConversation(data.reply);

          // Clear the form
          contentTextarea.value = "";
          contentTextarea.style.height = "auto";        } else {
          errorDiv.textContent = data.error || "Yanıt gönderilemedi.";
          errorDiv.classList.remove("hidden");
        }
      })
      .catch((error) => {
        console.error("Yanıt gönderme hatası:", error);
        errorDiv.textContent = "Yanıt gönderilirken bir hata oluştu.";
        errorDiv.classList.remove("hidden");
      });
  });

  // Auto-resize textarea
  contentTextarea.addEventListener("input", function () {
    this.style.height = "auto";
    this.style.height = Math.min(this.scrollHeight, 120) + "px";
  });
}

function addMessageToConversation(message) {
  const messagesContainer = document.getElementById("messages-container");
  if (!messagesContainer) return;

  const messageHtml = `
            <div class="flex justify-end">
                <div class="max-w-xs lg:max-w-md">
                    <div class="bg-gradient-to-r from-accent to-accent-dark text-black rounded-2xl px-4 py-3 shadow-lg border border-accent/30">
                        <p class="text-sm leading-relaxed">${message.content}</p>                        <div class="flex items-center justify-between mt-2 pt-2 border-t border-black/20">
                            <span class="text-black/70 text-xs">Şimdi</span>
                            <div class="flex items-center space-x-1">
                                <i data-lucide="check" class="w-3 h-3 text-black/70"></i>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;

  messagesContainer.insertAdjacentHTML("beforeend", messageHtml);

  // Re-initialize lucide icons for the new message
  lucide.createIcons();

  // Scroll to bottom
  messagesContainer.scrollTop = messagesContainer.scrollHeight;
}

function closeConversation() {
  // Go back to messages page
  window.location.href = "/Message";
}
