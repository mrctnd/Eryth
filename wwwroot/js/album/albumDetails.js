document.addEventListener("DOMContentLoaded", function () {
  lucide.createIcons();
  // Track click handlers
  document.querySelectorAll(".track-item").forEach((item) => {
    item.addEventListener("click", function () {
      // Add track play functionality here using audioPlayer.js
      const trackId = this.getAttribute('data-track-id');
      const title = this.getAttribute('data-track-title');
      const artist = this.getAttribute('data-track-artist');
      const audioUrl = this.getAttribute('data-track-audio');
      const coverUrl = this.getAttribute('data-track-cover');
      
      if (typeof window.playTrack === 'function' && audioUrl) {
        window.playTrack(trackId, title, artist, audioUrl, coverUrl);
      } else {
        console.log("Playing track:", title || "Unknown");
      }
    });
  });

  // Make functions global so they can be called from HTML
  window.showAddTrackModal = function() {
    const modal = document.getElementById("add-track-modal");
    const content = document.getElementById("add-track-content");

    if (modal && content) {
      modal.classList.remove("hidden");
      setTimeout(() => {
        content.classList.remove("scale-95", "opacity-0");
        content.classList.add("scale-100", "opacity-100");
      }, 10);
    }
  };

  window.hideAddTrackModal = function() {
    const modal = document.getElementById("add-track-modal");
    const content = document.getElementById("add-track-content");

    if (content) {
      content.classList.add("scale-95", "opacity-0");
      content.classList.remove("scale-100", "opacity-100");

      setTimeout(() => {
        if (modal) modal.classList.add("hidden");
        const form = document.getElementById("add-track-form");
        if (form) form.reset();
        clearTrackValidationErrors();
      }, 300);
    }
  };

  window.showDeleteAlbumModal = function() {
    console.log("Show delete modal called"); // Debug log
    const modal = document.getElementById("delete-album-modal");
    const content = document.getElementById("delete-album-content");

    if (modal && content) {
      modal.classList.remove("hidden");
      setTimeout(() => {
        content.classList.remove("scale-95", "opacity-0");
        content.classList.add("scale-100", "opacity-100");
      }, 10);
    } else {
      console.error("Delete modal elements not found:", { modal, content });
    }
  };

  window.hideDeleteAlbumModal = function() {
    const modal = document.getElementById("delete-album-modal");
    const content = document.getElementById("delete-album-content");

    if (content) {
      content.classList.add("scale-95", "opacity-0");      content.classList.remove("scale-100", "opacity-100");

      setTimeout(() => {
        if (modal) modal.classList.add("hidden");
      }, 300);
    }
  };
  window.deleteAlbum = function(albumId) {
    console.log("Delete album called with ID:", albumId); // Debug log
    
    // Get CSRF token from input or meta tag
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value ||
                  document.querySelector('meta[name="__RequestVerificationToken"]')?.getAttribute('content');

    console.log("CSRF Token:", token); // Debug log

    if (!token) {
      alert('Security token not found. Please refresh the page and try again.');
      return;
    }

    // Show loading state
    const deleteButton = event.target;
    const originalText = deleteButton.innerHTML;
    deleteButton.innerHTML = '<div class="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2 inline-block"></div>Deleting...';
    deleteButton.disabled = true;

    console.log("Sending delete request to:", `/Album/Delete/${albumId}`); // Debug log

    // Create a FormData object instead of manually constructing the body
    const formData = new FormData();
    formData.append('__RequestVerificationToken', token);

    fetch(`/Album/Delete/${albumId}`, {
      method: 'POST',
      body: formData
    })
    .then(response => {
      console.log("Response status:", response.status); // Debug log
      console.log("Response content-type:", response.headers.get('content-type')); // Debug log
      
      if (!response.ok) {
        return response.text().then(text => {
          console.log("Error response body:", text);
          throw new Error(`HTTP error! status: ${response.status}, body: ${text}`);
        });
      }
      
      const contentType = response.headers.get('content-type');
      if (contentType && contentType.includes('application/json')) {
        return response.json();
      } else {
        return response.text().then(text => {
          console.log("Non-JSON response:", text);
          // If we get a redirect or HTML response, assume success
          return { success: true, message: 'Album deleted successfully' };
        });
      }
    })
    .then(data => {
      console.log("Response data:", data); // Debug log
      if (data.success || data.success === undefined) {
        // Show success message
        window.hideDeleteAlbumModal();
        
        // Create success notification
        const notification = document.createElement('div');
        notification.className = 'fixed top-4 right-4 bg-green-500 text-white px-6 py-3 rounded-lg shadow-lg z-50 transform transition-all duration-300 translate-x-full';
        notification.innerHTML = '<i data-lucide="check-circle" class="w-5 h-5 inline mr-2"></i>Album deleted successfully!';
        document.body.appendChild(notification);
        
        // Animate in
        setTimeout(() => {
          notification.classList.remove('translate-x-full');
        }, 10);
        
        // Redirect after a short delay
        setTimeout(() => {
          window.location.href = '/User/Profile';
        }, 2000);
      } else {
        alert(data.message || 'Failed to delete album.');
        deleteButton.innerHTML = originalText;
        deleteButton.disabled = false;
      }
    })
    .catch(error => {
      console.error('Error deleting album:', error);
      alert('Failed to delete album: ' + error.message);
      deleteButton.innerHTML = originalText;
      deleteButton.disabled = false;
    });
  };

  // Handle add track form submission
  const addTrackForm = document.getElementById("add-track-form");
  if (addTrackForm) {
    addTrackForm.addEventListener("submit", function (e) {
      e.preventDefault();

      clearTrackValidationErrors();

      const title = document.getElementById("track-title").value;
      const duration = document.getElementById("track-duration").value;
      const albumId = document.getElementById("albumId").value;

      let hasErrors = false;

      if (!title.trim()) {
        showTrackValidationError("title-error", "Please enter a track title.");
        hasErrors = true;
      }

      if (!duration || duration < 1) {
        showTrackValidationError(
          "duration-error",
          "Please enter a valid duration."
        );
        hasErrors = true;
      }

      if (hasErrors) {
        return;
      }

      const formData = new FormData(this);
      formData.append("AlbumId", albumId);

      fetch("/Track/AddToAlbum", {
        method: "POST",
        body: formData,
      })
        .then((response) => response.json())
        .then((data) => {
          if (data.success) {
            alert("Track added successfully!");
            window.hideAddTrackModal();
            location.reload(); // Reload to show the new track
          } else {
            alert(data.error || "Failed to add track.");
          }
        })
        .catch((error) => {
          console.error("Error adding track:", error);
          alert("An error occurred while adding the track.");
        });
    });
  }

  function showTrackValidationError(errorId, message) {
    const errorElement = document.getElementById(errorId);
    if (errorElement) {
      errorElement.textContent = message;
      errorElement.classList.remove("hidden");
    }
  }

  function clearTrackValidationErrors() {
    const errorElements = document.querySelectorAll('[id$="-error"]');
    errorElements.forEach((element) => {
      element.classList.add("hidden");
    });
  }
});
