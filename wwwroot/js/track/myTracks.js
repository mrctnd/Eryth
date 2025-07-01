document.addEventListener("DOMContentLoaded", function () {
  // Initialize Lucide icons
  if (typeof lucide !== "undefined") {
    lucide.createIcons();
  }

  // Initialize play buttons
  initializePlayButtons();
  
  // Initialize delete buttons
  initializeDeleteButtons();
});

// Initialize play buttons functionality
function initializePlayButtons() {
  const playButtons = document.querySelectorAll('[data-play-track]');
  playButtons.forEach(button => {
    button.addEventListener('click', function() {
      const trackId = this.getAttribute('data-track-id');
      const trackTitle = this.getAttribute('data-track-title');
      const trackArtist = this.getAttribute('data-track-artist');
      const trackUrl = this.getAttribute('data-track-url');
      const trackCover = this.getAttribute('data-track-cover');
      
      if (trackUrl) {
        playTrack({
          id: trackId,
          title: trackTitle,
          artist: trackArtist,
          audioUrl: trackUrl,
          coverUrl: trackCover
        });
      }
    });
  });
}

// Initialize delete buttons
function initializeDeleteButtons() {
  const deleteButtons = document.querySelectorAll('[data-delete-track]');
  deleteButtons.forEach(button => {
    button.addEventListener('click', function() {
      const trackId = this.getAttribute('data-track-id');
      deleteTrack(trackId);
    });
  });
}

// Play track function
function playTrack(track) {
  // Call the global playTrack function from audioPlayer.js
  if (typeof window.playTrack === 'function') {
    window.playTrack(track.id, track.title, track.artist, track.audioUrl, track.coverUrl);
  } else {    console.warn('Global playTrack function not found from audioPlayer.js');
    showToast('Ses oynatıcı mevcut değil', 'warning');
  }
}

// Delete track function
function deleteTrack(trackId) {
  if (confirm("Bu müziği silmek istediğinizden emin misiniz? Bu işlem geri alınamaz.")) {
    // Get CSRF token
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value ||
                  document.querySelector('meta[name="csrf-token"]')?.getAttribute('content');
    
    const headers = {
      "Content-Type": "application/json"
    };
    
    if (token) {
      headers["RequestVerificationToken"] = token;
    }

    fetch("/Track/Delete", {
      method: "POST",
      headers: headers,
      body: JSON.stringify({ id: trackId })
    })
    .then(response => {
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return response.json();
    })
    .then(data => {
      if (data.success) {
        showToast('Müzik başarıyla silindi', 'success');
        // Remove the track element from DOM with animation
        const trackElement = document.querySelector(`[data-track-id="${trackId}"]`).closest('.group');
        if (trackElement) {
          trackElement.style.transition = 'opacity 0.3s ease-out, transform 0.3s ease-out';
          trackElement.style.opacity = '0';
          trackElement.style.transform = 'scale(0.95)';
          
          setTimeout(() => {
            trackElement.remove();
            // Check if no tracks left
            const remainingTracks = document.querySelectorAll('.group').length;
            if (remainingTracks === 0) {
              location.reload();
            }
          }, 300);
        } else {
          location.reload();
        }
      } else {
        showToast(data.message || "Müzik silinirken bir hata oluştu", 'error');
      }
    })    .catch(error => {
      console.error("Error:", error);
      showToast("Müzik silinirken bir hata oluştu", 'error');
    });
  }
}

// Toast notification function
function showToast(message, type = 'info', duration = 5000) {
  // Remove existing toasts
  const existingToasts = document.querySelectorAll('.toast-notification');
  existingToasts.forEach(toast => toast.remove());

  // Create toast element
  const toast = document.createElement('div');
  toast.className = 'toast-notification fixed top-4 right-4 z-50 max-w-sm rounded-lg shadow-lg p-4 transition-all duration-300 transform translate-x-full';
  
  // Set toast styling based on type
  const styles = {
    success: 'bg-green-600 text-white border-l-4 border-green-400',
    error: 'bg-red-600 text-white border-l-4 border-red-400',
    warning: 'bg-yellow-600 text-white border-l-4 border-yellow-400',
    info: 'bg-blue-600 text-white border-l-4 border-blue-400'
  };
  
  toast.className += ' ' + (styles[type] || styles.info);
  
  // Create toast content
  const icons = {
    success: '✓',
    error: '✕',
    warning: '⚠',
    info: 'ℹ'
  };
  
  toast.innerHTML = `
    <div class="flex items-center">
      <span class="text-xl mr-3">${icons[type] || icons.info}</span>
      <p class="text-sm font-medium">${message}</p>
      <button class="ml-auto text-white hover:text-gray-200 transition-colors" onclick="this.parentElement.parentElement.remove()">
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
        </svg>
      </button>
    </div>
  `;
  
  // Add to page
  document.body.appendChild(toast);
  
  // Animate in
  setTimeout(() => {
    toast.classList.remove('translate-x-full');
  }, 100);
  
  // Auto remove
  setTimeout(() => {
    if (toast.parentElement) {
      toast.classList.add('translate-x-full');
      setTimeout(() => {
        if (toast.parentElement) {
          toast.remove();
        }
      }, 300);
    }  }, duration);
}
