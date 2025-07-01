// Playlist Details JavaScript Functions
document.addEventListener("DOMContentLoaded", function () {
    // Initialize Lucide icons
    if (typeof lucide !== "undefined") {
        lucide.createIcons();
    }

    // Check if audio player is ready
    console.log('Playlist Details - DOM loaded, checking audio player readiness...');
    console.log('window.playTrack available:', typeof window.playTrack === 'function');
    console.log('window.audioPlayerInitialized:', window.audioPlayerInitialized);

    // Get playlist data from the page
    window.playlistData = {
        id: document.querySelector('[data-playlist-id]')?.getAttribute('data-playlist-id') || '',
        name: document.querySelector('[data-playlist-name]')?.getAttribute('data-playlist-name') || ''
    };

    // Set up event listeners
    document.addEventListener("click", function (event) {
        const dropdowns = document.querySelectorAll('[id$="-menu"]');
        dropdowns.forEach((dropdown) => {
            if (
                !dropdown.contains(event.target) &&
                !event.target.closest('[onclick*="toggleDropdown"]')
            ) {
                dropdown.classList.add("hidden");
            }
        });
    });

    // Setup search functionality
    const searchInput = document.getElementById("trackSearch");
    if (searchInput) {
        let searchTimeout;
        searchInput.addEventListener("input", function () {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                searchTracks(this.value);
            }, 300);
        });
    }
});

// Enhanced play functionality
function playPlaylist() {
    console.log("Playing playlist:", window.playlistData.id);
      // Get all tracks from the playlist
    const trackRows = document.querySelectorAll('tbody tr[data-track-id]');
    const playlistTracks = Array.from(trackRows).map((row, index) => {
        // Get title
        const title = row.querySelector('.track-title')?.textContent?.trim() || 'Unknown Title';
        
        // Try to get artist from multiple possible locations
        let artist = 'Unknown Artist';
        const artistElements = row.querySelectorAll('.track-artist');
        if (artistElements.length > 0) {
            // Use the first non-empty artist element
            for (const element of artistElements) {
                const artistText = element.textContent?.trim();
                if (artistText && artistText !== '') {
                    artist = artistText;
                    break;
                }
            }
        }
        
        return {
            trackId: row.getAttribute('data-track-id'),
            title: title,
            artist: artist,
            audioUrl: row.getAttribute('data-audio-url') || '',
            coverImage: row.getAttribute('data-cover-image') || ''
        };
    });
    
    if (playlistTracks.length > 0) {
        const firstTrack = playlistTracks[0];
        playTrackWithPlaylist(firstTrack.trackId, firstTrack.title, firstTrack.artist, firstTrack.audioUrl, firstTrack.coverImage, playlistTracks, 0);
        showNotification("Playing playlist: " + window.playlistData.name, "success");
    } else {
        showNotification("No tracks found in playlist", "error");
    }
}

