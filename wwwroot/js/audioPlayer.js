/**
 * Audio Player Module
 * Handles all audio playback functionality for the music platform.
 * Works with PJAX navigation (layout.js) so audio continues across page transitions.
 */

// ─── State ────────────────────────────────────────────────────────────────────
let currentAudio = null;
let isPlaying = false;
let currentTrackData = null;

let currentPlaylist = [];
let currentTrackIndex = -1;
let isShuffled = false;
let repeatMode = 'off'; // 'off' | 'one' | 'all'

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

// ─── Core playback ────────────────────────────────────────────────────────────

function playTrack(trackId, title, artist, audioUrl, coverImage, playlist = null, trackIndex = -1) {
    const safeTitle = (title && title.trim()) || 'Unknown Track';
    const safeArtist = (artist && artist.trim()) || 'Unknown Artist';

    currentTrackData = { trackId, title: safeTitle, artist: safeArtist, audioUrl, coverImage };

    if (playlist && Array.isArray(playlist)) {
        currentPlaylist = playlist;
        currentTrackIndex = trackIndex >= 0 ? trackIndex : currentPlaylist.findIndex(t => t.trackId === trackId);
    }

    if (currentAudio) {
        currentAudio.pause();
        currentAudio.src = '';
        currentAudio = null;
    }

    showAudioPlayer();
    updateTrackInfo(safeTitle, safeArtist, coverImage);

    if (audioUrl) {
        currentAudio = new Audio(audioUrl);
        initializeAudioVolume(currentAudio);
        setupAudioEventListeners();

        currentAudio.play().then(() => {
            isPlaying = true;
            updatePlayPauseButton('pause');
            savePlayerState();
        }).catch(err => {
            console.warn('Autoplay blocked:', err);
            isPlaying = false;
            updatePlayPauseButton('play');
        });
    }

    savePlayerState();
}

function playNextTrack() {
    if (!currentPlaylist.length) return false;

    let nextIndex;
    if (repeatMode === 'one') {
        nextIndex = currentTrackIndex;
    } else if (isShuffled) {
        nextIndex = Math.floor(Math.random() * currentPlaylist.length);
    } else {
        nextIndex = currentTrackIndex + 1;
        if (nextIndex >= currentPlaylist.length) {
            if (repeatMode === 'all') nextIndex = 0;
            else return false;
        }
    }

    const track = currentPlaylist[nextIndex];
    if (track) {
        playTrack(track.trackId, track.title, track.artist, track.audioUrl, track.coverImage, currentPlaylist, nextIndex);
        return true;
    }
    return false;
}

function playPreviousTrack() {
    if (!currentPlaylist.length) return false;

    let prevIndex;
    if (isShuffled) {
        prevIndex = Math.floor(Math.random() * currentPlaylist.length);
    } else {
        prevIndex = currentTrackIndex - 1;
        if (prevIndex < 0) {
            if (repeatMode === 'all') prevIndex = currentPlaylist.length - 1;
            else return false;
        }
    }

    const track = currentPlaylist[prevIndex];
    if (track) {
        playTrack(track.trackId, track.title, track.artist, track.audioUrl, track.coverImage, currentPlaylist, prevIndex);
        return true;
    }
    return false;
}

function togglePlayPause() {
    if (!currentAudio) return;
    if (isPlaying) {
        currentAudio.pause();
    } else {
        currentAudio.play().catch(err => {
            console.warn('Play failed:', err);
            isPlaying = false;
            updatePlayPauseButton('play');
        });
    }
}

function toggleShuffle() {
    isShuffled = !isShuffled;
    updateShuffleModeUI();
    savePlayerState();
}

function toggleRepeat() {
    const modes = ['off', 'all', 'one'];
    repeatMode = modes[(modes.indexOf(repeatMode) + 1) % modes.length];
    updateRepeatModeUI();
    savePlayerState();
}

// ─── UI Updates ───────────────────────────────────────────────────────────────

function updateTrackInfo(title, artist, coverImage) {
    const safeTitle = (title && title.trim()) || 'Unknown Track';
    const safeArtist = (artist && artist.trim()) || 'Unknown Artist';

    const titleEl = document.getElementById('track-title');
    const artistEl = document.getElementById('track-artist');
    const thumbEl = document.getElementById('track-thumbnail');
    const placeholderEl = document.getElementById('track-cover-placeholder');

    if (titleEl) titleEl.textContent = safeTitle;
    if (artistEl) artistEl.textContent = safeArtist;

    if (thumbEl) {
        if (coverImage) {
            thumbEl.onload = () => thumbEl.classList.remove('opacity-0');
            thumbEl.onerror = () => {
                thumbEl.classList.add('opacity-0');
                if (placeholderEl) placeholderEl.style.display = '';
            };
            thumbEl.src = coverImage;
            if (placeholderEl) placeholderEl.style.display = 'none';
        } else {
            thumbEl.classList.add('opacity-0');
            if (placeholderEl) placeholderEl.style.display = '';
        }
    }
}

