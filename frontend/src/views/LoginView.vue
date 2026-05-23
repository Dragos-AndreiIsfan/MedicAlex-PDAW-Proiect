<template>
  <div class="login-page">

    <!-- ═══════════════ LEFT PANEL ═══════════════ -->
    <div class="panel-left">

      <!-- Animated grid background -->
      <div class="grid-bg" aria-hidden="true"></div>

      <!-- Floating orbs -->
      <div class="orb orb-1" aria-hidden="true"></div>
      <div class="orb orb-2" aria-hidden="true"></div>
      <div class="orb orb-3" aria-hidden="true"></div>

      <!-- ECG Line SVG -->
      <svg class="ecg-svg" viewBox="0 0 600 80" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
        <path class="ecg-path" d="M0,40 L80,40 L100,40 L110,10 L120,70 L130,5 L140,60 L150,40 L230,40 L250,40 L260,10 L270,70 L280,5 L290,60 L300,40 L380,40 L400,40 L410,10 L420,70 L430,5 L440,60 L450,40 L600,40" stroke="url(#ecgGrad)" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
        <defs>
          <linearGradient id="ecgGrad" x1="0" y1="0" x2="600" y2="0" gradientUnits="userSpaceOnUse">
            <stop offset="0%" stop-color="#00d4ff" stop-opacity="0"/>
            <stop offset="40%" stop-color="#00d4ff" stop-opacity="0.9"/>
            <stop offset="60%" stop-color="#7af0c8" stop-opacity="0.9"/>
            <stop offset="100%" stop-color="#7af0c8" stop-opacity="0"/>
          </linearGradient>
        </defs>
      </svg>

      <!-- Branding content -->
      <div class="left-content">
        <div class="brand-badge">
          <span class="brand-badge-dot"></span>
          Sistem Activ
        </div>

        <div class="brand-logo" aria-label="MedSystem">
          <div class="logo-cross">
            <div class="cross-h"></div>
            <div class="cross-v"></div>
          </div>
          <span class="logo-text">Med<em>System</em></span>
        </div>

        <h1 class="left-headline">
          Platforma de<br/>
          <span class="headline-accent">Gestionare</span><br/>
          Pacienți
        </h1>

        <p class="left-desc">
          Acces securizat pentru personalul medical autorizat.
          Date protejate, fluxuri clare, decizii informate.
        </p>

        <div class="left-stats">
          <div class="stat-item">
            <span class="stat-icon">🏥</span>
            <div>
              <div class="stat-label">Spital</div>
              <div class="stat-value">inst.med.ro</div>
            </div>
          </div>
          <div class="stat-divider"></div>
          <div class="stat-item">
            <span class="stat-icon">🔒</span>
            <div>
              <div class="stat-label">Securitate</div>
              <div class="stat-value">JWT · TLS</div>
            </div>
          </div>
          <div class="stat-divider"></div>
          <div class="stat-item">
            <span class="stat-icon">⚡</span>
            <div>
              <div class="stat-label">Status</div>
              <div class="stat-value">Online</div>
            </div>
          </div>
        </div>
      </div>

    </div>

    <!-- ═══════════════ RIGHT PANEL ═══════════════ -->
    <div class="panel-right">
      <div class="form-wrapper">

        <!-- Header -->
        <div class="form-header">
          <p class="form-eyebrow">Bun venit înapoi</p>
          <h2 class="form-title">Autentificare</h2>
          <p class="form-subtitle">Introduceți credențialele contului dvs. medical</p>
        </div>

        <!-- Alert error -->
        <Transition name="alert-slide">
          <div v-if="errorMsg" class="alert-error" role="alert">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
            {{ errorMsg }}
          </div>
        </Transition>

        <!-- Form -->
        <form class="login-form" @submit.prevent="handleLogin" novalidate>

          <!-- Email field -->
          <div class="field-group" :class="{ 'field-error': fieldErrors.email, 'field-focused': focused === 'email' }">
            <label class="field-label" for="email">
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><rect x="2" y="4" width="20" height="16" rx="2"/><polyline points="2,4 12,13 22,4"/></svg>
              Adresă email
            </label>
            <div class="field-input-wrap">
              <input
                id="email"
                ref="emailRef"
                v-model="form.email"
                type="email"
                class="field-input"
                placeholder="prenume.nume@inst.med.ro"
                autocomplete="username"
                @focus="focused = 'email'"
                @blur="focused = null; validateEmail()"
                :disabled="loading"
              />
              <div class="field-border"></div>
            </div>
            <span v-if="fieldErrors.email" class="field-err-msg">{{ fieldErrors.email }}</span>
          </div>

          <!-- Password field -->
          <div class="field-group" :class="{ 'field-error': fieldErrors.password, 'field-focused': focused === 'password' }">
            <label class="field-label" for="password">
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
              Parolă
            </label>
            <div class="field-input-wrap">
              <input
                id="password"
                v-model="form.password"
                :type="showPassword ? 'text' : 'password'"
                class="field-input"
                placeholder="••••••••"
                autocomplete="current-password"
                @focus="focused = 'password'"
                @blur="focused = null; validatePassword()"
                :disabled="loading"
              />
              <button
                type="button"
                class="toggle-pw"
                @click="showPassword = !showPassword"
                :aria-label="showPassword ? 'Ascunde parola' : 'Arată parola'"
              >
                <!-- Eye open -->
                <svg v-if="!showPassword" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                <!-- Eye closed -->
                <svg v-else width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19"/><line x1="1" y1="1" x2="23" y2="23"/></svg>
              </button>
              <div class="field-border"></div>
            </div>
            <span v-if="fieldErrors.password" class="field-err-msg">{{ fieldErrors.password }}</span>
          </div>

          <!-- Role hint chips -->
          <div class="role-hints">
            <span class="hint-label">Cont pentru:</span>
            <span class="role-chip chip-admin">Administrator</span>
            <span class="role-chip chip-med">Dr. Medicină</span>
            <span class="role-chip chip-lab">Dr. Laborator</span>
          </div>

          <!-- Submit -->
          <button type="submit" class="btn-login" :disabled="loading">
            <span v-if="!loading" class="btn-login-inner">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4"/><polyline points="10 17 15 12 10 7"/><line x1="15" y1="12" x2="3" y2="12"/></svg>
              Autentificare
            </span>
            <span v-else class="btn-login-inner">
              <span class="spinner"></span>
              Se verifică...
            </span>
          </button>

        </form>

        <!-- Footer -->
        <p class="form-footer">
          Acces restricționat personalului autorizat.<br/>
          <span class="footer-muted">© {{ currentYear }} inst.med.ro — Toate drepturile rezervate</span>
        </p>

      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { authApi } from '@/api/auth'
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

