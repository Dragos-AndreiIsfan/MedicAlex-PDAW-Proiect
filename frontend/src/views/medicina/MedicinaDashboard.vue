<template>
  <div class="dashboard">

    <div class="welcome-banner">
      <div class="wb-text">
        <p class="wb-greeting">{{ greeting }},</p>
        <h2 class="wb-name">Dr. {{ authStore.fullName }}</h2>
        <p class="wb-spec">{{ authStore.user?.specialization || 'Medicină Generală' }}</p>
        <p class="wb-date">{{ currentDate }}</p>
      </div>
      <div class="wb-visual" aria-hidden="true">
        <div class="pulse-ring r1"></div>
        <div class="pulse-ring r2"></div>
        <div class="pulse-ring r3"></div>
        <span class="wb-icon">🩺</span>
      </div>
    </div>

    <div class="stats-grid">
      <div class="stat-card" v-for="s in stats" :key="s.label">
        <div class="sc-icon" :style="{ background: s.bg }">{{ s.icon }}</div>
        <div>
          <p class="sc-val">{{ loading ? '...' : s.value }}</p>
          <p class="sc-lbl">{{ s.label }}</p>
        </div>
      </div>
    </div>

    <div class="mid-grid">
      <div class="dash-card">
        <div class="dc-head"><h3 class="dc-title">Acțiuni Rapide</h3></div>
        <div class="dc-body qa-list">
          <RouterLink to="/medicina/pacientii-mei" class="qa-row">
            <div class="qa-ic" style="background:rgba(14,165,233,.1);color:#0ea5e9;">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="18" height="18"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/><line x1="19" y1="8" x2="19" y2="14"/><line x1="22" y1="11" x2="16" y2="11"/></svg>
            </div>
            <div class="qa-txt"><span class="qa-t">Adaugă Pacient Nou</span><span class="qa-s">Creează un profil medical</span></div>
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="13" height="13" class="qa-arr"><polyline points="9 18 15 12 9 6"/></svg>
          </RouterLink>
          <RouterLink to="/medicina/pacientii-mei" class="qa-row">
            <div class="qa-ic" style="background:rgba(5,150,105,.1);color:#059669;">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="18" height="18"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="12" y1="18" x2="12" y2="12"/><line x1="9" y1="15" x2="15" y2="15"/></svg>
            </div>
            <div class="qa-txt"><span class="qa-t">Cerere Analize</span><span class="qa-s">Solicită analize pentru pacient</span></div>
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="13" height="13" class="qa-arr"><polyline points="9 18 15 12 9 6"/></svg>
          </RouterLink>
          <RouterLink to="/medicina/pacienti" class="qa-row">
            <div class="qa-ic" style="background:rgba(124,58,237,.1);color:#7c3aed;">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="18" height="18"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
            </div>
            <div class="qa-txt"><span class="qa-t">Toți Pacienții</span><span class="qa-s">Vizualizează pacienții colegilor</span></div>
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="13" height="13" class="qa-arr"><polyline points="9 18 15 12 9 6"/></svg>
          </RouterLink>
        </div>
      </div>

      <div class="dash-card">
        <div class="dc-head">
          <h3 class="dc-title">Pacienții Mei Recenți</h3>
          <RouterLink to="/medicina/pacientii-mei" class="dc-link">Vezi toți →</RouterLink>
        </div>
        <div class="dc-body">
          <div class="recent-list">
            <div class="recent-row" v-for="p in recentPatients" :key="p.id" @click="goToPatient(p.id)">
              <div class="rr-avatar">{{ initials(p) }}</div>
              <div class="rr-info">
                <p class="rr-name">{{ p.lastName }} {{ p.firstName }}</p>
                <p class="rr-meta">{{ p.age }} ani · {{ p.gender }}</p>
              </div>
            </div>
            <div v-if="!loading && recentPatients.length === 0" class="empty-sm">
              Nu există pacienți adăugați încă
            </div>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { medicinaApi } from '@/api/medicina'

const authStore = useAuthStore()
const router    = useRouter()
const loading   = ref(false)

const myPatients    = ref([])
const otherPatients = ref([])

const greeting = computed(() => {
  const h = new Date().getHours()
  if (h < 12) return 'Bună dimineața'
  if (h < 18) return 'Bună ziua'
  return 'Bună seara'
})