// Play individual track with playlist context
function playTrack(trackId) {
    console.log("Playing track:", trackId);
      // Get all tracks from the playlist for context
    const trackRows = document.querySelectorAll('tbody tr[data-track-id]');
    const playlistTracks = Array.from(trackRows).map(row => {
        // Get title
        const title = row.querySelector('.track-title')?.textContent?.trim() || 'Unknown Title';
        
        // Try to get artist from multiple possible locations
        let artist = 'Unknown Artist';
        const artistElements = row.querySelectorAll('.track-artist');
        if (artistElements.length > 0) {
            // Use the first non-empty artist element
            for (const element of artistElements) {
                const artistText = element.textContent?.trim();
                if (artistText && artistText !== '') {
                    artist = artistText;
                    break;
                }
            }
        }
        
        return {
            trackId: row.getAttribute('data-track-id'),
            title: title,
            artist: artist,
            audioUrl: row.getAttribute('data-audio-url') || '',
            coverImage: row.getAttribute('data-cover-image') || ''
        };
    });
    
    // Find the index of the clicked track
    const trackIndex = playlistTracks.findIndex(track => track.trackId === trackId);
    
    // Wait for audio player to be ready
    function attemptPlay() {
        // Get track data from the DOM
        const trackRow = document.querySelector(`tr[data-track-id="${trackId}"]`);
        if (trackRow) {            // Extract track info from DOM - try multiple methods to get artist
            const title = trackRow.querySelector('.track-title')?.textContent?.trim() || 'Unknown Title';
            
            // Try to get artist from multiple possible locations
            let artist = 'Unknown Artist';
            const artistElements = trackRow.querySelectorAll('.track-artist');
            if (artistElements.length > 0) {
                // Use the first non-empty artist element
                for (const element of artistElements) {
                    const artistText = element.textContent?.trim();
                    if (artistText && artistText !== '') {
                        artist = artistText;
                        break;
                    }
                }
            }
            
            const audioUrl = trackRow.getAttribute('data-audio-url') || '';
            const coverImage = trackRow.getAttribute('data-cover-image') || '';            console.log('Playing track:', { title, artist });
            console.log('Artist info from DOM:', {
                'track-artist elements': trackRow.querySelectorAll('.track-artist'),
                'first track-artist text': trackRow.querySelector('.track-artist')?.textContent,
                'all artist texts': Array.from(trackRow.querySelectorAll('.track-artist')).map(el => el.textContent)
            });
            console.log('Checking global audio player functions...');
            console.log('window.playTrack type:', typeof window.playTrack);
            console.log('window.audioPlayerInitialized:', window.audioPlayerInitialized);
            
            // Call global audio player function with playlist context
            if (typeof window.audioPlayerPlay === 'function') {
                console.log('Calling global audioPlayerPlay function with playlist...');
                window.audioPlayerPlay(trackId, title, artist, audioUrl, coverImage, playlistTracks, trackIndex);
                showNotification(`Now playing: ${title}`, "success");
                return true; // Success
            } else if (typeof window.playTrack === 'function' && window.playTrack !== playTrack) {
                console.log('Calling global playTrack function with playlist...');
                window.playTrack(trackId, title, artist, audioUrl, coverImage, playlistTracks, trackIndex);
                showNotification(`Now playing: ${title}`, "success");
                return true; // Success
            } else {
                console.warn('Global audio player function not found, retrying...');
                console.warn('window.audioPlayerPlay type:', typeof window.audioPlayerPlay);
                console.warn('window.playTrack type:', typeof window.playTrack);
                return false; // Retry needed
            }
        } else {
            console.warn('Track row not found in DOM');
            return false;
        }
    }
    
    // Try immediately first
    if (attemptPlay()) {
        return;
    }
    
    // If failed, wait for audio player to load
    let retryCount = 0;
    const maxRetries = 10;
    const retryInterval = 200; // 200ms
    
    const retryTimer = setInterval(() => {
        retryCount++;
        console.log(`Retry ${retryCount}/${maxRetries} - Waiting for audio player...`);
        
        if (attemptPlay()) {
            clearInterval(retryTimer);
            return;
        }
        
        if (retryCount >= maxRetries) {
            clearInterval(retryTimer);
            console.error('Audio player not available after retries');
            
            // Try API fallback as last resort
            console.warn('Trying API fallback...');
            fetch(`/api/track/${trackId}`)
                .then(response => response.json())
                .then(data => {
                    if (data.success && data.track) {
                        const track = data.track;
                        const audioUrl = track.audioFileUrl?.startsWith('/') ? track.audioFileUrl : `/${track.audioFileUrl}`;
                        const coverImage = track.coverImageUrl?.startsWith('/') ? track.coverImageUrl : `/${track.coverImageUrl}`;                        if (typeof window.audioPlayerPlay === 'function') {
                            window.audioPlayerPlay(trackId, track.title, track.artistName, audioUrl, coverImage, playlistTracks, trackIndex);
                            showNotification(`Now playing: ${track.title}`, "success");
                        } else if (typeof window.playTrack === 'function') {
                            window.playTrack(trackId, track.title, track.artistName, audioUrl, coverImage, playlistTracks, trackIndex);
                            showNotification(`Now playing: ${track.title}`, "success");
                        } else {
                            showNotification("Audio player not available", "error");
                        }
                    } else {
                        showNotification("Track not found", "error");
                    }
                })
                .catch(error => {
                    console.error('Error fetching track:', error);
                    showNotification("Error loading track", "error");
                });
        }
    }, retryInterval);
}

// Helper function to play track with playlist context
function playTrackWithPlaylist(trackId, title, artist, audioUrl, coverImage, playlist, trackIndex) {
    if (typeof window.audioPlayerPlay === 'function') {
        window.audioPlayerPlay(trackId, title, artist, audioUrl, coverImage, playlist, trackIndex);
    } else if (typeof window.playTrack === 'function') {
        window.playTrack(trackId, title, artist, audioUrl, coverImage, playlist, trackIndex);
    } else {
        console.error('Audio player not available');
    }
}

// Toggle dropdown menu
function toggleDropdown(menuId) {
    const menu = document.getElementById(menuId);
    if (menu) {
        menu.classList.toggle("hidden");
    }
}

