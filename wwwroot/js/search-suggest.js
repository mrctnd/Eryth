/**
 * Search suggestions module for the navbar search input
 * Provides instant search suggestions as the user types
 */
class SearchSuggestions {
    constructor() {
        this.input = document.getElementById('navbar-search-input');
        this.dropdown = document.getElementById('search-suggestions');
        this.form = this.input?.closest('form');
        
        this.debounceTimer = null;
        this.cache = new Map(); // Simple cache for suggestions
        this.currentSuggestions = [];
        this.selectedIndex = -1;
        
        this.init();
    }

    init() {
        if (!this.input || !this.dropdown) return;

        // Add event listeners
        this.input.addEventListener('input', this.handleInput.bind(this));
        this.input.addEventListener('keydown', this.handleKeydown.bind(this));
        this.input.addEventListener('focus', this.handleFocus.bind(this));
        this.input.addEventListener('blur', this.handleBlur.bind(this));
        
        // Close dropdown when clicking outside
        document.addEventListener('click', this.handleDocumentClick.bind(this));
    }

    handleInput(event) {
        const query = event.target.value.trim();
        
        // Clear previous timer
        if (this.debounceTimer) {
            clearTimeout(this.debounceTimer);
        }

        // Hide dropdown if query is too short
        if (query.length < 2) {
            this.hideDropdown();
            return;
        }

        // Debounce the search request
        this.debounceTimer = setTimeout(() => {
            this.fetchSuggestions(query);
        }, 250);
    }

    handleKeydown(event) {
        if (!this.isDropdownVisible()) return;

        switch (event.key) {
            case 'ArrowDown':
                event.preventDefault();
                this.navigateDown();
                break;
            case 'ArrowUp':
                event.preventDefault();
                this.navigateUp();
                break;
            case 'Enter':
                if (this.selectedIndex >= 0) {
                    event.preventDefault();
                    this.selectSuggestion(this.currentSuggestions[this.selectedIndex]);
                }
                break;
            case 'Escape':
                event.preventDefault();
                this.hideDropdown();
                this.input.blur();
                break;
        }
    }

    handleFocus() {
        const query = this.input.value.trim();
        if (query.length >= 2 && this.currentSuggestions.length > 0) {
            this.showDropdown();
        }
    }

    handleBlur(event) {
        // Delay hiding to allow clicking on suggestions
        setTimeout(() => {
            if (!this.dropdown.contains(document.activeElement)) {
                this.hideDropdown();
            }
        }, 150);
    }

    handleDocumentClick(event) {
        if (!this.input.contains(event.target) && !this.dropdown.contains(event.target)) {
            this.hideDropdown();
        }
    }

    async fetchSuggestions(query) {
        // Check cache first
        if (this.cache.has(query)) {
            this.renderSuggestions(this.cache.get(query));
            return;
        }

        try {
            const response = await fetch(`/api/search/suggest?q=${encodeURIComponent(query)}&limit=5`);
            if (!response.ok) throw new Error('Failed to fetch suggestions');
            
            const data = await response.json();
            const suggestions = data.suggestions || [];
            
            // Cache the results
            this.cache.set(query, suggestions);
            
            // Clear old cache entries if too many
            if (this.cache.size > 50) {
                const firstKey = this.cache.keys().next().value;
                this.cache.delete(firstKey);
            }
            
            this.renderSuggestions(suggestions);
        } catch (error) {
            console.error('Error fetching search suggestions:', error);
            this.hideDropdown();
        }
    }

    renderSuggestions(suggestions) {
        this.currentSuggestions = suggestions;
        this.selectedIndex = -1;

        if (suggestions.length === 0) {
            this.hideDropdown();
            return;
        }

        const html = suggestions.map((suggestion, index) => {
            return `
                <div class="suggestion-item px-4 py-2 text-white hover:bg-accent/20 cursor-pointer transition-colors" 
                     data-index="${index}"
                     data-suggestion="${this.escapeHtml(suggestion)}">
                    <i data-lucide="search" class="w-4 h-4 inline mr-3 text-gray-400"></i>
                    ${this.escapeHtml(suggestion)}
                </div>
            `;
        }).join('');

        this.dropdown.innerHTML = html;
        
        // Add click listeners to suggestions
        this.dropdown.querySelectorAll('.suggestion-item').forEach(item => {
            item.addEventListener('click', () => {
                this.selectSuggestion(item.dataset.suggestion);
            });
        });

        // Re-initialize Lucide icons for the new content
        if (window.lucide) {
            window.lucide.createIcons();
        }

        this.showDropdown();
    }

    selectSuggestion(suggestion) {
        this.input.value = suggestion;
        this.hideDropdown();
        
        // Submit the form to perform the search
        if (this.form) {
            this.form.submit();
        }
    }

    navigateDown() {
        this.selectedIndex = Math.min(this.selectedIndex + 1, this.currentSuggestions.length - 1);
        this.updateSelectedItem();
    }

    navigateUp() {
        this.selectedIndex = Math.max(this.selectedIndex - 1, -1);
        this.updateSelectedItem();
    }

    updateSelectedItem() {
        // Remove previous selection
        this.dropdown.querySelectorAll('.suggestion-item').forEach(item => {
            item.classList.remove('bg-accent/20');
        });

        // Add selection to current item
        if (this.selectedIndex >= 0) {
            const selectedItem = this.dropdown.querySelector(`[data-index="${this.selectedIndex}"]`);
            if (selectedItem) {
                selectedItem.classList.add('bg-accent/20');
            }
        }
    }

    showDropdown() {
        this.dropdown.classList.remove('hidden');
    }

    hideDropdown() {
        this.dropdown.classList.add('hidden');
        this.selectedIndex = -1;
    }

    isDropdownVisible() {
        return !this.dropdown.classList.contains('hidden');
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
}

// Initialize search suggestions when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new SearchSuggestions();
});

// Export for potential external use
if (typeof module !== 'undefined' && module.exports) {
    module.exports = SearchSuggestions;
}