function updatePlayPauseButton(state) {
    const icon = document.getElementById('play-pause-icon');
    if (!icon) return;
    icon.setAttribute('data-lucide', state);
    if (typeof lucide !== 'undefined') lucide.createIcons();
}

function updateProgress() {
    if (!currentAudio) return;
    const current = currentAudio.currentTime;
    const duration = currentAudio.duration;

    const fill = document.getElementById('progress-fill');
    const thumb = document.getElementById('progress-thumb');
    const timeEl = document.getElementById('current-time');
    const durEl = document.getElementById('duration');

    if (fill && duration > 0) {
        const pct = (current / duration) * 100;
        fill.style.width = pct + '%';
        if (thumb) thumb.style.left = pct + '%';
    }
    if (timeEl) timeEl.textContent = formatTime(current);
    if (durEl) durEl.textContent = formatTime(isNaN(duration) ? 0 : duration);
}

function updateRepeatModeUI() {
    const icon = document.getElementById('repeat-icon');
    const btn = icon?.parentElement;
    if (!icon || !btn) return;
    btn.classList.toggle('text-accent', repeatMode !== 'off');
    btn.classList.toggle('text-gray-400', repeatMode === 'off');
    icon.setAttribute('data-lucide', repeatMode === 'one' ? 'repeat-1' : 'repeat');
    if (typeof lucide !== 'undefined') lucide.createIcons();
}

function updateShuffleModeUI() {
    const icon = document.getElementById('shuffle-icon');
    const btn = icon?.parentElement;
    if (!icon || !btn) return;
    btn.classList.toggle('text-accent', isShuffled);
    btn.classList.toggle('text-gray-400', !isShuffled);
}

function updateVolumeIcon(value) {
    const btn = document.getElementById('volume-button');
    if (!btn) return;
    const iconEl = btn.querySelector('i[data-lucide]');
    if (!iconEl) return;
    const vol = parseFloat(value);
    iconEl.setAttribute('data-lucide', vol === 0 ? 'volume-x' : vol < 50 ? 'volume-1' : 'volume-2');
    if (typeof lucide !== 'undefined') lucide.createIcons();
}

// ─── Player visibility ────────────────────────────────────────────────────────

function showAudioPlayer() {
    const player = document.getElementById('audio-player');
    if (!player) return;
    player.classList.add('visible');
    document.body.classList.add('player-visible');
    localStorage.setItem(STORAGE_KEYS.PLAYER_VISIBLE, 'true');
    if (typeof lucide !== 'undefined') lucide.createIcons();
}

function hideAudioPlayer() {
    const player = document.getElementById('audio-player');
    if (player) player.classList.remove('visible');
    document.body.classList.remove('player-visible');

    if (currentAudio) {
        currentAudio.pause();
        currentAudio.src = '';
        currentAudio = null;
    }
    isPlaying = false;
    currentTrackData = null;
    clearPlayerState();
}

// ─── Volume ───────────────────────────────────────────────────────────────────

function initializeAudioVolume(audioEl) {
    if (!audioEl) return;
    const stored = localStorage.getItem('preferredVolume');
    const slider = document.getElementById('volume-slider');
    const val = stored ? parseFloat(stored) : (slider ? parseFloat(slider.value) : 75);
    audioEl.volume = Math.min(Math.max(val / 100, 0), 1);
    if (slider) slider.value = val;
}

function updateVolume(value) {
    const numVal = parseFloat(value);
    const vol = Math.min(Math.max(numVal / 100, 0), 1);
    if (currentAudio) currentAudio.volume = vol;

    const slider = document.getElementById('volume-slider');
    if (slider && parseFloat(slider.value) !== numVal) slider.value = numVal;

    localStorage.setItem('preferredVolume', numVal.toString());
    updateVolumeIcon(numVal);
}

// ─── Progress bar ─────────────────────────────────────────────────────────────

function seekAudio(event) {
    if (!currentAudio || !currentAudio.duration) return;
    const bar = event.currentTarget || document.getElementById('progress-bar');
    if (!bar) return;
    const rect = bar.getBoundingClientRect();
    const pct = Math.max(0, Math.min(1, (event.clientX - rect.left) / rect.width));
    currentAudio.currentTime = currentAudio.duration * pct;
    updateProgress();
}

