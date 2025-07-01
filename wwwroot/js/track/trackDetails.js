document.addEventListener("DOMContentLoaded", function () {
  // Initialize Lucide icons
  if (typeof lucide !== "undefined") {
    lucide.createIcons();
  }

  // Debug: Check if trackData is available
  console.log("Track data:", window.trackData);

  // Load comments and artist info
  loadComments();
  loadArtistInfo();
  loadRelatedTracks();
});

// Edit Modal Functions
function openEditModal() {
  document.getElementById("edit-modal").classList.remove("hidden");
  document.getElementById("edit-modal").classList.add("flex");
}

function closeEditModal(event) {
  if (
    !event ||
    event.target === event.currentTarget ||
    event.type === "click"
  ) {
    document.getElementById("edit-modal").classList.add("hidden");
    document.getElementById("edit-modal").classList.remove("flex");
  }
}

// Handle edit form submission
document.getElementById("edit-form")?.addEventListener("submit", function (e) {
  e.preventDefault();

  const formData = {
    id: document.getElementById("track-id").value,
    title: document.getElementById("edit-title").value,
    description: document.getElementById("edit-description").value,
    genre: document.getElementById("edit-genre").value,
    subGenre: document.getElementById("edit-subgenre").value,
    composer: document.getElementById("edit-composer").value,
    producer: document.getElementById("edit-producer").value,
    lyricist: document.getElementById("edit-lyricist").value,
    copyright: document.getElementById("edit-copyright").value,
    isExplicit: document.getElementById("edit-explicit").checked,
  };

  fetch("/Track/UpdateTrack", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      RequestVerificationToken:
        document.querySelector('input[name="__RequestVerificationToken"]')
          ?.value || "",
    },
    body: JSON.stringify(formData),
  })
    .then((response) => response.json())
    .then((data) => {
      if (data.success) {
        showToast("Parça başarıyla güncellendi!");
        closeEditModal();
        location.reload(); // Refresh to show updated data
      } else {
        showToast(data.message || "Parça güncellenirken hata oluştu");
      }
    })
    .catch((error) => {
      console.error("Error:", error);
      showToast("Parça güncellenirken hata oluştu");
    });
});

// Load artist information
function loadArtistInfo() {
  console.log("loadArtistInfo called");
  console.log("window.trackData:", window.trackData);

  if (!window.trackData) {
    console.error("window.trackData is not available");
    return;
  }

  if (!window.trackData.artistId) {
    console.error("Artist ID not available:", window.trackData.artistId);
    return;
  }

  // Check if artistId is empty GUID
  if (window.trackData.artistId === '00000000-0000-0000-0000-000000000000') {
    console.error("Artist ID is empty GUID:", window.trackData.artistId);
    return;
  }

  const url = `/User/GetArtistInfo/${window.trackData.artistId}`;
  console.log("Loading artist info from:", url);

  fetch(url)
    .then((response) => {
      console.log("Artist info response status:", response.status);
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return response.json();
    })
    .then((data) => {
      console.log("Artist info data:", data);
      if (data.success) {
        const artistInfo = document.getElementById("artist-info");
        const artistStats = document.getElementById("artist-stats");

        // Update artist avatar if available
        if (data.profileImageUrl) {
          const avatarContainer = artistInfo.querySelector(".w-16.h-16");
          if (avatarContainer) {
            console.log("Updating avatar with:", data.profileImageUrl);
            avatarContainer.innerHTML = `<img src="/${data.profileImageUrl}" alt="${data.displayName}" class="w-16 h-16 rounded-full object-cover">`;
          }
        }

        // Update artist stats
        if (artistStats) {
          console.log("Updating artist stats");
          artistStats.innerHTML = `                                <div class="flex items-center justify-between text-sm">
                                    <span class="text-gray-400">Toplam Parça</span>
                                    <span class="text-white font-medium">${
                                      data.trackCount || 0
                                    }</span>
                                </div>
                                <div class="flex items-center justify-between text-sm">
                                    <span class="text-gray-400">Takipçi</span>
                                    <span class="text-white font-medium">${
                                      data.followerCount || 0
                                    }</span>
                                </div>
                            `;
        }
      } else {
        console.error("Artist info request failed:", data.message);
      }
    })
    .catch((error) => {
      console.error("Error loading artist info:", error);
    });
}

