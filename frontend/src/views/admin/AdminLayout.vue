<template>
  <div class="admin-shell">

    <!-- ═══ SIDEBAR ═══ -->
    <aside class="sidebar" :class="{ collapsed: sidebarCollapsed }">

      <!-- Brand -->
      <div class="sidebar-brand">
        <div class="brand-mark">
          <div class="bm-cross">
            <div class="bm-h"></div>
            <div class="bm-v"></div>
          </div>
        </div>
        <Transition name="label-fade">
          <span v-if="!sidebarCollapsed" class="brand-name">Medic<em>Alex</em></span>
        </Transition>
      </div>

      <!-- Role badge -->
      <Transition name="label-fade">
        <div v-if="!sidebarCollapsed" class="sidebar-role">
          <span class="role-dot"></span>
          Administrator
        </div>
      </Transition>

      <!-- Navigation -->
      <nav class="sidebar-nav">
        <p v-if="!sidebarCollapsed" class="nav-section-label">Principal</p>

        <RouterLink to="/admin" end class="nav-item" active-class="nav-active">
          <span class="nav-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
              <rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/>
              <rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/>
            </svg>
          </span>
          <Transition name="label-fade">
            <span v-if="!sidebarCollapsed" class="nav-label">Dashboard</span>
          </Transition>
        </RouterLink>

        <p v-if="!sidebarCollapsed" class="nav-section-label">Gestionare</p>

        <RouterLink to="/admin/doctori" class="nav-item" active-class="nav-active">
          <span class="nav-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
              <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
              <circle cx="9" cy="7" r="4"/>
              <path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/>
            </svg>
          </span>
          <Transition name="label-fade">
            <span v-if="!sidebarCollapsed" class="nav-label">Doctori</span>
          </Transition>
        </RouterLink>

        <RouterLink to="/admin/pacienti" class="nav-item" active-class="nav-active">
          <span class="nav-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
              <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>
              <circle cx="12" cy="7" r="4"/>
            </svg>
          </span>
          <Transition name="label-fade">
            <span v-if="!sidebarCollapsed" class="nav-label">Pacienți</span>
          </Transition>
        </RouterLink>
      </nav>

      <!-- Collapse toggle -->
      <button class="sidebar-toggle" @click="sidebarCollapsed = !sidebarCollapsed" :title="sidebarCollapsed ? 'Extinde' : 'Restrânge'">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round">
          <line x1="3" y1="12" x2="21" y2="12"/><line x1="3" y1="6" x2="21" y2="6"/><line x1="3" y1="18" x2="21" y2="18"/>
        </svg>
      </button>

      <!-- User section -->
      <div class="sidebar-user">
        <div class="user-avatar">{{ avatarInitials }}</div>
        <Transition name="label-fade">
          <div v-if="!sidebarCollapsed" class="user-info">
            <span class="user-name">{{ authStore.fullName }}</span>
            <span class="user-email">{{ authStore.user?.email || 'admin@inst.med.ro' }}</span>
          </div>
        </Transition>
        <Transition name="label-fade">
          <button v-if="!sidebarCollapsed" class="btn-logout" @click="handleLogout" title="Deconectare">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" width="15" height="15">
              <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
              <polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/>
            </svg>
          </button>
        </Transition>
      </div>

    </aside>

    <!-- ═══ MAIN CONTENT ═══ -->
    <main class="main-content">

      <!-- Topbar -->
      <header class="topbar">
        <div class="topbar-left">
          <h1 class="topbar-title">{{ pageTitle }}</h1>
          <div class="breadcrumb">
            <span>Admin</span>
            <span class="bc-sep">›</span>
            <span class="bc-current">{{ pageTitle }}</span>
          </div>
        </div>
        <div class="topbar-right">
          <div class="topbar-time">{{ currentTime }}</div>
          <div class="topbar-badge">
            <span class="tb-dot"></span>
            Sistem activ
          </div>
        </div>
      </header>

      <!-- Page content -->
      <div class="page-content">
        <RouterView />
      </div>

    </main>

  </div>
    <ChangePasswordModal :show="showPasswordModal" @close="showPasswordModal = false" />
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { RouterLink, RouterView, useRoute, useRouter } from 'vue-router'
import ChangePasswordModal from '@/views/shared/ChangePasswordModal.vue'
import { useAuthStore } from '@/stores/auth'

