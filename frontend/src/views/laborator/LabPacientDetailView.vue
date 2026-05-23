<template>
  <div class="page" v-if="patient">

    <button class="btn-back" @click="router.back()">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="15" height="15"><polyline points="15 18 9 12 15 6"/></svg>
      Înapoi la Pacienți
    </button>

    <!-- Header -->
    <div class="patient-header">
      <div class="ph-avatar">{{ initials(patient) }}</div>
      <div class="ph-info">
        <h2 class="ph-name">{{ patient.lastName }} {{ patient.firstName }}</h2>
        <div class="ph-chips">
          <span class="chip">{{ patient.age }} ani</span>
          <span class="chip">{{ patient.gender }}</span>
          <span class="chip chip-ro">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="11" height="11"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
            Dr. {{ patient.doctorName }}
          </span>
          <span class="chip chip-spec">{{ patient.doctorSpec }}</span>
        </div>
        <div class="readonly-notice">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="13" height="13"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
          Date în regim de citire — nu pot fi modificate
        </div>
      </div>
      <button class="btn-upload-main" @click="showUploadModal = true">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="15" height="15"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" y1="3" x2="12" y2="15"/></svg>
        Încarcă Rezultate PDF
      </button>
    </div>

    <!-- Medical description — read only -->
    <div class="desc-card">
      <div class="dc-head">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="15" height="15"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
        Descriere Medicală
        <span class="ro-badge">Doar citire</span>
      </div>
      <p class="dc-text">{{ patient.medicalDescription || 'Nicio descriere medicală adăugată.' }}</p>
    </div>

    <!-- Two cols -->
    <div class="detail-grid">

      <!-- Analysis requests -->
      <div class="detail-card">
        <div class="card-head">
          <h3 class="card-title">Cereri Analize</h3>
          <span class="card-count">{{ requests.length }}</span>
        </div>
        <div v-if="requests.length > 0">
          <div class="req-item" v-for="r in requests" :key="r.id">
            <div class="ri-left">
              <div class="ri-icon">🔬</div>
              <div>
                <p class="ri-type">{{ r.analysisType }}</p>
                <p class="ri-date">{{ formatDate(r.uploadedAt || r.createdAt) }}</p>
                <p v-if="r.acceptedBy" class="ri-accepted">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="11" height="11"><polyline points="20 6 9 17 4 12"/></svg>
                  {{ r.acceptedByName }}
                </p>
              </div>
            </div>
            <span class="ri-badge" :class="'badge-' + r.status">{{ statusLabel(r.status) }}</span>
          </div>
        </div>
        <div v-else class="empty-sm"><span>📋</span><p>Nicio cerere</p></div>
      </div>

      <!-- PDF results -->
      <div class="detail-card">
        <div class="card-head">
          <h3 class="card-title">Rezultate Încărcate</h3>
          <span class="card-count">{{ results.length }}</span>
        </div>
        <div v-if="results.length > 0">
          <div class="result-item" v-for="r in results" :key="r.id">
            <div class="res-icon">📄</div>
            <div class="res-info">
              <p class="res-title">{{ r.description }}</p>
              <p class="res-meta">{{ r.labDoctorName || (r.labDoctorFirstName + ' ' + r.labDoctorLastName) }} · {{ formatDate(r.uploadedAt || r.createdAt) }}</p>
            </div>
            <button class="btn-view-pdf" @click="viewPdf(r)">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="12" height="12"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
              Vezi
            </button>
          </div>
        </div>
        <div v-else class="empty-sm"><span>📂</span><p>Niciun rezultat încărcat</p></div>
      </div>

    </div>

    <!-- ═══ MODAL: Upload PDF ═══ -->
    <Transition name="mfade">
      <div v-if="showUploadModal" class="modal-overlay" @click.self="closeUpload">
        <div class="modal">
          <div class="modal-header">
            <h3 class="modal-title">Încarcă Rezultate PDF</h3>
            <button class="modal-close" @click="closeUpload">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="18" height="18"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
          </div>
          <div class="modal-body">

            <!-- Patient badge -->
            <div class="upload-patient-badge">
              <div class="upb-av">{{ initials(patient) }}</div>
              <div>
                <p class="upb-name">{{ patient.lastName }} {{ patient.firstName }}</p>
                <p class="upb-meta">{{ patient.age }} ani · {{ patient.gender }}</p>
              </div>
            </div>

            <!-- Link to request (optional) -->
            <div class="field">
              <label class="flbl">Asociat cu cererea (opțional)</label>
              <select v-model="uploadForm.requestId" class="finput">
                <option value="">— Fără asociere —</option>
                <option v-for="r in acceptedRequests" :key="r.id" :value="r.id">
                  {{ r.analysisType }} ({{ formatDate(r.uploadedAt || r.createdAt) }})
                </option>
              </select>
            </div>

            <div class="field">
              <label class="flbl">Descriere rezultate</label>
              <input v-model="uploadForm.description" class="finput" placeholder="ex: Rezultate sânge complet + colesterol" />
            </div>

            <!-- File drop zone -->
            <div
              class="drop-zone"
              :class="{ 'dz-over': isDragOver, 'dz-has-file': uploadForm.file }"
              @dragover.prevent="isDragOver = true"
              @dragleave="isDragOver = false"
              @drop.prevent="onDrop"
              @click="fileInput.click()"
            >
              <input ref="fileInput" type="file" accept=".pdf" style="display:none" @change="onFileChange" />
              <div v-if="!uploadForm.file" class="dz-empty">
                <div class="dz-icon">📄</div>
                <p class="dz-title">Trage PDF-ul aici sau apasă pentru a selecta</p>
                <p class="dz-sub">Acceptă fișiere .pdf · Recomandare: PDF cu mai multe pagini pentru multiple analize</p>
              </div>
              <div v-else class="dz-file">
                <div class="dz-file-icon">📄</div>
                <div class="dz-file-info">
                  <p class="dz-file-name">{{ uploadForm.file.name }}</p>
                  <p class="dz-file-size">{{ formatSize(uploadForm.file.size) }}</p>
                </div>
                <button class="dz-remove" @click.stop="uploadForm.file = null">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="14" height="14"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
                </button>
              </div>
            </div>

            <span v-if="uploadErr" class="ferr">{{ uploadErr }}</span>

            <div class="upload-tip">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="13" height="13"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
              <span>Poți include mai multe tipuri de analize în același PDF cu mai multe pagini, fără a suprascrie rezultatele anterioare.</span>
            </div>

          </div>
          <div class="modal-footer">
            <button class="btn-cancel" @click="closeUpload">Anulează</button>
            <button class="btn-upload-confirm" @click="submitUpload" :disabled="uploading">
              <span v-if="!uploading">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="14" height="14"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" y1="3" x2="12" y2="15"/></svg>
                Încarcă PDF
              </span>
              <span v-else class="spin"></span>
            </button>
          </div>
        </div>
      </div>
    </Transition>

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
              <p class="pp-meta">{{ pdfModal.meta }}</p>
            </div>
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
import { laboratorApi } from '@/api/laborator'

