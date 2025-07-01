// Initialize Lucide icons
document.addEventListener("DOMContentLoaded", function () {
  lucide.createIcons();
  
  // Make functions globally available immediately after DOM is loaded
  window.playTrackFromLiked = playTrackFromLiked;
  window.shufflePlaylist = shufflePlaylist;
  window.showAddToPlaylistModal = showAddToPlaylistModal;
  window.shareTrack = shareTrack;
  window.reportTrack = reportTrack;
  window.toggleDropdown = toggleDropdown;
});

let currentTrackId = null;

// Play track function
function playTrack(trackId) {
  // Call the global playTrack function from audioPlayer.js
  if (typeof window.playTrack === 'function') {
    // Note: You would need track details for full implementation
    console.log("Playing track from audioPlayer.js:", trackId);
    // TODO: Get track details and call window.playTrack(trackId, title, artist, audioUrl, coverUrl);
  } else {
    console.warn('Global playTrack function not found - check if audioPlayer.js is loaded');
  }
}

// Play track with full details function
function playTrackFromLiked(trackId, title, artist, audioUrl, coverUrl) {
  console.log('Playing track from liked tracks:', { trackId, title, artist, audioUrl, coverUrl });
  
  // Call the global playTrack function from audioPlayer.js
  if (typeof window.playTrack === 'function') {
    window.playTrack(trackId, title, artist, audioUrl, coverUrl);
  } else {
    console.error('Global playTrack function not found - check if audioPlayer.js is loaded');
    showNotification('Audio player not available', 'error');
  }
}

// Shuffle playlist
function shufflePlaylist() {
  console.log("Shuffling liked tracks playlist");
  
  // Get all track cards and extract track data
  const trackCards = document.querySelectorAll('[onclick*="playTrackFromLiked"]');
  if (trackCards.length === 0) {
    showNotification('No tracks available to shuffle', 'info');
    return;
  }
  
  // Get random track
  const randomIndex = Math.floor(Math.random() * trackCards.length);
  const randomTrack = trackCards[randomIndex];
  
  // Extract onclick parameters
  const onclickAttr = randomTrack.getAttribute('onclick');
  const match = onclickAttr.match(/playTrackFromLiked\('([^']+)',\s*'([^']+)',\s*'([^']+)',\s*'([^']+)',\s*'([^']+)'\)/);
  
  if (match) {
    const [, trackId, title, artist, audioUrl, coverUrl] = match;
    playTrackFromLiked(trackId, title, artist, audioUrl, coverUrl);
    showNotification(`Playing shuffled: ${title}`, 'success');
  } else {
    showNotification('Error shuffling tracks', 'error');
  }
}

