<template>
  <div class="page" v-if="patient">

    <!-- Back -->
    <button class="btn-back" @click="router.back()">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="15" height="15"><polyline points="15 18 9 12 15 6"/></svg>
      Înapoi
    </button>

    <!-- Header card -->
    <div class="patient-header">
      <div class="ph-avatar">{{ initials(patient) }}</div>
      <div class="ph-info">
        <h2 class="ph-name">{{ patient.lastName }} {{ patient.firstName }}</h2>
        <div class="ph-meta">
          <span class="ph-chip">{{ patient.age }} ani</span>
          <span class="ph-chip">{{ patient.gender }}</span>
          <span class="ph-chip chip-doctor">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="12" height="12"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
            Dr. {{ patient.doctorName }}
          </span>
          <span class="ph-chip chip-spec">{{ patient.doctorSpec }}</span>
        </div>
      </div>
      <div class="ph-actions" v-if="isMyPatient">
        <button class="btn-edit" @click="openEditModal">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="14" height="14"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4z"/></svg>
          Editează
        </button>
        <button class="btn-analize" @click="showAnalizaModal = true">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="14" height="14"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="12" y1="18" x2="12" y2="12"/><line x1="9" y1="15" x2="15" y2="15"/></svg>
          Cerere Analize
        </button>
      </div>
    </div>

    <!-- Description -->
    <div class="desc-card">
      <div class="dc-head">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
        Descriere Medicală
      </div>
      <p class="dc-text">{{ patient.medicalDescription || 'Nicio descriere medicală adăugată.' }}</p>
    </div>

    <!-- Two cols: requests + results -->
    <div class="detail-grid">

      <!-- Analysis Requests -->
      <div class="detail-card">
        <div class="card-head">
          <h3 class="card-title">Cereri Analize</h3>
          <span class="card-count">{{ analysisRequests.length }}</span>
        </div>
        <div class="card-body">
          <div v-if="analysisRequests.length > 0">
            <div class="req-item" v-for="req in analysisRequests" :key="req.id">
              <div class="req-left">
                <div class="req-icon">🔬</div>
                <div>
                  <p class="req-type">{{ req.analysisType }}</p>
                  <p class="req-date">{{ req.date }}</p>
                </div>
              </div>
              <span class="req-badge" :class="'badge-' + req.status">
                {{ statusLabel(req.status) }}
              </span>
              <p v-if="req.acceptedBy" class="req-accepted"> {{ req.acceptedByName }}</p>
            </div>
          </div>
          <div v-else class="empty-sm">
            <span>📋</span>
            <p>Nicio cerere de analize</p>
          </div>
        </div>
      </div>

      <!-- PDF Results -->
      <div class="detail-card">
        <div class="card-head">
          <h3 class="card-title">Rezultate Analize</h3>
          <span class="card-count">{{ results.length }}</span>
        </div>
        <div class="card-body">
          <div v-if="results.length > 0">
            <div class="result-item" v-for="r in results" :key="r.id">
              <div class="ri-icon">📄</div>
              <div class="ri-info">
                <p class="ri-title">{{ r.description || 'Rezultate analize' }}</p>
                <p class="ri-meta">Dr. {{ r.labDoctorName }} · {{ r.date }}</p>
              </div>
              <button class="btn-view-pdf" @click="viewPdf(r)">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="13" height="13"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                Vizualizează
              </button>
            </div>
          </div>
          <div v-else class="empty-sm">
            <span>📂</span>
            <p>Niciun rezultat disponibil</p>
          </div>
        </div>
      </div>

    </div>

    <!-- ═══ MODAL: PDF Viewer ═══ -->
    <Transition name="mfade">
      <div v-if="pdfModal.show" class="modal-overlay" @click.self="pdfModal.show = false">
        <div class="modal modal-pdf">
          <div class="modal-header">
            <h3 class="modal-title">{{ pdfModal.title }}</h3>
            <button class="modal-close" @click="pdfModal.show = false">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="18" height="18"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
          </div>
          <div class="pdf-viewer-body">
            <iframe v-if="pdfModal.url" :src="pdfModal.url" class="pdf-frame"></iframe>
            <div v-else class="pdf-placeholder">
              <div class="pp-icon">📄</div>
              <p class="pp-title">{{ pdfModal.title }}</p>
              <p class="pp-sub">PDF disponibil după conectarea la backend</p>
              <p class="pp-meta">Dr. {{ pdfModal.doctor }} · {{ pdfModal.date }}</p>
            </div>
          </div>
        </div>
      </div>
    </Transition>

    <!-- ═══ MODAL: Cerere Analize ═══ -->
    <Transition name="mfade">
      <div v-if="showAnalizaModal" class="modal-overlay" @click.self="showAnalizaModal = false">
        <div class="modal modal-sm">
          <div class="modal-header">
            <h3 class="modal-title">Cerere Analize Noi</h3>
            <button class="modal-close" @click="showAnalizaModal = false">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="18" height="18"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
          </div>
          <div class="modal-body">
            <div class="field">
              <label class="flbl">Tip Analiză *</label>
              <div class="analiza-chips">
                <button v-for="tip in tipuriAnalize" :key="tip" class="chip" :class="{ 'chip-active': analizaForm.type === tip }" @click="analizaForm.type = tip; analizaForm.customType = ''; analizaErr = ''">{{ tip }}</button>
              </div>
              <input v-model="analizaForm.customType" class="finput mt-1" placeholder="Alt tip de analiză..." @input="analizaForm.type = ''; analizaErr = ''" />
              <span v-if="analizaErr" class="ferr">{{ analizaErr }}</span>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn-cancel" @click="showAnalizaModal = false">Anulează</button>
            <button class="btn-save" @click="submitAnaliza" :disabled="sendingAnaliza">
              <span v-if="!sendingAnaliza">Trimite Cererea</span>
              <span v-else class="spin"></span>
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- Toast -->
    <Transition name="toast-slide">
      <div v-if="toast.show" class="toast">✅ {{ toast.message }}</div>
    </Transition>

  </div>

  <div v-else class="page">
    <div class="empty-state"><div class="es-icon">🔍</div><p class="es-title">Pacient negăsit</p></div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { medicinaApi } from '@/api/medicina'

