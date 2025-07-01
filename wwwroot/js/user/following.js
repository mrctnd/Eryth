document.addEventListener("DOMContentLoaded", function () {
  // Initialize Lucide icons
  if (typeof lucide !== "undefined") {
    lucide.createIcons();
  }
});

// Toggle follow function
function toggleFollow(username, button) {
  fetch("/User/ToggleFollow", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      RequestVerificationToken: document.querySelector(
        'input[name="__RequestVerificationToken"]'
      )?.value,
    },
    body: JSON.stringify({ username: username }),
  })
    .then((response) => response.json())
    .then((data) => {
      if (data.success) {
        if (data.isFollowing) {
          button.className =
            "bg-white/10 text-white hover:bg-red-500/20 hover:text-red-400 px-4 py-2 rounded-lg font-medium transition-all duration-200 text-sm border border-white/20 hover:border-red-400/50";
          button.innerHTML =
            '<i data-lucide="user-check" class="w-4 h-4 inline mr-2"></i>Following';
        } else {
          button.className =
            "bg-gradient-to-r from-accent to-accent-dark text-white hover:from-accent-dark hover:to-accent px-4 py-2 rounded-lg font-medium transition-all duration-200 text-sm";
          button.innerHTML =
            '<i data-lucide="user-plus" class="w-4 h-4 inline mr-2"></i>Follow';
        }

        if (typeof lucide !== "undefined") {
          lucide.createIcons();
        }
      } else {
        alert(data.message || "An error occurred");
      }
    })
    .catch((error) => {
      console.error("Error:", error);
      alert("An error occurred while updating follow status");
    });
}
