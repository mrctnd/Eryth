/**
 * Audio Player Module
 * Handles all audio playback functionality for the music platform
 */

// Global audio player state
let currentAudio = null;
let isPlaying = false;
let currentTrackData = null;

// Playlist management
let currentPlaylist = [];
let currentTrackIndex = -1;
let isShuffled = false;
let repeatMode = 'off'; // 'off', 'one', 'all'

// Audio player state persistence keys
const STORAGE_KEYS = {
    CURRENT_TRACK: 'audioPlayer_currentTrack',
    IS_PLAYING: 'audioPlayer_isPlaying',
    CURRENT_TIME: 'audioPlayer_currentTime',
    VOLUME: 'audioPlayer_volume',
    PLAYER_VISIBLE: 'audioPlayer_visible',
    CURRENT_PLAYLIST: 'audioPlayer_currentPlaylist',
    CURRENT_TRACK_INDEX: 'audioPlayer_currentTrackIndex',
    REPEAT_MODE: 'audioPlayer_repeatMode',
    IS_SHUFFLED: 'audioPlayer_isShuffled'
};

/**
 * Initialize audio volume for consistency across tabs
 * @param {HTMLAudioElement} audioElement - The audio element to initialize
 */
function initializeAudioVolume(audioElement) {
    if (!audioElement) return;
    
    // Priority 1: Use the slider value if it exists
    const volumeSlider = document.getElementById('volume-slider');
    
    // Priority 2: Use stored preference if available
    const storedVolume = localStorage.getItem('preferredVolume');
    
    // Priority 3: Default to 75% volume
    const defaultVolume = 0.75;
    
    if (volumeSlider) {
        // Make sure the slider is in sync with stored preference
        if (storedVolume && volumeSlider.value !== storedVolume) {
            volumeSlider.value = storedVolume;
        }
        
        // Get the current slider position
        const sliderValue = parseFloat(volumeSlider.value);
        
        // Apply the volume to the audio
        audioElement.volume = sliderValue / 100;
    } else if (storedVolume) {
        // Use stored preference if slider doesn't exist
        audioElement.volume = parseFloat(storedVolume) / 100;
    } else {
        // Fallback to default volume
        audioElement.volume = defaultVolume;
    }
    
    console.log('Audio volume initialized to:', audioElement.volume);
}

/**
 * Play a track with given parameters
 * @param {string} trackId - The track ID
 * @param {string} title - Track title
 * @param {string} artist - Artist name
 * @param {string} audioUrl - Audio file URL
 * @param {string} coverImage - Cover image URL
 */
/**
 * Play a track with optional playlist context
 * @param {string} trackId - Track ID
 * @param {string} title - Track title
 * @param {string} artist - Artist name
 * @param {string} audioUrl - Audio file URL
 * @param {string} coverImage - Cover image URL
 * @param {Array} playlist - Optional playlist data
 * @param {number} trackIndex - Optional track index in playlist
 */
function playTrack(trackId, title, artist, audioUrl, coverImage, playlist = null, trackIndex = -1) {
    // Ensure we have valid string values for title and artist
    const safeTitle = (title && title.trim()) || 'Unknown Track';
    const safeArtist = (artist && artist.trim()) || 'Unknown Artist';
    
    console.log(`Now playing: ${safeTitle} by ${safeArtist}`);
    
    // Store current track data with safe values
    currentTrackData = { trackId, title: safeTitle, artist: safeArtist, audioUrl, coverImage };
    
    // Update playlist context if provided
    if (playlist && Array.isArray(playlist)) {
        currentPlaylist = playlist;
        currentTrackIndex = trackIndex >= 0 ? trackIndex : currentPlaylist.findIndex(t => t.trackId === trackId);
        console.log(`Playing track ${currentTrackIndex + 1} of ${currentPlaylist.length} in playlist`);
    }
    
    // Stop current audio if playing
    if (currentAudio) {
        currentAudio.pause();
        currentAudio = null;
    }
    
    // Show audio player
    showAudioPlayer();
      // Update track info with safe values
    updateTrackInfo(safeTitle, safeArtist, coverImage);
      // Create and configure audio element
    if (audioUrl) {
        currentAudio = new Audio(audioUrl);
        
        // Set initial volume using a structured approach
        initializeAudioVolume(currentAudio);
        
        // Setup event listeners
        setupAudioEventListeners();
        
        // Auto-play the track
        currentAudio.play().then(() => {
            isPlaying = true;
            updatePlayPauseButton('pause');
            // Ensure track info stays updated even after play starts
            updateTrackInfo(safeTitle, safeArtist, coverImage);
            savePlayerState(); // Save state when track starts playing
        }).catch(error => {
            console.error('Error playing audio:', error);
            isPlaying = false;
            updatePlayPauseButton('play');
        });
    }    
    // Save state immediately
    savePlayerState();
}

