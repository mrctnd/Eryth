/**
 * Centralized Like Toggle Module
 * Handles like/unlike functionality for tracks across the entire application
 */

class LikeToggleManager {
    constructor() {
        this.init();
    }

    init() {
        // Bind event listeners for like buttons
        document.addEventListener('click', this.handleLikeClick.bind(this));
        
        // Initialize existing like buttons on page load
        this.initializeLikeButtons();
    }

    async handleLikeClick(event) {
        // Check if clicked element is a like button
        const likeButton = event.target.closest('.js-like-toggle');
        if (!likeButton) return;

        event.preventDefault();
        event.stopPropagation();

        const trackId = likeButton.dataset.trackId;
        if (!trackId) {
            console.error('Track ID not found on like button');
            return;
        }

        await this.toggleLike(trackId, likeButton);
    }

    async toggleLike(trackId, button) {
        try {
            // Optimistic UI update
            const wasLiked = button.classList.contains('is-liked');
            const heartIcon = button.querySelector('.js-like-icon');
            const countElement = button.querySelector('.js-like-count');
            
            // Add loading state
            button.disabled = true;
            button.classList.add('is-loading');
            
            // Optimistic update
            this.updateButtonState(button, !wasLiked, null);

            // Make API call
            const method = wasLiked ? 'DELETE' : 'POST';
            const response = await fetch(`/api/tracks/${trackId}/like`, {
                method: method,
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': this.getCSRFToken()
                }
            });

            // Handle authentication required
            if (response.status === 401) {
                this.showLoginPrompt();
                // Revert optimistic update
                this.updateButtonState(button, wasLiked, null);
                return;
            }

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();
            
            // Update UI with real data
            this.updateButtonState(button, data.liked, data.likeCount);
            
            // Show success feedback
            this.showFeedback(data.liked ? 'Track liked!' : 'Track unliked', 'success');

        } catch (error) {
            console.error('Error toggling like:', error);
            
            // Revert optimistic update on error
            const wasLiked = !button.classList.contains('is-liked');
            this.updateButtonState(button, wasLiked, null);
              // Show error message
            this.showFeedback('Beğeni durumu güncellenemedi. Lütfen tekrar deneyin.', 'error');
        } finally {
            // Remove loading state
            button.disabled = false;
            button.classList.remove('is-loading');
        }
    }

    updateButtonState(button, isLiked, likeCount) {
        const heartIcon = button.querySelector('.js-like-icon');
        const countElement = button.querySelector('.js-like-count');
        
        // Update button state
        button.classList.toggle('is-liked', isLiked);
        
        // Update icon styling
        if (heartIcon) {
            if (isLiked) {
                heartIcon.classList.remove('text-gray-400');
                heartIcon.classList.add('text-red-500', 'fill-current');
                // Add heart animation
                this.animateHeart(heartIcon);
            } else {
                heartIcon.classList.remove('text-red-500', 'fill-current');
                heartIcon.classList.add('text-gray-400');
            }
        }
        
        // Update count if provided and element exists
        if (countElement && likeCount !== null) {
            countElement.textContent = likeCount;
        }
    }

    animateHeart(heartIcon) {
        // Add a scale animation to the heart
        heartIcon.style.transform = 'scale(0.8)';
        heartIcon.style.transition = 'transform 0.15s ease-out';
        
        setTimeout(() => {
            heartIcon.style.transform = 'scale(1.2)';
            setTimeout(() => {
                heartIcon.style.transform = 'scale(1)';
            }, 150);
        }, 100);
    }

    initializeLikeButtons() {
        const likeButtons = document.querySelectorAll('.js-like-toggle');
        likeButtons.forEach(button => {
            const trackId = button.dataset.trackId;
            if (trackId) {
                // Initialize button state if needed
                // This could be enhanced to fetch current like status if not already set
            }
        });
    }

    getCSRFToken() {
        // Try to get CSRF token from various sources
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        if (tokenInput) return tokenInput.value;
        
        const tokenMeta = document.querySelector('meta[name="__RequestVerificationToken"]');
        if (tokenMeta) return tokenMeta.getAttribute('content');
        
        return '';
    }

    showLoginPrompt() {
        if (confirm('You need to be logged in to like tracks. Would you like to log in now?')) {
            window.location.href = '/Account/Login';
        }
    }

    showFeedback(message, type = 'info') {
        // Create and show a toast notification
        const toast = document.createElement('div');
        toast.className = `fixed top-4 right-4 z-50 px-4 py-2 rounded-lg shadow-lg text-white transition-all duration-300 transform translate-x-full`;
        
        // Set color based on type
        switch (type) {
            case 'success':
                toast.classList.add('bg-green-600');
                break;
            case 'error':
                toast.classList.add('bg-red-600');
                break;
            default:
                toast.classList.add('bg-blue-600');
        }
        
        toast.innerHTML = `
            <div class="flex items-center space-x-2">
                <span>${message}</span>
                <button onclick="this.parentElement.parentElement.remove()" class="text-white hover:text-gray-200">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                    </svg>
                </button>
            </div>
        `;
        
        document.body.appendChild(toast);
        
        // Animate in
        setTimeout(() => {
            toast.classList.remove('translate-x-full');
        }, 100);
        
        // Auto remove after 3 seconds
        setTimeout(() => {
            toast.classList.add('translate-x-full');
            setTimeout(() => {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
            }, 300);
        }, 3000);
    }
}

// Initialize the like toggle manager when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.likeToggleManager = new LikeToggleManager();
});

// Export for module usage if needed
if (typeof module !== 'undefined' && module.exports) {
    module.exports = LikeToggleManager;
}
