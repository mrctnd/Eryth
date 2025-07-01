document.addEventListener("DOMContentLoaded", function () {
  // Initialize Lucide icons
  if (typeof lucide !== "undefined") {
    lucide.createIcons();
  }
});

// Play playlist function
function playPlaylist(playlistId) {
  // Call the global playTrack function from audioPlayer.js for playlist playback
  if (typeof window.playTrack === 'function') {
    console.log("Starting playlist playback via audioPlayer.js:", playlistId);
    // TODO: Implement playlist queue management with audioPlayer.js
  } else {
    console.warn('Global playTrack function not found - check if audioPlayer.js is loaded');
    console.log("Playing playlist:", playlistId);
  }
}

// Share playlist function
function sharePlaylist(playlistId, playlistName) {
  const url = window.location.origin + "/Playlist/Details/" + playlistId;

  if (navigator.share) {
    navigator.share({
      title: playlistName,
      text: "Check out this playlist on Eryth",
      url: url,
    });
  } else {
    // Fallback to copying to clipboard
    navigator.clipboard
      .writeText(url)
      .then(() => {
        alert("Link copied to clipboard!");
      })
      .catch(() => {
        prompt("Copy this link:", url);
      });
  }
}

// Duplicate playlist function
function duplicatePlaylist(playlistId) {
  if (confirm("Are you sure you want to duplicate this playlist?")) {
    fetch("/Playlist/Duplicate", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        RequestVerificationToken:
          document.querySelector('input[name="__RequestVerificationToken"]')
            ?.value || "",
      },
      body: JSON.stringify({ id: playlistId }),
    })
      .then((response) => response.json())
      .then((data) => {
        if (data.success) {
          location.reload();
        } else {
          alert(data.message || "Error duplicating playlist");
        }
      })
      .catch((error) => {
        console.error("Error:", error);
        alert("Error duplicating playlist");
      });
  }
}

// Delete playlist function
function deletePlaylist(playlistId, playlistName) {
  if (
    confirm(
      `Are you sure you want to delete "${playlistName}"? This action cannot be undone.`
    )
  ) {
    fetch("/Playlist/Delete", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        RequestVerificationToken:
          document.querySelector('input[name="__RequestVerificationToken"]')
            ?.value || "",
      },
      body: JSON.stringify({ id: playlistId }),
    })
      .then((response) => response.json())
      .then((data) => {
        if (data.success) {
          location.reload();
        } else {
          alert(data.message || "Error deleting playlist");
        }
      })
      .catch((error) => {
        console.error("Error:", error);
        alert("Error deleting playlist");
      });
  }
}

// Dropdown toggle function
function toggleDropdown(menuId) {
  // Close all other dropdowns first
  document.querySelectorAll('[id^="playlist-menu-"]').forEach((dropdown) => {
    if (dropdown.id !== menuId) {
      dropdown.classList.add("hidden");
    }
  });

  // Toggle the clicked dropdown
  const menu = document.getElementById(menuId);
  if (menu) {
    menu.classList.toggle("hidden");

    // Focus management for accessibility
    if (!menu.classList.contains("hidden")) {
      const firstLink = menu.querySelector("a, button");
      if (firstLink) {
        firstLink.focus();
      }
    }
  }
}

// Close dropdowns when clicking outside
document.addEventListener("click", function (event) {
  const dropdowns = document.querySelectorAll('[id^="playlist-menu-"]');
  dropdowns.forEach((dropdown) => {
    const button = document.querySelector(`[onclick*="${dropdown.id}"]`);
    if (
      !dropdown.contains(event.target) &&
      !button?.contains(event.target) &&
      !event.target.closest('[onclick*="toggleDropdown"]')
    ) {
      dropdown.classList.add("hidden");
    }
  });
});

// Close dropdowns with Escape key
document.addEventListener("keydown", function (event) {
  if (event.key === "Escape") {
    document.querySelectorAll('[id^="playlist-menu-"]').forEach((dropdown) => {
      dropdown.classList.add("hidden");
    });
  }
});