const router = useRouter()
const authStore = useAuthStore()

// ── State ──────────────────────────────────────────
const form = reactive({ email: '', password: '' })
const fieldErrors = reactive({ email: '', password: '' })
const errorMsg  = ref('')
const loading   = ref(false)
const focused   = ref(null)
const showPassword = ref(false)
const emailRef  = ref(null)
const currentYear = computed(() => new Date().getFullYear())

// ── Validation ─────────────────────────────────────
function validateEmail() {
  if (!form.email) {
    fieldErrors.email = 'Emailul este obligatoriu.'
    return false
  }
  if (!form.email.includes('@')) {
    fieldErrors.email = 'Introduceți o adresă email validă.'
    return false
  }
  fieldErrors.email = ''
  return true
}

function validatePassword() {
  if (!form.password) {
    fieldErrors.password = 'Parola este obligatorie.'
    return false
  }
  if (form.password.length < 4) {
    fieldErrors.password = 'Parola trebuie să aibă minim 4 caractere.'
    return false
  }
  fieldErrors.password = ''
  return true
}

// ── Submit ─────────────────────────────────────────
async function handleLogin() {
  errorMsg.value = ''
  const emailOk    = validateEmail()
  const passwordOk = validatePassword()
  if (!emailOk || !passwordOk) return

  loading.value = true

  try {
    const { data } = await authApi.login(form.email, form.password)
    console.log('✅ Login success - token:', data.token?.substring(0,20) + '...')
    console.log('✅ User object:', JSON.stringify(data.user))
    console.log('✅ Role value:', data.user?.role)
    authStore.setAuth(data.token, data.user)
    console.log('✅ Auth stored, navigating to role:', data.user?.role)
    routeByRole(data.user?.role)
  } catch (err) {
    const msg = err.response?.data?.message
    errorMsg.value = msg || 'Email sau parolă incorectă.'
  } finally {
    loading.value = false
  }
}