/**
 * Play next track in playlist
 */
function playNextTrack() {
    if (currentPlaylist.length === 0) {
        console.log('No playlist available for next track');
        return false;
    }
    
    let nextIndex;
    
    if (repeatMode === 'one') {
        // Repeat current track
        nextIndex = currentTrackIndex;
    } else if (isShuffled) {
        // Random next track
        nextIndex = Math.floor(Math.random() * currentPlaylist.length);
    } else {
        // Linear next track
        nextIndex = currentTrackIndex + 1;
        
        if (nextIndex >= currentPlaylist.length) {
            if (repeatMode === 'all') {
                nextIndex = 0; // Loop back to start
            } else {
                console.log('End of playlist reached');
                return false;
            }
        }
    }
    
    const nextTrack = currentPlaylist[nextIndex];
    if (nextTrack) {
        playTrack(
            nextTrack.trackId,
            nextTrack.title,
            nextTrack.artist,
            nextTrack.audioUrl,
            nextTrack.coverImage,
            currentPlaylist,
            nextIndex
        );
        return true;
    }
    
    return false;
}

/**
 * Play previous track in playlist
 */
function playPreviousTrack() {
    if (currentPlaylist.length === 0) {
        console.log('No playlist available for previous track');
        return false;
    }
    
    let prevIndex;
    
    if (isShuffled) {
        // Random previous track
        prevIndex = Math.floor(Math.random() * currentPlaylist.length);
    } else {
        // Linear previous track
        prevIndex = currentTrackIndex - 1;
        
        if (prevIndex < 0) {
            if (repeatMode === 'all') {
                prevIndex = currentPlaylist.length - 1; // Loop to end
            } else {
                console.log('Start of playlist reached');
                return false;
            }
        }
    }
    
    const prevTrack = currentPlaylist[prevIndex];
    if (prevTrack) {
        playTrack(
            prevTrack.trackId,
            prevTrack.title,
            prevTrack.artist,
            prevTrack.audioUrl,
            prevTrack.coverImage,
            currentPlaylist,
            prevIndex
        );
        return true;
    }
    
    return false;
}

/**
 * Update track information in the player UI
 * @param {string} title - Track title
 * @param {string} artist - Artist name
 * @param {string} coverImage - Cover image URL
 */
function updateTrackInfo(title, artist, coverImage) {
    const trackTitle = document.getElementById('track-title');
    const trackArtist = document.getElementById('track-artist');
    const trackThumbnail = document.getElementById('track-thumbnail');
    
    // Ensure we have valid string values
    const safeTitle = (title && title.trim()) || 'Unknown Track';
    const safeArtist = (artist && artist.trim()) || 'Unknown Artist';
    
    if (trackTitle) {
        trackTitle.textContent = safeTitle;
    }
    if (trackArtist) {
        trackArtist.textContent = safeArtist;
    }
    if (trackThumbnail && coverImage) {
        trackThumbnail.src = coverImage;
    } else if (trackThumbnail) {
        trackThumbnail.src = 'https://via.placeholder.com/48?text=♪';
    }
    
    console.log('Track info updated:', { title: safeTitle, artist: safeArtist });
}

/**
 * Toggle play/pause state
 */
function togglePlayPause() {
    if (currentAudio) {
        if (isPlaying) {
            currentAudio.pause();
            // Note: pause event listener will handle state update
        } else {
            currentAudio.play().then(() => {
                // Note: play event listener will handle state update
            }).catch(error => {
                console.error('Error playing audio:', error);
                isPlaying = false;
                updatePlayPauseButton('play');
                savePlayerState();
            });
        }
    }
}

/**
 * Update play/pause button icon
 * @param {string} state - 'play' or 'pause'
 */
function updatePlayPauseButton(state) {
    const playPauseIcon = document.getElementById('play-pause-icon');
    if (playPauseIcon) {
        playPauseIcon.setAttribute('data-lucide', state);
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }
    }
}

