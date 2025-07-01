/**
 * Unified Follow Toggle Module
 * Handles follow/unfollow functionality for users, artists, and playlists
 */

(function() {
    'use strict';

    // Configuration
    const CONFIG = {
        selectors: {
            followButton: '.js-follow-toggle',
            followIcon: '.js-follow-icon',
            followText: '.js-follow-text',
            followCount: '.js-follow-count'
        },
        classes: {
            followed: 'is-followed',
            loading: 'is-loading'
        },
        endpoints: {
            user: '/api/user/{id}/follow',
            artist: '/api/artists/{id}/follow',
            playlist: '/api/playlists/{id}/follow'
        },
        icons: {
            user: {
                follow: 'user-plus',
                following: 'user-check'
            },
            artist: {
                follow: 'user-plus',
                following: 'user-check'
            },
            playlist: {
                follow: 'heart',
                following: 'heart'
            }
        },
        text: {
            user: {
                follow: 'Follow',
                following: 'Following'
            },
            artist: {
                follow: 'Follow',
                following: 'Following'
            },
            playlist: {
                follow: 'Follow',
                following: 'Following'
            }
        },
        styles: {
            followed: 'bg-white/10 text-white hover:bg-red-500/20 hover:text-red-400 border border-white/20 hover:border-red-400/50',
            notFollowed: 'bg-gradient-to-r from-accent to-accent-dark text-white hover:from-accent-dark hover:to-accent'
        }
    };

    // State management
    let isInitialized = false;

    /**
     * Initialize the follow toggle functionality
     */
    function init() {
        if (isInitialized) return;
        
        document.addEventListener('click', handleFollowClick);
        isInitialized = true;
        
        console.log('Follow toggle module initialized');
    }

    /**
     * Handle follow button clicks
     */
    function handleFollowClick(event) {
        const button = event.target.closest(CONFIG.selectors.followButton);
        if (!button) return;

        event.preventDefault();
        event.stopPropagation();

        // Prevent double-clicks
        if (button.classList.contains(CONFIG.classes.loading)) {
            return;
        }

        const entityId = button.getAttribute('data-entity-id');
        const entityType = button.getAttribute('data-entity-type');

        if (!entityId || !entityType) {
            console.error('Follow button missing required data attributes');
            return;
        }

        toggleFollow(button, entityId, entityType);
    }

    /**
     * Toggle follow status for an entity
     */
    async function toggleFollow(button, entityId, entityType) {
        const isCurrentlyFollowed = button.classList.contains(CONFIG.classes.followed);
        
        // Set loading state
        setButtonLoading(button, true);

        try {
            const result = await makeFollowRequest(entityId, entityType, isCurrentlyFollowed);
            
            if (result.success) {
                updateButtonState(button, entityType, result.following, result.followerCount);
                showNotification(
                    result.following ? 
                    `Now following!` : 
                    `Unfollowed`, 
                    'success'
                );
            } else {
                throw new Error(result.message || 'Unknown error');
            }
        } catch (error) {
            console.error('Follow toggle error:', error);
            
            if (error.status === 401) {
                // Redirect to login
                showNotification('Please log in to follow', 'info');
                setTimeout(() => {
                    window.location.href = '/Account/Login';
                }, 1500);
            } else {
                showNotification('Error updating follow status', 'error');
            }
        } finally {
            setButtonLoading(button, false);
        }
    }

    /**
     * Make the follow/unfollow API request
     */
    async function makeFollowRequest(entityId, entityType, isCurrentlyFollowed) {
        let url;
        let method;

        switch (entityType) {
            case 'user':
                url = `/api/user/${entityId}/follow`;
                method = isCurrentlyFollowed ? 'DELETE' : 'POST';
                break;
            case 'artist':
                url = `/api/artists/${entityId}/follow`;
                method = 'POST'; // Toggle endpoint
                break;
            case 'playlist':
                url = `/api/playlists/${entityId}/follow`;
                method = 'POST'; // Toggle endpoint
                break;
            default:
                throw new Error(`Unknown entity type: ${entityType}`);
        }

        const response = await fetch(url, {
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            }
        });

        if (!response.ok) {
            const error = new Error(`HTTP ${response.status}`);
            error.status = response.status;
            throw error;
        }

        return await response.json();
    }

    /**
     * Update button visual state
     */
    function updateButtonState(button, entityType, isFollowing, followerCount) {
        const iconElement = button.querySelector(CONFIG.selectors.followIcon);
        const textElement = button.querySelector(CONFIG.selectors.followText);
        const countElement = button.querySelector(CONFIG.selectors.followCount);

        // Update classes
        if (isFollowing) {
            button.classList.add(CONFIG.classes.followed);
            // Remove old classes and add new ones
            button.className = button.className
                .replace(/bg-gradient-to-r from-accent to-accent-dark text-white hover:from-accent-dark hover:to-accent/g, '')
                .trim() + ' ' + CONFIG.styles.followed;
        } else {
            button.classList.remove(CONFIG.classes.followed);
            // Remove old classes and add new ones
            button.className = button.className
                .replace(/bg-white\/10 text-white hover:bg-red-500\/20 hover:text-red-400 border border-white\/20 hover:border-red-400\/50/g, '')
                .trim() + ' ' + CONFIG.styles.notFollowed;
        }

        // Update icon
        if (iconElement) {
            const iconConfig = CONFIG.icons[entityType];
            if (iconConfig) {
                iconElement.setAttribute('data-lucide', isFollowing ? iconConfig.following : iconConfig.follow);
            }
        }

        // Update text
        if (textElement) {
            const textConfig = CONFIG.text[entityType];
            if (textConfig) {
                textElement.textContent = isFollowing ? textConfig.following : textConfig.follow;
            }
        }

        // Update count
        if (countElement && typeof followerCount === 'number') {
            countElement.textContent = `(${followerCount})`;
        }

        // Reinitialize Lucide icons
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }
    }

    /**
     * Set button loading state
     */
    function setButtonLoading(button, isLoading) {
        if (isLoading) {
            button.classList.add(CONFIG.classes.loading);
            button.disabled = true;
            
            const textElement = button.querySelector(CONFIG.selectors.followText);
            if (textElement) {
                textElement.dataset.originalText = textElement.textContent;
                textElement.textContent = 'Loading...';
            }
        } else {
            button.classList.remove(CONFIG.classes.loading);
            button.disabled = false;
            
            const textElement = button.querySelector(CONFIG.selectors.followText);
            if (textElement && textElement.dataset.originalText) {
                textElement.textContent = textElement.dataset.originalText;
                delete textElement.dataset.originalText;
            }
        }
    }

    /**
     * Get anti-forgery token
     */
    function getAntiForgeryToken() {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        return token ? token.value : '';
    }

    /**
     * Show notification (assumes global showNotification function exists)
     */
    function showNotification(message, type) {
        if (typeof window.showNotification === 'function') {
            window.showNotification(message, type);
        } else {
            console.log(`Notification (${type}): ${message}`);
        }
    }

    /**
     * Public API
     */
    window.FollowToggle = {
        init: init,
        toggleFollow: function(entityId, entityType) {
            const button = document.querySelector(`[data-entity-id="${entityId}"][data-entity-type="${entityType}"]`);
            if (button) {
                toggleFollow(button, entityId, entityType);
            }
        }
    };

    // Auto-initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();
