<template>
  <div class="page">
    <div class="page-header">
      <div>
        <h2 class="page-title">Pacienți</h2>
        <p class="page-sub">Pacienții celorlalți doctori de medicină — vizualizare</p>
      </div>
    </div>

    <div class="search-bar">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="15" height="15" class="si">
        <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
      </svg>
      <input v-model="search" class="si-input" placeholder="Caută după nume, doctor, specializare..." />
    </div>

    <!-- Grid -->
    <div v-if="filtered.length > 0" class="patients-grid">
      <div class="patient-card" v-for="p in filtered" :key="p.id" @click="goToPatient(p.id)">
        <div class="pc-avatar">{{ initials(p) }}</div>
        <div class="pc-info">
          <h3 class="pc-name">{{ p.lastName }} {{ p.firstName }}</h3>
          <p class="pc-meta">{{ p.age }} ani · {{ p.gender }}</p>
          <div class="pc-doctor">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="12" height="12"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
            Dr. {{ (p.doctorFirstName || '') + ' ' + (p.doctorLastName || '') }}
            <span class="pc-spec-badge">{{ p.doctorSpecializationialization }}</span>
          </div>
        </div>
        <div class="pc-arrow">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="14" height="14"><polyline points="9 18 15 12 9 6"/></svg>
        </div>
      </div>
    </div>

    <div v-else class="empty-state">
      <div class="es-icon">👥</div>
      <p class="es-title">{{ search ? 'Niciun rezultat' : 'Nu există pacienți' }}</p>
      <p class="es-sub">{{ search ? 'Încearcă alt termen de căutare' : 'Niciun alt doctor nu a adăugat pacienți' }}</p>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { medicinaApi } from '@/api/medicina'

const router = useRouter()
const search = ref('')
const patients = ref([])

async function fetchPatients() {
  try {
    const { data } = await medicinaApi.getOtherPatients()
    patients.value = data
  } catch (e) {
    console.error('Eroare:', e)
  }
}
onMounted(fetchPatients)

const filtered = computed(() => {
  const q = search.value.toLowerCase().trim()
  if (!q) return patients.value
  return patients.value.filter(p =>
    `${p.firstName} ${p.lastName} ${(p.doctorFirstName || '') + ' ' + (p.doctorLastName || '')} ${p.doctorSpecializationialization}`.toLowerCase().includes(q)
  )
})

function initials(p) { return ((p.firstName?.[0] ?? '') + (p.lastName?.[0] ?? '')).toUpperCase() }
function goToPatient(id) { router.push(`/medicina/pacient/${id}`) }
</script>

<style scoped>
.page { display: flex; flex-direction: column; gap: 1.5rem; }
.page-header { display: flex; align-items: flex-start; justify-content: space-between; }
.page-title { font-family: 'DM Serif Display', serif; font-size: 1.5rem; color: #071a2e; }
.page-sub { font-size: 0.85rem; color: #64748b; margin-top: 0.2rem; }

.search-bar { position: relative; display: flex; align-items: center; }
.si { position: absolute; left: 1rem; color: #94a3b8; pointer-events: none; }
.si-input {
  width: 100%; padding: 0.7rem 1rem 0.7rem 2.6rem;
  border: 1.5px solid #e2e8f0; border-radius: 10px;
  font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.9rem;
  outline: none; transition: border-color 0.2s; background: #fff; color: #071a2e;
}
.si-input:focus { border-color: #0ea5e9; box-shadow: 0 0 0 3px rgba(14,165,233,.1); }

.patients-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 1rem; }
.patient-card {
  background: #fff; border: 1px solid #e8edf5; border-radius: 14px;
  padding: 1.2rem; cursor: pointer; transition: all 0.2s;
  display: flex; align-items: center; gap: 1rem;
  box-shadow: 0 1px 3px rgba(15,34,64,.05);
}
.patient-card:hover { box-shadow: 0 6px 18px rgba(15,34,64,.1); border-color: #0ea5e9; transform: translateY(-2px); }
.pc-avatar {
  width: 46px; height: 46px; border-radius: 50%; flex-shrink: 0;
  background: linear-gradient(135deg, #7c3aed, #a855f7);
  display: flex; align-items: center; justify-content: center;
  font-family: 'DM Serif Display', serif; font-size: 1.1rem; font-weight: 700; color: #fff;
}
.pc-info { flex: 1; min-width: 0; }
.pc-name { font-family: 'DM Serif Display', serif; font-size: 1rem; color: #071a2e; }
.pc-meta { font-size: 0.8rem; color: #64748b; margin: 0.15rem 0; }
.pc-doctor { font-size: 0.8rem; color: #0ea5e9; font-weight: 600; display: flex; align-items: center; gap: 0.35rem; flex-wrap: wrap; }
.pc-spec-badge { font-size: 0.72rem; background: rgba(14,165,233,.08); padding: 0.1rem 0.5rem; border-radius: 100px; color: #0284c7; }
.pc-arrow { color: #cbd5e1; flex-shrink: 0; }
.patient-card:hover .pc-arrow { color: #0ea5e9; }

.empty-state { text-align: center; padding: 4rem 2rem; display: flex; flex-direction: column; align-items: center; gap: 0.6rem; }
.es-icon { font-size: 3rem; opacity: 0.3; }
.es-title { font-family: 'DM Serif Display', serif; font-size: 1.15rem; color: #64748b; }
.es-sub { font-size: 0.85rem; color: #94a3b8; }
</style>
