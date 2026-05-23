<template>
  <div class="dashboard">

    <!-- Welcome banner -->
    <div class="welcome-banner">
      <div class="wb-text">
        <p class="wb-greeting">{{ greeting }},</p>
        <h2 class="wb-name">{{ authStore.fullName || 'Administrator' }}</h2>
        <p class="wb-date">{{ currentDate }}</p>
      </div>
      <div class="wb-visual" aria-hidden="true">
        <div class="wb-cross"><div class="wbc-h"></div><div class="wbc-v"></div></div>
        <div class="wb-ring wb-ring-1"></div>
        <div class="wb-ring wb-ring-2"></div>
        <div class="wb-ring wb-ring-3"></div>
      </div>
    </div>

    <!-- Stats row -->
    <div class="stats-grid">
      <div class="stat-card" v-for="stat in stats" :key="stat.label">
        <div class="sc-icon" :style="{ background: stat.iconBg }">
          <span>{{ stat.icon }}</span>
        </div>
        <div class="sc-body">
          <p class="sc-value">{{ loading ? '...' : stat.value }}</p>
          <p class="sc-label">{{ stat.label }}</p>
        </div>
      </div>
    </div>

    <!-- Two-column section -->
    <div class="dash-grid">

      <!-- Quick actions -->
      <div class="dash-card">
        <div class="dc-header"><h3 class="dc-title">Acțiuni Rapide</h3></div>
        <div class="dc-body">
          <div class="quick-actions">
            <RouterLink to="/admin/doctori" class="qa-item">
              <div class="qa-icon" style="background:rgba(0,136,204,.1);color:#0088cc;">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="20" height="20"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><line x1="19" y1="8" x2="19" y2="14"/><line x1="22" y1="11" x2="16" y2="11"/></svg>
              </div>
              <div class="qa-text"><span class="qa-title">Adaugă Doctor</span><span class="qa-sub">Creează un cont nou</span></div>
              <svg class="qa-arrow" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="14" height="14"><polyline points="9 18 15 12 9 6"/></svg>
            </RouterLink>
            <RouterLink to="/admin/pacienti" class="qa-item">
              <div class="qa-icon" style="background:rgba(5,150,105,.1);color:#059669;">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="20" height="20"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/><line x1="19" y1="8" x2="19" y2="14"/><line x1="22" y1="11" x2="16" y2="11"/></svg>
              </div>
              <div class="qa-text"><span class="qa-title">Adaugă Pacient</span><span class="qa-sub">Înregistrează un pacient</span></div>
              <svg class="qa-arrow" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="14" height="14"><polyline points="9 18 15 12 9 6"/></svg>
            </RouterLink>
            <RouterLink to="/admin/doctori" class="qa-item">
              <div class="qa-icon" style="background:rgba(220,38,38,.08);color:#dc2626;">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="20" height="20"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><line x1="22" y1="11" x2="16" y2="11"/></svg>
              </div>
              <div class="qa-text"><span class="qa-title">Șterge Doctor</span><span class="qa-sub">Elimină un cont existent</span></div>
              <svg class="qa-arrow" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="14" height="14"><polyline points="9 18 15 12 9 6"/></svg>
            </RouterLink>
          </div>
        </div>
      </div>

      <!-- Rezumat sistem -->
      <div class="dash-card">
        <div class="dc-header"><h3 class="dc-title">Rezumat Sistem</h3></div>
        <div class="dc-body">
          <div class="summary-list">
            <div class="sum-row">
              <span class="sum-icon">🩺</span>
              <span class="sum-label">Doctori Medicină</span>
              <span class="sum-val">{{ loading ? '...' : doctoriMedicina }}</span>
            </div>
            <div class="sum-row">
              <span class="sum-icon">🔬</span>
              <span class="sum-label">Doctori Laborator</span>
              <span class="sum-val">{{ loading ? '...' : doctoriLaborator }}</span>
            </div>
            <div class="sum-row">
              <span class="sum-icon">🏥</span>
              <span class="sum-label">Pacienți înregistrați</span>
              <span class="sum-val">{{ loading ? '...' : totalPacienti }}</span>
            </div>
            <div class="sum-row">
              <span class="sum-icon">✅</span>
              <span class="sum-label">Sistem MedicAlex</span>
              <span class="sum-val sum-ok">Operațional</span>
            </div>
          </div>
        </div>
      </div>

    </div>

    <!-- Info cards -->
    <div class="info-grid">
      <div class="info-card info-medicina">
        <div class="ic-icon">🩺</div>
        <div class="ic-body">
          <p class="ic-label">Doctori Medicină</p>
          <p class="ic-value">{{ loading ? '...' : doctoriMedicina }}</p>
          <p class="ic-sub">Gestionează pacienți și analize</p>
        </div>
        <RouterLink to="/admin/doctori" class="ic-link">Vezi toți →</RouterLink>
      </div>
      <div class="info-card info-laborator">
        <div class="ic-icon">🔬</div>
        <div class="ic-body">
          <p class="ic-label">Doctori Laborator</p>
          <p class="ic-value">{{ loading ? '...' : doctoriLaborator }}</p>
          <p class="ic-sub">Procesează cereri de analize</p>
        </div>
        <RouterLink to="/admin/doctori" class="ic-link">Vezi toți →</RouterLink>
      </div>
      <div class="info-card info-pacienti">
        <div class="ic-icon">🏥</div>
        <div class="ic-body">
          <p class="ic-label">Pacienți Înregistrați</p>
          <p class="ic-value">{{ loading ? '...' : totalPacienti }}</p>
          <p class="ic-sub">În evidența spitalului</p>
        </div>
        <RouterLink to="/admin/pacienti" class="ic-link">Gestionează →</RouterLink>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { adminApi } from '@/api/admin'