const router    = useRouter()
const route     = useRoute()
const authStore = useAuthStore()

const patient          = ref(null)
const analysisRequests = ref([])
const results          = ref([])
const pageLoading      = ref(false)

const isMyPatient = computed(() =>
  patient.value?.doctorId === authStore.user?.id
)

function initials(p) { return ((p.firstName?.[0] ?? '') + (p.lastName?.[0] ?? '')).toUpperCase() }

async function fetchAll() {
  pageLoading.value = true
  try {
    const id = Number(route.params.id)
    const [pRes, rqRes, rsRes] = await Promise.all([
      medicinaApi.getPatient(id),
      medicinaApi.getPatientRequests(id),
      medicinaApi.getPatientResults(id)
    ])
    patient.value          = pRes.data
    analysisRequests.value = rqRes.data
    results.value          = rsRes.data
  } catch (e) {
    console.error('Eroare la încărcarea datelor pacientului:', e)
  } finally {
    pageLoading.value = false
  }
}

onMounted(fetchAll)

function statusLabel(s) {
  return { pending: 'În așteptare', accepted: 'Acceptat', completed: 'Finalizat' }[s] || s
}

function formatDate(d) {
  if (!d) return ''
  return new Date(d).toLocaleDateString('ro-RO', { day: 'numeric', month: 'short', year: 'numeric' })
}

// ── PDF viewer ──────────────────────────────────────────
const pdfModal = reactive({ show: false, url: null, title: '', doctor: '', date: '' })
function viewPdf(r) {
  pdfModal.url    = r.pdfUrl
  pdfModal.title  = r.description || 'Rezultate analize'
  pdfModal.doctor = r.labDoctorName
  pdfModal.date   = r.date
  pdfModal.show   = true
}

// ── Cerere analize ──────────────────────────────────────
const showAnalizaModal = ref(false)
const sendingAnaliza   = ref(false)
const analizaErr       = ref('')
const analizaForm      = reactive({ type: '', customType: '' })
const tipuriAnalize    = ['Sânge', 'Colesterol', 'Glucoză', 'Urină', 'Coagulare', 'Ecografie', 'Inimă', 'Hepatită']