// Toggle playlist like
function toggleLike(playlistId) {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    
    fetch("/Playlist/ToggleLike", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "RequestVerificationToken": token || ""
        },
        body: JSON.stringify({ playlistId: playlistId })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            const button = document.getElementById("like-button");
            const icon = button.querySelector("i");
            const countElement = document.querySelector(".like-count");
            
            if (data.isLiked) {
                icon.classList.add("text-accent", "fill-accent");
                showNotification("Added to liked playlists", "success");
            } else {
                icon.classList.remove("text-accent", "fill-accent");
                showNotification("Removed from liked playlists", "success");
            }
            
            if (countElement) {
                countElement.textContent = data.likeCount;
            }
        } else {
            showNotification(data.message || "Error updating like status", "error");
        }
    })
    .catch(error => {
        console.error("Error:", error);
        showNotification("Error updating like status", "error");
    });
}

// Toggle track like
function toggleTrackLike(trackId) {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    
    fetch("/Track/ToggleLike", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "RequestVerificationToken": token || ""
        },
        body: JSON.stringify({ trackId: trackId })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            const button = document.querySelector(`[onclick="toggleTrackLike('${trackId}')"]`);
            const icon = button.querySelector("i");
            
            if (data.isLiked) {
                icon.classList.add("text-accent", "fill-accent");
            } else {
                icon.classList.remove("text-accent", "fill-accent");
            }
            
            showNotification(data.isLiked ? "Track liked" : "Track unliked", "success");
        } else {
            showNotification(data.message || "Error updating like status", "error");
        }
    })
    .catch(error => {
        console.error("Error:", error);
        showNotification("Error updating like status", "error");
    });
}

// Share playlist function
function sharePlaylist(playlistId, playlistName) {
    const url = window.location.origin + "/Playlist/Details/" + playlistId;
    
    if (navigator.share) {
        navigator.share({
            title: playlistName,
            text: "Check out this playlist on Eryth",
            url: url
        }).catch(err => console.log("Error sharing:", err));
    } else if (navigator.clipboard) {
        navigator.clipboard.writeText(url).then(() => {
            showNotification("Playlist link copied to clipboard", "success");
        }).catch(err => {
            console.log("Error copying to clipboard:", err);
            prompt("Copy this link:", url);
        });
    } else {
        prompt("Copy this link:", url);
    }
}

// Add Track Modal Functions
function openAddTrackModal() {
    const modal = document.getElementById("addTrackModal");
    if (modal) {
        modal.classList.remove("hidden");
        document.body.style.overflow = "hidden";
        const searchInput = document.getElementById("trackSearch");
        if (searchInput) {
            searchInput.focus();
        }
    }
}

function closeAddTrackModal() {
    const modal = document.getElementById("addTrackModal");
    if (modal) {
        modal.classList.add("hidden");
        document.body.style.overflow = "auto";
        const searchInput = document.getElementById("trackSearch");
        if (searchInput) {
            searchInput.value = "";
        }
        resetSearchResults();
    }
}

function resetSearchResults() {
    const searchResults = document.getElementById("searchResults");
    if (searchResults) {
        searchResults.innerHTML = `
            <div class="p-12 text-center text-gray-400">
                <div class="w-16 h-16 mx-auto mb-4 bg-gradient-to-br from-accent/20 to-black rounded-full flex items-center justify-center border border-accent/30">
                    <i data-lucide="search" class="w-8 h-8 text-accent"></i>
                </div>
                <p class="text-lg font-semibold mb-2 text-white">Search for tracks</p>
                <p class="text-sm text-gray-400">Type to find tracks you want to add to your playlist</p>
            </div>
        `;
        if (typeof lucide !== "undefined") {
            lucide.createIcons();
        }
    }
}

