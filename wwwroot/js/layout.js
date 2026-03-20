// ─── PJAX Navigation ──────────────────────────────────────────────────────────
// Intercepts link clicks and swaps only #main-content + #page-scripts,
// keeping the audio player and navbar alive so music never stops.

let _pjaxLoading = false;

// ─── PJAX progress bar ────────────────────────────────────────────────────────

function _pjaxBarStart() {
    let bar = document.getElementById('pjax-progress-bar');
    if (!bar) {
        bar = document.createElement('div');
        bar.id = 'pjax-progress-bar';
        document.body.appendChild(bar);
    }
    bar.style.transition = 'none';
    bar.style.width = '0%';
    bar.style.opacity = '1';
    bar.offsetWidth; // force reflow so reset takes effect
    bar.style.transition = 'width 1s cubic-bezier(0.1, 0.5, 0.5, 1)';
    bar.style.width = '70%';
}

function _pjaxBarDone() {
    const bar = document.getElementById('pjax-progress-bar');
    if (!bar) return;
    bar.style.transition = 'width 0.15s ease';
    bar.style.width = '100%';
    setTimeout(() => {
        bar.style.transition = 'opacity 0.25s ease';
        bar.style.opacity = '0';
    }, 150);
}

function shouldPjaxNavigate(link) {
    const href = link.href;
    if (!href || href === window.location.href) return false;
    if (href.startsWith('#')) return false;
    if (href.startsWith('javascript:') || href.startsWith('mailto:') || href.startsWith('tel:')) return false;
    if (!href.startsWith(window.location.origin)) return false;
    if (link.hasAttribute('download')) return false;
    if (link.target && link.target !== '_self') return false;
    if (link.getAttribute('data-no-pjax') !== null) return false;

    // Skip auth routes – they redirect & handle cookies specially
    try {
        const path = new URL(href).pathname;
        if (path.startsWith('/Auth/') || path.startsWith('/Admin/')) return false;
    } catch { return false; }

    return true;
}

async function pjaxNavigate(url, pushState = true) {
    if (_pjaxLoading) return;
    _pjaxLoading = true;

    const curMain = document.getElementById('main-content');

    // Close any open dropdowns before navigating
    document.querySelectorAll('[id$="-menu"]').forEach(m => m.classList.add('hidden'));

    // Start accent progress bar + fade-out current content
    _pjaxBarStart();
    if (curMain) {
        curMain.style.transition = 'opacity 0.15s ease, transform 0.15s ease';
        curMain.style.opacity = '0';
        curMain.style.transform = 'translateY(10px)';
    }

    try {
        // Fetch page + wait minimum 150ms for exit animation
        const [res] = await Promise.all([
            fetch(url, {
                headers: { 'X-PJAX': 'true', 'X-Requested-With': 'XMLHttpRequest' },
                credentials: 'same-origin'
            }),
            new Promise(r => setTimeout(r, 150))
        ]);

        // Handle redirects to a different origin or to auth pages
        const finalUrl = res.url;
        if (!finalUrl.startsWith(window.location.origin)) {
            window.location.href = finalUrl; return;
        }
        try {
            const fp = new URL(finalUrl).pathname;
            if (fp.startsWith('/Auth/') || fp.startsWith('/Admin/')) {
                window.location.href = finalUrl; return;
            }
        } catch {}

        const html = await res.text();
        const doc = new DOMParser().parseFromString(html, 'text/html');

        const newMain = doc.getElementById('main-content');
        if (!newMain) { window.location.href = url; return; }

        // Swap title
        document.title = doc.title;

        // Swap main content (still invisible — opacity: 0)
        if (curMain) {
            curMain.innerHTML = newMain.innerHTML;
            curMain.style.transition = 'none';
            curMain.style.transform = 'translateY(-10px)'; // enter from above
        }

        // Swap and execute page-specific scripts
        const curScripts = document.getElementById('page-scripts');
        const newScripts = doc.getElementById('page-scripts');
        if (curScripts) curScripts.innerHTML = '';
        if (newScripts && curScripts) {
            await _executePjaxScripts(newScripts, curScripts);
        }

        // Update URL / history
        if (pushState) {
            history.pushState({ pjax: true, url: finalUrl }, document.title, finalUrl);
        }

        // Re-init Lucide icons for swapped content
        if (typeof lucide !== 'undefined') lucide.createIcons();

        // Update active nav link
        setActiveNavLink();

        // Update antiforgery token (so AJAX POSTs keep working)
        const newToken = doc.querySelector('meta[name="__RequestVerificationToken"]');
        const curToken = document.querySelector('meta[name="__RequestVerificationToken"]');
        if (newToken && curToken) curToken.content = newToken.content;

        window.scrollTo(0, 0);

        // Fade in new content
        _pjaxBarDone();
        requestAnimationFrame(() => {
            if (curMain) {
                curMain.style.transition = 'opacity 0.25s ease, transform 0.25s ease';
                curMain.style.opacity = '1';
                curMain.style.transform = 'translateY(0)';
            }
        });

    } catch (err) {
        _pjaxBarDone();
        if (curMain) {
            curMain.style.transition = '';
            curMain.style.opacity = '1';
            curMain.style.transform = '';
        }
        console.warn('PJAX navigation failed, falling back:', err);
        window.location.href = url;
    } finally {
        _pjaxLoading = false;
    }
}