const authStore = useAuthStore()
const loading   = ref(false)

const doctoriMedicina  = ref(0)
const doctoriLaborator = ref(0)
const totalPacienti    = ref(0)

const greeting = computed(() => {
  const h = new Date().getHours()
  if (h < 12) return 'Bună dimineața'
  if (h < 18) return 'Bună ziua'
  return 'Bună seara'
})

const currentDate = computed(() =>
  new Date().toLocaleDateString('ro-RO', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
)

const stats = computed(() => [
  { icon: '👥', label: 'Pacienți totali',      value: totalPacienti.value,    iconBg: 'rgba(5,150,105,.1)' },
  { icon: '🩺', label: 'Doctori Medicină',     value: doctoriMedicina.value,  iconBg: 'rgba(0,136,204,.1)' },
  { icon: '🔬', label: 'Doctori Laborator',    value: doctoriLaborator.value, iconBg: 'rgba(139,92,246,.1)' },
  { icon: '🏥', label: 'Total Doctori',        value: doctoriMedicina.value + doctoriLaborator.value, iconBg: 'rgba(217,119,6,.1)' },
])

async function fetchStats() {
  loading.value = true
  try {
    const [doctorsRes, patientsRes] = await Promise.all([
      adminApi.getDoctors(),
      adminApi.getPatients()
    ])
    doctoriMedicina.value  = doctorsRes.data.filter(d => d.role === 'doctor_medicina').length
    doctoriLaborator.value = doctorsRes.data.filter(d => d.role === 'doctor_laborator').length
    totalPacienti.value    = patientsRes.data.length
  } catch (e) {
    console.error('Eroare la încărcarea statisticilor:', e)
  } finally {
    loading.value = false
  }
}

onMounted(fetchStats)
</script>

<style scoped>
.dashboard { display: flex; flex-direction: column; gap: 1.5rem; }
.welcome-banner { background: linear-gradient(135deg, #07111f 0%, #0d2040 60%, #0a2a50 100%); border-radius: 16px; padding: 2rem 2.5rem; display: flex; align-items: center; justify-content: space-between; overflow: hidden; border: 1px solid rgba(0,180,216,.15); }
.wb-greeting { font-size: 0.85rem; color: rgba(200,215,235,.5); text-transform: uppercase; letter-spacing: 0.08em; font-weight: 600; }
.wb-name { font-family: 'DM Serif Display', serif; font-size: 2rem; color: #e8edf5; line-height: 1.2; margin: 0.15rem 0 0.5rem; }
.wb-date { font-size: 0.82rem; color: rgba(200,215,235,.4); text-transform: capitalize; }
.wb-visual { position: relative; width: 120px; height: 120px; flex-shrink: 0; }
.wb-cross { position: absolute; top: 50%; left: 50%; transform: translate(-50%,-50%); width: 40px; height: 40px; }
.wbc-h, .wbc-v { position: absolute; background: rgba(0,212,255,.4); border-radius: 3px; }
.wbc-h { width: 100%; height: 30%; top: 35%; }
.wbc-v { width: 30%; height: 100%; left: 35%; }
.wb-ring { position: absolute; border-radius: 50%; border: 1px solid rgba(0,212,255,.15); top: 50%; left: 50%; transform: translate(-50%,-50%); animation: ringPulse 3s ease-in-out infinite; }
.wb-ring-1 { width: 60px; height: 60px; animation-delay: 0s; }
.wb-ring-2 { width: 90px; height: 90px; animation-delay: 0.5s; }
.wb-ring-3 { width: 120px; height: 120px; animation-delay: 1s; }
@keyframes ringPulse { 0%,100%{opacity:.4}50%{opacity:.8} }
.stats-grid { display: grid; grid-template-columns: repeat(4,1fr); gap: 1rem; }
.stat-card { background: #fff; border-radius: 12px; border: 1px solid #e8edf5; padding: 1.25rem; display: flex; align-items: center; gap: 1rem; box-shadow: 0 1px 3px rgba(15,34,64,.06); }
.sc-icon { width: 44px; height: 44px; border-radius: 10px; display: flex; align-items: center; justify-content: center; font-size: 1.3rem; flex-shrink: 0; }
.sc-value { font-family: 'DM Serif Display', serif; font-size: 1.6rem; color: #07111f; line-height: 1; }
.sc-label { font-size: 0.78rem; color: #64748b; font-weight: 500; margin-top: 0.2rem; }
.dash-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1.25rem; }
.dash-card { background: #fff; border-radius: 12px; border: 1px solid #e8edf5; box-shadow: 0 1px 3px rgba(15,34,64,.06); overflow: hidden; }
.dc-header { padding: 1.1rem 1.4rem; border-bottom: 1px solid #f1f5f9; }
.dc-title { font-family: 'DM Serif Display', serif; font-size: 0.95rem; color: #07111f; }
.dc-body { padding: 0.5rem 0; }
.quick-actions { display: flex; flex-direction: column; }
.qa-item { display: flex; align-items: center; gap: 0.9rem; padding: 0.85rem 1.4rem; text-decoration: none; transition: background 0.18s; border-bottom: 1px solid #f8fafc; }
.qa-item:last-child { border-bottom: none; }
.qa-item:hover { background: #f8fafc; }
.qa-icon { width: 38px; height: 38px; border-radius: 9px; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.qa-text { flex: 1; }
.qa-title { display: block; font-size: 0.88rem; font-weight: 600; color: #0f2240; }
.qa-sub { display: block; font-size: 0.75rem; color: #94a3b8; margin-top: 0.1rem; }
.qa-arrow { color: #cbd5e1; flex-shrink: 0; }
.qa-item:hover .qa-arrow { color: #0088cc; }
.summary-list { padding: 0.5rem 0; }
.sum-row { display: flex; align-items: center; gap: 0.85rem; padding: 0.8rem 1.4rem; border-bottom: 1px solid #f8fafc; }
.sum-row:last-child { border-bottom: none; }
.sum-icon { font-size: 1.1rem; flex-shrink: 0; }
.sum-label { flex: 1; font-size: 0.87rem; color: #334155; }
.sum-val { font-size: 0.9rem; font-weight: 700; color: #07111f; }
.sum-ok { color: #059669; }
.info-grid { display: grid; grid-template-columns: repeat(3,1fr); gap: 1rem; }
.info-card { border-radius: 12px; padding: 1.4rem; border: 1px solid transparent; display: flex; flex-direction: column; gap: 0.5rem; }
.info-medicina  { background: linear-gradient(135deg, #f0f9ff, #e0f2fe); border-color: #bae6fd; }
.info-laborator { background: linear-gradient(135deg, #faf5ff, #f3e8ff); border-color: #e9d5ff; }
.info-pacienti  { background: linear-gradient(135deg, #f0fdf4, #dcfce7); border-color: #bbf7d0; }
.ic-icon { font-size: 1.75rem; }
.ic-label { font-size: 0.72rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.07em; color: #64748b; }
.ic-value { font-family: 'DM Serif Display', serif; font-size: 2rem; color: #07111f; line-height: 1; }
.ic-sub { font-size: 0.78rem; color: #64748b; }
.ic-link { margin-top: 0.5rem; font-size: 0.8rem; font-weight: 700; color: #0088cc; text-decoration: none; }
.ic-link:hover { text-decoration: underline; }
@media(max-width:1100px) { .stats-grid { grid-template-columns: repeat(2,1fr); } .info-grid { grid-template-columns: repeat(2,1fr); } }
@media(max-width:750px) { .dash-grid { grid-template-columns: 1fr; } .wb-visual { display: none; } }
</style>
