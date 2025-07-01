// Profile Page JavaScript Functions
document.addEventListener('DOMContentLoaded', function() {
    // Initialize Lucide icons
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }    // Follow/Unfollow functionality
    const followBtn = document.getElementById('follow-btn');
    if (followBtn) {
        followBtn.addEventListener('click', function() {
            const userId = this.dataset.userId;
            const isFollowing = this.innerHTML.includes('Following');
            
            fetch('/User/ToggleFollow', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: JSON.stringify({ userId: userId })
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    if (data.isFollowing) {
                        this.innerHTML = '<i data-lucide="user-check" class="w-5 h-5"></i><span>Following</span>';
                        this.className = 'bg-white/10 text-white hover:bg-red-500/20 hover:text-red-400 border border-white/20 hover:border-red-400/50 px-6 py-3 rounded-lg font-medium transition-all duration-300 transform hover:-translate-y-1 flex items-center gap-2';
                    } else {
                        this.innerHTML = '<i data-lucide="user-plus" class="w-5 h-5"></i><span>Follow</span>';
                        this.className = 'bg-gradient-to-r from-accent to-accent-dark text-white hover:from-accent-dark hover:to-accent px-6 py-3 rounded-lg font-medium transition-all duration-300 transform hover:-translate-y-1 flex items-center gap-2';
                    }
                    
                    // Reinitialize Lucide icons after DOM change
                    if (typeof lucide !== 'undefined') {
                        lucide.createIcons();
                    }
                    
                    showNotification(data.isFollowing ? 'Now following!' : 'Unfollowed', 'success');
                } else {
                    showNotification(data.message || 'An error occurred', 'error');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                showNotification('An error occurred while updating follow status', 'error');
            });
        });
    }
});

// Track interaction functions - playTrack is now global from audioPlayer.js

function playTrackFromButton(button) {
    const trackId = button.getAttribute('data-track-id');
    const title = button.getAttribute('data-track-title');
    const artist = button.getAttribute('data-track-artist');
    const audioUrl = button.getAttribute('data-track-audio');
    const coverUrl = button.getAttribute('data-track-cover');
    
    console.log('Playing track from button:', { trackId, title, artist, audioUrl, coverUrl });
    console.log('Window playTrack function exists:', window.playTrack && typeof window.playTrack === 'function');
    
    // Call the global playTrack function from audioPlayer.js
    if (window.playTrack && typeof window.playTrack === 'function') {
        console.log('Calling window.playTrack from audioPlayer.js...');
        window.playTrack(trackId, title, artist, audioUrl, coverUrl);
        console.log('window.playTrack called successfully');
    } else {
        console.error('Global playTrack function not found - check if audioPlayer.js is loaded');
        showNotification('Audio player not available', 'error');
    }
}

function toggleLike(trackId) {
    fetch(`/Likes/Toggle/${trackId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
        }
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            const likeBtn = document.querySelector(`button[onclick*="toggleLike(${trackId})"] i`);
            const likeCount = document.querySelector(`button[onclick*="toggleLike(${trackId})"] span`);
            
            if (likeBtn && likeCount) {
                if (data.isLiked) {
                    likeBtn.classList.add('fill-current', 'text-red-400');
                    showNotification('Track liked!', 'success');
                } else {
                    likeBtn.classList.remove('fill-current', 'text-red-400');
                    showNotification('Track unliked', 'info');
                }
                likeCount.textContent = data.likeCount;
            }
        } else {
            showNotification('Error toggling like', 'error');
        }
    })
    .catch(error => {
        console.error('Error toggling like:', error);
        showNotification('Error toggling like', 'error');
    });
}

function showComments(trackId) {
    window.location.href = `/Track/Details/${trackId}#comments`;
}

function shareTrack(trackId, trackTitle) {
    const url = `${window.location.origin}/Track/Details/${trackId}`;
    
    if (navigator.share) {
        navigator.share({
            title: trackTitle,
            text: `Check out this track: ${trackTitle}`,
            url: url
        }).then(() => {
            showNotification('Track shared!', 'success');
        }).catch(error => {
            if (error.name !== 'AbortError') {
                copyToClipboard(url, 'Track URL copied to clipboard!');
            }
        });
    } else {
        copyToClipboard(url, 'Track URL copied to clipboard!');
    }
}

function shareProfile() {
    const url = window.location.href;
    const profileName = document.querySelector('h1').textContent;
    
    if (navigator.share) {
        navigator.share({
            title: `${profileName}'s Profile`,
            text: `Check out ${profileName}'s music profile`,
            url: url
        }).then(() => {
            showNotification('Profile shared!', 'success');
        }).catch(error => {
            if (error.name !== 'AbortError') {
                copyToClipboard(url, 'Profile URL copied to clipboard!');
            }
        });
    } else {
        copyToClipboard(url, 'Profile URL copied to clipboard!');
    }
}

function showMoreOptions(trackId) {
    // TODO: Implement more options modal/dropdown
    console.log('Show more options for track:', trackId);
    showNotification('More options coming soon!', 'info');
}

// Utility Functions
function copyToClipboard(text, successMessage) {
    navigator.clipboard.writeText(text).then(() => {
        showNotification(successMessage, 'success');
    }).catch(() => {
        // Fallback for older browsers
        const textArea = document.createElement('textarea');
        textArea.value = text;
        document.body.appendChild(textArea);
        textArea.select();
        try {
            document.execCommand('copy');
            showNotification(successMessage, 'success');
        } catch (err) {
            showNotification('Failed to copy to clipboard', 'error');
        }
        document.body.removeChild(textArea);
    });
}

function showNotification(message, type = 'info') {
    // Remove existing notifications
    const existingNotifications = document.querySelectorAll('.notification');
    existingNotifications.forEach(notification => notification.remove());

    const notification = document.createElement('div');
    notification.className = 'notification fixed top-4 right-4 p-4 rounded-lg shadow-lg z-50 text-white transform translate-x-full transition-transform duration-300 max-w-xs';
    
    // Set background color based on type
    switch(type) {
        case 'success':
            notification.classList.add('bg-green-600');
            break;
        case 'error':
            notification.classList.add('bg-red-600');
            break;
        case 'warning':
            notification.classList.add('bg-yellow-600');
            break;
        default:
            notification.classList.add('bg-blue-600');
    }
    
    notification.textContent = message;
    document.body.appendChild(notification);
    
    // Animate in
    setTimeout(() => {
        notification.classList.remove('translate-x-full');
    }, 100);
    
    // Animate out and remove
    setTimeout(() => {
        notification.classList.add('translate-x-full');
        setTimeout(() => {
            if (notification.parentNode) {
                notification.remove();
            }
        }, 300);
    }, 3000);
}

// Waveform animation for track items
function initializeWaveforms() {
    const waveforms = document.querySelectorAll('.waveform');
    waveforms.forEach(waveform => {
        const bars = waveform.querySelectorAll('.waveform-bar');
        bars.forEach((bar, index) => {
            // Add subtle animation
            bar.style.animationDelay = `${index * 10}ms`;
        });
    });
}

// Call waveform initialization after DOM is loaded
document.addEventListener('DOMContentLoaded', initializeWaveforms);