async function submitAnaliza() {
  const finalType = analizaForm.type || analizaForm.customType.trim()
  if (!finalType) { analizaErr.value = 'Selectați sau scrieți tipul de analiză'; return }
  sendingAnaliza.value = true
  try {
    await medicinaApi.createAnalysisRequest(patient.value.id, finalType)
    await fetchAll()
    sendingAnaliza.value   = false
    showAnalizaModal.value = false
    showToast(`Cerere „${finalType}" trimisă`)
  } catch (e) {
    analizaErr.value = e.response?.data?.message || 'Eroare la trimitere'
    sendingAnaliza.value = false
  }
}

// ── Edit modal (simplified, redirect to PacientiiMei) ──
function openEditModal() { router.push('/medicina/pacientii-mei') }

// ── Toast ───────────────────────────────────────────────
const toast = reactive({ show: false, message: '' })
function showToast(msg) { toast.message = msg; toast.show = true; setTimeout(() => toast.show = false, 3000) }
</script>

<style scoped>
.page { display: flex; flex-direction: column; gap: 1.5rem; }

.btn-back {
  display: inline-flex; align-items: center; gap: 0.4rem;
  background: none; border: none; cursor: pointer;
  font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.85rem; font-weight: 600;
  color: #64748b; padding: 0; transition: color 0.15s;
}
.btn-back:hover { color: #0ea5e9; }

.patient-header {
  background: #fff; border-radius: 14px; border: 1px solid #e8edf5;
  padding: 1.5rem; display: flex; align-items: center; gap: 1.25rem;
  box-shadow: 0 1px 3px rgba(15,34,64,.05); flex-wrap: wrap;
}
.ph-avatar {
  width: 64px; height: 64px; border-radius: 50%; flex-shrink: 0;
  background: linear-gradient(135deg, #0ea5e9, #06b6d4);
  display: flex; align-items: center; justify-content: center;
  font-family: 'DM Serif Display', serif; font-size: 1.5rem; font-weight: 700; color: #fff;
}
.ph-info { flex: 1; min-width: 0; }
.ph-name { font-family: 'DM Serif Display', serif; font-size: 1.6rem; color: #071a2e; }
.ph-meta { display: flex; flex-wrap: wrap; gap: 0.5rem; margin-top: 0.5rem; }
.ph-chip {
  padding: 0.25rem 0.7rem; border-radius: 100px;
  font-size: 0.78rem; font-weight: 600;
  background: #f1f5f9; color: #64748b;
}
.chip-doctor { background: rgba(14,165,233,.1); color: #0284c7; display: flex; align-items: center; gap: 0.3rem; }
.chip-spec { background: rgba(14,165,233,.08); color: #0ea5e9; }
.ph-actions { display: flex; gap: 0.6rem; flex-shrink: 0; }

.btn-edit {
  display: inline-flex; align-items: center; gap: 0.4rem;
  padding: 0.55rem 1rem; background: #f8fafc; border: 1.5px solid #e2e8f0; border-radius: 9px;
  font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.82rem; font-weight: 600; color: #64748b;
  cursor: pointer; transition: all 0.18s;
}
.btn-edit:hover { border-color: #0ea5e9; color: #0ea5e9; background: #f0f9ff; }

.btn-analize {
  display: inline-flex; align-items: center; gap: 0.4rem;
  padding: 0.55rem 1rem; background: #0ea5e9; border: none; border-radius: 9px;
  font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.82rem; font-weight: 600; color: #fff;
  cursor: pointer; transition: all 0.18s;
}
.btn-analize:hover { background: #0284c7; transform: translateY(-1px); }

.desc-card {
  background: #fff; border-radius: 14px; border: 1px solid #e8edf5;
  padding: 1.4rem; box-shadow: 0 1px 3px rgba(15,34,64,.05);
}
.dc-head {
  display: flex; align-items: center; gap: 0.5rem;
  font-family: 'DM Serif Display', serif; font-size: 0.95rem; font-weight: 700; color: #071a2e;
  margin-bottom: 0.85rem;
}
.dc-text { font-size: 0.9rem; color: #334155; line-height: 1.7; }

.detail-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1.25rem; }
.detail-card { background: #fff; border-radius: 14px; border: 1px solid #e8edf5; overflow: hidden; box-shadow: 0 1px 3px rgba(15,34,64,.05); }
.card-head {
  padding: 1rem 1.4rem; border-bottom: 1px solid #f1f5f9;
  display: flex; align-items: center; justify-content: space-between;
}
.card-title { font-family: 'DM Serif Display', serif; font-size: 0.95rem; color: #071a2e; }
.card-count {
  width: 24px; height: 24px; border-radius: 50%; background: #f1f5f9;
  display: flex; align-items: center; justify-content: center;
  font-size: 0.75rem; font-weight: 700; color: #64748b;
}
.card-body { padding: 0.5rem 0; }

.req-item {
  padding: 0.85rem 1.4rem; border-bottom: 1px solid #f8fafc;
  display: flex; align-items: center; gap: 0.85rem; flex-wrap: wrap;
}
.req-item:last-child { border-bottom: none; }
.req-left { display: flex; align-items: center; gap: 0.65rem; flex: 1; min-width: 0; }
.req-icon { font-size: 1.1rem; flex-shrink: 0; }
.req-type { font-size: 0.87rem; font-weight: 700; color: #071a2e; }
.req-date { font-size: 0.74rem; color: #94a3b8; margin-top: 0.1rem; }
.req-badge { padding: 0.25rem 0.65rem; border-radius: 100px; font-size: 0.72rem; font-weight: 700; }
.badge-pending   { background: #fffbeb; color: #d97706; }
.badge-accepted  { background: #f0f9ff; color: #0ea5e9; }
.badge-completed { background: #f0fdf4; color: #059669; }
.req-accepted { font-size: 0.72rem; color: #64748b; width: 100%; padding-left: 1.75rem; font-style: italic; }

.result-item {
  padding: 0.85rem 1.4rem; border-bottom: 1px solid #f8fafc;
  display: flex; align-items: center; gap: 0.85rem;
}
.result-item:last-child { border-bottom: none; }
.ri-icon { font-size: 1.3rem; flex-shrink: 0; }
.ri-info { flex: 1; min-width: 0; }
.ri-title { font-size: 0.87rem; font-weight: 700; color: #071a2e; }
.ri-meta { font-size: 0.74rem; color: #94a3b8; margin-top: 0.1rem; }
.btn-view-pdf {
  display: inline-flex; align-items: center; gap: 0.35rem;
  padding: 0.4rem 0.85rem; background: rgba(14,165,233,.1); color: #0ea5e9;
  border: 1px solid rgba(14,165,233,.2); border-radius: 7px;
  font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.75rem; font-weight: 600;
  cursor: pointer; transition: all 0.18s; white-space: nowrap;
}
.btn-view-pdf:hover { background: #0ea5e9; color: #fff; }

.empty-sm { display: flex; flex-direction: column; align-items: center; gap: 0.4rem; padding: 2rem; text-align: center; font-size: 0.85rem; color: #94a3b8; }
.empty-sm span { font-size: 1.5rem; opacity: 0.4; }

.empty-state { text-align: center; padding: 4rem; display: flex; flex-direction: column; align-items: center; gap: 0.6rem; }
.es-icon { font-size: 3rem; opacity: 0.3; }
.es-title { font-family: 'DM Serif Display', serif; font-size: 1.15rem; color: #64748b; }

/* PDF Modal */
.modal-pdf { max-width: 800px; }
.pdf-viewer-body { height: 560px; display: flex; align-items: center; justify-content: center; background: #f8fafc; }
.pdf-frame { width: 100%; height: 100%; border: none; }
.pdf-placeholder { display: flex; flex-direction: column; align-items: center; gap: 0.75rem; padding: 2.5rem; text-align: center; }
.pp-icon  { font-size: 3.5rem; opacity: 0.3; }
.pp-title { font-family: 'DM Serif Display', serif; font-size: 1.15rem; color: #071a2e; }
.pp-sub   { font-size: 0.85rem; color: #94a3b8; }
.pp-meta  { font-size: 0.8rem; color: #64748b; font-style: italic; }

/* Shared modal + form styles */
.modal-overlay { position: fixed; inset: 0; background: rgba(7,26,46,.55); backdrop-filter: blur(4px); z-index: 200; display: flex; align-items: center; justify-content: center; padding: 1rem; }
.modal { background: #fff; border-radius: 16px; box-shadow: 0 24px 48px rgba(7,26,46,.2); width: 100%; max-width: 500px; max-height: 92vh; overflow-y: auto; }
.modal-sm { max-width: 420px; }
.modal-header { padding: 1.4rem 1.5rem; border-bottom: 1px solid #f1f5f9; display: flex; align-items: center; justify-content: space-between; }
.modal-title { font-family: 'DM Serif Display', serif; font-size: 1.15rem; color: #071a2e; }
.modal-close { background: none; border: none; cursor: pointer; color: #94a3b8; padding: 0.3rem; border-radius: 6px; display: flex; align-items: center; transition: all 0.15s; }
.modal-close:hover { background: #f1f5f9; color: #334155; }
.modal-body { padding: 1.4rem 1.5rem; display: flex; flex-direction: column; gap: 1rem; }
.modal-footer { padding: 1rem 1.5rem; border-top: 1px solid #f1f5f9; display: flex; justify-content: flex-end; gap: 0.75rem; }

.field { display: flex; flex-direction: column; gap: 0.5rem; }
.flbl { font-size: 0.78rem; font-weight: 700; color: #64748b; text-transform: uppercase; letter-spacing: 0.05em; }
.finput { padding: 0.65rem 0.9rem; border: 1.5px solid #e2e8f0; border-radius: 8px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.9rem; color: #071a2e; outline: none; transition: border-color 0.2s; background: #fff; width: 100%; }
.finput:focus { border-color: #0ea5e9; box-shadow: 0 0 0 3px rgba(14,165,233,.1); }
.ferr { font-size: 0.75rem; color: #dc2626; }
.mt-1 { margin-top: 0.5rem; }

.analiza-chips { display: flex; flex-wrap: wrap; gap: 0.5rem; }
.chip { padding: 0.35rem 0.85rem; border-radius: 100px; border: 1.5px solid #e2e8f0; background: #f8fafc; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.82rem; font-weight: 600; color: #64748b; cursor: pointer; transition: all 0.18s; }
.chip:hover { border-color: #0ea5e9; color: #0ea5e9; }
.chip-active { border-color: #0ea5e9; color: #0ea5e9; background: rgba(14,165,233,.1); }

.btn-cancel { padding: 0.6rem 1.2rem; background: #f1f5f9; border: none; border-radius: 8px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 600; color: #64748b; cursor: pointer; }
.btn-cancel:hover { background: #e2e8f0; }
.btn-save { padding: 0.6rem 1.4rem; background: #0ea5e9; border: none; border-radius: 8px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 700; color: #fff; cursor: pointer; display: flex; align-items: center; min-width: 130px; justify-content: center; }
.btn-save:hover:not(:disabled) { background: #0284c7; }
.btn-save:disabled { opacity: 0.55; cursor: not-allowed; }
.spin { display: inline-block; width: 16px; height: 16px; border: 2px solid rgba(255,255,255,.3); border-top-color: #fff; border-radius: 50%; animation: sp 0.7s linear infinite; }
@keyframes sp { to { transform: rotate(360deg); } }

.toast { position: fixed; bottom: 2rem; right: 2rem; z-index: 999; padding: 0.85rem 1.25rem; border-radius: 10px; background: #071a2e; color: #fff; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 600; box-shadow: 0 8px 24px rgba(0,0,0,.15); }
.toast-slide-enter-active { animation: ti .3s ease; }
.toast-slide-leave-active { animation: ti .25s ease reverse; }
@keyframes ti { from { opacity: 0; transform: translateY(12px); } to { opacity: 1; transform: translateY(0); } }

.mfade-enter-active { animation: mIn .22s ease; }
.mfade-leave-active { animation: mIn .18s ease reverse; }
@keyframes mIn { from { opacity: 0; transform: scale(.96) translateY(8px); } to { opacity: 1; transform: scale(1) translateY(0); } }

@media(max-width:800px) { .detail-grid { grid-template-columns: 1fr; } .ph-actions { width: 100%; } }
</style>
