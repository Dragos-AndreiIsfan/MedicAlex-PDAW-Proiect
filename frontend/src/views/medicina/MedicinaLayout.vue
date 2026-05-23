<template>
  <div class="med-shell">

    <!-- ═══ SIDEBAR ═══ -->
    <aside class="sidebar" :class="{ collapsed: sidebarCollapsed }">

      <div class="sidebar-brand">
        <div class="brand-mark">
          <div class="bm-h"></div>
          <div class="bm-v"></div>
        </div>
        <Transition name="lf">
          <span v-if="!sidebarCollapsed" class="brand-name">Med<em>System</em></span>
        </Transition>
      </div>

      <Transition name="lf">
        <div v-if="!sidebarCollapsed" class="sidebar-role">
          <span class="role-dot"></span>
          Doctor Medicină
        </div>
      </Transition>

      <nav class="sidebar-nav">
        <p v-if="!sidebarCollapsed" class="nav-section">Pacienti</p>

        <RouterLink to="/medicina" end class="nav-item" active-class="nav-active">
          <span class="nav-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/></svg>
          </span>
          <Transition name="lf"><span v-if="!sidebarCollapsed" class="nav-label">Dashboard</span></Transition>
        </RouterLink>

        <RouterLink to="/medicina/pacientii-mei" class="nav-item" active-class="nav-active">
          <span class="nav-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
          </span>
          <Transition name="lf">
            <span v-if="!sidebarCollapsed" class="nav-label">
              Pacienții Mei
              <span v-if="myPatientCount > 0" class="nav-badge">{{ myPatientCount }}</span>
            </span>
          </Transition>
        </RouterLink>

        <RouterLink to="/medicina/pacienti" class="nav-item" active-class="nav-active">
          <span class="nav-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
          </span>
          <Transition name="lf"><span v-if="!sidebarCollapsed" class="nav-label">Pacienți</span></Transition>
        </RouterLink>

        <p v-if="!sidebarCollapsed" class="nav-section">Doctori</p>

        <RouterLink to="/medicina/doctori-medicina" class="nav-item" active-class="nav-active">
          <span class="nav-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>
          </span>
          <Transition name="lf"><span v-if="!sidebarCollapsed" class="nav-label">Doctori Medicină</span></Transition>
        </RouterLink>

        <RouterLink to="/medicina/doctori-laborator" class="nav-item" active-class="nav-active">
          <span class="nav-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M9 3H5a2 2 0 0 0-2 2v4m6-6h10a2 2 0 0 1 2 2v4M9 3v18m0 0h10a2 2 0 0 0 2-2V9M9 21H5a2 2 0 0 1-2-2V9m0 0h18"/></svg>
          </span>
          <Transition name="lf"><span v-if="!sidebarCollapsed" class="nav-label">Doctori Laborator</span></Transition>
        </RouterLink>
      </nav>

      <button class="sidebar-toggle" @click="sidebarCollapsed = !sidebarCollapsed">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" width="16" height="16"><line x1="3" y1="12" x2="21" y2="12"/><line x1="3" y1="6" x2="21" y2="6"/><line x1="3" y1="18" x2="21" y2="18"/></svg>
      </button>

      <div class="sidebar-user">
        <div class="user-avatar">{{ avatarInitials }}</div>
        <Transition name="lf">
          <div v-if="!sidebarCollapsed" class="user-info">
            <span class="user-name">{{ authStore.fullName }}</span>
            <span class="user-spec">{{ userSpec }}</span>
          </div>
        </Transition>
        <Transition name="lf">
          <button v-if="!sidebarCollapsed" class="btn-logout" @click="handleLogout" title="Deconectare">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" width="15" height="15"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/></svg>
          </button>
        </Transition>
      </div>

    </aside>

    <!-- ═══ MAIN ═══ -->
    <main class="main-content">
      <header class="topbar">
        <div class="topbar-left">
          <h1 class="topbar-title">{{ pageTitle }}</h1>
          <div class="breadcrumb">
            <span>Medicină</span>
            <span class="bc-sep">›</span>
            <span class="bc-cur">{{ pageTitle }}</span>
          </div>
        </div>
        <div class="topbar-right">
          <div class="topbar-time">{{ currentTime }}</div>
          <div class="topbar-status">
            <span class="status-dot"></span>
            Online
          </div>
        </div>
      </header>

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
import { medicinaApi } from '@/api/medicina'

const authStore        = useAuthStore()
const router           = useRouter()
const route            = useRoute()
const sidebarCollapsed   = ref(false)
const showPasswordModal = ref(false)
const currentTime      = ref('')
const myPatientCount   = ref(0)