// Load comments
function loadComments() {
  if (!window.trackData || !window.trackData.id) {
    console.error("Track ID not available");
    return;
  }
  fetch(`/Comment/GetTrackComments/${window.trackData.id}`)
    .then((response) => response.json())
    .then((data) => {
      console.log("Comments data received:", data);
      
      const container = document.getElementById("comments-container");
      if (data.success && data.comments && data.comments.length > 0) {
        // Debug: Log the first comment to see profile image data
        console.log("First comment profile image URL:", data.comments[0]?.userProfileImageUrl);
        console.log("First comment user:", data.comments[0]?.userDisplayName);
        
        container.innerHTML = data.comments
          .map(
            (comment) => `
                            <div class="border-b border-gray-700/50 pb-4 mb-4 last:border-b-0 last:pb-0 last:mb-0">
                                <div class="flex items-start space-x-3">
                                    ${
                                      comment.userProfileImageUrl
                                        ? `<img src="${comment.userProfileImageUrl.startsWith('/') ? comment.userProfileImageUrl : '/' + comment.userProfileImageUrl}" alt="${comment.userDisplayName}" class="w-10 h-10 rounded-full object-cover">`
                                        : `<div class="w-10 h-10 rounded-full bg-green-600/20 flex items-center justify-center">
                                            <i data-lucide="user" class="w-5 h-5 text-white"></i>
                                           </div>`
                                    }
                                    <div class="flex-1">
                                        <div class="flex items-center space-x-2 mb-1">
                                            <span class="text-white font-medium">${
                                              comment.userDisplayName
                                            }</span>
                                            <span class="text-gray-400 text-sm">${
                                              comment.relativeCreatedDate
                                            }</span>
                                        </div>
                                        <p class="text-gray-300">${
                                          comment.content
                                        }</p>
                                    </div>
                                </div>
                            </div>
                        `
          )
          .join("");
      } else {
        container.innerHTML = `
                            <div class="text-center py-8 text-gray-400">
                                <i data-lucide="message-circle" class="w-12 h-12 mx-auto mb-3 opacity-50"></i>
                                <p>Henüz yorum yok. İlk yorumu sen yap!</p>
                            </div>
                        `;
      }

      // Re-initialize Lucide icons for new content
      if (typeof lucide !== "undefined") {
        lucide.createIcons();
      }
    })
    .catch((error) => {      console.error("Error loading comments:", error);
      document.getElementById("comments-container").innerHTML = `
                        <div class="text-center py-8 text-gray-400">
                            <i data-lucide="message-circle" class="w-12 h-12 mx-auto mb-3 opacity-50"></i>
                            <p>Yorumlar yüklenirken hata oluştu</p>
                        </div>
                    `;
    });
}

