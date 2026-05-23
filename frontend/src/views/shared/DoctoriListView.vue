<template>
  <div class="page">
    <div class="page-header">
      <div>
        <h2 class="page-title">{{ title }}</h2>
        <p class="page-sub">{{ subtitle }}</p>
      </div>
    </div>

    <div class="search-bar">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="15" height="15" class="si">
        <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
      </svg>
      <input v-model="search" class="si-input" placeholder="Caută după nume, specializare..." />
    </div>

    <div v-if="loading" class="loading-state">
      <div class="spinner-lg"></div>
      <p>Se încarcă...</p>
    </div>

    <div v-else-if="filtered.length > 0" class="doctors-grid">
      <div class="doctor-card" v-for="d in filtered" :key="d.id">
        <div class="dc-avatar" :style="{ background: avatarGradient }">
          {{ initials(d) }}
        </div>
        <div class="dc-info">
          <p class="dc-name">Dr. {{ d.lastName }} {{ d.firstName }}</p>
          <p class="dc-email">{{ d.email }}</p>
          <p class="dc-spec" v-if="d.specialization">{{ d.specialization }}</p>
        </div>
        <span class="dc-badge" :style="{ background: badgeBg, color: badgeColor, borderColor: badgeBorder }">
          {{ badgeLabel }}
        </span>
        <div v-if="isCurrentUser(d)" class="dc-you-tag">Tu</div>
      </div>
    </div>

    <div v-else class="empty-state">
      <div class="es-icon">{{ emptyIcon }}</div>
      <p class="es-title">{{ search ? 'Niciun rezultat' : 'Niciun doctor înregistrat' }}</p>
      <p class="es-sub">{{ search ? 'Încearcă alt termen' : 'Administratorul poate adăuga conturi noi' }}</p>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import api from '@/api/index'

const props = defineProps({
  type: { type: String, required: true } // 'medicina' | 'laborator'
})

const authStore  = useAuthStore()
const search     = ref('')
const doctors    = ref([])
const loading    = ref(false)

const isMedicina     = computed(() => props.type === 'medicina')
const title          = computed(() => isMedicina.value ? 'Doctori Medicină'  : 'Doctori Laborator')
const subtitle       = computed(() => isMedicina.value ? 'Toți doctorii de medicină din sistem' : 'Toți doctorii de laborator din sistem')
const emptyIcon      = computed(() => isMedicina.value ? '🩺' : '🔬')
const avatarGradient = computed(() => isMedicina.value ? 'linear-gradient(135deg, #0ea5e9, #06b6d4)' : 'linear-gradient(135deg, #7c3aed, #a855f7)')
const badgeBg        = computed(() => isMedicina.value ? 'rgba(14,165,233,.08)'  : 'rgba(124,58,237,.08)')
const badgeColor     = computed(() => isMedicina.value ? '#0284c7' : '#7c3aed')
const badgeBorder    = computed(() => isMedicina.value ? 'rgba(14,165,233,.2)'  : 'rgba(124,58,237,.2)')
const badgeLabel     = computed(() => isMedicina.value ? '🩺 Medicină' : '🔬 Laborator')

async function fetchDoctors() {
  loading.value = true
  try {
    // Determină rolul curent pentru a alege endpoint-ul corect
    const role    = authStore.user?.role
    const segment = isMedicina.value ? 'medicina' : 'laborator'

    let endpoint = ''
    if (role === 'admin')            endpoint = `/admin/doctors`
    else if (role === 'doctor_medicina')  endpoint = `/medicina/doctors/${segment}`
    else if (role === 'doctor_laborator') endpoint = `/laborator/doctors/${segment}`

    const { data } = await api.get(endpoint)

    // Dacă admin returnează toți doctorii, filtrăm după rol
    if (role === 'admin') {
      const targetRole = isMedicina.value ? 'doctor_medicina' : 'doctor_laborator'
      doctors.value = data.filter(d => d.role === targetRole)
    } else {
      doctors.value = data
    }
  } catch (e) {
    console.error('Eroare la încărcarea doctorilor:', e)
  } finally {
    loading.value = false
  }
}

onMounted(fetchDoctors)

const filtered = computed(() => {
  const q = search.value.toLowerCase().trim()
  if (!q) return doctors.value
  return doctors.value.filter(d =>
    `${d.firstName} ${d.lastName} ${d.specialization ?? ''} ${d.email}`.toLowerCase().includes(q)
  )
})

function initials(d) {
  return ((d.firstName?.[0] ?? '') + (d.lastName?.[0] ?? '')).toUpperCase()
}

function isCurrentUser(d) {
  return d.email === authStore.user?.email
}
</script>

<style scoped>
.page { display: flex; flex-direction: column; gap: 1.5rem; }
.page-header { display: flex; align-items: flex-start; justify-content: space-between; }
.page-title { font-family: 'DM Serif Display', serif; font-size: 1.5rem; color: #071a2e; }
.page-sub { font-size: 0.85rem; color: #64748b; margin-top: 0.2rem; }
.search-bar { position: relative; display: flex; align-items: center; }
.si { position: absolute; left: 1rem; color: #94a3b8; pointer-events: none; }
.si-input { width: 100%; padding: 0.7rem 1rem 0.7rem 2.6rem; border: 1.5px solid #e2e8f0; border-radius: 10px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.9rem; outline: none; transition: border-color 0.2s; background: #fff; color: #071a2e; }
.si-input:focus { border-color: #0ea5e9; box-shadow: 0 0 0 3px rgba(14,165,233,.1); }
.loading-state { display: flex; flex-direction: column; align-items: center; gap: 1rem; padding: 3rem; color: #64748b; }
.spinner-lg { width: 36px; height: 36px; border: 3px solid #e2e8f0; border-top-color: #0ea5e9; border-radius: 50%; animation: spin 0.8s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
.doctors-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 1rem; }
.doctor-card { background: #fff; border: 1px solid #e8edf5; border-radius: 14px; padding: 1.2rem; display: flex; align-items: center; gap: 1rem; box-shadow: 0 1px 3px rgba(15,34,64,.05); position: relative; transition: box-shadow 0.18s, border-color 0.18s; }
.doctor-card:hover { box-shadow: 0 4px 14px rgba(15,34,64,.08); border-color: #cbd5e1; }
.dc-avatar { width: 46px; height: 46px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-family: 'DM Serif Display', serif; font-size: 1.1rem; font-weight: 700; color: #fff; flex-shrink: 0; }
.dc-info { flex: 1; min-width: 0; }
.dc-name { font-size: 0.92rem; font-weight: 700; color: #071a2e; }
.dc-email { font-size: 0.77rem; color: #94a3b8; margin-top: 0.1rem; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.dc-spec { font-size: 0.78rem; color: #0ea5e9; font-weight: 600; margin-top: 0.15rem; }
.dc-badge { padding: 0.25rem 0.7rem; border-radius: 100px; border: 1px solid; font-size: 0.74rem; font-weight: 700; white-space: nowrap; flex-shrink: 0; }
.dc-you-tag { position: absolute; top: -6px; right: 12px; background: #0ea5e9; color: #fff; padding: 0.15rem 0.6rem; border-radius: 100px; font-size: 0.68rem; font-weight: 800; letter-spacing: 0.05em; }
.empty-state { text-align: center; padding: 4rem; display: flex; flex-direction: column; align-items: center; gap: 0.6rem; }
.es-icon { font-size: 3rem; opacity: 0.3; }
.es-title { font-family: 'DM Serif Display', serif; font-size: 1.15rem; color: #64748b; }
.es-sub { font-size: 0.85rem; color: #94a3b8; }
</style>