async function fetchMyPatientCount() {
  try {
    const { data } = await medicinaApi.getMyPatients()
    myPatientCount.value = data.length
  } catch (e) {
    console.error('Eroare la fetch count:', e)
  }
}

const pageTitles = {
  '/medicina':                  'Dashboard',
  '/medicina/pacientii-mei':    'Pacienții Mei',
  '/medicina/pacienti':         'Pacienți',
  '/medicina/doctori-medicina': 'Doctori Medicină',
  '/medicina/doctori-laborator':'Doctori Laborator',
}
const pageTitle = computed(() => {
  if (route.path.includes('/medicina/pacient/')) return 'Detalii Pacient'
  return pageTitles[route.path] || 'Dashboard'
})

const avatarInitials = computed(() => {
  const name = authStore.fullName
  if (!name) return 'D'
  const parts = name.split(' ')
  return parts.length >= 2
    ? (parts[0][0] + parts[1][0]).toUpperCase()
    : name[0].toUpperCase()
})

const userSpec = computed(() => authStore.user?.specialization || 'Medicină Generală')

function updateTime() {
  currentTime.value = new Date().toLocaleTimeString('ro-RO', { hour: '2-digit', minute: '2-digit' })
}
function handleLogout() {
  authStore.logout()
  router.push('/login')
}

let timer
onMounted(() => { updateTime(); timer = setInterval(updateTime, 60000); fetchMyPatientCount() })
onUnmounted(() => clearInterval(timer))
</script>