const router  = useRouter()
const route   = useRoute()

const patient     = ref(null)
const requests    = ref([])
const results     = ref([])
const pageLoading = ref(false)

function initials(p) { return ((p.firstName?.[0] ?? '') + (p.lastName?.[0] ?? '')).toUpperCase() }
function statusLabel(s) { return { pending: 'În așteptare', accepted: 'Acceptat', completed: 'Finalizat' }[s] || s }
function formatDate(d) {
  if (!d) return ''
  return new Date(d).toLocaleDateString('ro-RO', { day: 'numeric', month: 'short', year: 'numeric' })
}

async function fetchAll() {
  pageLoading.value = true
  try {
    const id = Number(route.params.id)
    const [pRes, rqRes, rsRes] = await Promise.all([
      laboratorApi.getPatient(id),
      laboratorApi.getPatientResults(id),
      laboratorApi.getPatientResults(id)
    ])
    patient.value = pRes.data
    results.value = rsRes.data
    // Fetch requests via patient ID from pending+mine
    const [pendRes, mineRes] = await Promise.all([
      laboratorApi.getPendingRequests(),
      laboratorApi.getMyRequests()
    ])
    requests.value = [...pendRes.data, ...mineRes.data].filter(r => r.patientId === id)
  } catch (e) {
    console.error('Eroare la încărcarea datelor:', e)
  } finally {
    pageLoading.value = false
  }
}

