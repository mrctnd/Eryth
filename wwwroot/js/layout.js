// Audio player functionality moved to audioPlayer.js

// Page transition management with audio player preservation
function setupPageTransitions() {
    // Intercept all navigation links with enhanced audio player preservation
    document.addEventListener('click', function(e) {
        const link = e.target.closest('a');
        
        if (link && 
            link.href && 
            !link.href.startsWith('#') && 
            !link.href.startsWith('javascript:') &&
            !link.href.startsWith('mailto:') &&
            !link.href.startsWith('tel:') &&
            link.href.startsWith(window.location.origin) &&
            !link.target &&
            !link.hasAttribute('download')) {
            
            e.preventDefault();
            
            // Ensure audio player state is saved before navigation
            if (typeof savePlayerState === 'function') {
                savePlayerState();
            }
            
            // Set navigation flag
            sessionStorage.setItem('audioPlayer_navigating', 'true');
            
            // Show loading overlay
            const overlay = document.getElementById('page-transition');
            if (overlay) {
                overlay.style.opacity = '1';
                overlay.style.pointerEvents = 'auto';
            }
            
            // Navigate after ensuring state is saved
            setTimeout(() => {
                window.location.href = link.href;
            }, 150); // Slightly longer delay to ensure state persistence
        }
    });
    
    // Handle form submissions with audio player preservation
    document.addEventListener('submit', function(e) {
        const form = e.target;
        
        // Don't interfere with AJAX forms or specific forms
        if (form.method && form.method.toLowerCase() === 'post' && !form.hasAttribute('data-ajax')) {
            
            // Save audio player state before form submission
            if (typeof savePlayerState === 'function') {
                savePlayerState();
            }
            
            // Set navigation flag
            sessionStorage.setItem('audioPlayer_navigating', 'true');
            
            // Show loading overlay for form submissions
            const overlay = document.getElementById('page-transition');
            if (overlay) {
                setTimeout(() => {
                    overlay.style.opacity = '1';
                    overlay.style.pointerEvents = 'auto';
                }, 50);
            }
        }
    });
    
    // Hide loading overlay when page loads and restore audio state
    window.addEventListener('load', function() {
        const overlay = document.getElementById('page-transition');
        if (overlay) {
            setTimeout(() => {
                overlay.style.opacity = '0';
                overlay.style.pointerEvents = 'none';
                
                // Check if we need to restore audio player state
                const wasNavigating = sessionStorage.getItem('audioPlayer_navigating');
                if (wasNavigating && typeof loadPlayerState === 'function') {
                    // Give the audio player module time to initialize
                    setTimeout(() => {
                        loadPlayerState();
                    }, 200);
                }
            }, 100);
        }
    });
    window.addEventListener('load', function() {
        const overlay = document.getElementById('page-transition');
        if (overlay) {
            setTimeout(() => {
                overlay.style.opacity = '0';
                overlay.style.pointerEvents = 'none';
            }, 200);
        }
    });
}

// Global functions for HTML onclick handlers
function toggleDropdown(menuId) {
    const menu = document.getElementById(menuId);
    const isHidden = menu.classList.contains('hidden');
    
    // Close all dropdowns first
    document.querySelectorAll('[id$="-menu"]').forEach(dropdown => {
        dropdown.classList.add('hidden');
    });
    
    // Toggle current dropdown
    if (isHidden) {
        menu.classList.remove('hidden');
    }
}

// Global toggle track like function - DEPRECATED
// Like functionality has been moved to like-toggle.js module for better consistency
// Use .js-like-toggle class on buttons with data-track-id attribute

function showAuthModal(mode) {
    const modal = document.getElementById('auth-modal');
    const authContent = document.getElementById('auth-content');
    const signinForm = document.getElementById('signin-form');
    const signupForm = document.getElementById('signup-form');
    
    // Show modal
    modal.classList.remove('hidden');
    
    // Show appropriate form
    if (mode === 'signin') {
        signinForm.classList.remove('hidden');
        signupForm.classList.add('hidden');
    } else {
        signinForm.classList.add('hidden');
        signupForm.classList.remove('hidden');
    }
    
    // Trigger animations with a slight delay
    setTimeout(() => {
        authContent.classList.remove('scale-95', 'opacity-0');
        authContent.classList.add('scale-100', 'opacity-100');
    }, 50);
}