// Load related tracks
function loadRelatedTracks() {
  console.log("loadRelatedTracks called");
  console.log("window.trackData:", window.trackData);

  if (!window.trackData) {
    console.error("window.trackData is not available");
    return;
  }

  if (!window.trackData.id) {
    console.error("Track ID not available:", window.trackData.id);
    return;
  }

  const url = `/Track/GetRelatedTracks/${window.trackData.id}`;
  console.log("Loading related tracks from:", url);

  fetch(url)
    .then((response) => {
      console.log("Related tracks response status:", response.status);
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return response.json();
    })
    .then((data) => {
      console.log("Related tracks data:", data);
      const container = document.getElementById("related-tracks");
      if (data.success && data.tracks && data.tracks.length > 0) {
        console.log("Found", data.tracks.length, "related tracks");
        container.innerHTML = data.tracks
          .map(
            (track) => `
                            <div class="flex items-center space-x-3 p-3 rounded-lg hover:bg-black/30 transition-colors duration-200 cursor-pointer"
                                 onclick="location.href='/Track/Details/${
                                   track.id
                                 }'">
                                <div class="w-12 h-12 rounded-lg overflow-hidden">                                    ${
                                  track.coverImageUrl
                                    ? `<img src="/${track.coverImageUrl}" alt="${track.title}" class="w-full h-full object-cover">`
                                    : `<div class="w-full h-full bg-green-600/20 flex items-center justify-center">
                                            <i data-lucide="music" class="w-6 h-6 text-green-400"></i>
                                        </div>`
                                }
                                </div>
                                <div class="flex-1 min-w-0">
                                    <p class="text-white font-medium truncate">${
                                      track.title
                                    }</p>
                                    <p class="text-gray-400 text-sm">${
                                      track.formattedDuration
                                    }</p>
                                </div>
                            </div>
                        `
          )
          .join("");
      } else {
        console.log("No related tracks found");
        container.innerHTML = `
                            <div class="text-center py-8 text-gray-400">
                                <i data-lucide="music" class="w-12 h-12 mx-auto mb-3 opacity-50"></i>
                                <p class="text-sm">Bu sanatçının başka parçası yok</p>
                            </div>
                        `;
      }

      // Re-initialize Lucide icons
      if (typeof lucide !== "undefined") {
        lucide.createIcons();
      }
    })
    .catch((error) => {
      console.error("Error loading related tracks:", error);
    });
} // Like toggle function
// DEPRECATED: Like functionality moved to like-toggle.js module
// Use .js-like-toggle class on buttons with data-track-id attribute
/*
function toggleTrackLike(trackId, button) {
  const icon = button.querySelector('i[data-lucide="heart"]');
  const countSpan = button.querySelector(".like-count");
  const isLiked = button.dataset.liked === "true";

  if (icon) {
    icon.style.transform = "scale(0.8)";
    setTimeout(() => {
      icon.style.transform = "scale(1.2)";
      setTimeout(() => {
        icon.style.transform = "scale(1)";
      }, 150);
    }, 100);
  }

  fetch("/Track/ToggleLike", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      RequestVerificationToken:
        document.querySelector('input[name="__RequestVerificationToken"]')
          ?.value || "",
    },
    body: JSON.stringify({ id: trackId }),
  })
    .then((response) => response.json())
    .then((data) => {
      if (data.success) {
        button.dataset.liked = (!isLiked).toString();

        if (icon) {
          if (isLiked) {
            icon.classList.remove("fill-current");
            button.classList.remove("bg-accent", "text-white");
            button.classList.add(
              "bg-secondary",
              "text-gray-300",
              "hover:bg-muted"
            );
          } else {
            icon.classList.add("fill-current");
            button.classList.remove(
              "bg-secondary",
              "text-gray-300",
              "hover:bg-muted"
            );
            button.classList.add("bg-accent", "text-white");
          }
        }

        if (countSpan) {
          const currentCount = parseInt(countSpan.textContent) || 0;
          countSpan.textContent = isLiked ? currentCount - 1 : currentCount + 1;
        }
      } else {
        showToast(data.message || "Error occurred while updating like status");
      }
    })
    .catch((error) => {
      console.error("Error:", error);
      showToast("Error occurred while updating like status");
    });
}
*/

// Share track function
function shareTrack(trackId, trackTitle) {
  const url = window.location.origin + "/Track/Details/" + trackId;

  if (navigator.share) {
    navigator.share({
      title: trackTitle,
      text: "Eryth'te bu parçaya göz at",
      url: url,
    });
  } else {
    navigator.clipboard
      .writeText(url)
      .then(() => {
        showToast("Bağlantı panoya kopyalandı!");
      })
      .catch(() => {
        prompt("Bu bağlantıyı kopyala:", url);
      });
  }
}

// Download track function
function downloadTrack(trackId) {
  window.open("/Track/Download/" + trackId, "_blank");
}

// Comment functions
function showCommentForm() {
  document.getElementById("comment-form").classList.remove("hidden");
  document.getElementById("comment-text").focus();
}

function hideCommentForm() {
  document.getElementById("comment-form").classList.add("hidden");
  document.getElementById("comment-text").value = "";
} // Play track function - calls the global playTrack function from audioPlayer.js
function playTrackFromDetails(trackId, title, artist, audioUrl, coverUrl) {
  console.log("Track Details playTrackFromDetails called:", {
    trackId,
    title,
    artist,
    audioUrl,
    coverUrl,
  });

  // Add visual feedback
  const button = event.target?.closest("button");
  if (button) {
    button.style.transform = "scale(0.95)";
    setTimeout(() => {
      button.style.transform = "scale(1)";
    }, 150);
  }

  // Call the global play function from audioPlayer.js
  if (typeof window.playTrack === "function") {
    console.log("Calling global playTrack function from audioPlayer.js");
    window.playTrack(trackId, title, artist, audioUrl, coverUrl);
  } else {
    console.warn(
      "Global playTrack function not found, check if audioPlayer.js is loaded"
    );
    console.log("Playing track:", title, "by", artist, "from", audioUrl);
  }
}

