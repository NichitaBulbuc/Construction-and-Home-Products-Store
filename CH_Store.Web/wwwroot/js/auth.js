/**
 * CH Store — Auth Module
 * Gestioneaza JWT-ul, sesiunea si guard-urile de pagina.
 *
 * Fluxul:
 *  1. Login/Register → API returnează AuthResponse → salvat în localStorage
 *  2. Fiecare pagina protejata apeleaza Auth.requireAuth() sau Auth.requireAdmin()
 *  3. api.js citeste token-ul si il adauga ca Bearer la fiecare request
 *  4. La DOMContentLoaded: navbar-ul este actualizat cu starea sesiunii curente
 */

const Auth = (() => {
  const TOKEN_KEY = 'ch_token';
  const USER_KEY  = 'ch_user';

  // ── Decodeaza payload JWT (base64url) ──────────────────────────────────────
  function decodeJWT(token) {
    try {
      const b64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
      // Pad la multiplu de 4
      const padded = b64 + '='.repeat((4 - b64.length % 4) % 4);
      return JSON.parse(atob(padded));
    } catch { return null; }
  }

  // ── Getters ────────────────────────────────────────────────────────────────
  function getToken()  { return localStorage.getItem(TOKEN_KEY); }
  function getUser()   {
    try { return JSON.parse(localStorage.getItem(USER_KEY) || 'null'); }
    catch { return null; }
  }

  // ── Verificare sesiune activa ──────────────────────────────────────────────
  function isAuthenticated() {
    const token = getToken();
    if (!token) return false;
    const payload = decodeJWT(token);
    if (!payload) return false;
    // exp este in secunde Unix
    return payload.exp * 1000 > Date.now();
  }

  function isAdmin() {
    const user = getUser();
    return user?.role === 'Admin';
  }

  // ── Salvare / Stergere sesiune ─────────────────────────────────────────────
  function setSession(authResponse) {
    localStorage.setItem(TOKEN_KEY, authResponse.token);
    localStorage.setItem(USER_KEY,  JSON.stringify({
      userId:   authResponse.userId,
      username: authResponse.username,
      email:    authResponse.email,
      role:     authResponse.role,
      expiresAt: authResponse.expiresAt
    }));
    // Sincronizeaza ch_userId folosit de api.js / cart
    localStorage.setItem('ch_userId', String(authResponse.userId));
  }

  function logout() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    localStorage.removeItem('ch_userId');
    // Redirect la login indiferent de pagina curenta
    location.href = '/login.html';
  }

  // ── Guard-uri (apelate in <head> → redirect inainte de render) ────────────
  function requireAuth() {
    if (!isAuthenticated()) {
      const redirect = encodeURIComponent(location.pathname + location.search);
      location.href = '/login.html?redirect=' + redirect;
      return false;
    }
    return true;
  }

  function requireAdmin() {
    if (!isAuthenticated()) {
      location.href = '/login.html?redirect=' + encodeURIComponent(location.pathname);
      return false;
    }
    if (!isAdmin()) {
      location.href = '/index.html?error=forbidden';
      return false;
    }
    return true;
  }

  // ── Injectare dinamica navbar ──────────────────────────────────────────────
  function renderNav() {
    const nav = document.querySelector('.navbar nav');
    if (!nav) return;

    const cartBtn = nav.querySelector('.cart-btn');

    // Sterge eventual auth-el anterior (evita duplicate pe hot-reload)
    nav.querySelector('#auth-nav-el')?.remove();

    const el = document.createElement('div');
    el.id = 'auth-nav-el';
    el.style.cssText = 'display:flex;align-items:center;gap:.5rem';

    if (isAuthenticated()) {
      const user = getUser();
      el.innerHTML = `
        ${isAdmin()
          ? `<a href="/admin/index.html" style="color:var(--primary);font-weight:600;font-size:.85rem;padding:.3rem .6rem;border-radius:6px;border:1px solid rgba(212,98,30,.4)">⚙️ Admin</a>`
          : ''
        }
        <span style="color:#c8bfb8;font-size:.85rem;display:flex;align-items:center;gap:.3rem">
          <span style="background:rgba(255,255,255,.1);border-radius:999px;padding:.15rem .6rem">👤 ${escHtmlAuth(user?.username || '')}</span>
        </span>
        <button onclick="Auth.logout()"
          style="background:transparent;border:1px solid rgba(255,255,255,.2);color:#a09890;
                 padding:.3rem .75rem;border-radius:6px;cursor:pointer;font-size:.8rem;
                 transition:.15s ease"
          onmouseover="this.style.borderColor='var(--primary)';this.style.color='#fff'"
          onmouseout="this.style.borderColor='rgba(255,255,255,.2)';this.style.color='#a09890'">
          Ieșire
        </button>`;
    } else {
      el.innerHTML = `
        <a href="/login.html"
           style="color:#c8bfb8;font-size:.85rem;padding:.35rem .8rem;border-radius:6px;
                  border:1px solid rgba(255,255,255,.15);transition:.15s"
           onmouseover="this.style.borderColor='var(--primary)';this.style.color='#fff'"
           onmouseout="this.style.borderColor='rgba(255,255,255,.15)';this.style.color='#c8bfb8'">
          Autentificare
        </a>
        <a href="/register.html"
           style="background:var(--primary);color:#fff;font-size:.85rem;padding:.35rem .8rem;
                  border-radius:6px;font-weight:600;transition:.15s"
           onmouseover="this.style.background='var(--primary-h)'"
           onmouseout="this.style.background='var(--primary)'">
          Înregistrare
        </a>`;
    }

    if (cartBtn) nav.insertBefore(el, cartBtn);
    else nav.appendChild(el);

    // Afiseaza eroarea de acces, daca exista
    const urlError = new URLSearchParams(location.search).get('error');
    if (urlError === 'forbidden') {
      setTimeout(() => {
        if (typeof showToast === 'function')
          showToast('Acces interzis — doar adminii pot accesa acea pagina.', 'error');
      }, 300);
    }
  }

  // Helper XSS mic — nu are nevoie de api.js
  function escHtmlAuth(s) {
    return String(s).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;');
  }

  // ── Auto-render la incarcarea paginii ─────────────────────────────────────
  document.addEventListener('DOMContentLoaded', renderNav);

  // ── Export ────────────────────────────────────────────────────────────────
  return { getToken, getUser, isAuthenticated, isAdmin,
           setSession, logout, requireAuth, requireAdmin, renderNav };
})();