onMounted(fetchAll)

const acceptedRequests = computed(() => requests.value.filter(r => r.status === 'accepted'))

// ── PDF Viewer ────────────────────────────────────────────
const pdfModal = reactive({ show: false, url: null, title: '', meta: '' })
function viewPdf(r) {
  pdfModal.url   = r.pdfUrl
  pdfModal.title = r.description
  pdfModal.meta  = `${r.labDoctorName || (r.labDoctorFirstName + ' ' + r.labDoctorLastName)} · ${formatDate(r.uploadedAt || r.createdAt)}`
  pdfModal.show  = true
}

// ── Upload ────────────────────────────────────────────────
const showUploadModal = ref(false)
const uploading       = ref(false)
const isDragOver      = ref(false)
const uploadErr       = ref('')
const fileInput       = ref(null)
const uploadForm      = reactive({ file: null, description: '', requestId: '' })

function closeUpload() {
  showUploadModal.value = false
  uploadForm.file = null; uploadForm.description = ''; uploadForm.requestId = ''
  uploadErr.value = ''
}
function onDrop(e) {
  isDragOver.value = false
  const f = e.dataTransfer.files[0]
  if (f && f.type === 'application/pdf') { uploadForm.file = f; uploadErr.value = '' }
  else uploadErr.value = 'Doar fișiere PDF sunt acceptate'
}
function onFileChange(e) {
  const f = e.target.files[0]
  if (f) { uploadForm.file = f; uploadErr.value = '' }
}
function formatSize(bytes) {
  if (bytes < 1024) return bytes + ' B'
  if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB'
  return (bytes / (1024 * 1024)).toFixed(1) + ' MB'
}

async function submitUpload() {
  if (!uploadForm.file) { uploadErr.value = 'Selectați un fișier PDF'; return }
  uploading.value = true
  try {
    const formData = new FormData()
    formData.append('file', uploadForm.file)
    formData.append('patientId', patient.value.id)
    if (uploadForm.description) formData.append('description', uploadForm.description)
    if (uploadForm.requestId)   formData.append('analysisRequestId', uploadForm.requestId)

    await laboratorApi.uploadResult(formData)
    await fetchAll()
    uploading.value = false
    closeUpload()
    showToast('Rezultate încărcate cu succes')
  } catch (e) {
    uploadErr.value = e.response?.data?.message || 'Eroare la încărcare'
    uploading.value = false
  }
}

// ── Toast ─────────────────────────────────────────────────
const toast = reactive({ show: false, message: '' })
function showToast(msg) { toast.message = msg; toast.show = true; setTimeout(() => toast.show = false, 3000) }
</script>

<style scoped>
.page { display: flex; flex-direction: column; gap: 1.5rem; }