// Search tracks function
function searchTracks(query) {
    console.log('Search called with query:', query);
    
    if (!query || query.length < 2) {
        resetSearchResults();
        return;
    }

    const searchResults = document.getElementById("searchResults");
    if (searchResults) {
        searchResults.innerHTML = `
            <div class="p-12 text-center text-gray-400">
                <div class="w-8 h-8 border-2 border-accent border-t-transparent rounded-full animate-spin mx-auto mb-4"></div>
                <p class="text-white">Searching for tracks...</p>
            </div>
        `;
    }

    // Make actual API call to search tracks
    const searchUrl = `/api/search?query=${encodeURIComponent(query)}&type=tracks&pageSize=20`;
    console.log('Searching at URL:', searchUrl);
    
    fetch(searchUrl)
        .then(response => {
            console.log('Search response status:', response.status);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })        .then(data => {
            console.log('Search response data:', data);
            if (data && data.data) {
                displaySearchResults(data.data);
            } else {
                console.warn('No data property in response:', data);
                displaySearchResults([]);
            }
        })
        .catch(error => {
            console.error('Search error:', error);
            if (searchResults) {
                searchResults.innerHTML = `
                    <div class="p-12 text-center text-gray-400">
                        <div class="w-16 h-16 mx-auto mb-4 bg-red-500/20 rounded-full flex items-center justify-center border border-red-500/30">
                            <i data-lucide="alert-circle" class="w-8 h-8 text-red-400"></i>
                        </div>
                        <p class="text-lg font-semibold mb-2 text-white">Search failed</p>
                        <p class="text-sm text-gray-400">Error: ${error.message}</p>
                    </div>
                `;
                if (typeof lucide !== "undefined") {
                    lucide.createIcons();
                }
            }
        });
}

function displaySearchResults(tracks) {
    console.log('displaySearchResults called with:', tracks);
    const searchResults = document.getElementById("searchResults");
    if (!searchResults) {
        console.error('searchResults element not found');
        return;
    }

    if (!tracks || tracks.length === 0) {
        console.log('No tracks to display');
        searchResults.innerHTML = `
            <div class="p-12 text-center text-gray-400">
                <div class="w-16 h-16 mx-auto mb-4 bg-gradient-to-br from-accent/20 to-black rounded-full flex items-center justify-center border border-accent/30">
                    <i data-lucide="music-off" class="w-8 h-8 text-accent"></i>
                </div>
                <p class="text-lg font-semibold mb-2 text-white">No tracks found</p>
                <p class="text-sm text-gray-400">Try searching with different keywords</p>
            </div>
        `;
        if (typeof lucide !== "undefined") {
            lucide.createIcons();
        }
        return;
    }    console.log(`Displaying ${tracks.length} tracks`);
    const tracksHtml = tracks.map(track => {
        // Fix image URL path
        const imageUrl = track.coverImageUrl ? 
            (track.coverImageUrl.startsWith('/') ? track.coverImageUrl : `/${track.coverImageUrl}`) : 
            null;
        
        return `
        <div style="display: flex; align-items: center; justify-content: space-between; padding: 12px; margin: 8px 0; background: rgba(255,255,255,0.05); border-radius: 12px; border: 1px solid transparent; transition: all 0.3s ease;" 
             onmouseover="this.style.background='rgba(255,255,255,0.1)'; this.style.borderColor='rgba(0,255,135,0.3)'" 
             onmouseout="this.style.background='rgba(255,255,255,0.05)'; this.style.borderColor='transparent'">
            <div style="display: flex; align-items: center; gap: 16px; flex: 1; min-width: 0;">
                <div style="width: 48px; height: 48px; border-radius: 8px; background: linear-gradient(135deg, rgba(0,255,135,0.2), rgba(0,0,0,1)); display: flex; align-items: center; justify-content: center; flex-shrink: 0; border: 1px solid rgba(0,255,135,0.3); overflow: hidden;">
                    ${imageUrl ? 
                        `<img src="${imageUrl}" alt="${track.title}" style="width: 100%; height: 100%; object-fit: cover; border-radius: 8px;" onerror="this.style.display='none'; this.nextElementSibling.style.display='flex';">
                         <div style="display: none; width: 100%; height: 100%; align-items: center; justify-content: center;">
                             <i data-lucide="music" style="width: 24px; height: 24px; color: #00ff87;"></i>
                         </div>` :
                        `<i data-lucide="music" style="width: 24px; height: 24px; color: #00ff87;"></i>`
                    }
                </div>
                <div style="flex: 1; min-width: 0; display: flex; flex-direction: column; justify-content: center;">
                    <h4 style="color: white; font-weight: 500; margin: 0; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; line-height: 1.4;">${track.title}</h4>
                    <p style="color: #999; font-size: 14px; margin: 2px 0 0 0; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; line-height: 1.3;">${track.artistName}</p>
                </div>
                <div style="color: #999; font-size: 14px; flex-shrink: 0; margin-right: 12px;">
                    ${track.formattedDuration || ''}
                </div>
            </div>
            <button onclick="addTrackToPlaylist('${track.id}')" 
                    style="background: linear-gradient(135deg, #00ff87, #00cc6a); color: black; border: none; padding: 8px 16px; border-radius: 8px; font-weight: 500; cursor: pointer; transition: all 0.3s ease; display: flex; align-items: center; gap: 8px; flex-shrink: 0;"
                    onmouseover="this.style.background='linear-gradient(135deg, #00cc6a, #00ff87)'"
                    onmouseout="this.style.background='linear-gradient(135deg, #00ff87, #00cc6a)'">
                <i data-lucide="plus" style="width: 16px; height: 16px;"></i>
                Add
            </button>
        </div>
        `;
    }).join('');

    searchResults.innerHTML = `<div style="display: flex; flex-direction: column; gap: 4px;">${tracksHtml}</div>`;
    
    if (typeof lucide !== "undefined") {
        lucide.createIcons();
    }
}

