document.addEventListener("DOMContentLoaded", function () {
  lucide.createIcons();
  setupUserSearch();
});

function setupUserSearch() {
  const recipientInput = document.getElementById("recipient");
  const recipientIdInput = document.getElementById("recipientId");
  const searchResults = document.getElementById("user-search-results");
  let searchTimeout;

  recipientInput.addEventListener("input", function () {
    const query = this.value.trim();

    // Clear previous timeout
    clearTimeout(searchTimeout);

    if (query.length < 2) {
      hideSearchResults();
      recipientIdInput.value = "";
      return;
    }

    // Debounce search
    searchTimeout = setTimeout(() => {
      searchUsers(query);
    }, 300);
  });

  recipientInput.addEventListener("blur", function () {
    // Hide search results after a delay to allow clicking
    setTimeout(() => {
      hideSearchResults();
    }, 200);
  });

  recipientInput.addEventListener("focus", function () {
    const query = this.value.trim();
    if (query.length >= 2) {
      searchUsers(query);
    }
  });

  function searchUsers(query) {
    fetch(`/Message/SearchUsers?query=${encodeURIComponent(query)}`)
      .then((response) => response.json())
      .then((data) => {
        if (data.success && data.data) {
          displaySearchResults(data.data);
        } else {
          hideSearchResults();
        }
      })      .catch((error) => {
        console.error("Kullanıcı arama hatası:", error);
        hideSearchResults();
      });
  }

  function displaySearchResults(users) {
    if (users.length === 0) {      searchResults.innerHTML =
        '<div class="p-3 text-gray-400 text-sm">Kullanıcı bulunamadı</div>';
    } else {
      searchResults.innerHTML = users
        .map(
          (user) => `
                    <div class="search-result-item p-3 hover:bg-white/10 cursor-pointer transition-colors duration-200 border-b border-white/10 last:border-b-0" 
                         data-user-id="${user.id}" 
                         data-username="${user.username}"
                         data-display-name="${user.displayName}">
                        <div class="flex items-center space-x-3">
                            <div class="w-8 h-8 bg-gradient-to-br from-accent/20 to-accent/10 rounded-full flex items-center justify-center flex-shrink-0">
                                ${
                                  user.profileImageUrl
                                    ? `<img src="${user.profileImageUrl}" alt="${user.displayName}" class="w-8 h-8 rounded-full object-cover">`
                                    : `<i data-lucide="user" class="w-4 h-4 text-accent"></i>`
                                }
                            </div>
                            <div class="flex-1 min-w-0">
                                <p class="text-white font-medium text-sm truncate">${
                                  user.displayName
                                }</p>
                                <p class="text-gray-400 text-xs truncate">@@${
                                  user.username
                                }</p>
                            </div>
                        </div>
                    </div>
                `
        )
        .join("");

      // Add click listeners to search results
      searchResults.querySelectorAll(".search-result-item").forEach((item) => {
        item.addEventListener("click", function () {
          const userId = this.dataset.userId;
          const username = this.dataset.username;
          const displayName = this.dataset.displayName;
          recipientInput.value = `${displayName} (@@${username})`;
          recipientIdInput.value = userId;
          document.getElementById("recipientUsername").value = username;
          hideSearchResults();
        });
      });

      // Reinitialize lucide icons
      lucide.createIcons();
    }

    searchResults.classList.remove("hidden");
  }

  function hideSearchResults() {
    searchResults.classList.add("hidden");
  }
}

function showComposeModal() {
  const modal = document.getElementById("compose-modal");
  const content = document.getElementById("compose-content");

  modal.classList.remove("hidden");
  setTimeout(() => {
    content.classList.remove("scale-95", "opacity-0");
    content.classList.add("scale-100", "opacity-100");
  }, 10);
}
function hideComposeModal() {
  const modal = document.getElementById("compose-modal");
  const content = document.getElementById("compose-content");

  content.classList.add("scale-95", "opacity-0");
  content.classList.remove("scale-100", "opacity-100");
  setTimeout(() => {
    modal.classList.add("hidden");
    document.getElementById("compose-form").reset();
    document.getElementById("recipientId").value = "";
    document.getElementById("recipientUsername").value = "";
    document.getElementById("user-search-results").classList.add("hidden");
    clearValidationErrors();
  }, 300);
} // Handle form submission
document
  .getElementById("compose-form")
  .addEventListener("submit", function (e) {
    e.preventDefault();

    // Clear previous errors
    clearValidationErrors();
    const recipientId = document.getElementById("recipientId").value;
    const recipientUsername =
      document.getElementById("recipientUsername").value;
    const subject = document.getElementById("subject").value;
    const content = document.getElementById("message-content").value;

    let hasErrors = false; 
    
    // Validate recipient
    if (!recipientId || !recipientUsername) {
      showValidationError(
        "recipient-error",
        "Lütfen arama sonuçlarından bir alıcı seçin."
      );
      hasErrors = true;
    }

    // Validate subject
    if (!subject.trim()) {
      showValidationError("subject-error", "Lütfen bir konu girin.");
      hasErrors = true;
    }

    // Validate content
    if (!content.trim()) {
      showValidationError("content-error", "Lütfen bir mesaj girin.");
      hasErrors = true;
    }    if (hasErrors) {
      return;
    } 
    
    // Submit the form via AJAX
    const formData = new FormData();
    formData.append("ReceiverId", recipientId);
    formData.append("RecipientUsername", recipientUsername);
    formData.append("Subject", subject.trim());
    formData.append("Content", content.trim());
    formData.append(
      "__RequestVerificationToken",
      document.querySelector('input[name="__RequestVerificationToken"]').value
    );

    fetch("/Message/Send", {
      method: "POST",
      body: formData,
    })
      .then((response) => response.json())      .then((data) => {
        if (data.success) {
          alert(data.message || "Mesaj başarıyla gönderildi!");
          hideComposeModal();
          // Reload the page to show the new message
          location.reload();
        } else {
          alert(data.error || "Mesaj gönderilemedi. Lütfen tekrar deneyin.");
        }
      })
      .catch((error) => {
        console.error("Mesaj gönderme hatası:", error);
        alert("Mesaj gönderilirken bir hata oluştu.");
      });
  });

function showValidationError(errorId, message) {
  const errorElement = document.getElementById(errorId);
  if (errorElement) {
    errorElement.textContent = message;
    errorElement.classList.remove("hidden");
  }
}

function clearValidationErrors() {
  const errorElements = document.querySelectorAll('[id$="-error"]');
  errorElements.forEach((element) => {
    element.classList.add("hidden");
  });
} // Handle conversation item clicks
// Note: Now using HTML links directly, but keeping this for debugging
document.addEventListener("DOMContentLoaded", function () {
  console.log("DOM yüklendi, konuşma linkleri hazır");

  const conversationLinks = document.querySelectorAll(
    'a[href*="/Message/Conversation/"]'
  );
  console.log("Bulunan konuşma linkleri:", conversationLinks.length);
});