// Like toggle function
function toggleTrackLike(trackId, button) {
  const icon = button.querySelector('i[data-lucide="heart"]');
  const countSpan = button.querySelector(".like-count");
  const isLiked = button.dataset.liked === "true";

  // Add animation
  if (icon) {
    icon.style.transform = "scale(0.8)";
    setTimeout(() => {
      icon.style.transform = "scale(1.2)";
      setTimeout(() => {
        icon.style.transform = "scale(1)";
      }, 150);
    }, 100);
  }

  // Make API call
  fetch("/Track/ToggleLike", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      RequestVerificationToken:
        document.querySelector('input[name="__RequestVerificationToken"]')
          ?.value || "",
    },
    body: JSON.stringify({ id: trackId }),
  })
    .then((response) => response.json())
    .then((data) => {
      if (data.success) {
        // Update UI
        button.dataset.liked = (!isLiked).toString();

        if (icon) {
          if (isLiked) {
            icon.classList.remove("fill-current");
            button.classList.remove("bg-accent", "text-white");
            button.classList.add(
              "bg-secondary",
              "text-gray-300",
              "hover:bg-muted"
            );
          } else {
            icon.classList.add("fill-current");
            button.classList.remove(
              "bg-secondary",
              "text-gray-300",
              "hover:bg-muted"
            );
            button.classList.add("bg-accent", "text-white");
          }
        }

        if (countSpan) {
          const currentCount = parseInt(countSpan.textContent) || 0;
          countSpan.textContent = isLiked ? currentCount - 1 : currentCount + 1;
        }
      } else {
        alert(data.message || "Beğeni durumu güncellenirken hata oluştu");
      }
    })
    .catch((error) => {
      console.error("Error:", error);
      alert("Beğeni durumu güncellenirken hata oluştu");
    });
}

// Share track function
function shareTrack(trackId, trackTitle) {
  const url = window.location.origin + "/Track/Details/" + trackId;

  if (navigator.share) {
    navigator.share({
      title: trackTitle,
      text: "Eryth'te bu parçaya göz at",
      url: url,
    });
  } else {
    // Fallback to copying to clipboard
    navigator.clipboard
      .writeText(url)
      .then(() => {
        // Show a toast notification
        showToast("Bağlantı panoya kopyalandı!");
      })
      .catch(() => {
        prompt("Bu bağlantıyı kopyala:", url);
      });
  }
}

// Download track function
function downloadTrack(trackId) {
  window.open("/Track/Download/" + trackId, "_blank");
}

// Comment functions
function showCommentForm() {
  document.getElementById("comment-form").classList.remove("hidden");
  document.getElementById("comment-text").focus();
}

function hideCommentForm() {
  document.getElementById("comment-form").classList.add("hidden");
  document.getElementById("comment-text").value = "";
}

function submitComment() {
  const commentText = document.getElementById("comment-text").value.trim();
  if (!commentText) {
    alert("Lütfen bir yorum girin");
    return;
  }

  // Check if trackData is available
  if (!window.trackData || !window.trackData.id) {
    console.error("Track data not available:", window.trackData);
    alert("Hata: Parça bilgisi mevcut değil");
    return;
  }

  // Make API call to submit comment
  const formData = new FormData();
  formData.append("TrackId", window.trackData.id);
  formData.append("Content", commentText);
  formData.append(
    "__RequestVerificationToken",
    document.querySelector('input[name="__RequestVerificationToken"]')?.value ||
      ""
  );

  console.log("Submitting comment with data:", {
    trackId: window.trackData.id,
    content: commentText,
    hasToken: !!document.querySelector(
      'input[name="__RequestVerificationToken"]'
    )?.value,
  });

  fetch("/Comment/Create", {
    method: "POST",
    headers: {
      "X-Requested-With": "XMLHttpRequest",
    },
    body: formData,
  })
    .then((response) => {
      console.log("Response status:", response.status);
      console.log("Response headers:", response.headers);

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      return response.json();
    })
    .then((data) => {
      console.log("Comment response data:", data);
      if (data.success) {
        hideCommentForm();
        document.getElementById("comment-text").value = ""; // Clear the text area
        showToast("Yorum başarıyla gönderildi!");
        // Reload comments or add new comment to the list
        loadComments(); // Reload comments dynamically instead of full page reload
      } else {        alert(data.message || "Yorum gönderilirken hata oluştu");
      }
    })
    .catch((error) => {
      console.error("Comment submission error:", error);
      alert("Yorum gönderilirken hata oluştu: " + error.message);
    });
}

// Toast notification function
function showToast(message) {
  const toast = document.createElement("div");
  toast.className =
    "fixed top-4 right-4 bg-muted text-white px-6 py-3 rounded-lg shadow-lg z-50 border border-accent";
  toast.textContent = message;
  document.body.appendChild(toast);

  setTimeout(() => {
    toast.style.opacity = "0";
    toast.style.transform = "translateY(-20px)";
    setTimeout(() => {
      if (document.body.contains(toast)) {
        document.body.removeChild(toast);
      }
    }, 300);
  }, 3000);
}