/**
 * Update progress bar and time displays
 */
function updateProgress() {
    if (currentAudio) {
        const currentTime = currentAudio.currentTime;
        const duration = currentAudio.duration;
        const progressFill = document.getElementById('progress-fill');
        const progressThumb = document.getElementById('progress-thumb');
        const currentTimeElement = document.getElementById('current-time');
        const durationElement = document.getElementById('duration');
        
        if (progressFill && duration > 0) {
            const progressPercent = (currentTime / duration) * 100;
            progressFill.style.width = progressPercent + '%';
            
            // Update thumb position
            if (progressThumb) {
                progressThumb.style.left = progressPercent + '%';
            }
        }
        
        if (currentTimeElement) {
            currentTimeElement.textContent = formatTime(currentTime);
        }
        
        if (durationElement) {
            durationElement.textContent = formatTime(duration);
        }
    }
}

/**
 * Seek audio to specific position
 * @param {Event} event - Click event on progress bar
 */
function seekAudio(event) {
    if (currentAudio) {
        const progressBar = event.currentTarget;
        const rect = progressBar.getBoundingClientRect();
        const clickX = event.clientX - rect.left;
        const width = rect.width;
        const percentage = clickX / width;
        const seekTime = currentAudio.duration * percentage;
        
        currentAudio.currentTime = seekTime;
        updateProgress();
    }
}

/**
 * Format time in MM:SS format
 * @param {number} seconds - Time in seconds
 * @returns {string} Formatted time string
 */
