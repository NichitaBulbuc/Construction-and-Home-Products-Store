/**
 * CH Store — API Client
 * Toate cererile catre backend-ul ASP.NET Core.
 * Base URL relativa → acelasi origin ca frontend-ul.
 */

const API = (() => {
  const BASE = '';   // same-origin, servit din wwwroot

  async function request(method, path, body = null) {
    const opts = {
      method,
      headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' }
    };
    // Adauga Bearer token daca exista sesiune activa
    const token = localStorage.getItem('ch_token');
    if (token) opts.headers['Authorization'] = `Bearer ${token}`;
    if (body !== null) opts.body = JSON.stringify(body);
    const res = await fetch(BASE + path, opts);
    if (res.status === 204) return null;

    // Token expirat / lipsa → logout si redirect la login
    // Exceptie: daca suntem deja pe /login.html, nu redirectam (evita loop infinit → 414)
    if (res.status === 401) {
      localStorage.removeItem('ch_token');
      localStorage.removeItem('ch_user');
      localStorage.removeItem('ch_userId');
      if (!location.pathname.endsWith('/login.html')) {
        const redirect = encodeURIComponent(location.pathname + location.search);
        location.href = '/login.html?redirect=' + redirect + '&reason=session_expired';
      }
      return;
    }

    const data = await res.json().catch(() => null);
    if (!res.ok) throw { status: res.status, data };
    return data;
  }

  const get    = (path)        => request('GET',    path);
  const post   = (path, body)  => request('POST',   path, body);
  const put    = (path, body)  => request('PUT',    path, body);
  const patch  = (path, body)  => request('PATCH',  path, body);
  const del    = (path, body)  => request('DELETE', path, body);

  // ── Auth ──────────────────────────────────────────────────────
  const auth = {
    login:    (body)  => request('POST', '/api/auth/login',    body),
    register: (body)  => request('POST', '/api/auth/register', body),
    me:       ()      => request('GET',  '/api/auth/me'),
  };

  // ── Products ──────────────────────────────────────────────────
  const products = {
    getAll:       ()           => get('/api/product'),
    getById:      (id)         => get(`/api/product/${id}`),
    getByCategory:(cat)        => get(`/api/product/category/${cat}`),
  };

  // ── Admin Products ────────────────────────────────────────────
  const adminProducts = {
    getAll:       (category)   => get(`/api/admin/products${category ? '?category='+category : ''}`),
    getById:      (id)         => get(`/api/admin/products/${id}`),
    create:       (body)       => post('/api/admin/products', body),
    update:       (id, body)   => put(`/api/admin/products/${id}`, body),
    delete:       (id)         => del(`/api/admin/products/${id}`),
    updateStock:  (id, body)   => patch(`/api/admin/products/${id}/stock`, body),
    lowStock:     (threshold)  => get(`/api/admin/products/low-stock?threshold=${threshold ?? 10}`),
  };

  // ── Cart ──────────────────────────────────────────────────────
  const cart = {
    get:          (userId)              => get(`/api/cart/${userId}`),
    addItem:      (userId, body)        => post(`/api/cart/${userId}/items`, body),
    updateItem:   (userId, productId, body) => patch(`/api/cart/${userId}/items/${productId}`, body),
    removeItem:   (userId, productId)   => del(`/api/cart/${userId}/items/${productId}`),
    clear:        (userId)              => del(`/api/cart/${userId}`),
    undo:         (userId)              => post(`/api/cart/${userId}/undo`, null),
    history:      (userId)              => get(`/api/cart/${userId}/history`),
  };

  // ── Orders ────────────────────────────────────────────────────
  const orders = {
    placeWithPayment: (body)  => post('/api/order/pay', body),   // endpoint: [HttpPost("pay")]
    validate:         (body)  => post('/api/order/validate', body),
    strategies:       ()      => get('/api/order/strategies'),
  };

  // ── Admin Orders ──────────────────────────────────────────────
  const adminOrders = {
    getAll:        (params)  => get('/api/admin/orders?' + new URLSearchParams(params || {})),
    getById:       (id)      => get(`/api/admin/orders/${id}`),
    pending:       ()        => get('/api/admin/orders/pending-approval'),
    approve:       (id, body)=> post(`/api/admin/orders/${id}/approve`, body),
    reject:        (id, body)=> post(`/api/admin/orders/${id}/reject`, body),
    forceStatus:   (id, body)=> patch(`/api/admin/orders/${id}/status`, body),
  };

  // ── Admin Users ───────────────────────────────────────────────
  const adminUsers = {
    getAll:   (page, pageSize, search) => {
      const p = new URLSearchParams({ page: page||1, pageSize: pageSize||20 });
      if (search) p.set('search', search);
      return get('/api/admin/users?' + p);
    },
    getById:  (id)       => get(`/api/admin/users/${id}`),
    update:   (id, body) => put(`/api/admin/users/${id}`, body),
    orders:   (id)       => get(`/api/admin/users/${id}/orders`),
  };

  // ── Admin Dashboard ───────────────────────────────────────────
  const dashboard = {
    stats:          ()         => get('/api/admin/dashboard'),
    revenue:        (days)     => get(`/api/admin/dashboard/revenue?days=${days||30}`),
    activity:       (count)    => get(`/api/admin/dashboard/activity?count=${count||50}`),
    ordersByStatus: ()         => get('/api/admin/dashboard/orders-by-status'),
  };

  return { auth, products, adminProducts, cart, orders, adminOrders, adminUsers, dashboard };
})();