const authStore       = useAuthStore()
const router          = useRouter()
const route           = useRoute()
const sidebarCollapsed   = ref(false)
const showPasswordModal = ref(false)
const currentTime      = ref('')

const pageTitles = {
  '/admin':          'Dashboard',
  '/admin/doctori':  'Gestionare Doctori',
  '/admin/pacienti': 'Gestionare Pacienți',
}

const pageTitle = computed(() => pageTitles[route.path] || 'Admin')

const avatarInitials = computed(() => {
  const name = authStore.fullName
  if (!name) return 'A'
  const parts = name.split(' ')
  return parts.length >= 2
    ? (parts[0][0] + parts[1][0]).toUpperCase()
    : name[0].toUpperCase()
})

function updateTime() {
  const now = new Date()
  currentTime.value = now.toLocaleTimeString('ro-RO', { hour: '2-digit', minute: '2-digit' })
}

function handleLogout() {
  authStore.logout()
  router.push('/login')
}

let timer
onMounted(() => { updateTime(); timer = setInterval(updateTime, 60000) })
onUnmounted(() => clearInterval(timer))
</script>

<style scoped>
/* ═══════════════════════════════════════
   SHELL
═══════════════════════════════════════ */
.admin-shell {
  display: flex;
  min-height: 100vh;
  background: #f0f4f9;
}

/* ═══════════════════════════════════════
   SIDEBAR
═══════════════════════════════════════ */
.sidebar {
  width: 240px;
  min-height: 100vh;
  background: #07111f;
  display: flex;
  flex-direction: column;
  padding: 0;
  transition: width 0.3s cubic-bezier(0.4,0,0.2,1);
  position: sticky;
  top: 0;
  flex-shrink: 0;
  border-right: 1px solid rgba(255,255,255,0.05);
  z-index: 100;
}
.sidebar.collapsed { width: 68px; }

