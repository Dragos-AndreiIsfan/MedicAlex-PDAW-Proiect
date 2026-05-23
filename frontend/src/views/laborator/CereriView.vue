<template>
  <div class="page">

    <div class="page-header">
      <div>
        <h2 class="page-title">Cereri Analize</h2>
        <p class="page-sub">Cereri în așteptare de la doctorii de medicină</p>
      </div>
      <div class="header-tabs">
        <button class="tab" :class="{ active: tab === 'pending' }"   @click="tab = 'pending'">
          În așteptare <span class="tab-count">{{ pending.length }}</span>
        </button>
        <button class="tab" :class="{ active: tab === 'accepted' }"  @click="tab = 'accepted'">
          Acceptate de mine <span class="tab-count">{{ accepted.length }}</span>
        </button>
      </div>
    </div>

    <!-- Pending requests -->
    <div v-if="tab === 'pending'">
      <div v-if="pending.length > 0" class="requests-list">
        <div class="req-card" v-for="r in pending" :key="r.id">
          <div class="rc-left">
            <div class="rc-icon">🔬</div>
            <div class="rc-body">
              <div class="rc-top">
                <h3 class="rc-type">{{ r.analysisType }}</h3>
                <span class="rc-badge badge-pending">În așteptare</span>
              </div>
              <div class="rc-meta">
                <span class="rc-meta-item">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="12" height="12"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
                  Pacient: <strong>{{ r.patientName || (r.patientFirstName + ' ' + r.patientLastName) }}</strong>
                </span>
                <span class="rc-meta-item">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="12" height="12"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
                  {{ formatDate(r.createdAt) }}
                </span>
                <span class="rc-meta-item">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="12" height="12"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/></svg>
                  Dr. {{ r.requestingDoctorName }}
                </span>
              </div>
            </div>
          </div>
          <button class="btn-accept" @click="acceptRequest(r)">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="15" height="15"><polyline points="20 6 9 17 4 12"/></svg>
            Acceptă Cererea
          </button>
        </div>
      </div>
      <div v-else class="empty-state">
        <div class="es-icon">✅</div>
        <p class="es-title">Nicio cerere în așteptare</p>
        <p class="es-sub">Toate cererile au fost procesate</p>
      </div>
    </div>

    <!-- Accepted by me -->
    <div v-if="tab === 'accepted'">
      <div v-if="accepted.length > 0" class="requests-list">
        <div class="req-card req-card-accepted" v-for="r in accepted" :key="r.id">
          <div class="rc-left">
            <div class="rc-icon">✅</div>
            <div class="rc-body">
              <div class="rc-top">
                <h3 class="rc-type">{{ r.analysisType }}</h3>
                <span class="rc-badge" :class="r.status === 'completed' ? 'badge-completed' : 'badge-accepted'">
                  {{ r.status === 'completed' ? 'Finalizat' : 'Acceptat de mine' }}
                </span>
              </div>
              <div class="rc-meta">
                <span class="rc-meta-item">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="12" height="12"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
                  Pacient: <strong>{{ r.patientName || (r.patientFirstName + ' ' + r.patientLastName) }}</strong>
                </span>
                <span class="rc-meta-item">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="12" height="12"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
                  {{ formatDate(r.createdAt) }}
                </span>
              </div>
              <p class="accepted-by-label">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="12" height="12"><polyline points="20 6 9 17 4 12"/></svg>
                Acceptat de Dr. {{ r.acceptedByFirstName }} {{ r.acceptedByLastName }}
              </p>
            </div>
          </div>
          <RouterLink :to="`/laborator/pacient/${r.patientId}`" class="btn-upload">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="14" height="14"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" y1="3" x2="12" y2="15"/></svg>
            Încarcă Rezultate
          </RouterLink>
        </div>
      </div>
      <div v-else class="empty-state">
        <div class="es-icon">📋</div>
        <p class="es-title">Nicio cerere acceptată</p>
        <p class="es-sub">Acceptă cereri din tab-ul „În așteptare"</p>
      </div>
    </div>

    <!-- Toast -->
    <Transition name="toast-slide">
      <div v-if="toast.show" class="toast">✅ {{ toast.message }}</div>
    </Transition>

  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import { laboratorApi } from '@/api/laborator'
import { useAuthStore } from '@/stores/auth'

const tab       = ref('pending')
const authStore = useAuthStore()
const pending   = ref([])
const accepted  = ref([])
const loading   = ref(false)

function formatDate(d) {
  if (!d) return ''
  return new Date(d).toLocaleDateString('ro-RO', { day: 'numeric', month: 'short', year: 'numeric' })
}

async function fetchRequests() {
  loading.value = true
  try {
    const [pRes, aRes] = await Promise.all([
      laboratorApi.getPendingRequests(),
      laboratorApi.getMyRequests()
    ])
    pending.value  = pRes.data
    accepted.value = aRes.data
  } catch (e) {
    console.error('Eroare:', e)
  } finally {
    loading.value = false
  }
}

onMounted(fetchRequests)

const toast = reactive({ show: false, message: '' })
function showToast(msg) { toast.message = msg; toast.show = true; setTimeout(() => toast.show = false, 3000) }