function routeByRole(role) {
  // Va fi extins pe măsură ce adăugăm paginile
  const destinations = {
    admin:            '/admin',
    doctor_medicina:  '/medicina',
    doctor_laborator: '/laborator',
  }
  router.push(destinations[role] ?? '/login')
}
</script>

<style scoped>
/* ═══════════════════════════════════════════════
   LAYOUT
═══════════════════════════════════════════════ */
.login-page {
  display: flex;
  min-height: 100vh;
  background: #040d1a;
}

/* ═══════════════════════════════════════════════
   LEFT PANEL
═══════════════════════════════════════════════ */
.panel-left {
  position: relative;
  flex: 1.1;
  display: flex;
  align-items: flex-end;
  padding: 3.5rem;
  overflow: hidden;
  background: linear-gradient(140deg, #051428 0%, #071e3d 50%, #08254d 100%);
}

/* Grid background */
.grid-bg {
  position: absolute;
  inset: 0;
  background-image:
    linear-gradient(rgba(0,212,255,0.04) 1px, transparent 1px),
    linear-gradient(90deg, rgba(0,212,255,0.04) 1px, transparent 1px);
  background-size: 48px 48px;
  mask-image: radial-gradient(ellipse 80% 80% at 50% 50%, black 30%, transparent 100%);
}

/* Floating orbs */
.orb {
  position: absolute;
  border-radius: 50%;
  filter: blur(80px);
  pointer-events: none;
}
.orb-1 {
  width: 420px; height: 420px;
  background: radial-gradient(circle, rgba(0,140,255,0.18) 0%, transparent 70%);
  top: -100px; left: -80px;
  animation: orbFloat1 12s ease-in-out infinite;
}
.orb-2 {
  width: 300px; height: 300px;
  background: radial-gradient(circle, rgba(0,212,200,0.12) 0%, transparent 70%);
  bottom: 80px; right: -60px;
  animation: orbFloat2 15s ease-in-out infinite;
}
.orb-3 {
  width: 200px; height: 200px;
  background: radial-gradient(circle, rgba(120,80,255,0.10) 0%, transparent 70%);
  top: 45%; left: 40%;
  animation: orbFloat3 9s ease-in-out infinite;
}

@keyframes orbFloat1 { 0%,100% { transform: translate(0,0); } 50% { transform: translate(20px, 30px); } }
@keyframes orbFloat2 { 0%,100% { transform: translate(0,0); } 50% { transform: translate(-15px, -20px); } }
@keyframes orbFloat3 { 0%,100% { transform: translate(0,0); } 33% { transform: translate(10px,-15px); } 66% { transform: translate(-10px,10px); } }

/* ECG line */
.ecg-svg {
  position: absolute;
  bottom: 200px;
  left: 0; right: 0;
  width: 100%;
  opacity: 0.6;
}
.ecg-path {
  stroke-dasharray: 1200;
  stroke-dashoffset: 1200;
  animation: ecgDraw 3s ease forwards 0.5s, ecgPulse 4s ease-in-out infinite 3.5s;
}
@keyframes ecgDraw {
  to { stroke-dashoffset: 0; }
}
@keyframes ecgPulse {
  0%, 100% { opacity: 0.6; }
  50%       { opacity: 1;   }
}

/* Left content */
.left-content {
  position: relative;
  z-index: 2;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  animation: contentReveal 0.8s ease both 0.2s;
}
@keyframes contentReveal {
  from { opacity: 0; transform: translateY(24px); }
  to   { opacity: 1; transform: translateY(0); }
}

.brand-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.35rem 0.9rem;
  border-radius: 100px;
  border: 1px solid rgba(0,212,255,0.3);
  background: rgba(0,212,255,0.08);
  font-size: 0.78rem;
  font-weight: 600;
  color: #7af0c8;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  width: fit-content;
}
.brand-badge-dot {
  width: 7px; height: 7px;
  border-radius: 50%;
  background: #7af0c8;
  box-shadow: 0 0 6px #7af0c8;
  animation: pulse 2s ease infinite;
}
@keyframes pulse {
  0%,100% { box-shadow: 0 0 6px #7af0c8; }
  50%      { box-shadow: 0 0 14px #7af0c8, 0 0 24px rgba(122,240,200,0.4); }
}

.brand-logo {
  display: flex;
  align-items: center;
  gap: 0.85rem;
}
.logo-cross {
  position: relative;
  width: 36px; height: 36px;
}
.cross-h, .cross-v {
  position: absolute;
  background: linear-gradient(135deg, #00d4ff, #7af0c8);
  border-radius: 3px;
}
.cross-h { width: 100%; height: 33%; top: 33%; left: 0; }
.cross-v { width: 33%; height: 100%; left: 33%; top: 0; }

.logo-text {
  font-family: var(--font-display);
  font-size: 1.5rem;
  color: #e8edf5;
  letter-spacing: -0.01em;
}
.logo-text em {
  font-style: italic;
  background: linear-gradient(90deg, #00d4ff, #7af0c8);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.left-headline {
  font-family: var(--font-display);
  font-size: clamp(2.2rem, 3.5vw, 3rem);
  line-height: 1.15;
  color: #e8edf5;
}
.headline-accent {
  background: linear-gradient(90deg, #00d4ff 0%, #7af0c8 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.left-desc {
  font-size: 0.95rem;
  color: rgba(200,215,235,0.65);
  line-height: 1.7;
  max-width: 380px;
}

.left-stats {
  display: flex;
  align-items: center;
  gap: 1.25rem;
  flex-wrap: wrap;
}
.stat-item {
  display: flex;
  align-items: center;
  gap: 0.65rem;
}
.stat-icon { font-size: 1.1rem; }
.stat-label {
  font-size: 0.7rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: rgba(200,215,235,0.45);
}
.stat-value {
  font-size: 0.85rem;
  font-weight: 600;
  color: rgba(200,215,235,0.85);
}
.stat-divider {
  width: 1px; height: 28px;
  background: rgba(200,215,235,0.12);
}

/* ═══════════════════════════════════════════════
   RIGHT PANEL
═══════════════════════════════════════════════ */
.panel-right {
  flex: 0 0 480px;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2.5rem 3rem;
  background: #07111f;
  border-left: 1px solid rgba(255,255,255,0.06);
  position: relative;
}
.panel-right::before {
  content: '';
  position: absolute;
  inset: 0;
  background: radial-gradient(ellipse 70% 50% at 50% 0%, rgba(0,180,255,0.05) 0%, transparent 70%);
  pointer-events: none;
}

.form-wrapper {
  width: 100%;
  max-width: 360px;
  display: flex;
  flex-direction: column;
  gap: 1.75rem;
  animation: formReveal 0.7s ease both 0.4s;
}
@keyframes formReveal {
  from { opacity: 0; transform: translateY(20px); }
  to   { opacity: 1; transform: translateY(0); }
}

/* Form header */
.form-header { display: flex; flex-direction: column; gap: 0.35rem; }

.form-eyebrow {
  font-size: 0.78rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  color: #00d4ff;
}
.form-title {
  font-family: var(--font-display);
  font-size: 2rem;
  color: #e8edf5;
  line-height: 1.15;
}
.form-subtitle {
  font-size: 0.875rem;
  color: rgba(200,215,235,0.5);
  margin-top: 0.1rem;
}

/* Alert */
.alert-error {
  display: flex;
  align-items: flex-start;
  gap: 0.6rem;
  padding: 0.85rem 1rem;
  border-radius: 10px;
  background: rgba(239,68,68,0.1);
  border: 1px solid rgba(239,68,68,0.25);
  color: #fca5a5;
  font-size: 0.875rem;
  line-height: 1.5;
}
.alert-error svg { flex-shrink: 0; margin-top: 2px; }

.alert-slide-enter-active { animation: alertIn 0.25s ease; }
.alert-slide-leave-active { animation: alertIn 0.2s ease reverse; }
@keyframes alertIn {
  from { opacity: 0; transform: translateY(-6px); }
  to   { opacity: 1; transform: translateY(0); }
}

/* ── FORM ── */
.login-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

/* Field group */
.field-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.field-label {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.78rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.07em;
  color: rgba(200,215,235,0.55);
  transition: color 0.2s;
}
.field-focused .field-label { color: #00d4ff; }
.field-error   .field-label { color: #fca5a5; }

.field-input-wrap {
  position: relative;
}

.field-input {
  width: 100%;
  padding: 0.8rem 2.75rem 0.8rem 1rem;
  background: rgba(255,255,255,0.04);
  border: 1.5px solid rgba(255,255,255,0.08);
  border-radius: 10px;
  color: #e8edf5;
  font-family: var(--font-ui);
  font-size: 0.95rem;
  outline: none;
  transition: border-color 0.2s, background 0.2s, box-shadow 0.2s;
}
.field-input::placeholder { color: rgba(200,215,235,0.25); }
.field-input:disabled { opacity: 0.5; cursor: not-allowed; }

.field-focused .field-input {
  border-color: #00d4ff;
  background: rgba(0,212,255,0.05);
  box-shadow: 0 0 0 3px rgba(0,212,255,0.1);
}
.field-error .field-input {
  border-color: rgba(239,68,68,0.5);
  background: rgba(239,68,68,0.04);
}

/* Animated bottom border */
.field-border {
  position: absolute;
  bottom: -1px; left: 50%;
  width: 0; height: 2px;
  background: linear-gradient(90deg, #00d4ff, #7af0c8);
  border-radius: 1px;
  transform: translateX(-50%);
  transition: width 0.3s ease;
  pointer-events: none;
}
.field-focused .field-border { width: calc(100% - 16px); }

.toggle-pw {
  position: absolute;
  right: 0.9rem; top: 50%;
  transform: translateY(-50%);
  background: none; border: none; cursor: pointer;
  color: rgba(200,215,235,0.35);
  display: flex; align-items: center;
  padding: 0.2rem;
  transition: color 0.2s;
}
.toggle-pw:hover { color: rgba(200,215,235,0.8); }

.field-err-msg {
  font-size: 0.775rem;
  color: #fca5a5;
  display: flex;
  align-items: center;
  gap: 0.3rem;
  padding-left: 0.25rem;
}

/* Role hint chips */
.role-hints {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
  padding: 0.1rem 0;
}
.hint-label {
  font-size: 0.75rem;
  color: rgba(200,215,235,0.35);
  margin-right: 0.15rem;
}
.role-chip {
  padding: 0.2rem 0.65rem;
  border-radius: 100px;
  font-size: 0.72rem;
  font-weight: 600;
  letter-spacing: 0.03em;
}
.chip-admin { background: rgba(168,85,247,0.12); color: #c084fc; border: 1px solid rgba(168,85,247,0.2); }
.chip-med   { background: rgba(0,212,255,0.1);   color: #67e8f9;  border: 1px solid rgba(0,212,255,0.2); }
.chip-lab   { background: rgba(122,240,200,0.1); color: #7af0c8;  border: 1px solid rgba(122,240,200,0.2); }

/* Login button */
.btn-login {
  width: 100%;
  padding: 0.9rem;
  margin-top: 0.25rem;
  border: none;
  border-radius: 10px;
  background: linear-gradient(135deg, #0088cc 0%, #00b4d8 50%, #7af0c8 100%);
  background-size: 200% 100%;
  background-position: 0% 0%;
  color: #04111f;
  font-family: var(--font-ui);
  font-size: 0.95rem;
  font-weight: 700;
  cursor: pointer;
  transition: background-position 0.4s ease, transform 0.2s ease, box-shadow 0.2s ease, opacity 0.2s;
  position: relative;
  overflow: hidden;
}
.btn-login:not(:disabled):hover {
  background-position: 100% 0%;
  transform: translateY(-2px);
  box-shadow: 0 8px 28px rgba(0,180,216,0.35);
}
.btn-login:not(:disabled):active { transform: translateY(0); }
.btn-login:disabled { opacity: 0.55; cursor: not-allowed; }

.btn-login-inner {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.6rem;
}

/* Spinner */
.spinner {
  display: inline-block;
  width: 16px; height: 16px;
  border: 2px solid rgba(4,17,31,0.3);
  border-top-color: #04111f;
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

/* Form footer */
.form-footer {
  text-align: center;
  font-size: 0.78rem;
  color: rgba(200,215,235,0.3);
  line-height: 1.8;
}
.footer-muted { color: rgba(200,215,235,0.2); }

/* ═══════════════════════════════════════════════
   RESPONSIVE
═══════════════════════════════════════════════ */
@media (max-width: 900px) {
  .login-page { flex-direction: column; }
  .panel-left {
    flex: none;
    padding: 2.5rem 2rem;
    min-height: 320px;
    align-items: flex-start;
  }
  .ecg-svg { bottom: 50px; }
  .left-stats { display: none; }
  .left-headline { font-size: 2rem; }
  .panel-right {
    flex: none;
    border-left: none;
    border-top: 1px solid rgba(255,255,255,0.06);
    padding: 2.5rem 2rem;
  }
  .form-wrapper { max-width: 100%; }
}
</style>