function hideAuthModal() {
    const modal = document.getElementById('auth-modal');
    const authContent = document.getElementById('auth-content');
    
    // Start exit animations
    authContent.classList.remove('scale-100', 'opacity-100');
    authContent.classList.add('scale-95', 'opacity-0');
    
    // Hide modal after animation completes
    setTimeout(() => {
        modal.classList.add('hidden');
    }, 300);
}

function switchAuthMode(mode) {
    const signinForm = document.getElementById('signin-form');
    const signupForm = document.getElementById('signup-form');
    const authContent = document.getElementById('auth-content');
    
    // Add slight scale animation when switching
    authContent.classList.remove('scale-100');
    authContent.classList.add('scale-95');
    
    setTimeout(() => {
        if (mode === 'signin') {
            signinForm.classList.remove('hidden');
            signupForm.classList.add('hidden');
        } else {
            signinForm.classList.add('hidden');
            signupForm.classList.remove('hidden');
        }
        
        // Return to normal scale
        authContent.classList.remove('scale-95');
        authContent.classList.add('scale-100');
    }, 150);
}



function showForgotPassword() {
    hideAuthModal();
    window.location.href = '/Auth/ForgotPassword';
}

function signOut() {
    // Create and submit logout form
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = '/Auth/Logout';
    
    // Add antiforgery token if available
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    if (token) {
        const tokenInput = document.createElement('input');
        tokenInput.type = 'hidden';
        tokenInput.name = '__RequestVerificationToken';
        tokenInput.value = token.value;
        form.appendChild(tokenInput);
    }
    
    document.body.appendChild(form);
    form.submit();
}

// Form validation functions
function setupFormValidation() {
    const signinForm = document.getElementById('signin-form-element');
    const signupForm = document.getElementById('signup-form-element');

    // Sign in form validation
    if (signinForm) {
        signinForm.addEventListener('submit', function(e) {
            e.preventDefault();
            hideAuthErrors('signin');
            
            const formData = new FormData(signinForm);
            const emailOrUsername = formData.get('EmailOrUsername');
            const password = formData.get('Password');
            
            // Basic validation
            if (!emailOrUsername || !password) {
                showAuthError('signin', 'Lütfen tüm gerekli alanları doldurun.');
                return;
            }
            
            // Submit form
            this.submit();
        });
    }

    // Sign up form validation
    if (signupForm) {
        signupForm.addEventListener('submit', function(e) {
            e.preventDefault();
            hideAuthErrors('signup');
            
            const formData = new FormData(signupForm);
            const username = formData.get('Username');
            const email = formData.get('Email');
            const password = formData.get('Password');
            const confirmPassword = formData.get('ConfirmPassword');
            const agreeToTerms = formData.get('AgreeToTerms');
            
            // Basic validation
            if (!username || !email || !password || !confirmPassword) {
                showAuthError('signup', 'Lütfen tüm gerekli alanları doldurun.');
                return;
            }
            
            // Username validation
            if (username.length < 3 || username.length > 50) {
                showAuthError('signup', 'Kullanıcı adı 3 ile 50 karakter arasında olmalıdır.');
                return;
            }
            
            const usernameRegex = /^[a-zA-Z0-9_-]+$/;
            if (!usernameRegex.test(username)) {
                showAuthError('signup', 'Kullanıcı adı sadece harf, rakam, alt çizgi ve tire içerebilir.');
                return;
            }
            
            // Email validation
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(email)) {
                showAuthError('signup', 'Lütfen geçerli bir e-posta adresi girin.');
                return;
            }
            
            // Password validation
            if (password.length < 8) {
                showAuthError('signup', 'Şifre en az 8 karakter uzunluğunda olmalıdır.');
                return;
            }
            
            const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&\-_=+\[\]{}|;:,.<>])[A-Za-z\d@$!%*?&\-_=+\[\]{}|;:,.<>]+$/;
            if (!passwordRegex.test(password)) {
                showAuthError('signup', 'Şifre en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.');
                return;
            }
            
            // Password confirmation
            if (password !== confirmPassword) {
                showAuthError('signup', 'Şifreler eşleşmiyor.');
                return;
            }
            
            // Terms agreement
            if (!agreeToTerms) {
                showAuthError('signup', 'Kullanım Şartları ve Gizlilik Politikası\'nı kabul etmelisiniz.');
                return;
            }
            
            // Submit form
            this.submit();
        });
    }
}