async function acceptRequest(r) {
  try {
    await laboratorApi.acceptRequest(r.id)
    await fetchRequests()
    showToast(`✅ Cerere „${r.analysisType}" acceptată. Tu te ocupi de aceasta.`)
    tab.value = 'accepted'
  } catch (e) {
    showToast('⚠️ ' + (e.response?.data?.message || 'Cererea nu mai este disponibilă'))
  }
}
</script>

<style scoped>
.page { display: flex; flex-direction: column; gap: 1.5rem; }
.page-header { display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem; flex-wrap: wrap; }
.page-title { font-family: 'DM Serif Display', serif; font-size: 1.5rem; color: #0f1f14; }
.page-sub { font-size: 0.85rem; color: #64748b; margin-top: 0.2rem; }

.header-tabs { display: flex; gap: 0.5rem; }
.tab {
  display: flex; align-items: center; gap: 0.5rem;
  padding: 0.55rem 1.1rem;
  background: #fff; border: 1.5px solid #e8edf5;
  border-radius: 8px; font-family: 'Plus Jakarta Sans', sans-serif;
  font-size: 0.85rem; font-weight: 500; color: #64748b;
  cursor: pointer; transition: all 0.18s;
}
.tab:hover { border-color: #16a34a; color: #16a34a; }
.tab.active { background: #0f1f14; border-color: #0f1f14; color: #fff; }
.tab-count {
  padding: 0.1rem 0.45rem; border-radius: 100px;
  font-size: 0.72rem; font-weight: 700;
}
.tab.active .tab-count { background: rgba(255,255,255,0.2); }
.tab:not(.active) .tab-count { background: #f1f5f9; color: #64748b; }

.requests-list { display: flex; flex-direction: column; gap: 0.75rem; }
.req-card {
  background: #fff; border: 1px solid #e8edf5; border-radius: 14px;
  padding: 1.25rem 1.5rem;
  display: flex; align-items: center; justify-content: space-between; gap: 1.25rem;
  box-shadow: 0 1px 3px rgba(15,34,64,.05);
  transition: box-shadow 0.18s, border-color 0.18s;
}
.req-card:hover { box-shadow: 0 4px 14px rgba(15,34,64,.08); border-color: #cbd5e1; }
.req-card-accepted { border-left: 3px solid #16a34a; }

.rc-left { display: flex; align-items: flex-start; gap: 1rem; flex: 1; min-width: 0; }
.rc-icon { font-size: 1.6rem; flex-shrink: 0; line-height: 1; margin-top: 0.1rem; }
.rc-body { flex: 1; min-width: 0; }
.rc-top { display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; margin-bottom: 0.5rem; }
.rc-type { font-family: 'DM Serif Display', serif; font-size: 1.05rem; color: #0f1f14; }
.rc-badge { padding: 0.25rem 0.7rem; border-radius: 100px; font-size: 0.74rem; font-weight: 700; }
.badge-pending   { background: #fffbeb; color: #d97706; }
.badge-accepted  { background: #f0f9ff; color: #0ea5e9; }
.badge-completed { background: #f0fdf4; color: #059669; }

.rc-meta { display: flex; flex-wrap: wrap; gap: 1rem; }
.rc-meta-item {
  display: flex; align-items: center; gap: 0.35rem;
  font-size: 0.8rem; color: #64748b;
}
.rc-meta-item strong { color: #334155; }

.accepted-by-label {
  display: inline-flex; align-items: center; gap: 0.35rem;
  font-size: 0.78rem; color: #16a34a; font-weight: 600; font-style: italic;
  margin-top: 0.4rem;
}

.btn-accept {
  display: inline-flex; align-items: center; gap: 0.5rem;
  padding: 0.65rem 1.25rem; background: #16a34a; color: #fff;
  border: none; border-radius: 10px;
  font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 700;
  cursor: pointer; transition: all 0.2s; white-space: nowrap; flex-shrink: 0;
}
.btn-accept:hover { background: #15803d; transform: translateY(-1px); box-shadow: 0 4px 12px rgba(22,163,74,.3); }

.btn-upload {
  display: inline-flex; align-items: center; gap: 0.5rem;
  padding: 0.6rem 1.1rem; background: rgba(14,165,233,.1); color: #0ea5e9;
  border: 1px solid rgba(14,165,233,.2); border-radius: 9px;
  font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.82rem; font-weight: 600;
  text-decoration: none; white-space: nowrap; flex-shrink: 0;
  transition: all 0.18s;
}
.btn-upload:hover { background: #0ea5e9; color: #fff; }

.empty-state { text-align: center; padding: 4rem; display: flex; flex-direction: column; align-items: center; gap: 0.6rem; }
.es-icon { font-size: 3rem; opacity: 0.35; }
.es-title { font-family: 'DM Serif Display', serif; font-size: 1.15rem; color: #64748b; }
.es-sub { font-size: 0.85rem; color: #94a3b8; }

.toast { position: fixed; bottom: 2rem; right: 2rem; z-index: 999; padding: 0.85rem 1.25rem; border-radius: 10px; background: #0f1f14; color: #fff; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 600; box-shadow: 0 8px 24px rgba(0,0,0,.15); }
.toast-slide-enter-active { animation: ti .3s ease; }
.toast-slide-leave-active { animation: ti .25s ease reverse; }
@keyframes ti { from { opacity: 0; transform: translateY(12px); } to { opacity: 1; transform: translateY(0); } }
</style>