function formatTime(seconds) {
    const minutes = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${minutes}:${secs.toString().padStart(2, '0')}`;
}

/**
 * Hide the audio player
 */
function hideAudioPlayer() {
    const player = document.getElementById('audio-player');
    if (player) {
        player.style.transform = 'translateY(100%)';
        setTimeout(() => {
            player.classList.add('hidden');
            player.style.display = 'none';
        }, 300);
    }
    
    if (currentAudio) {
        currentAudio.pause();
        currentAudio = null;
        isPlaying = false;
        currentTrackData = null;
    }
    
    // Clear saved state when manually hiding player
    clearPlayerState();
}

/**
 * Update volume level
 * @param {number} value - Volume value (0-100)
 */
function updateVolume(value) {
    // Ensure value is properly parsed as a number
    const numericValue = parseFloat(value);
    
    // Volume must be between 0-1
    const volumeValue = numericValue / 100;
    
    // Clamp the value between 0 and 1 to avoid any potential issues
    const clampedVolume = Math.min(Math.max(volumeValue, 0), 1);
    
    // Apply to audio if it exists
    if (currentAudio) {
        // Apply the volume directly to the audio element
        currentAudio.volume = clampedVolume;
        
        // Verify volume change
        console.log('Volume set to:', clampedVolume, 'Current audio volume:', currentAudio.volume);
    }
    
    // Update the volume slider UI to reflect the change
    const volumeSlider = document.getElementById('volume-slider');
    if (volumeSlider && volumeSlider.value !== numericValue.toString()) {
        volumeSlider.value = numericValue;
    }
    
    // Store the volume preference for persistence across page loads and tabs
    localStorage.setItem('preferredVolume', numericValue);
    
    // Broadcast the volume change to other tabs using the storage event
    try {
        // Use a timestamp to ensure the event is triggered even if the same value is set
        sessionStorage.setItem('volumeChangeTimestamp', Date.now().toString());
    } catch (e) {
        console.warn('Could not broadcast volume change to other tabs', e);
    }
}

/**
 * Setup progress bar interactions
 */
function setupProgressBarInteractions() {
    const progressBar = document.getElementById('progress-bar');
    const progressThumb = document.getElementById('progress-thumb');
    
    if (progressBar && progressThumb) {
        progressBar.addEventListener('mouseenter', () => {
            progressThumb.style.opacity = '1';
        });

        progressBar.addEventListener('mouseleave', () => {
            progressThumb.style.opacity = '0';
        });
    }
}

/**
 * Setup volume controls
 */
function setupVolumeControls() {
    const volumeSlider = document.getElementById('volume-slider');
    const volumeButton = document.getElementById('volume-button');
    
    if (volumeSlider) {
        // Check for stored volume preference
        const storedVolume = localStorage.getItem('preferredVolume');
        if (storedVolume) {
            // Update slider UI to match stored preference
            volumeSlider.value = storedVolume;
        }
        
        // Add event listener for live volume changes
        volumeSlider.addEventListener('input', function() {
            updateVolume(this.value);
        });
        
        // Also handle the change event for when the user finishes dragging
        volumeSlider.addEventListener('change', function() {
            updateVolume(this.value);
        });
        
        // Set initial volume when slider exists
        updateVolume(volumeSlider.value);
    }
    
    // Add manual sync function to the volume button
    if (volumeButton) {
        volumeButton.addEventListener('click', function() {
            // Force re-sync volume across tabs
            const currentValue = volumeSlider ? volumeSlider.value : '75';
            
            // Clear and reset the storage to trigger events
            localStorage.removeItem('preferredVolume');
            setTimeout(() => {
                // Update the volume with the current setting
                updateVolume(currentValue);
                
                // Visual feedback
                this.classList.add('text-[#FF5500]');
                setTimeout(() => {
                    this.classList.remove('text-[#FF5500]');
                }, 300);
                
                console.log('Volume manually re-synchronized');
            }, 50);
        });
    }
}

/**
 * Setup cross-tab volume synchronization
 */
function setupCrossTabSync() {
    // Listen for volume changes from other tabs
    window.addEventListener('storage', function(e) {
        // Check for volume changes from other tabs
        if (e.key === 'preferredVolume' && e.newValue) {
            console.log('Volume change detected from another tab:', e.newValue);
            
            // Update the slider if it exists
            const volumeSlider = document.getElementById('volume-slider');
            if (volumeSlider) {
                volumeSlider.value = e.newValue;
            }
            
            // Apply the new volume to the current audio if playing
            if (currentAudio) {
                const newVolume = parseFloat(e.newValue) / 100;
                currentAudio.volume = newVolume;
                console.log('Updated audio volume from another tab:', newVolume);
            }
        }
    });
    
    // Always apply the stored volume preference on page load
    const storedVolumePreference = localStorage.getItem('preferredVolume');
    if (storedVolumePreference && currentAudio) {
        currentAudio.volume = parseFloat(storedVolumePreference) / 100;
        console.log('Applied stored volume preference:', currentAudio.volume);
    }
}

/**
 * Setup tab visibility change handlers
 */
function setupTabVisibilityHandlers() {
    // Handle tab visibility changes
    document.addEventListener('visibilitychange', function() {
        if (document.visibilityState === 'visible' && currentAudio) {
            // Re-sync volume settings when tab becomes visible again
            const storedVol = localStorage.getItem('preferredVolume');
            if (storedVol) {
                // Re-apply volume settings
                const volumeValue = parseFloat(storedVol) / 100;
                if (currentAudio.volume !== volumeValue) {
                    currentAudio.volume = volumeValue;
                    console.log('Re-synchronized audio volume on tab focus:', volumeValue);
                }
            }
        }
    });
    
    // Handle browser cache issues for multi-tab support
    document.addEventListener('visibilitychange', function() {
        if (document.visibilityState === 'visible') {
            // Force refresh the volume slider and audio state when switching tabs
            setTimeout(() => {
                const storedVolume = localStorage.getItem('preferredVolume');
                if (storedVolume && currentAudio) {
                    // Update slider visually
                    const volumeSlider = document.getElementById('volume-slider');
                    if (volumeSlider && volumeSlider.value !== storedVolume) {
                        volumeSlider.value = storedVolume;
                    }
                    
                    // Apply volume to audio
                    const volumeValue = parseFloat(storedVolume) / 100;
                    if (currentAudio.volume !== volumeValue) {
                        currentAudio.volume = volumeValue;
                        console.log('Re-synced volume on tab visibility change:', volumeValue);
                    }
                }
            }, 100);
        }
    });
}

/**
 * Setup page navigation handlers to preserve audio state
 */
function setupPageNavigationHandlers() {
    // Save state before page unload - this is critical for page transitions
    window.addEventListener('beforeunload', (e) => {
        savePlayerState();
        
        // Set a flag to indicate we're in the middle of a navigation
        sessionStorage.setItem('audioPlayer_navigating', 'true');
    });
    
    // Save state on page visibility change
    document.addEventListener('visibilitychange', () => {
        if (document.visibilityState === 'hidden') {
            savePlayerState();
            sessionStorage.setItem('audioPlayer_navigating', 'true');
        } else if (document.visibilityState === 'visible') {
            // When page becomes visible, check if we need to restore state
            const wasNavigating = sessionStorage.getItem('audioPlayer_navigating');
            if (wasNavigating) {
                sessionStorage.removeItem('audioPlayer_navigating');
                // Small delay to ensure DOM is ready
                setTimeout(() => {
                    loadPlayerState();
                }, 100);
            }
        }
    });
    
    // Handle back/forward navigation
    window.addEventListener('popstate', () => {
        // Small delay to ensure new page is loaded
        setTimeout(() => {
            loadPlayerState();
        }, 150);
    });
    
    // Enhanced link interception with better state preservation
    document.addEventListener('click', (e) => {
        const target = e.target.closest('a');
        
        // Check if it's a navigation link
        if (target && target.href && 
            !target.href.startsWith('#') && 
            !target.href.startsWith('javascript:') &&
            !target.href.startsWith('mailto:') &&
            !target.href.startsWith('tel:') &&
            target.href.startsWith(window.location.origin) &&
            !target.hasAttribute('download') &&
            !target.target) {
            
            // Save state immediately before navigation
            savePlayerState();
            sessionStorage.setItem('audioPlayer_navigating', 'true');
            
            // Add a small delay to ensure state is saved
            e.preventDefault();
            setTimeout(() => {
                window.location.href = target.href;
            }, 50);
        }
    });
    
    // Handle form submissions
    document.addEventListener('submit', (e) => {
        const form = e.target;
        
        // Save state before form submission
        savePlayerState();
        sessionStorage.setItem('audioPlayer_navigating', 'true');
    });
    
    // Enhanced AJAX interception
    const originalFetch = window.fetch;
    window.fetch = function(...args) {
        const url = args[0];
        
        // Check if this is a navigation request
        if (typeof url === 'string') {
            // Skip API calls and asset requests
            if (!url.includes('/api/') && 
                !url.includes('.json') && 
                !url.includes('.css') && 
                !url.includes('.js') && 
                !url.includes('.png') && 
                !url.includes('.jpg') && 
                !url.includes('.jpeg') && 
                !url.includes('.gif') && 
                !url.includes('.svg')) {
                
                savePlayerState();
                sessionStorage.setItem('audioPlayer_navigating', 'true');
            }
        }
        
        return originalFetch.apply(this, args);
    };
    
    // Intercept XMLHttpRequest as well
    const originalXHROpen = XMLHttpRequest.prototype.open;
    XMLHttpRequest.prototype.open = function(method, url, ...args) {
        // Save state for potential navigation requests
        if (typeof url === 'string' && 
            !url.includes('/api/') && 
            !url.includes('.json') &&
            (method.toUpperCase() === 'GET' || method.toUpperCase() === 'POST')) {
            
            savePlayerState();
            sessionStorage.setItem('audioPlayer_navigating', 'true');
        }
        
        return originalXHROpen.call(this, method, url, ...args);
    };
    
    // Handle page focus/blur for tab switching
    window.addEventListener('focus', () => {
        // When window regains focus, check if state needs restoration
        setTimeout(() => {
            const wasNavigating = sessionStorage.getItem('audioPlayer_navigating');
            if (wasNavigating) {
                sessionStorage.removeItem('audioPlayer_navigating');
                loadPlayerState();
            }
        }, 100);
    });
    
    // Save state periodically as a backup
    setInterval(() => {
        if (currentAudio && (isPlaying || currentAudio.currentTime > 0)) {
            savePlayerState();
        }
    }, 10000); // Every 10 seconds
}

/**
 * Save current audio player state to localStorage
 */
function savePlayerState() {
    try {
        if (currentTrackData) {
            localStorage.setItem(STORAGE_KEYS.CURRENT_TRACK, JSON.stringify(currentTrackData));
        }
        localStorage.setItem(STORAGE_KEYS.IS_PLAYING, isPlaying.toString());
        localStorage.setItem(STORAGE_KEYS.CURRENT_PLAYLIST, JSON.stringify(currentPlaylist));
        localStorage.setItem(STORAGE_KEYS.CURRENT_TRACK_INDEX, currentTrackIndex.toString());
        localStorage.setItem(STORAGE_KEYS.REPEAT_MODE, repeatMode);
        localStorage.setItem(STORAGE_KEYS.IS_SHUFFLED, isShuffled.toString());
        
        if (currentAudio) {
            localStorage.setItem(STORAGE_KEYS.CURRENT_TIME, currentAudio.currentTime.toString());
        }
        
        const player = document.getElementById('audio-player');
        if (player) {
            const isVisible = !player.classList.contains('hidden');
            localStorage.setItem(STORAGE_KEYS.PLAYER_VISIBLE, isVisible.toString());
        }
        
        console.log('Audio player state saved');
    } catch (error) {
        console.warn('Failed to save audio player state:', error);
    }
}

/**
 * Load audio player state from localStorage
 */
function loadPlayerState() {
    try {
        const savedTrack = localStorage.getItem(STORAGE_KEYS.CURRENT_TRACK);
        const savedIsPlaying = localStorage.getItem(STORAGE_KEYS.IS_PLAYING);
        const savedCurrentTime = localStorage.getItem(STORAGE_KEYS.CURRENT_TIME);
        const savedPlayerVisible = localStorage.getItem(STORAGE_KEYS.PLAYER_VISIBLE);
        const savedPlaylist = localStorage.getItem(STORAGE_KEYS.CURRENT_PLAYLIST);
        const savedTrackIndex = localStorage.getItem(STORAGE_KEYS.CURRENT_TRACK_INDEX);
        const savedRepeatMode = localStorage.getItem(STORAGE_KEYS.REPEAT_MODE);
        const savedIsShuffled = localStorage.getItem(STORAGE_KEYS.IS_SHUFFLED);
        
        console.log('Loading player state:', { savedTrack: !!savedTrack, savedIsPlaying, savedCurrentTime, savedPlayerVisible });
        
        // Restore playlist state
        if (savedPlaylist) {
            try {
                currentPlaylist = JSON.parse(savedPlaylist);
                currentTrackIndex = savedTrackIndex ? parseInt(savedTrackIndex) : -1;
                console.log('Restored playlist with', currentPlaylist.length, 'tracks, current index:', currentTrackIndex);
            } catch (e) {
                console.warn('Failed to restore playlist state:', e);
                currentPlaylist = [];
                currentTrackIndex = -1;
            }
        }
        
        // Restore repeat and shuffle modes
        if (savedRepeatMode) {
            repeatMode = savedRepeatMode;
            updateRepeatModeUI();
        }
        
        if (savedIsShuffled) {
            isShuffled = savedIsShuffled === 'true';
            updateShuffleModeUI();
        }
        
        if (savedTrack) {
            currentTrackData = JSON.parse(savedTrack);
            
            // Restore track info in UI
            updateTrackInfo(currentTrackData.title, currentTrackData.artist, currentTrackData.coverImage);
            
            // Show player if it was visible
            if (savedPlayerVisible === 'true') {
                showAudioPlayer();
            }
            
            // Create audio element if track data exists
            if (currentTrackData.audioUrl) {
                // Dispose of existing audio first
                if (currentAudio) {
                    currentAudio.pause();
                    currentAudio.src = '';
                    currentAudio = null;
                }
                
                currentAudio = new Audio(currentTrackData.audioUrl);
                initializeAudioVolume(currentAudio);
                
                // Restore current time
                if (savedCurrentTime) {
                    const timeToRestore = parseFloat(savedCurrentTime);
                    
                    // Set up event listener to restore time when metadata loads
                    currentAudio.addEventListener('loadedmetadata', () => {
                        if (timeToRestore > 0 && timeToRestore < currentAudio.duration) {
                            currentAudio.currentTime = timeToRestore;
                            console.log('Restored playback position to:', timeToRestore);
                        }
                        updateProgress();
                    });
                    
                    // If metadata is already loaded (cached), set time immediately
                    if (currentAudio.readyState >= 1) {
                        if (timeToRestore > 0 && timeToRestore < currentAudio.duration) {
                            currentAudio.currentTime = timeToRestore;
                        }
                        updateProgress();
                    }
                }
                
                // Setup event listeners
                setupAudioEventListeners();
                
                // Restore playing state - but don't auto-play due to browser restrictions
                // User will need to click play to resume playback
                if (savedIsPlaying === 'true') {
                    // Mark as not playing initially due to autoplay restrictions
                    isPlaying = false;
                    updatePlayPauseButton('play');
                      // Show a visual indicator that playback was paused due to page navigation - REMOVED
                } else {
                    isPlaying = false;
                    updatePlayPauseButton('play');
                }
                
                console.log('Audio player state restored successfully');
            }
        }
        
        // Clear navigation flag
        sessionStorage.removeItem('audioPlayer_navigating');
        
    } catch (error) {
        console.warn('Failed to load audio player state:', error);
        sessionStorage.removeItem('audioPlayer_navigating');
    }
}

/**
 * Clear saved player state
 */
function clearPlayerState() {
    try {
        Object.values(STORAGE_KEYS).forEach(key => {
            localStorage.removeItem(key);
        });
        console.log('Audio player state cleared');
    } catch (error) {
        console.warn('Failed to clear audio player state:', error);
    }
}

/**
 * Setup audio event listeners
 */
function setupAudioEventListeners() {
    if (!currentAudio) return;
    
    currentAudio.addEventListener('loadedmetadata', () => {
        updateProgress();
    });
    
    currentAudio.addEventListener('timeupdate', () => {
        updateProgress();
        // Periodically save current time
        if (Math.floor(currentAudio.currentTime) % 5 === 0) {
            savePlayerState();
        }
    });
      currentAudio.addEventListener('ended', () => {
        isPlaying = false;
        updatePlayPauseButton('play');
        
        // Try to play next track automatically
        if (!playNextTrack()) {
            console.log('Playlist ended or no next track available');
        }
        
        savePlayerState();
    });
    
    currentAudio.addEventListener('pause', () => {
        isPlaying = false;
        updatePlayPauseButton('play');
        savePlayerState();
    });
    
    currentAudio.addEventListener('play', () => {
        isPlaying = true;
        updatePlayPauseButton('pause');
        savePlayerState();
    });
}

/**
 * Show audio player with enhanced persistence
 */
function showAudioPlayer() {
    const player = document.getElementById('audio-player');
    if (player) {
        // Ensure player is properly configured for persistence
        player.style.display = 'block';
        player.style.position = 'fixed';
        player.style.bottom = '0';
        player.style.left = '0';
        player.style.right = '0';
        player.style.zIndex = '40';
        
        player.classList.remove('hidden');
        player.classList.remove('translate-y-full');
        
        // Animate in
        setTimeout(() => {
            player.style.transform = 'translateY(0)';
            player.style.opacity = '1';
        }, 10);
        
        // Save visibility state
        localStorage.setItem(STORAGE_KEYS.PLAYER_VISIBLE, 'true');
        
        console.log('Audio player shown and positioned for persistence');
    }
}

/**
 * Update repeat mode UI
 */
function updateRepeatModeUI() {
    const repeatIcon = document.getElementById('repeat-icon');
    const repeatButton = repeatIcon?.parentElement;
    
    if (!repeatIcon || !repeatButton) return;
    
    repeatButton.classList.remove('text-accent', 'text-white');
    
    switch (repeatMode) {
        case 'off':
            repeatButton.classList.add('text-white');
            repeatIcon.setAttribute('data-lucide', 'repeat');
            break;
        case 'all':
            repeatButton.classList.add('text-accent');
            repeatIcon.setAttribute('data-lucide', 'repeat');
            break;
        case 'one':
            repeatButton.classList.add('text-accent');
            repeatIcon.setAttribute('data-lucide', 'repeat-1');
            break;
    }
    
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
}

/**
 * Update shuffle mode UI
 */
function updateShuffleModeUI() {
    const shuffleIcon = document.getElementById('shuffle-icon');
    const shuffleButton = shuffleIcon?.parentElement;
    
    if (!shuffleIcon || !shuffleButton) return;
    
    shuffleButton.classList.remove('text-accent', 'text-white');
    
    if (isShuffled) {
        shuffleButton.classList.add('text-accent');
    } else {
        shuffleButton.classList.add('text-white');
    }
}

/**
 * Initialize the audio player
 */
function initializeAudioPlayer() {
    console.log('Initializing Audio Player...');
    
    // Check if we're in the middle of a navigation
    const wasNavigating = sessionStorage.getItem('audioPlayer_navigating');
    
    // Setup all audio player components
    setupProgressBarInteractions();
    setupVolumeControls();
    setupCrossTabSync();
    setupTabVisibilityHandlers();
    setupPageNavigationHandlers();
    
    // Load saved player state - either immediately or after a short delay if navigating
    if (wasNavigating) {
        // We're coming from a navigation, load state after DOM settles
        setTimeout(() => {
            loadPlayerState();
        }, 100);
    } else {
        // Normal page load, check for existing state
        loadPlayerState();
    }
    
    // Add global error handler for audio elements
    window.addEventListener('error', (e) => {
        if (e.target && e.target.tagName === 'AUDIO') {
            console.warn('Audio error detected:', e.error);
            // Attempt to recover by reloading state
            setTimeout(() => {
                loadPlayerState();
            }, 1000);
        }
    }, true);
    
    console.log('Audio Player initialized successfully');
}

/**
 * Get current playing track data
 * @returns {object|null} Current track data or null
 */
function getCurrentTrack() {
    return currentTrackData;
}

/**
 * Check if audio is currently playing
 * @returns {boolean} True if playing, false otherwise
 */
function isAudioPlaying() {
    return isPlaying;
}

/**
 * Get current audio element
 * @returns {HTMLAudioElement|null} Current audio element or null
 */
function getCurrentAudio() {
    return currentAudio;
}

/**
 * Toggle shuffle mode
 */
function toggleShuffle() {
    isShuffled = !isShuffled;
    const shuffleIcon = document.getElementById('shuffle-icon');
    const shuffleButton = shuffleIcon?.parentElement;
    
    if (isShuffled) {
        shuffleButton?.classList.add('text-accent');
        shuffleButton?.classList.remove('text-white');
    } else {
        shuffleButton?.classList.add('text-white');
        shuffleButton?.classList.remove('text-accent');
    }
    
    savePlayerState();
    console.log('Shuffle mode:', isShuffled ? 'ON' : 'OFF');
}

/**
 * Toggle repeat mode (off -> all -> one -> off)
 */
function toggleRepeat() {
    const repeatIcon = document.getElementById('repeat-icon');
    const repeatButton = repeatIcon?.parentElement;
    
    switch (repeatMode) {
        case 'off':
            repeatMode = 'all';
            repeatButton?.classList.add('text-accent');
            repeatButton?.classList.remove('text-white');
            repeatIcon?.setAttribute('data-lucide', 'repeat');
            break;
        case 'all':
            repeatMode = 'one';
            repeatIcon?.setAttribute('data-lucide', 'repeat-1');
            break;
        case 'one':
            repeatMode = 'off';
            repeatButton?.classList.add('text-white');
            repeatButton?.classList.remove('text-accent');
            repeatIcon?.setAttribute('data-lucide', 'repeat');
            break;
    }
    
    // Update icons
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
    
    savePlayerState();
    console.log('Repeat mode:', repeatMode);
}

// Make functions globally accessible
window.audioPlayerPlay = playTrack;  // Different name to avoid conflicts
window.playTrack = playTrack;
window.playNextTrack = playNextTrack;
window.playPreviousTrack = playPreviousTrack;
window.toggleShuffle = toggleShuffle;
window.toggleRepeat = toggleRepeat;
window.togglePlayPause = togglePlayPause;
window.seekAudio = seekAudio;
window.hideAudioPlayer = hideAudioPlayer;
window.updateVolume = updateVolume;
window.initializeAudioPlayer = initializeAudioPlayer;
window.getCurrentTrack = getCurrentTrack;
window.isAudioPlaying = isAudioPlaying;
window.getCurrentAudio = getCurrentAudio;
window.toggleShuffle = toggleShuffle;
window.toggleRepeat = toggleRepeat;

// Auto-initialize when DOM is loaded with enhanced state management
document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM Content Loaded - Initializing Audio Player');
    initializeAudioPlayer();
});

// Also initialize on window load as a fallback
window.addEventListener('load', function() {
    // Only re-initialize if not already done
    if (!window.audioPlayerInitialized) {
        console.log('Window Load - Initializing Audio Player as fallback');
        initializeAudioPlayer();
    }
});

// Add page show event for back/forward navigation
window.addEventListener('pageshow', function(event) {
    console.log('Page show event - checking audio player state');
    
    // If page was restored from cache (back/forward navigation)
    if (event.persisted) {
        console.log('Page restored from cache - reloading audio player state');
        setTimeout(() => {
            loadPlayerState();
        }, 100);
    }
});

// Mark as initialized
window.audioPlayerInitialized = true;

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        playTrack,
        togglePlayPause,
        seekAudio,
        hideAudioPlayer,
        updateVolume,
        initializeAudioPlayer,
        getCurrentTrack,
        isAudioPlaying,
        getCurrentAudio
    };
}