/* ── Toast ──────────────────────────────────────────────────── */
function showToast(msg, type = 'info') {
  let container = document.getElementById('toast-container');
  if (!container) {
    container = document.createElement('div');
    container.id = 'toast-container';
    document.body.appendChild(container);
  }
  const toast = document.createElement('div');
  const icons = { success:'✅', error:'❌', warning:'⚠️', info:'ℹ️' };
  toast.className = `toast ${type}`;
  toast.innerHTML = `<span>${icons[type]||'ℹ️'}</span><span>${msg}</span>`;
  container.appendChild(toast);
  setTimeout(() => toast.remove(), 3800);
}

/* ── Utilities ──────────────────────────────────────────────── */
function formatPrice(n) { return (Number(n)||0).toFixed(2) + ' MDL'; }
function fmtDate(d) { return d ? new Date(d).toLocaleDateString('ro-RO', {day:'2-digit',month:'short',year:'numeric'}) : '—'; }
function statusBadge(s) { return `<span class="badge status-${s}">${s}</span>`; }
function escHtml(s) { return String(s).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;'); }

function getUserId() {
  // Prioritate: userId din sesiunea JWT; fallback la ch_userId manual; fallback 1
  try {
    const user = JSON.parse(localStorage.getItem('ch_user') || 'null');
    if (user?.userId) return parseInt(user.userId);
  } catch {}
  const id = localStorage.getItem('ch_userId') || '1';
  return parseInt(id);
}
function setUserId(id) { localStorage.setItem('ch_userId', String(id)); }

/* ── Cart badge update ──────────────────────────────────────── */
async function refreshCartBadge() {
  // Nu apela API-ul daca nu exista token — CartController cere [Authorize]
  // si o cerere fara token de pe login.html ar declansa loop-ul 401 → redirect → 414
  if (!localStorage.getItem('ch_token')) return;
  try {
    const cart = await API.cart.get(getUserId());
    const badge = document.getElementById('cart-count');
    if (badge) badge.textContent = cart?.lineCount || 0;
  } catch {}
}

/* ── Modal helpers ──────────────────────────────────────────── */
function openModal(id) { document.getElementById(id).classList.remove('d-none'); }
function closeModal(id) { document.getElementById(id).classList.add('d-none'); }
function confirmDialog(msg) { return confirm(msg); }

/* ── Navbar active link ─────────────────────────────────────── */
document.addEventListener('DOMContentLoaded', () => {
  const path = location.pathname.replace(/\/$/, '') || '/index.html';
  document.querySelectorAll('.navbar nav a').forEach(a => {
    const href = a.getAttribute('href');
    if (href && path.endsWith(href.replace(/^\.\.\//, '/'))) a.classList.add('active');
  });
  refreshCartBadge();
});