async function _executePjaxScripts(source, target) {
    const scripts = Array.from(source.querySelectorAll('script'));
    if (!scripts.length) return;

    const dclCallbacks = [];
    const origAEL = document.addEventListener.bind(document);

    // Intercept DOMContentLoaded registrations so we can fire them after load
    document.addEventListener = function(type, fn, opts) {
        if (type === 'DOMContentLoaded') {
            dclCallbacks.push(fn);
        } else {
            origAEL(type, fn, opts);
        }
    };

    const loadPromises = [];

    try {
        for (const old of scripts) {
            const el = document.createElement('script');
            for (const attr of old.attributes) el.setAttribute(attr.name, attr.value);

            if (old.src) {
                el.src = old.src;
                const p = new Promise(resolve => {
                    el.onload = resolve;
                    el.onerror = resolve;
                    setTimeout(resolve, 5000);
                });
                loadPromises.push(p);
                target.appendChild(el);
            } else if (old.textContent.trim()) {
                el.textContent = old.textContent;
                target.appendChild(el);
            }
        }

        if (loadPromises.length) await Promise.all(loadPromises);
    } finally {
        // Always restore original addEventListener
        document.addEventListener = origAEL;
    }

    // Fire collected DOMContentLoaded callbacks
    for (const cb of dclCallbacks) {
        try { cb({ type: 'DOMContentLoaded', target: document, currentTarget: document }); }
        catch (e) { console.warn('PJAX DCL callback error:', e); }
    }
}

function setupPageTransitions() {
    // Init history state for current page
    if (!history.state || !history.state.pjax) {
        history.replaceState({ pjax: true, url: window.location.href }, document.title, window.location.href);
    }

    // Intercept link clicks
    document.addEventListener('click', function(e) {
        const link = e.target.closest('a');
        if (link && shouldPjaxNavigate(link)) {
            e.preventDefault();
            pjaxNavigate(link.href);
        }
    });

    // Handle browser back/forward
    window.addEventListener('popstate', function(e) {
        if (e.state && e.state.pjax) {
            pjaxNavigate(e.state.url, false);
        }
    });

    // Hide legacy transition overlay
    const overlay = document.getElementById('page-transition');
    if (overlay) overlay.style.display = 'none';
}

// ─── Layout utilities ─────────────────────────────────────────────────────────

function toggleDropdown(menuId) {
    const menu = document.getElementById(menuId);
    const isHidden = menu.classList.contains('hidden');

    document.querySelectorAll('[id$="-menu"]').forEach(d => d.classList.add('hidden'));

    if (isHidden) menu.classList.remove('hidden');
}

function showAuthModal(mode) {
    const modal = document.getElementById('auth-modal');
    const authContent = document.getElementById('auth-content');
    const signinForm = document.getElementById('signin-form');
    const signupForm = document.getElementById('signup-form');

    modal.classList.remove('hidden');

    if (mode === 'signin') {
        signinForm.classList.remove('hidden');
        signupForm.classList.add('hidden');
    } else {
        signinForm.classList.add('hidden');
        signupForm.classList.remove('hidden');
    }

    setTimeout(() => {
        authContent.classList.remove('scale-95', 'opacity-0');
        authContent.classList.add('scale-100', 'opacity-100');
    }, 50);
}

function hideAuthModal() {
    const modal = document.getElementById('auth-modal');
    const authContent = document.getElementById('auth-content');

    authContent.classList.remove('scale-100', 'opacity-100');
    authContent.classList.add('scale-95', 'opacity-0');

    setTimeout(() => modal.classList.add('hidden'), 300);
}

function switchAuthMode(mode) {
    const signinForm = document.getElementById('signin-form');
    const signupForm = document.getElementById('signup-form');
    const authContent = document.getElementById('auth-content');

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
        authContent.classList.remove('scale-95');
        authContent.classList.add('scale-100');
    }, 150);
}

function showForgotPassword() {
    hideAuthModal();
    window.location.href = '/Auth/ForgotPassword';
}

function signOut() {
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = '/Auth/Logout';

    const token = document.querySelector('meta[name="__RequestVerificationToken"]');
    if (token) {
        const input = document.createElement('input');
        input.type = 'hidden';
        input.name = '__RequestVerificationToken';
        input.value = token.content;
        form.appendChild(input);
    }

    document.body.appendChild(form);
    form.submit();
}