<style scoped>
.med-shell { display: flex; min-height: 100vh; background: #f0f4f9; }

/* Sidebar — identical structure to admin, teal accent */
.sidebar {
  width: 240px; min-height: 100vh;
  background: #071a2e;
  display: flex; flex-direction: column;
  position: sticky; top: 0; flex-shrink: 0;
  border-right: 1px solid rgba(255,255,255,0.05);
  transition: width 0.3s cubic-bezier(0.4,0,0.2,1);
  z-index: 100; overflow: hidden;
}
.sidebar.collapsed { width: 68px; }

.sidebar-brand {
  display: flex; align-items: center; gap: 0.75rem;
  padding: 1.5rem 1.1rem 1rem;
  border-bottom: 1px solid rgba(255,255,255,0.05); overflow: hidden;
}
.brand-mark {
  width: 34px; height: 34px; border-radius: 8px;
  background: linear-gradient(135deg, #0ea5e9, #06b6d4);
  position: relative; flex-shrink: 0;
  display: flex; align-items: center; justify-content: center;
}
.bm-h, .bm-v { position: absolute; background: rgba(255,255,255,0.9); border-radius: 2px; }
.bm-h { width: 60%; height: 22%; }
.bm-v { width: 22%; height: 60%; }
.brand-name {
  font-family: 'DM Serif Display', serif; font-size: 1.25rem;
  color: #e8edf5; white-space: nowrap;
}
.brand-name em {
  font-style: italic;
  background: linear-gradient(90deg, #38bdf8, #67e8f9);
  -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text;
}

.sidebar-role {
  margin: 0.6rem 1.1rem 0.2rem;
  display: flex; align-items: center; gap: 0.5rem;
  padding: 0.35rem 0.75rem;
  background: rgba(14,165,233,0.12);
  border: 1px solid rgba(14,165,233,0.25);
  border-radius: 100px;
  font-size: 0.72rem; font-weight: 700; color: #38bdf8;
  letter-spacing: 0.06em; text-transform: uppercase;
  white-space: nowrap; width: fit-content;
}
.role-dot { width: 6px; height: 6px; border-radius: 50%; background: #38bdf8; flex-shrink: 0; animation: rp 2s infinite; }
@keyframes rp { 0%,100%{box-shadow:0 0 4px #38bdf8}50%{box-shadow:0 0 10px #38bdf8} }

.sidebar-nav { flex: 1; padding: 0.75rem 0.65rem; display: flex; flex-direction: column; gap: 0.15rem; overflow: hidden; }
.nav-section {
  font-size: 0.65rem; font-weight: 800; text-transform: uppercase;
  letter-spacing: 0.1em; color: rgba(200,215,235,0.3);
  padding: 0.75rem 0.5rem 0.3rem; white-space: nowrap;
}
.nav-item {
  display: flex; align-items: center; gap: 0.75rem;
  padding: 0.65rem 0.75rem; border-radius: 9px;
  text-decoration: none; color: rgba(200,215,235,0.55);
  font-size: 0.875rem; font-weight: 500;
  transition: all 0.18s; white-space: nowrap; overflow: hidden;
}
.nav-item:hover { background: rgba(255,255,255,0.06); color: rgba(200,215,235,0.9); }
.nav-active { background: rgba(14,165,233,0.12) !important; color: #38bdf8 !important; border: 1px solid rgba(56,189,248,0.15); }
.nav-icon { width: 20px; height: 20px; flex-shrink: 0; display: flex; align-items: center; justify-content: center; }
.nav-icon svg { width: 18px; height: 18px; }
.nav-label { display: flex; align-items: center; gap: 0.5rem; flex: 1; overflow: hidden; }
.nav-badge {
  margin-left: auto; min-width: 20px; height: 20px; padding: 0 0.4rem;
  background: #0ea5e9; color: #fff;
  border-radius: 100px; font-size: 0.7rem; font-weight: 700;
  display: flex; align-items: center; justify-content: center;
}

.sidebar-toggle {
  margin: 0 0.65rem 0.5rem; padding: 0.6rem;
  background: rgba(255,255,255,0.04); border: 1px solid rgba(255,255,255,0.07);
  border-radius: 8px; color: rgba(200,215,235,0.4);
  cursor: pointer; display: flex; align-items: center; justify-content: center;
  transition: all 0.18s; width: calc(100% - 1.3rem);
}
.sidebar-toggle:hover { background: rgba(255,255,255,0.08); color: rgba(200,215,235,0.8); }

.sidebar-user {
  padding: 0.9rem 0.75rem; border-top: 1px solid rgba(255,255,255,0.05);
  display: flex; align-items: center; gap: 0.65rem; overflow: hidden;
}
.user-avatar {
  width: 34px; height: 34px; border-radius: 50%;
  background: linear-gradient(135deg, #0ea5e9, #06b6d4);
  display: flex; align-items: center; justify-content: center;
  font-size: 0.78rem; font-weight: 700; color: #fff; flex-shrink: 0;
}
.user-info { flex: 1; display: flex; flex-direction: column; min-width: 0; overflow: hidden; }
.user-name { font-size: 0.82rem; font-weight: 600; color: #e8edf5; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.user-spec { font-size: 0.7rem; color: #38bdf8; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; font-weight: 500; }

.btn-password {
  background: none; border: none; cursor: pointer; color: rgba(200,215,235,0.35);
  padding: 0.3rem; border-radius: 6px; display: flex; align-items: center;
  transition: all 0.18s; flex-shrink: 0;
}
.btn-password:hover { color: #fbbf24; background: rgba(251,191,36,0.1); }
.btn-logout {
  background: none; border: none; cursor: pointer; color: rgba(200,215,235,0.35);
  padding: 0.3rem; border-radius: 6px; display: flex; align-items: center;
  transition: all 0.18s; flex-shrink: 0;
}
.btn-logout:hover { color: #fca5a5; background: rgba(239,68,68,0.1); }

/* Transitions */
.lf-enter-active { transition: opacity 0.2s ease 0.1s, transform 0.2s ease 0.1s; }
.lf-leave-active { transition: opacity 0.1s ease, transform 0.1s ease; }
.lf-enter-from, .lf-leave-to { opacity: 0; transform: translateX(-6px); }

/* Main */
.main-content { flex: 1; display: flex; flex-direction: column; min-width: 0; }

.topbar {
  background: #fff; border-bottom: 1px solid #e8edf5;
  padding: 0 2rem; height: 64px;
  display: flex; align-items: center; justify-content: space-between;
  position: sticky; top: 0; z-index: 50;
  box-shadow: 0 1px 3px rgba(15,34,64,0.06);
}
.topbar-title { font-family: 'DM Serif Display', serif; font-size: 1.15rem; color: #071a2e; }
.breadcrumb { display: flex; align-items: center; gap: 0.4rem; font-size: 0.75rem; color: #94a3b8; margin-top: 0.15rem; }
.bc-sep { color: #cbd5e1; }
.bc-cur { color: #0ea5e9; font-weight: 600; }
.topbar-right { display: flex; align-items: center; gap: 1rem; }
.topbar-time { font-size: 0.82rem; font-weight: 600; color: #64748b; font-variant-numeric: tabular-nums; }
.topbar-status {
  display: flex; align-items: center; gap: 0.4rem;
  padding: 0.3rem 0.75rem; background: #f0fdf4;
  border: 1px solid #bbf7d0; border-radius: 100px;
  font-size: 0.72rem; font-weight: 700; color: #16a34a;
}
.status-dot { width: 6px; height: 6px; border-radius: 50%; background: #22c55e; animation: sp 2s infinite; }
@keyframes sp { 0%,100%{box-shadow:0 0 0 0 rgba(34,197,94,.4)}50%{box-shadow:0 0 0 4px rgba(34,197,94,0)} }

.page-content { flex: 1; padding: 2rem; overflow-y: auto; }
</style>