function formatTime(secs) {
    if (isNaN(secs) || secs < 0) return '0:00';
    return `${Math.floor(secs / 60)}:${Math.floor(secs % 60).toString().padStart(2, '0')}`;
}

// ─── Audio event listeners ────────────────────────────────────────────────────

function setupAudioEventListeners() {
    if (!currentAudio) return;

    currentAudio.addEventListener('timeupdate', () => {
        updateProgress();
        if (Math.floor(currentAudio.currentTime) % 5 === 0 && isPlaying) {
            localStorage.setItem(STORAGE_KEYS.CURRENT_TIME, currentAudio.currentTime.toString());
        }
    });

    currentAudio.addEventListener('loadedmetadata', updateProgress);

    currentAudio.addEventListener('ended', () => {
        isPlaying = false;
        updatePlayPauseButton('play');
        if (!playNextTrack()) savePlayerState();
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

    currentAudio.addEventListener('error', () => {
        isPlaying = false;
        updatePlayPauseButton('play');
    });
}

// ─── Setup ────────────────────────────────────────────────────────────────────

function setupVolumeControls() {
    const slider = document.getElementById('volume-slider');
    const btn = document.getElementById('volume-button');

    if (slider) {
        const stored = localStorage.getItem('preferredVolume');
        if (stored) slider.value = stored;
        slider.addEventListener('input', function() { updateVolume(this.value); });
        updateVolumeIcon(slider.value);
    }

    if (btn) {
        btn.addEventListener('click', () => {
            const s = document.getElementById('volume-slider');
            if (!s) return;
            if (parseFloat(s.value) > 0) {
                btn._prevVol = s.value;
                s.value = '0';
            } else {
                s.value = btn._prevVol || '75';
            }
            updateVolume(s.value);
        });
    }
}

function setupProgressBarInteractions() {
    const bar = document.getElementById('progress-bar');
    if (!bar) return;

    let dragging = false;

    bar.addEventListener('mousedown', (e) => {
        dragging = true;
        e.preventDefault();
        if (currentAudio && currentAudio.duration) {
            const rect = bar.getBoundingClientRect();
            const pct = Math.max(0, Math.min(1, (e.clientX - rect.left) / rect.width));
            currentAudio.currentTime = currentAudio.duration * pct;
            updateProgress();
        }
    });

    document.addEventListener('mousemove', (e) => {
        if (!dragging) return;
        const b = document.getElementById('progress-bar');
        if (!b || !currentAudio || !currentAudio.duration) return;
        const rect = b.getBoundingClientRect();
        const pct = Math.max(0, Math.min(1, (e.clientX - rect.left) / rect.width));
        currentAudio.currentTime = currentAudio.duration * pct;
        updateProgress();
    });

    document.addEventListener('mouseup', () => { dragging = false; });
}

function setupCrossTabSync() {
    window.addEventListener('storage', (e) => {
        if (e.key === 'preferredVolume' && e.newValue) {
            const slider = document.getElementById('volume-slider');
            if (slider) slider.value = e.newValue;
            if (currentAudio) currentAudio.volume = parseFloat(e.newValue) / 100;
            updateVolumeIcon(e.newValue);
        }
    });
}

// ─── State persistence ────────────────────────────────────────────────────────

function savePlayerState() {
    try {
        if (currentTrackData) localStorage.setItem(STORAGE_KEYS.CURRENT_TRACK, JSON.stringify(currentTrackData));
        localStorage.setItem(STORAGE_KEYS.IS_PLAYING, isPlaying.toString());
        localStorage.setItem(STORAGE_KEYS.CURRENT_PLAYLIST, JSON.stringify(currentPlaylist));
        localStorage.setItem(STORAGE_KEYS.CURRENT_TRACK_INDEX, currentTrackIndex.toString());
        localStorage.setItem(STORAGE_KEYS.REPEAT_MODE, repeatMode);
        localStorage.setItem(STORAGE_KEYS.IS_SHUFFLED, isShuffled.toString());
        if (currentAudio) localStorage.setItem(STORAGE_KEYS.CURRENT_TIME, currentAudio.currentTime.toString());
        const player = document.getElementById('audio-player');
        localStorage.setItem(STORAGE_KEYS.PLAYER_VISIBLE, player?.classList.contains('visible') ? 'true' : 'false');
    } catch (e) {
        console.warn('savePlayerState failed:', e);
    }
}

function loadPlayerState() {
    try {
        const savedTrack = localStorage.getItem(STORAGE_KEYS.CURRENT_TRACK);
        const savedIsPlaying = localStorage.getItem(STORAGE_KEYS.IS_PLAYING);
        const savedTime = localStorage.getItem(STORAGE_KEYS.CURRENT_TIME);
        const savedVisible = localStorage.getItem(STORAGE_KEYS.PLAYER_VISIBLE);
        const savedPlaylist = localStorage.getItem(STORAGE_KEYS.CURRENT_PLAYLIST);
        const savedIndex = localStorage.getItem(STORAGE_KEYS.CURRENT_TRACK_INDEX);
        const savedRepeat = localStorage.getItem(STORAGE_KEYS.REPEAT_MODE);
        const savedShuffle = localStorage.getItem(STORAGE_KEYS.IS_SHUFFLED);

        if (savedPlaylist) {
            try { currentPlaylist = JSON.parse(savedPlaylist); } catch { currentPlaylist = []; }
            currentTrackIndex = savedIndex ? parseInt(savedIndex) : -1;
        }
        if (savedRepeat) { repeatMode = savedRepeat; updateRepeatModeUI(); }
        if (savedShuffle) { isShuffled = savedShuffle === 'true'; updateShuffleModeUI(); }

        if (!savedTrack) return;

        currentTrackData = JSON.parse(savedTrack);
        updateTrackInfo(currentTrackData.title, currentTrackData.artist, currentTrackData.coverImage);

        if (savedVisible === 'true') showAudioPlayer();

        if (!currentTrackData.audioUrl) return;

        if (currentAudio) { currentAudio.pause(); currentAudio.src = ''; currentAudio = null; }

        currentAudio = new Audio(currentTrackData.audioUrl);
        initializeAudioVolume(currentAudio);
        setupAudioEventListeners();

        const restoreTime = savedTime ? parseFloat(savedTime) : 0;
        const wasPlaying = savedIsPlaying === 'true';

        const onReady = () => {
            if (restoreTime > 0 && restoreTime < currentAudio.duration) {
                currentAudio.currentTime = restoreTime;
            }
            updateProgress();

            if (wasPlaying) {
                // Try auto-resume (works if user has previously interacted)
                currentAudio.play().then(() => {
                    isPlaying = true;
                    updatePlayPauseButton('pause');
                }).catch(() => {
                    // Autoplay blocked — show paused; user can click play to resume
                    isPlaying = false;
                    updatePlayPauseButton('play');
                });
            } else {
                isPlaying = false;
                updatePlayPauseButton('play');
            }
        };

        if (currentAudio.readyState >= 1) {
            onReady();
        } else {
            currentAudio.addEventListener('loadedmetadata', onReady, { once: true });
        }

    } catch (e) {
        console.warn('loadPlayerState failed:', e);
    }
}

function clearPlayerState() {
    Object.values(STORAGE_KEYS).forEach(k => localStorage.removeItem(k));
}

// ─── Init ─────────────────────────────────────────────────────────────────────

function initializeAudioPlayer() {
    setupProgressBarInteractions();
    setupVolumeControls();
    setupCrossTabSync();

    // Persist state on unload (fallback for non-PJAX full page navigations)
    window.addEventListener('beforeunload', savePlayerState);

    loadPlayerState();
}

// ─── Global exports ───────────────────────────────────────────────────────────

window.playTrack = playTrack;
window.audioPlayerPlay = playTrack;
window.playNextTrack = playNextTrack;
window.playPreviousTrack = playPreviousTrack;
window.toggleShuffle = toggleShuffle;
window.toggleRepeat = toggleRepeat;
window.togglePlayPause = togglePlayPause;
window.seekAudio = seekAudio;
window.hideAudioPlayer = hideAudioPlayer;
window.updateVolume = updateVolume;
window.initializeAudioPlayer = initializeAudioPlayer;
window.savePlayerState = savePlayerState;
window.loadPlayerState = loadPlayerState;
window.getCurrentTrack = () => currentTrackData;
window.isAudioPlaying = () => isPlaying;
window.getCurrentAudio = () => currentAudio;

document.addEventListener('DOMContentLoaded', () => {
    initializeAudioPlayer();
    window.audioPlayerInitialized = true;
});

// Restore state after bfcache (back/forward navigation)
window.addEventListener('pageshow', (e) => {
    if (e.persisted) setTimeout(loadPlayerState, 100);
});

if (typeof module !== 'undefined' && module.exports) {
    module.exports = { playTrack, togglePlayPause, seekAudio, hideAudioPlayer, updateVolume };
}