function showAuthError(formType, message) {
    const errorDiv = document.getElementById(formType + '-errors');
    const errorMessage = document.getElementById(formType + '-error-message');
    
    if (errorMessage) errorMessage.textContent = message;
    if (errorDiv) errorDiv.classList.remove('hidden');
}

function hideAuthErrors(formType) {
    const errorDiv = document.getElementById(formType + '-errors');
    if (errorDiv) errorDiv.classList.add('hidden');
}

function setupPasswordConfirmation() {
    const passwordField = document.getElementById('signup-password');
    const confirmPasswordField = document.getElementById('signup-confirm-password');
    
    function validatePasswordMatch() {
        if (passwordField.value && confirmPasswordField.value) {
            if (passwordField.value !== confirmPasswordField.value) {
                confirmPasswordField.setCustomValidity('Passwords do not match');
            } else {
                confirmPasswordField.setCustomValidity('');
            }
        }
    }
    
    if (passwordField && confirmPasswordField) {
        passwordField.addEventListener('input', validatePasswordMatch);        confirmPasswordField.addEventListener('input', validatePasswordMatch);
    }
}

function setActiveNavLink() {
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('nav a[id^="nav-"]');
    
    navLinks.forEach(link => {
        link.classList.remove('border-white');
        link.classList.add('border-transparent');
    });
    
    // Set active link based on current path
    const activeLink = document.querySelector(`nav a[href="${currentPath}"]`);
    if (activeLink) {
        activeLink.classList.remove('border-transparent');
        activeLink.classList.add('border-white');
    }
}

function updateAuthenticationUI(isLoggedIn) {
    const loggedInActions = document.getElementById('logged-in-actions');
    const loggedOutActions = document.getElementById('logged-out-actions');
    
    if (isLoggedIn) {
        if (loggedInActions) loggedInActions.classList.remove('hidden');
        if (loggedOutActions) loggedOutActions.classList.add('hidden');
    } else {
        if (loggedInActions) loggedInActions.classList.add('hidden');
        if (loggedOutActions) loggedOutActions.classList.remove('hidden');
    }
}

function setupProtectedNavigation() {
    const exploreLink = document.getElementById('nav-explore');
    const libraryLink = document.getElementById('nav-library');
    
    // Add click event listeners to protected links
    if (exploreLink) {
        exploreLink.addEventListener('click', function(e) {
            e.preventDefault();
            showAuthModal('signin');
        });
        exploreLink.classList.add('cursor-pointer');
        exploreLink.title = 'Keşfete erişmek için giriş yapın';
    }
    
    if (libraryLink) {
        libraryLink.addEventListener('click', function(e) {
            e.preventDefault();
            showAuthModal('signin');
        });
        libraryLink.classList.add('cursor-pointer');
        libraryLink.title = 'Kütüphanenize erişmek için giriş yapın';
    }
}

// Initialize when DOM loads
document.addEventListener('DOMContentLoaded', function() {
    // Initialize Lucide icons
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
    
    // Setup form validation
    setupFormValidation();
    setupPasswordConfirmation();
    
    // Set active navigation link
    setActiveNavLink();
    
    // Check authentication state
    const loggedInActions = document.getElementById('logged-in-actions');
    const isLoggedIn = loggedInActions && !loggedInActions.classList.contains('hidden');
    
    updateAuthenticationUI(isLoggedIn);
    
    if (!isLoggedIn) {
        setupProtectedNavigation();
    }
    
    // Close dropdowns when clicking outside
    document.addEventListener('click', function(event) {
        const dropdowns = document.querySelectorAll('[id$="-dropdown"]');
        dropdowns.forEach(dropdown => {
            if (!dropdown.contains(event.target)) {
                const menu = dropdown.querySelector('[id$="-menu"]');
                if (menu) {
                    menu.classList.add('hidden');
                }
            }
        });
    });    // Audio player setup moved to audioPlayer.js
    setupPageTransitions();
});