// Toggle like function
function toggleLike(trackId) {
  fetch(`/Likes/Toggle/${trackId}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      RequestVerificationToken: $(
        'input[name="__RequestVerificationToken"]'
      ).val(),
    },
  })
    .then((response) => response.json())
    .then((data) => {
      if (data.success) {
        // Remove the track from the page since it's unliked
        location.reload();
      }
    })
    .catch((error) => {
      console.error("Error toggling like:", error);
    });
}

// Add to playlist functions
function showAddToPlaylistModal(trackId) {
  currentTrackId = trackId;
  loadUserPlaylists();
  document.getElementById("addToPlaylistModal").classList.remove("hidden");
  document.getElementById("addToPlaylistModal").classList.add("flex");
}

function closeAddToPlaylistModal() {
  document.getElementById("addToPlaylistModal").classList.add("hidden");
  document.getElementById("addToPlaylistModal").classList.remove("flex");
  currentTrackId = null;
}

function loadUserPlaylists() {
  fetch("/Playlist/GetUserPlaylists")
    .then((response) => response.json())
    .then((playlists) => {
      const playlistList = document.getElementById("playlistList");
      playlistList.innerHTML = "";

      playlists.forEach((playlist) => {
        const playlistItem = document.createElement("button");
        playlistItem.className =
          "w-full text-left p-3 rounded-lg bg-white/5 hover:bg-white/10 text-white transition-colors duration-200";
        playlistItem.innerHTML = `
                        <div class="flex items-center space-x-3">
                            <div class="w-10 h-10 bg-accent/20 rounded-lg flex items-center justify-center">
                                <i data-lucide="list-music" class="w-5 h-5 text-accent"></i>
                            </div>
                            <div>
                                <div class="font-medium">${playlist.name}</div>
                                <div class="text-sm text-gray-400">${playlist.trackCount} tracks</div>
                            </div>
                        </div>
                    `;
        playlistItem.onclick = () => addToPlaylist(playlist.id);
        playlistList.appendChild(playlistItem);
      });

      // Re-initialize icons
      lucide.createIcons();
    });
}

function addToPlaylist(playlistId) {
  fetch(`/Playlist/AddTrack/${playlistId}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      RequestVerificationToken: $(
        'input[name="__RequestVerificationToken"]'
      ).val(),
    },
    body: JSON.stringify({ trackId: currentTrackId }),
  })
    .then((response) => response.json())
    .then((data) => {
      if (data.success) {
        closeAddToPlaylistModal();
        // Show success message
        showNotification("Track added to playlist successfully!", "success");
      } else {
        showNotification("Failed to add track to playlist.", "error");
      }
    })
    .catch((error) => {
      console.error("Error adding to playlist:", error);
      showNotification("An error occurred.", "error");
    });
}

function createNewPlaylist() {
  closeAddToPlaylistModal();
  // Redirect to create playlist page with track preselected
  window.location.href = `/Playlist/Create?trackId=${currentTrackId}`;
}

// Share track function
function shareTrack(trackId, trackTitle) {
  const url = `${window.location.origin}/Track/Details/${trackId}`;

  if (navigator.share) {
    navigator.share({
      title: trackTitle,
      text: `Check out this track: ${trackTitle}`,
      url: url,
    });
  } else {
    // Fallback to clipboard
    navigator.clipboard.writeText(url).then(() => {
      showNotification("Track URL copied to clipboard!", "success");
    });
  }
}

// Report track function
function reportTrack(trackId) {
  if (confirm("Are you sure you want to report this track?")) {
    fetch(`/Track/Report/${trackId}`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        RequestVerificationToken: $(
          'input[name="__RequestVerificationToken"]'
        ).val(),
      },
    })
      .then((response) => response.json())
      .then((data) => {
        if (data.success) {
          showNotification("Track reported successfully.", "success");
        } else {
          showNotification("Failed to report track.", "error");
        }
      })
      .catch((error) => {
        console.error("Error reporting track:", error);
        showNotification("An error occurred.", "error");
      });
  }
}

// Dropdown toggle function
function toggleDropdown(dropdownId) {
  const dropdown = document.getElementById(dropdownId);
  const isHidden = dropdown.classList.contains("hidden");

  // Close all other dropdowns
  document.querySelectorAll('[id^="dropdown-"]').forEach((d) => {
    d.classList.add("hidden");
  });

  if (isHidden) {
    dropdown.classList.remove("hidden");
  }
}

// Close dropdowns when clicking outside
document.addEventListener("click", function (event) {
  if (
    !event.target.closest('[onclick*="toggleDropdown"]') &&
    !event.target.closest('[id^="dropdown-"]')
  ) {
    document.querySelectorAll('[id^="dropdown-"]').forEach((dropdown) => {
      dropdown.classList.add("hidden");
    });
  }
});

// Notification function
function showNotification(message, type = "info") {
  // Create notification element
  const notification = document.createElement("div");
  notification.className = `fixed top-4 right-4 p-4 rounded-lg shadow-lg z-50 ${
    type === "success"
      ? "bg-green-600"
      : type === "error"
      ? "bg-red-600"
      : "bg-blue-600"
  } text-white`;
  notification.textContent = message;

  document.body.appendChild(notification);  // Remove after 3 seconds
  setTimeout(() => {
    notification.remove();
  }, 3000);
}
