<template>
  <div class="dashboard">

    <div class="welcome-banner">
      <div class="wb-text">
        <p class="wb-greeting">{{ greeting }},</p>
        <h2 class="wb-name">Dr. {{ authStore.fullName }}</h2>
        <p class="wb-spec">{{ authStore.user?.specialization || 'Laborator' }}</p>
        <p class="wb-date">{{ currentDate }}</p>
      </div>
      <div class="wb-visual" aria-hidden="true">
        <div class="flask-wrap">
          <div class="flask-body">
            <div class="flask-liquid"></div>
            <div class="bubble b1"></div>
            <div class="bubble b2"></div>
            <div class="bubble b3"></div>
          </div>
        </div>
        <div class="pulse-ring r1"></div>
        <div class="pulse-ring r2"></div>
      </div>
    </div>

    <!-- Stats -->
    <div class="stats-grid">
      <div class="stat-card" v-for="s in stats" :key="s.label">
        <div class="sc-icon" :style="{ background: s.bg }">{{ s.icon }}</div>
        <div>
          <p class="sc-val">{{ s.value }}</p>
          <p class="sc-lbl">{{ s.label }}</p>
        </div>
      </div>
    </div>

    <!-- Two cols -->
    <div class="mid-grid">

      <!-- Pending requests -->
      <div class="dash-card">
        <div class="dc-head">
          <h3 class="dc-title">Cereri în Așteptare</h3>
          <RouterLink to="/laborator/cereri" class="dc-link">Vezi toate →</RouterLink>
        </div>
        <div class="dc-body">
          <div class="req-list">
            <div class="req-row" v-for="r in pendingRequests" :key="r.id">
              <div class="rr-left">
                <div class="rr-icon">🔬</div>
                <div class="rr-info">
                  <p class="rr-type">{{ r.analysisType }}</p>
                  <p class="rr-patient">{{ (r.patientFirstName || '') + ' ' + (r.patientLastName || '') }} · Dr. {{ (r.requestingDoctorFirstName || '') + ' ' + (r.requestingDoctorLastName || '') }}</p>
                </div>
              </div>
              <button class="btn-accept-sm" @click="acceptRequest(r)">Acceptă</button>
            </div>
            <div v-if="pendingRequests.length === 0" class="empty-sm">
              <span>✅</span><p>Nicio cerere în așteptare</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Recent notifications -->
      <div class="dash-card">
        <div class="dc-head">
          <h3 class="dc-title">Notificări Recente</h3>
          <RouterLink to="/laborator/notificari" class="dc-link">Vezi toate →</RouterLink>
        </div>
        <div class="dc-body">
          <div class="notif-list">
            <div class="notif-row" v-for="n in recentNotifs" :key="n.id" :class="{ unread: !n.isRead }">
              <div class="nr-dot" :class="{ 'dot-unread': !n.isRead }"></div>
              <div class="nr-content">
                <p class="nr-msg">{{ n.message }}</p>
                <p class="nr-time">{{ formatTime(n.createdAt) }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>

    </div>

    <!-- Quick actions -->
    <div class="dash-card full-card">
      <div class="dc-head"><h3 class="dc-title">Acțiuni Rapide</h3></div>
      <div class="qa-grid">
        <RouterLink to="/laborator/cereri" class="qa-item">
          <div class="qa-ic" style="background:rgba(217,119,6,.1);color:#d97706;">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="22" height="22"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="12" y1="18" x2="12" y2="12"/><line x1="9" y1="15" x2="15" y2="15"/></svg>
          </div>
          <p class="qa-t">Cereri Analize</p>
          <p class="qa-s">Acceptă și procesează cereri</p>
        </RouterLink>
        <RouterLink to="/laborator/pacienti" class="qa-item">
          <div class="qa-ic" style="background:rgba(14,165,233,.1);color:#0ea5e9;">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="22" height="22"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
          </div>
          <p class="qa-t">Pacienți</p>
          <p class="qa-s">Vizualizează și încarcă rezultate</p>
        </RouterLink>
        <RouterLink to="/laborator/notificari" class="qa-item">
          <div class="qa-ic" style="background:rgba(34,197,94,.1);color:#16a34a;">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="22" height="22"><path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 0 1-3.46 0"/></svg>
          </div>
          <p class="qa-t">Notificări</p>
          <p class="qa-s">{{ unreadCount }} necitite</p>
        </RouterLink>
      </div>
    </div>

    <!-- Toast -->
    <Transition name="toast-slide">
      <div v-if="toast.show" class="toast">✅ {{ toast.message }}</div>
    </Transition>

  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { laboratorApi } from '@/api/laborator'

const authStore = useAuthStore()
const loading   = ref(false)

const pendingRequests = ref([])
const recentNotifs    = ref([])
const unreadCount     = computed(() => recentNotifs.value.filter(n => !n.isRead).length)

const greeting = computed(() => {
  const h = new Date().getHours()
  if (h < 12) return 'Bună dimineața'
  if (h < 18) return 'Bună ziua'
  return 'Bună seara'
})

const currentDate = computed(() =>
  new Date().toLocaleDateString('ro-RO', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
)

function formatTime(d) {
  if (!d) return ''
  const diff = Math.floor((new Date() - new Date(d)) / 1000)
  if (diff < 60)    return 'acum câteva secunde'
  if (diff < 3600)  return `acum ${Math.floor(diff/60)} min`
  if (diff < 86400) return `acum ${Math.floor(diff/3600)} ore`
  return new Date(d).toLocaleDateString('ro-RO', { day: 'numeric', month: 'short' })
}

const stats = computed(() => [
  { icon: '📋', label: 'Cereri în așteptare', value: pendingRequests.value.length,                          bg: 'rgba(217,119,6,.1)'  },
  { icon: '🔔', label: 'Notificări noi',       value: unreadCount.value,                                    bg: 'rgba(124,58,237,.1)' },
  { icon: '📄', label: 'PDF-uri încărcate',   value: '—',                                                  bg: 'rgba(14,165,233,.1)' },
  { icon: '✅', label: 'Cereri acceptate',     value: '—',                                                  bg: 'rgba(34,197,94,.1)'  },
])

async function fetchData() {
  loading.value = true
  try {
    const [reqRes, notifRes] = await Promise.all([
      laboratorApi.getPendingRequests(),
      laboratorApi.getNotifications()
    ])
    pendingRequests.value = reqRes.data
    recentNotifs.value    = notifRes.data.slice(0, 4)
  } catch (e) {
    console.error('Eroare:', e)
  } finally {
    loading.value = false
  }
}

onMounted(fetchData)

const toast = reactive({ show: false, message: '' })
function showToast(msg) { toast.message = msg; toast.show = true; setTimeout(() => toast.show = false, 3000) }

async function acceptRequest(r) {
  try {
    await laboratorApi.acceptRequest(r.id)
    await fetchData()
    showToast(`✅ Cerere „${r.analysisType}" acceptată`)
  } catch (e) {
    showToast('⚠️ ' + (e.response?.data?.message || 'Cererea nu mai este disponibilă'))
  }
}
</script>

<style scoped>
.dashboard { display: flex; flex-direction: column; gap: 1.5rem; }

.welcome-banner {
  background: linear-gradient(135deg, #0f1f14 0%, #1a3520 60%, #1e3d24 100%);
  border-radius: 16px; padding: 2rem 2.5rem;
  display: flex; align-items: center; justify-content: space-between;
  border: 1px solid rgba(34,197,94,0.2); overflow: hidden; position: relative;
}
.wb-greeting { font-size: 0.85rem; color: rgba(200,215,235,0.5); text-transform: uppercase; letter-spacing: 0.08em; font-weight: 600; }
.wb-name { font-family: 'DM Serif Display', serif; font-size: 1.9rem; color: #e8edf5; line-height: 1.2; margin: 0.1rem 0 0.2rem; }
.wb-spec { font-size: 0.85rem; color: #4ade80; font-weight: 600; margin-bottom: 0.3rem; }
.wb-date { font-size: 0.8rem; color: rgba(200,215,235,0.4); text-transform: capitalize; }

.wb-visual { position: relative; width: 120px; height: 120px; flex-shrink: 0; display: flex; align-items: center; justify-content: center; }
.pulse-ring { position: absolute; border-radius: 50%; border: 1px solid rgba(34,197,94,0.2); animation: pr 3s ease-in-out infinite; }
.r1 { width: 70px; height: 70px; animation-delay: 0s; }
.r2 { width: 110px; height: 110px; animation-delay: 0.5s; }
@keyframes pr { 0%,100%{opacity:.4}50%{opacity:.9} }
.flask-wrap { position: relative; z-index: 1; }
.flask-body {
  width: 40px; height: 52px;
  background: rgba(74,222,128,0.15);
  border: 2px solid rgba(74,222,128,0.5);
  border-radius: 4px 4px 16px 16px;
  position: relative; overflow: hidden;
}
.flask-liquid {
  position: absolute; bottom: 0; left: 0; right: 0;
  height: 55%; background: rgba(34,197,94,0.4);
  border-radius: 0 0 14px 14px;
  animation: liquidWave 3s ease-in-out infinite;
}
@keyframes liquidWave { 0%,100%{height:55%}50%{height:60%} }
.bubble {
  position: absolute; border-radius: 50%;
  background: rgba(74,222,128,0.6); animation: bubbleUp 2s ease-in infinite;
}
.b1 { width: 5px; height: 5px; left: 8px; bottom: 15%; animation-delay: 0s; }
.b2 { width: 4px; height: 4px; left: 20px; bottom: 10%; animation-delay: 0.6s; }
.b3 { width: 6px; height: 6px; left: 28px; bottom: 5%; animation-delay: 1.2s; }
@keyframes bubbleUp { 0%{opacity:1;transform:translateY(0)}100%{opacity:0;transform:translateY(-30px)} }

.stats-grid { display: grid; grid-template-columns: repeat(4,1fr); gap: 1rem; }
.stat-card { background: #fff; border-radius: 12px; border: 1px solid #e8edf5; padding: 1.2rem; display: flex; align-items: center; gap: 1rem; box-shadow: 0 1px 3px rgba(15,34,64,.06); }
.sc-icon { width: 44px; height: 44px; border-radius: 10px; display: flex; align-items: center; justify-content: center; font-size: 1.3rem; flex-shrink: 0; }
.sc-val { font-family: 'DM Serif Display', serif; font-size: 1.6rem; color: #0f1f14; line-height: 1; }
.sc-lbl { font-size: 0.77rem; color: #64748b; font-weight: 500; margin-top: 0.15rem; }

.mid-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1.25rem; }
.dash-card { background: #fff; border-radius: 12px; border: 1px solid #e8edf5; box-shadow: 0 1px 3px rgba(15,34,64,.06); overflow: hidden; }
.full-card { grid-column: 1 / -1; }
.dc-head { padding: 1.1rem 1.4rem; border-bottom: 1px solid #f1f5f9; display: flex; align-items: center; justify-content: space-between; }
.dc-title { font-family: 'DM Serif Display', serif; font-size: 0.95rem; color: #0f1f14; }
.dc-link { font-size: 0.8rem; font-weight: 700; color: #16a34a; text-decoration: none; }
.dc-link:hover { text-decoration: underline; }
.dc-body { padding: 0.25rem 0; }

.req-list { display: flex; flex-direction: column; }
.req-row { display: flex; align-items: center; justify-content: space-between; gap: 0.75rem; padding: 0.85rem 1.4rem; border-bottom: 1px solid #f8fafc; }
.req-row:last-child { border-bottom: none; }
.rr-left { display: flex; align-items: center; gap: 0.65rem; flex: 1; min-width: 0; }
.rr-icon { font-size: 1.1rem; flex-shrink: 0; }
.rr-type { font-size: 0.87rem; font-weight: 700; color: #0f1f14; }
.rr-patient { font-size: 0.74rem; color: #94a3b8; margin-top: 0.1rem; }
.btn-accept-sm {
  padding: 0.35rem 0.85rem; background: rgba(34,197,94,.1); color: #16a34a;
  border: 1px solid rgba(34,197,94,.25); border-radius: 7px;
  font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.78rem; font-weight: 700;
  cursor: pointer; transition: all 0.18s; white-space: nowrap; flex-shrink: 0;
}
.btn-accept-sm:hover { background: #16a34a; color: #fff; }

.notif-list { display: flex; flex-direction: column; }
.notif-row { display: flex; align-items: flex-start; gap: 0.75rem; padding: 0.85rem 1.4rem; border-bottom: 1px solid #f8fafc; transition: background 0.15s; }
.notif-row:last-child { border-bottom: none; }
.notif-row.unread { background: #f0fdf4; }
.nr-dot { width: 8px; height: 8px; border-radius: 50%; background: #e2e8f0; flex-shrink: 0; margin-top: 0.35rem; }
.dot-unread { background: #22c55e; }
.nr-msg { font-size: 0.84rem; color: #334155; line-height: 1.4; }
.nr-time { font-size: 0.72rem; color: #94a3b8; margin-top: 0.2rem; }

.qa-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 0; border-top: 1px solid #f1f5f9; }
.qa-item {
  display: flex; flex-direction: column; align-items: center;
  gap: 0.6rem; padding: 1.5rem; text-decoration: none;
  border-right: 1px solid #f1f5f9; transition: background 0.18s;
  text-align: center;
}
.qa-item:last-child { border-right: none; }
.qa-item:hover { background: #f8fafc; }
.qa-ic { width: 48px; height: 48px; border-radius: 12px; display: flex; align-items: center; justify-content: center; }
.qa-t { font-size: 0.88rem; font-weight: 700; color: #0f1f14; }
.qa-s { font-size: 0.75rem; color: #94a3b8; }

.empty-sm { display: flex; flex-direction: column; align-items: center; gap: 0.4rem; padding: 2rem; text-align: center; font-size: 0.85rem; color: #94a3b8; }
.empty-sm span { font-size: 1.5rem; opacity: 0.4; }

.toast { position: fixed; bottom: 2rem; right: 2rem; z-index: 999; padding: 0.85rem 1.25rem; border-radius: 10px; background: #0f1f14; color: #fff; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 600; box-shadow: 0 8px 24px rgba(0,0,0,.15); }
.toast-slide-enter-active { animation: ti .3s ease; }
.toast-slide-leave-active { animation: ti .25s ease reverse; }
@keyframes ti { from { opacity: 0; transform: translateY(12px); } to { opacity: 1; transform: translateY(0); } }

@media(max-width:1100px) { .stats-grid { grid-template-columns: repeat(2,1fr); } .qa-grid { grid-template-columns: 1fr; } }
@media(max-width:750px) { .mid-grid { grid-template-columns: 1fr; } .wb-visual { display: none; } }
</style>