const currentDate = computed(() =>
  new Date().toLocaleDateString('ro-RO', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
)

const recentPatients = computed(() => myPatients.value.slice(0, 3))

const stats = computed(() => [
  { icon: '👥', label: 'Pacienții mei',        value: myPatients.value.length,    bg: 'rgba(14,165,233,.1)' },
  { icon: '🏥', label: 'Total pacienți sistem', value: myPatients.value.length + otherPatients.value.length, bg: 'rgba(124,58,237,.1)' },
  { icon: '📋', label: 'Cereri analize',        value: '—',  bg: 'rgba(245,158,11,.1)' },
  { icon: '📄', label: 'Rezultate primite',     value: '—',  bg: 'rgba(5,150,105,.1)'  },
])

async function fetchData() {
  loading.value = true
  try {
    const [mineRes, othersRes] = await Promise.all([
      medicinaApi.getMyPatients(),
      medicinaApi.getOtherPatients()
    ])
    myPatients.value    = mineRes.data
    otherPatients.value = othersRes.data
  } catch (e) {
    console.error('Eroare:', e)
  } finally {
    loading.value = false
  }
}

onMounted(fetchData)

function initials(p) { return ((p.firstName?.[0] ?? '') + (p.lastName?.[0] ?? '')).toUpperCase() }
function goToPatient(id) { router.push(`/medicina/pacient/${id}`) }
</script>

<style scoped>
.dashboard { display: flex; flex-direction: column; gap: 1.5rem; }
.welcome-banner { background: linear-gradient(135deg, #071a2e 0%, #0c2d4a 60%, #0e3860 100%); border-radius: 16px; padding: 2rem 2.5rem; display: flex; align-items: center; justify-content: space-between; border: 1px solid rgba(14,165,233,.2); overflow: hidden; }
.wb-greeting { font-size: 0.85rem; color: rgba(200,215,235,.5); text-transform: uppercase; letter-spacing: 0.08em; font-weight: 600; }
.wb-name { font-family: 'DM Serif Display', serif; font-size: 1.9rem; color: #e8edf5; line-height: 1.2; margin: 0.1rem 0 0.2rem; }
.wb-spec { font-size: 0.85rem; color: #38bdf8; font-weight: 600; margin-bottom: 0.3rem; }
.wb-date { font-size: 0.8rem; color: rgba(200,215,235,.4); text-transform: capitalize; }
.wb-visual { position: relative; width: 110px; height: 110px; flex-shrink: 0; display: flex; align-items: center; justify-content: center; }
.pulse-ring { position: absolute; border-radius: 50%; border: 1px solid rgba(14,165,233,.2); animation: pr 3s ease-in-out infinite; }
.r1 { width: 50px; height: 50px; } .r2 { width: 80px; height: 80px; animation-delay: .4s; } .r3 { width: 110px; height: 110px; animation-delay: .8s; }
@keyframes pr { 0%,100%{opacity:.4}50%{opacity:.9} }
.wb-icon { font-size: 2rem; position: relative; z-index: 1; }
.stats-grid { display: grid; grid-template-columns: repeat(4,1fr); gap: 1rem; }
.stat-card { background: #fff; border-radius: 12px; border: 1px solid #e8edf5; padding: 1.2rem; display: flex; align-items: center; gap: 1rem; box-shadow: 0 1px 3px rgba(15,34,64,.06); }
.sc-icon { width: 44px; height: 44px; border-radius: 10px; display: flex; align-items: center; justify-content: center; font-size: 1.3rem; flex-shrink: 0; }
.sc-val { font-family: 'DM Serif Display', serif; font-size: 1.6rem; color: #071a2e; line-height: 1; }
.sc-lbl { font-size: 0.77rem; color: #64748b; font-weight: 500; margin-top: 0.15rem; }
.mid-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1.25rem; }
.dash-card { background: #fff; border-radius: 12px; border: 1px solid #e8edf5; box-shadow: 0 1px 3px rgba(15,34,64,.06); overflow: hidden; }
.dc-head { padding: 1.1rem 1.4rem; border-bottom: 1px solid #f1f5f9; display: flex; align-items: center; justify-content: space-between; }
.dc-title { font-family: 'DM Serif Display', serif; font-size: 0.95rem; color: #071a2e; }
.dc-link { font-size: 0.8rem; font-weight: 700; color: #0ea5e9; text-decoration: none; }
.dc-body { padding: 0.25rem 0; }
.qa-list { display: flex; flex-direction: column; }
.qa-row { display: flex; align-items: center; gap: 0.9rem; padding: 0.85rem 1.4rem; text-decoration: none; border-bottom: 1px solid #f8fafc; transition: background 0.15s; }
.qa-row:last-child { border-bottom: none; }
.qa-row:hover { background: #f8fafc; }
.qa-ic { width: 36px; height: 36px; border-radius: 9px; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.qa-txt { flex: 1; }
.qa-t { display: block; font-size: 0.87rem; font-weight: 600; color: #0f2240; }
.qa-s { display: block; font-size: 0.74rem; color: #94a3b8; margin-top: 0.1rem; }
.qa-arr { color: #cbd5e1; flex-shrink: 0; }
.qa-row:hover .qa-arr { color: #0ea5e9; }
.recent-list { padding: 0.25rem 0; }
.recent-row { display: flex; align-items: center; gap: 0.85rem; padding: 0.8rem 1.4rem; cursor: pointer; border-bottom: 1px solid #f8fafc; transition: background 0.15s; }
.recent-row:last-child { border-bottom: none; }
.recent-row:hover { background: #f0f9ff; }
.rr-avatar { width: 36px; height: 36px; border-radius: 50%; background: linear-gradient(135deg, #0ea5e9, #06b6d4); display: flex; align-items: center; justify-content: center; font-family: 'DM Serif Display', serif; font-size: 0.85rem; font-weight: 700; color: #fff; flex-shrink: 0; }
.rr-name { font-size: 0.87rem; font-weight: 700; color: #071a2e; }
.rr-meta { font-size: 0.74rem; color: #94a3b8; margin-top: 0.1rem; }
.empty-sm { padding: 1.5rem; text-align: center; font-size: 0.85rem; color: #94a3b8; }
@media(max-width:1100px) { .stats-grid { grid-template-columns: repeat(2,1fr); } }
@media(max-width:750px) { .mid-grid { grid-template-columns: 1fr; } .wb-visual { display: none; } }
</style>