/* Brand */
.sidebar-brand {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1.5rem 1.1rem 1rem;
  border-bottom: 1px solid rgba(255,255,255,0.05);
  overflow: hidden;
}
.brand-mark {
  width: 34px; height: 34px;
  background: linear-gradient(135deg, #0088cc, #00d4aa);
  border-radius: 8px;
  position: relative;
  flex-shrink: 0;
}
.bm-cross { position: absolute; inset: 0; display: flex; align-items: center; justify-content: center; }
.bm-h, .bm-v {
  position: absolute;
  background: rgba(255,255,255,0.9);
  border-radius: 2px;
}
.bm-h { width: 60%; height: 22%; }
.bm-v { width: 22%; height: 60%; }
.brand-name {
  font-family: 'DM Serif Display', serif;
  font-size: 1.25rem;
  color: #e8edf5;
  white-space: nowrap;
}
.brand-name em {
  font-style: italic;
  background: linear-gradient(90deg, #00d4ff, #7af0c8);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

/* Role badge */
.sidebar-role {
  margin: 0.6rem 1.1rem 0.2rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.35rem 0.75rem;
  background: rgba(168,85,247,0.12);
  border: 1px solid rgba(168,85,247,0.2);
  border-radius: 100px;
  font-size: 0.72rem;
  font-weight: 700;
  color: #c084fc;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  white-space: nowrap;
  overflow: hidden;
  width: fit-content;
}
.role-dot {
  width: 6px; height: 6px;
  border-radius: 50%;
  background: #c084fc;
  flex-shrink: 0;
}

/* Nav */
.sidebar-nav {
  flex: 1;
  padding: 0.75rem 0.65rem;
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
  overflow: hidden;
}
.nav-section-label {
  font-size: 0.65rem;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  color: rgba(200,215,235,0.3);
  padding: 0.75rem 0.5rem 0.3rem;
  white-space: nowrap;
}
.nav-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.65rem 0.75rem;
  border-radius: 9px;
  text-decoration: none;
  color: rgba(200,215,235,0.55);
  font-size: 0.875rem;
  font-weight: 500;
  transition: all 0.18s ease;
  white-space: nowrap;
  overflow: hidden;
}
.nav-item:hover {
  background: rgba(255,255,255,0.06);
  color: rgba(200,215,235,0.9);
}
.nav-active {
  background: rgba(0,180,216,0.12) !important;
  color: #00d4ff !important;
  border: 1px solid rgba(0,212,255,0.15);
}
.nav-icon {
  width: 20px; height: 20px;
  flex-shrink: 0;
  display: flex; align-items: center; justify-content: center;
}
.nav-icon svg { width: 18px; height: 18px; }
.nav-label { overflow: hidden; }

/* Toggle */
.sidebar-toggle {
  margin: 0 0.65rem 0.5rem;
  padding: 0.6rem;
  background: rgba(255,255,255,0.04);
  border: 1px solid rgba(255,255,255,0.07);
  border-radius: 8px;
  color: rgba(200,215,235,0.4);
  cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  transition: all 0.18s;
  width: calc(100% - 1.3rem);
}
.sidebar-toggle svg { width: 16px; height: 16px; }
.sidebar-toggle:hover {
  background: rgba(255,255,255,0.08);
  color: rgba(200,215,235,0.8);
}

/* User */
.sidebar-user {
  padding: 0.9rem 0.75rem;
  border-top: 1px solid rgba(255,255,255,0.05);
  display: flex;
  align-items: center;
  gap: 0.65rem;
  overflow: hidden;
}
.user-avatar {
  width: 34px; height: 34px;
  border-radius: 50%;
  background: linear-gradient(135deg, #0088cc, #00d4aa);
  display: flex; align-items: center; justify-content: center;
  font-size: 0.78rem; font-weight: 700; color: #fff;
  flex-shrink: 0;
}
.user-info {
  flex: 1;
  display: flex; flex-direction: column;
  overflow: hidden;
  min-width: 0;
}
.user-name {
  font-size: 0.82rem;
  font-weight: 600;
  color: #e8edf5;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.user-email {
  font-size: 0.7rem;
  color: rgba(200,215,235,0.4);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.btn-password {
  background: none; border: none; cursor: pointer; color: rgba(200,215,235,0.35);
  padding: 0.3rem; border-radius: 6px; display: flex; align-items: center;
  transition: all 0.18s; flex-shrink: 0;
}
.btn-password:hover { color: #fbbf24; background: rgba(251,191,36,0.1); }
.btn-logout {
  background: none; border: none;
  color: rgba(200,215,235,0.35);
  cursor: pointer; padding: 0.3rem;
  border-radius: 6px;
  display: flex; align-items: center;
  transition: all 0.18s;
  flex-shrink: 0;
}
.btn-logout:hover { color: #fca5a5; background: rgba(239,68,68,0.1); }

/* Transition */
.label-fade-enter-active { transition: opacity 0.2s ease 0.1s, transform 0.2s ease 0.1s; }
.label-fade-leave-active { transition: opacity 0.1s ease, transform 0.1s ease; }
.label-fade-enter-from  { opacity: 0; transform: translateX(-6px); }
.label-fade-leave-to    { opacity: 0; transform: translateX(-6px); }

/* ═══════════════════════════════════════
   MAIN
═══════════════════════════════════════ */
.main-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
}

/* Topbar */
.topbar {
  background: #fff;
  border-bottom: 1px solid #e8edf5;
  padding: 0 2rem;
  height: 64px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  position: sticky;
  top: 0;
  z-index: 50;
  box-shadow: 0 1px 3px rgba(15,34,64,0.06);
}
.topbar-title {
  font-family: 'DM Serif Display', serif;
  font-size: 1.15rem;
  color: #07111f;
  line-height: 1;
}
.breadcrumb {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.75rem;
  color: #94a3b8;
  margin-top: 0.15rem;
}
.bc-sep { color: #cbd5e1; }
.bc-current { color: #0088cc; font-weight: 600; }
.topbar-right {
  display: flex;
  align-items: center;
  gap: 1rem;
}
.topbar-time {
  font-size: 0.82rem;
  font-weight: 600;
  color: #64748b;
  font-variant-numeric: tabular-nums;
}
.topbar-badge {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.3rem 0.75rem;
  background: #f0fdf4;
  border: 1px solid #bbf7d0;
  border-radius: 100px;
  font-size: 0.72rem;
  font-weight: 700;
  color: #16a34a;
  letter-spacing: 0.04em;
}
.tb-dot {
  width: 6px; height: 6px;
  border-radius: 50%;
  background: #22c55e;
  animation: tbPulse 2s ease infinite;
}
@keyframes tbPulse {
  0%,100% { box-shadow: 0 0 0 0 rgba(34,197,94,0.4); }
  50% { box-shadow: 0 0 0 4px rgba(34,197,94,0); }
}

/* Page content area */
.page-content {
  flex: 1;
  padding: 2rem;
  overflow-y: auto;
}
</style>
