// Initialize Lucide icons and interactions
document.addEventListener("DOMContentLoaded", function () {
  lucide.createIcons();

  // Add smooth hover effects
  document.querySelectorAll(".group").forEach((card) => {
    card.addEventListener("mouseenter", function () {
      this.style.transform = "translateY(-2px) scale(1.02)";
    });

    card.addEventListener("mouseleave", function () {
      this.style.transform = "translateY(0) scale(1)";
    });
  });
});

// Delete track — shows custom modal
let _pendingDeleteId = null;

function deleteTrack(trackId) {
  _pendingDeleteId = trackId;
  const modal = document.getElementById("deleteConfirmModal");
  if (modal) {
    modal.classList.remove("hidden");
    modal.classList.add("flex");
    if (typeof lucide !== "undefined") lucide.createIcons();
  }
}

function closeDeleteModal() {
  const modal = document.getElementById("deleteConfirmModal");
  if (modal) {
    modal.classList.add("hidden");
    modal.classList.remove("flex");
  }
  _pendingDeleteId = null;
}

function confirmDelete() {
  if (!_pendingDeleteId) return;
  const trackId = _pendingDeleteId;

  const btn = document.getElementById("confirmDeleteBtn");
  if (btn) { btn.textContent = "Siliniyor..."; btn.disabled = true; }

  const token =
    document.querySelector('input[name="__RequestVerificationToken"]')?.value ||
    document.querySelector('meta[name="__RequestVerificationToken"]')?.content;

  fetch("/Library/DeleteTrack", {
    method: "POST",
    headers: {
      "Content-Type": "application/x-www-form-urlencoded",
      RequestVerificationToken: token,
    },
    body: `id=${encodeURIComponent(trackId)}&__RequestVerificationToken=${encodeURIComponent(token || "")}`,
  })
    .then((response) => response.json())
    .then((data) => {
      closeDeleteModal();
      if (data.success) {
        const trackRows = document.querySelectorAll('tr[onclick*="' + trackId + '"]');
        trackRows.forEach((row) => row.remove());
        showNotification("Şarkı başarıyla silindi", "success");
        setTimeout(() => window.location.reload(), 1500);
      } else {
        showNotification(data.message || "Şarkı silinirken hata oluştu", "error");
      }
    })
    .catch((error) => {
      console.error("Error:", error);
      closeDeleteModal();
      showNotification("Şarkı silinirken hata oluştu", "error");
    })
    .finally(() => {
      if (btn) { btn.textContent = "Evet, Sil"; btn.disabled = false; }
    });
}

// Open edit modal
function openEditModal(trackId, title, description) {
  document.getElementById("editTrackId").value = trackId;
  document.getElementById("editTitle").value = title;
  document.getElementById("editDescription").value = description || "";

  const modal = document.getElementById("editTrackModal");
  modal.classList.remove("hidden");
  modal.classList.add("flex");

  // Focus on title input
  setTimeout(() => {
    document.getElementById("editTitle").focus();
  }, 100);
}

// Close edit modal
function closeEditModal() {
  const modal = document.getElementById("editTrackModal");
  modal.classList.add("hidden");
  modal.classList.remove("flex");

  // Reset form
  document.getElementById("editTrackForm").reset();
}

// Update track
function updateTrack(event) {
  event.preventDefault();

  const trackId = document.getElementById("editTrackId").value;
  const title = document.getElementById("editTitle").value.trim();
  const description = document.getElementById("editDescription").value.trim();

  if (!title) {
    showNotification("Title is required", "error");
    return;
  }

  // Get CSRF token
  const token =
    document.querySelector('input[name="__RequestVerificationToken"]')?.value ||
    document.querySelector('meta[name="__RequestVerificationToken"]')?.content;

  const submitButton = event.target.querySelector('button[type="submit"]');
  const originalText = submitButton.textContent;
  submitButton.textContent = "Saving...";
  submitButton.disabled = true;

  fetch("/Library/UpdateTrack", {
    method: "POST",
    headers: {
      "Content-Type": "application/x-www-form-urlencoded",
      RequestVerificationToken: token,
    },
    body: `id=${encodeURIComponent(trackId)}&title=${encodeURIComponent(
      title
    )}&description=${encodeURIComponent(
      description
    )}&__RequestVerificationToken=${encodeURIComponent(token || "")}`,
  })
    .then((response) => response.json())
    .then((data) => {
      if (data.success) {
        // Update the track title in the table
        const trackRows = document.querySelectorAll(
          'tr[onclick*="' + trackId + '"]'
        );
        trackRows.forEach((row) => {
          const titleCell = row.querySelector("td:nth-child(2) .font-semibold");
          if (titleCell) {
            titleCell.textContent = data.track.title;
          }
        });

        closeEditModal();
        showNotification("Track updated successfully", "success");
      } else {
        showNotification(data.message || "Error updating track", "error");
      }
    })
    .catch((error) => {
      console.error("Error:", error);
      showNotification("Error updating track", "error");
    })
    .finally(() => {
      submitButton.textContent = originalText;
      submitButton.disabled = false;
    });
}

// Show notification function
function showNotification(message, type = "info") {
  // Create notification element
  const notification = document.createElement("div");
  notification.className = `fixed top-4 right-4 z-50 px-6 py-3 rounded-lg text-white font-medium transform transition-all duration-300 translate-x-full`;

  if (type === "success") {
    notification.classList.add("bg-green-500");
  } else if (type === "error") {
    notification.classList.add("bg-red-500");
  } else {
    notification.classList.add("bg-blue-500");
  }

  notification.textContent = message;
  document.body.appendChild(notification);

  // Animate in
  setTimeout(() => {
    notification.classList.remove("translate-x-full");
  }, 100);

  // Remove after delay
  setTimeout(() => {
    notification.classList.add("translate-x-full");
    setTimeout(() => {
      document.body.removeChild(notification);
    }, 300);
  }, 3000);
}

// Close modals when clicking on backdrop
document.addEventListener("click", function (event) {
  const editModal = document.getElementById("editTrackModal");
  if (event.target === editModal) closeEditModal();

  const deleteModal = document.getElementById("deleteConfirmModal");
  if (event.target === deleteModal) closeDeleteModal();
});

// Close modals with Escape key
document.addEventListener("keydown", function (event) {
  if (event.key === "Escape") {
    closeEditModal();
    closeDeleteModal();
  }
});