function setupFormValidation() {
    const signinForm = document.getElementById('signin-form-element');
    const signupForm = document.getElementById('signup-form-element');

    if (signinForm) {
        signinForm.addEventListener('submit', function(e) {
            e.preventDefault();
            hideAuthErrors('signin');
            const data = new FormData(signinForm);
            if (!data.get('EmailOrUsername') || !data.get('Password')) {
                showAuthError('signin', 'Lütfen tüm gerekli alanları doldurun.');
                return;
            }
            this.submit();
        });
    }

    if (signupForm) {
        signupForm.addEventListener('submit', function(e) {
            e.preventDefault();
            hideAuthErrors('signup');
            const data = new FormData(signupForm);
            const username = data.get('Username');
            const email = data.get('Email');
            const password = data.get('Password');
            const confirm = data.get('ConfirmPassword');
            const terms = data.get('AgreeToTerms');

            if (!username || !email || !password || !confirm) {
                showAuthError('signup', 'Lütfen tüm gerekli alanları doldurun.'); return;
            }
            if (username.length < 3 || username.length > 50) {
                showAuthError('signup', 'Kullanıcı adı 3 ile 50 karakter arasında olmalıdır.'); return;
            }
            if (!/^[a-zA-Z0-9_-]+$/.test(username)) {
                showAuthError('signup', 'Kullanıcı adı sadece harf, rakam, alt çizgi ve tire içerebilir.'); return;
            }
            if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
                showAuthError('signup', 'Lütfen geçerli bir e-posta adresi girin.'); return;
            }
            if (password.length < 8) {
                showAuthError('signup', 'Şifre en az 8 karakter uzunluğunda olmalıdır.'); return;
            }
            if (!/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&\-_=+\[\]{}|;:,.<>])[A-Za-z\d@$!%*?&\-_=+\[\]{}|;:,.<>]+$/.test(password)) {
                showAuthError('signup', 'Şifre en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.'); return;
            }
            if (password !== confirm) {
                showAuthError('signup', 'Şifreler eşleşmiyor.'); return;
            }
            if (!terms) {
                showAuthError('signup', 'Kullanım Şartları ve Gizlilik Politikası\'nı kabul etmelisiniz.'); return;
            }
            this.submit();
        });
    }
}

function showAuthError(formType, message) {
    const div = document.getElementById(formType + '-errors');
    const msg = document.getElementById(formType + '-error-message');
    if (msg) msg.textContent = message;
    if (div) div.classList.remove('hidden');
}

function hideAuthErrors(formType) {
    const div = document.getElementById(formType + '-errors');
    if (div) div.classList.add('hidden');
}

function setupPasswordConfirmation() {
    const pw = document.getElementById('signup-password');
    const cpw = document.getElementById('signup-confirm-password');
    if (!pw || !cpw) return;

    const check = () => {
        if (pw.value && cpw.value) {
            cpw.setCustomValidity(pw.value !== cpw.value ? 'Passwords do not match' : '');
        }
    };
    pw.addEventListener('input', check);
    cpw.addEventListener('input', check);
}

function setActiveNavLink() {
    const currentPath = window.location.pathname;
    document.querySelectorAll('nav a[id^="nav-"]').forEach(link => {
        link.classList.remove('nav-link-active');
    });
    const active = document.querySelector(`nav a[href="${currentPath}"]`);
    if (active) {
        active.classList.add('nav-link-active');
    }
}

function updateAuthenticationUI(isLoggedIn) {
    const loggedIn = document.getElementById('logged-in-actions');
    const loggedOut = document.getElementById('logged-out-actions');
    if (isLoggedIn) {
        loggedIn?.classList.remove('hidden');
        loggedOut?.classList.add('hidden');
    } else {
        loggedIn?.classList.add('hidden');
        loggedOut?.classList.remove('hidden');
    }
}

function setupProtectedNavigation() {
    ['nav-explore', 'nav-library'].forEach(id => {
        const link = document.getElementById(id);
        if (link) {
            link.addEventListener('click', e => { e.preventDefault(); showAuthModal('signin'); });
            link.classList.add('cursor-pointer');
        }
    });
}

// ─── Init ─────────────────────────────────────────────────────────────────────

document.addEventListener('DOMContentLoaded', function() {
    if (typeof lucide !== 'undefined') lucide.createIcons();

    setupFormValidation();
    setupPasswordConfirmation();
    setActiveNavLink();

    const loggedInActions = document.getElementById('logged-in-actions');
    const isLoggedIn = loggedInActions && !loggedInActions.classList.contains('hidden');
    updateAuthenticationUI(isLoggedIn);
    if (!isLoggedIn) setupProtectedNavigation();

    // Close dropdowns on outside click
    document.addEventListener('click', function(e) {
        document.querySelectorAll('[id$="-dropdown"]').forEach(dd => {
            if (!dd.contains(e.target)) {
                dd.querySelector('[id$="-menu"]')?.classList.add('hidden');
            }
        });
    });

    setupPageTransitions();
});