function addTrackToPlaylist(trackId) {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    const playlistId = window.playlistData.id;
    
    if (!playlistId) {
        showNotification("Playlist ID not found", "error");
        return;
    }

    fetch('/Playlist/AddTrack', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token || ''
        },
        body: JSON.stringify({ 
            PlaylistId: playlistId, 
            TrackId: trackId 
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showNotification('Track added to playlist', 'success');
            // Optionally reload the page to show the updated playlist
            setTimeout(() => {
                window.location.reload();
            }, 1000);
        } else {
            showNotification(data.message || 'Error adding track to playlist', 'error');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showNotification('Error adding track to playlist', 'error');
    });
}

function deletePlaylist(playlistId, playlistName) {
    if (confirm(`Are you sure you want to delete "${playlistName}"? This action cannot be undone.`)) {
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        
        // Create form data for proper anti-forgery token handling
        const formData = new FormData();
        formData.append('id', playlistId);
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }
        
        fetch('/Playlist/Delete', {
            method: 'POST',
            body: formData
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showNotification('Playlist deleted successfully', 'success');
                setTimeout(() => {
                    window.location.href = '/Playlist/MyPlaylists';
                }, 1000);
            } else {
                showNotification(data.message || 'Error deleting playlist', 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showNotification('Error deleting playlist', 'error');
        });
    }
}

function removeTrack(playlistId, trackId, trackName) {
    if (confirm(`Remove "${trackName}" from this playlist?`)) {
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        
        fetch('/Playlist/RemoveTrack', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token || ''
            },
            body: JSON.stringify({ 
                PlaylistId: playlistId, 
                TrackId: trackId 
            })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showNotification('Track removed from playlist', 'success');
                // Remove the track from the UI
                const trackElement = document.querySelector(`[data-track-id="${trackId}"]`);
                if (trackElement) {
                    trackElement.remove();
                }
            } else {
                showNotification(data.message || 'Error removing track', 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showNotification('Error removing track', 'error');
        });
    }
}

function duplicatePlaylist(playlistId) {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    
    fetch('/Playlist/Duplicate', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token || ''
        },
        body: JSON.stringify({ id: playlistId })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showNotification('Playlist duplicated successfully', 'success');
            if (data.newPlaylistId) {
                setTimeout(() => {
                    window.location.href = `/Playlist/Details/${data.newPlaylistId}`;
                }, 1000);
            }
        } else {
            showNotification(data.message || 'Error duplicating playlist', 'error');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showNotification('Error duplicating playlist', 'error');
    });
}

function addToQueue(playlistId) {
    // TODO: Implement add to queue functionality
    showNotification('Playlist added to queue', 'success');
}

// Notification helper function
function showNotification(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `fixed top-4 right-4 z-50 p-4 rounded-lg shadow-lg transition-all duration-300 transform translate-x-full`;
    
    // Set colors based on type
    switch (type) {
        case 'success':
            notification.className += ' bg-green-500 text-white';
            break;
        case 'error':
            notification.className += ' bg-red-500 text-white';
            break;
        case 'warning':
            notification.className += ' bg-yellow-500 text-black';
            break;
        default:
            notification.className += ' bg-gray-800 text-white';
    }
    
    notification.innerHTML = `
        <div class="flex items-center space-x-2">
            <span>${message}</span>
            <button onclick="this.parentElement.parentElement.remove()" class="text-white hover:text-gray-300">
                <i data-lucide="x" class="w-4 h-4"></i>
            </button>
        </div>
    `;
    
    document.body.appendChild(notification);
    
    if (typeof lucide !== "undefined") {
        lucide.createIcons();
    }
    
    // Show notification
    setTimeout(() => {
        notification.classList.remove('translate-x-full');
    }, 100);
    
    // Auto remove after 5 seconds
    setTimeout(() => {
        notification.classList.add('translate-x-full');
        setTimeout(() => {
            notification.remove();
        }, 300);
    }, 5000);
}