.btn-back { display: inline-flex; align-items: center; gap: 0.4rem; background: none; border: none; cursor: pointer; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.85rem; font-weight: 600; color: #64748b; padding: 0; transition: color 0.15s; }
.btn-back:hover { color: #16a34a; }

.patient-header { background: #fff; border-radius: 14px; border: 1px solid #e8edf5; padding: 1.5rem; display: flex; align-items: center; gap: 1.25rem; box-shadow: 0 1px 3px rgba(15,34,64,.05); flex-wrap: wrap; }
.ph-avatar { width: 64px; height: 64px; border-radius: 50%; flex-shrink: 0; background: linear-gradient(135deg, #16a34a, #22c55e); display: flex; align-items: center; justify-content: center; font-family: 'DM Serif Display', serif; font-size: 1.5rem; font-weight: 700; color: #fff; }
.ph-info { flex: 1; min-width: 0; }
.ph-name { font-family: 'DM Serif Display', serif; font-size: 1.6rem; color: #0f1f14; }
.ph-chips { display: flex; flex-wrap: wrap; gap: 0.5rem; margin-top: 0.5rem; }
.chip { padding: 0.25rem 0.7rem; border-radius: 100px; font-size: 0.78rem; font-weight: 600; background: #f1f5f9; color: #64748b; }
.chip-ro { background: rgba(14,165,233,.1); color: #0284c7; display: flex; align-items: center; gap: 0.3rem; }
.chip-spec { background: rgba(14,165,233,.08); color: #0ea5e9; }
.readonly-notice { display: flex; align-items: center; gap: 0.4rem; font-size: 0.75rem; color: #94a3b8; margin-top: 0.6rem; font-style: italic; }
.btn-upload-main { display: inline-flex; align-items: center; gap: 0.5rem; padding: 0.65rem 1.25rem; background: #16a34a; color: #fff; border: none; border-radius: 10px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 600; cursor: pointer; transition: all 0.2s; white-space: nowrap; flex-shrink: 0; }
.btn-upload-main:hover { background: #15803d; transform: translateY(-1px); box-shadow: 0 4px 12px rgba(22,163,74,.3); }

.desc-card { background: #fff; border-radius: 14px; border: 1px solid #e8edf5; padding: 1.4rem; box-shadow: 0 1px 3px rgba(15,34,64,.05); }
.dc-head { display: flex; align-items: center; gap: 0.5rem; font-family: 'DM Serif Display', serif; font-size: 0.95rem; font-weight: 700; color: #0f1f14; margin-bottom: 0.85rem; }
.ro-badge { margin-left: auto; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.7rem; font-weight: 700; padding: 0.15rem 0.6rem; border-radius: 100px; background: #f1f5f9; color: #94a3b8; }
.dc-text { font-size: 0.9rem; color: #334155; line-height: 1.7; }

.detail-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1.25rem; }
.detail-card { background: #fff; border-radius: 14px; border: 1px solid #e8edf5; overflow: hidden; box-shadow: 0 1px 3px rgba(15,34,64,.05); }
.card-head { padding: 1rem 1.4rem; border-bottom: 1px solid #f1f5f9; display: flex; align-items: center; justify-content: space-between; }
.card-title { font-family: 'DM Serif Display', serif; font-size: 0.95rem; color: #0f1f14; }
.card-count { width: 24px; height: 24px; border-radius: 50%; background: #f1f5f9; display: flex; align-items: center; justify-content: center; font-size: 0.75rem; font-weight: 700; color: #64748b; }

.req-item { padding: 0.85rem 1.4rem; border-bottom: 1px solid #f8fafc; display: flex; align-items: flex-start; justify-content: space-between; gap: 0.85rem; }
.req-item:last-child { border-bottom: none; }
.ri-left { display: flex; align-items: flex-start; gap: 0.65rem; flex: 1; }
.ri-icon { font-size: 1.1rem; flex-shrink: 0; margin-top: 0.1rem; }
.ri-type { font-size: 0.87rem; font-weight: 700; color: #0f1f14; }
.ri-date { font-size: 0.74rem; color: #94a3b8; margin-top: 0.1rem; }
.ri-accepted { display: flex; align-items: center; gap: 0.3rem; font-size: 0.74rem; color: #16a34a; font-weight: 600; font-style: italic; margin-top: 0.2rem; }
.ri-badge { padding: 0.25rem 0.65rem; border-radius: 100px; font-size: 0.72rem; font-weight: 700; white-space: nowrap; flex-shrink: 0; }
.badge-pending   { background: #fffbeb; color: #d97706; }
.badge-accepted  { background: #f0f9ff; color: #0ea5e9; }
.badge-completed { background: #f0fdf4; color: #059669; }

.result-item { padding: 0.85rem 1.4rem; border-bottom: 1px solid #f8fafc; display: flex; align-items: center; gap: 0.85rem; }
.result-item:last-child { border-bottom: none; }
.res-icon { font-size: 1.3rem; flex-shrink: 0; }
.res-info { flex: 1; min-width: 0; }
.res-title { font-size: 0.87rem; font-weight: 700; color: #0f1f14; }
.res-meta { font-size: 0.74rem; color: #94a3b8; margin-top: 0.1rem; }
.btn-view-pdf { display: inline-flex; align-items: center; gap: 0.35rem; padding: 0.35rem 0.75rem; background: rgba(34,197,94,.1); color: #16a34a; border: 1px solid rgba(34,197,94,.2); border-radius: 7px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.74rem; font-weight: 600; cursor: pointer; transition: all 0.18s; white-space: nowrap; }
.btn-view-pdf:hover { background: #16a34a; color: #fff; }

.empty-sm { display: flex; flex-direction: column; align-items: center; gap: 0.4rem; padding: 2rem; text-align: center; font-size: 0.85rem; color: #94a3b8; }
.empty-sm span { font-size: 1.5rem; opacity: 0.4; }
.empty-state { text-align: center; padding: 4rem; display: flex; flex-direction: column; align-items: center; gap: 0.6rem; }
.es-icon { font-size: 3rem; opacity: 0.3; }
.es-title { font-family: 'DM Serif Display', serif; font-size: 1.15rem; color: #64748b; }

/* Modal */
.modal-overlay { position: fixed; inset: 0; background: rgba(15,31,20,.55); backdrop-filter: blur(4px); z-index: 200; display: flex; align-items: center; justify-content: center; padding: 1rem; }
.modal { background: #fff; border-radius: 16px; box-shadow: 0 24px 48px rgba(15,31,20,.2); width: 100%; max-width: 520px; max-height: 92vh; overflow-y: auto; }
.modal-pdf { max-width: 800px; }
.modal-header { padding: 1.4rem 1.5rem; border-bottom: 1px solid #f1f5f9; display: flex; align-items: center; justify-content: space-between; }
.modal-title { font-family: 'DM Serif Display', serif; font-size: 1.15rem; color: #0f1f14; }
.modal-close { background: none; border: none; cursor: pointer; color: #94a3b8; padding: 0.3rem; border-radius: 6px; display: flex; align-items: center; transition: all 0.15s; }
.modal-close:hover { background: #f1f5f9; color: #334155; }
.modal-body { padding: 1.4rem 1.5rem; display: flex; flex-direction: column; gap: 1rem; }
.modal-footer { padding: 1rem 1.5rem; border-top: 1px solid #f1f5f9; display: flex; justify-content: flex-end; gap: 0.75rem; }

.upload-patient-badge { display: flex; align-items: center; gap: 0.75rem; padding: 0.75rem 1rem; background: #f0fdf4; border: 1px solid #bbf7d0; border-radius: 10px; }
.upb-av { width: 38px; height: 38px; border-radius: 50%; background: linear-gradient(135deg, #16a34a, #22c55e); display: flex; align-items: center; justify-content: center; font-family: 'DM Serif Display', serif; font-weight: 700; font-size: 0.9rem; color: #fff; flex-shrink: 0; }
.upb-name { font-size: 0.9rem; font-weight: 700; color: #0f1f14; }
.upb-meta { font-size: 0.75rem; color: #64748b; margin-top: 0.1rem; }

.field { display: flex; flex-direction: column; gap: 0.4rem; }
.flbl { font-size: 0.78rem; font-weight: 700; color: #64748b; text-transform: uppercase; letter-spacing: 0.05em; }
.finput { padding: 0.65rem 0.9rem; border: 1.5px solid #e2e8f0; border-radius: 8px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.9rem; color: #0f1f14; outline: none; transition: border-color 0.2s; background: #fff; }
.finput:focus { border-color: #16a34a; box-shadow: 0 0 0 3px rgba(34,197,94,.1); }

/* Drop zone */
.drop-zone {
  border: 2px dashed #e2e8f0; border-radius: 12px;
  cursor: pointer; transition: all 0.2s; overflow: hidden;
}
.drop-zone:hover, .dz-over { border-color: #16a34a; background: #f0fdf4; }
.dz-has-file { border-color: #16a34a; border-style: solid; }
.dz-empty { display: flex; flex-direction: column; align-items: center; gap: 0.5rem; padding: 2.5rem 1.5rem; text-align: center; }
.dz-icon { font-size: 2.5rem; opacity: 0.4; }
.dz-title { font-size: 0.88rem; font-weight: 600; color: #334155; }
.dz-sub { font-size: 0.75rem; color: #94a3b8; line-height: 1.5; }
.dz-file { display: flex; align-items: center; gap: 0.85rem; padding: 1rem 1.25rem; background: #f0fdf4; }
.dz-file-icon { font-size: 1.75rem; flex-shrink: 0; }
.dz-file-info { flex: 1; min-width: 0; }
.dz-file-name { font-size: 0.88rem; font-weight: 600; color: #0f1f14; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.dz-file-size { font-size: 0.74rem; color: #94a3b8; margin-top: 0.1rem; }
.dz-remove { background: none; border: none; cursor: pointer; color: #94a3b8; padding: 0.3rem; border-radius: 6px; display: flex; align-items: center; transition: all 0.15s; flex-shrink: 0; }
.dz-remove:hover { background: #fef2f2; color: #dc2626; }

.upload-tip { display: flex; align-items: flex-start; gap: 0.5rem; padding: 0.75rem 1rem; background: #f8fafc; border-radius: 8px; font-size: 0.78rem; color: #64748b; line-height: 1.5; }
.upload-tip svg { flex-shrink: 0; margin-top: 2px; }

.ferr { font-size: 0.75rem; color: #dc2626; }

.btn-cancel { padding: 0.6rem 1.2rem; background: #f1f5f9; border: none; border-radius: 8px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 600; color: #64748b; cursor: pointer; }
.btn-cancel:hover { background: #e2e8f0; }
.btn-upload-confirm { display: inline-flex; align-items: center; gap: 0.5rem; padding: 0.6rem 1.4rem; background: #16a34a; border: none; border-radius: 8px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 700; color: #fff; cursor: pointer; transition: all 0.18s; min-width: 130px; justify-content: center; }
.btn-upload-confirm:hover:not(:disabled) { background: #15803d; }
.btn-upload-confirm:disabled { opacity: 0.55; cursor: not-allowed; }
.spin { display: inline-block; width: 16px; height: 16px; border: 2px solid rgba(255,255,255,.3); border-top-color: #fff; border-radius: 50%; animation: sp 0.7s linear infinite; }
@keyframes sp { to { transform: rotate(360deg); } }

/* PDF modal */
.pdf-viewer-body { height: 560px; display: flex; align-items: center; justify-content: center; background: #f8fafc; }
.pdf-frame { width: 100%; height: 100%; border: none; }
.pdf-placeholder { display: flex; flex-direction: column; align-items: center; gap: 0.75rem; padding: 2.5rem; text-align: center; }
.pp-icon { font-size: 3.5rem; opacity: 0.3; }
.pp-title { font-family: 'DM Serif Display', serif; font-size: 1.1rem; color: #0f1f14; }
.pp-sub { font-size: 0.85rem; color: #94a3b8; }
.pp-meta { font-size: 0.8rem; color: #64748b; font-style: italic; }

.toast { position: fixed; bottom: 2rem; right: 2rem; z-index: 999; padding: 0.85rem 1.25rem; border-radius: 10px; background: #0f1f14; color: #fff; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 600; box-shadow: 0 8px 24px rgba(0,0,0,.15); }
.toast-slide-enter-active { animation: ti .3s ease; }
.toast-slide-leave-active { animation: ti .25s ease reverse; }
@keyframes ti { from { opacity: 0; transform: translateY(12px); } to { opacity: 1; transform: translateY(0); } }

.mfade-enter-active { animation: mIn .22s ease; }
.mfade-leave-active { animation: mIn .18s ease reverse; }
@keyframes mIn { from { opacity: 0; transform: scale(.96) translateY(8px); } to { opacity: 1; transform: scale(1) translateY(0); } }

@media(max-width:800px) { .detail-grid { grid-template-columns: 1fr; } .ph-actions { width: 100%; } }
</style>
