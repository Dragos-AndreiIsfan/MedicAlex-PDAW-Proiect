<template>
  <div class="doctori-page">

    <div class="page-header">
      <div>
        <h2 class="page-title">Gestionare Doctori</h2>
        <p class="page-sub">Creează și administrează conturile medicilor</p>
      </div>
      <button class="btn-add" @click="openCreateModal">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="16" height="16"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
        Doctor Nou
      </button>
    </div>

    <div class="filter-tabs">
      <button v-for="tab in tabs" :key="tab.value" class="tab-btn" :class="{ active: activeTab === tab.value }" @click="activeTab = tab.value">
        {{ tab.label }} <span class="tab-count">{{ tab.count }}</span>
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="loading-state">
      <div class="spinner-lg"></div>
      <p>Se încarcă...</p>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="error-state">
      <p>⚠️ {{ error }}</p>
      <button class="btn-add" @click="fetchDoctors">Reîncearcă</button>
    </div>

    <div v-else class="doctors-grid">
      <TransitionGroup name="card-list">
        <div v-for="doctor in filteredDoctors" :key="doctor.id" class="doctor-card">
          <div class="dc-avatar" :class="doctor.role === 'doctor_medicina' ? 'av-med' : 'av-lab'">
            {{ initials(doctor) }}
          </div>
          <div class="dc-info">
            <p class="dc-name">Dr. {{ doctor.lastName }} {{ doctor.firstName }}</p>
            <p class="dc-email">{{ doctor.email }}</p>
            <p class="dc-spec" v-if="doctor.specialization">{{ doctor.specialization }}</p>
          </div>
          <div class="dc-badge-wrap">
            <span class="dc-role-badge" :class="doctor.role === 'doctor_medicina' ? 'badge-med' : 'badge-lab'">
              {{ doctor.role === 'doctor_medicina' ? '🩺 Medicină' : '🔬 Laborator' }}
            </span>
          </div>
          <button class="btn-delete" @click="confirmDelete(doctor)" title="Șterge cont">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="15" height="15"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/><path d="M10 11v6"/><path d="M14 11v6"/><path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2"/></svg>
          </button>
        </div>
      </TransitionGroup>
      <div v-if="filteredDoctors.length === 0" class="empty-state">
        <div class="es-icon">👨‍⚕️</div>
        <p class="es-title">Niciun doctor găsit</p>
        <p class="es-sub">Apasă „Doctor Nou" pentru a adăuga primul cont</p>
      </div>
    </div>

    <!-- MODAL: Creare Doctor -->
    <Transition name="modal-fade">
      <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
        <div class="modal">
          <div class="modal-header">
            <h3 class="modal-title">Cont Doctor Nou</h3>
            <button class="modal-close" @click="closeModal">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="18" height="18"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
          </div>
          <div class="modal-body">
            <div v-if="formError" class="form-alert">⚠️ {{ formError }}</div>
            <div class="email-preview">
              <span class="ep-label">Email generat:</span>
              <span class="ep-value">{{ generatedEmail }}</span>
            </div>
            <div class="form-row">
              <div class="field">
                <label class="field-label">Prenume *</label>
                <input v-model="form.firstName" class="field-input" :class="{ error: errors.firstName }" placeholder="ex: Ion" @input="errors.firstName=''" />
                <span v-if="errors.firstName" class="field-err">{{ errors.firstName }}</span>
              </div>
              <div class="field">
                <label class="field-label">Nume *</label>
                <input v-model="form.lastName" class="field-input" :class="{ error: errors.lastName }" placeholder="ex: Popescu" @input="errors.lastName=''" />
                <span v-if="errors.lastName" class="field-err">{{ errors.lastName }}</span>
              </div>
            </div>
            <div class="field">
              <label class="field-label">Specializare</label>
              <input v-model="form.specialization" class="field-input" placeholder="ex: Cardiologie, Neurologie..." />
            </div>
            <div class="field">
              <label class="field-label">Tip Cont *</label>
              <div class="role-selector">
                <label class="role-option" :class="{ selected: form.role === 'doctor_medicina' }">
                  <input type="radio" v-model="form.role" value="doctor_medicina" />
                  <span class="ro-icon">🩺</span>
                  <div class="ro-text"><span class="ro-title">Doctor Medicină</span><span class="ro-sub">Gestionează pacienți și solicită analize</span></div>
                </label>
                <label class="role-option" :class="{ selected: form.role === 'doctor_laborator' }">
                  <input type="radio" v-model="form.role" value="doctor_laborator" />
                  <span class="ro-icon">🔬</span>
                  <div class="ro-text"><span class="ro-title">Doctor Laborator</span><span class="ro-sub">Procesează cereri și încarcă rezultate</span></div>
                </label>
              </div>
              <span v-if="errors.role" class="field-err">{{ errors.role }}</span>
            </div>
            <div class="pw-notice">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="14" height="14"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
              Parola inițială va fi: <strong>Medic@1234</strong>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn-cancel" @click="closeModal">Anulează</button>
            <button class="btn-confirm" @click="createDoctor" :disabled="creating">
              <span v-if="!creating">Creează Cont</span>
              <span v-else class="spinner-sm"></span>
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- MODAL: Confirmare Ștergere -->
    <Transition name="modal-fade">
      <div v-if="showDeleteModal" class="modal-overlay" @click.self="showDeleteModal = false">
        <div class="modal modal-sm">
          <div class="modal-header">
            <h3 class="modal-title">Confirmare Ștergere</h3>
            <button class="modal-close" @click="showDeleteModal = false">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="18" height="18"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
          </div>
          <div class="modal-body">
            <div class="delete-confirm-body">
              <div class="dc-warn-icon">⚠️</div>
              <p class="dc-warn-text">Ești sigur că vrei să ștergi contul lui<br/><strong>Dr. {{ doctorToDelete?.lastName }} {{ doctorToDelete?.firstName }}</strong>?</p>
              <p class="dc-warn-sub">Această acțiune este ireversibilă.</p>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn-cancel" @click="showDeleteModal = false">Anulează</button>
            <button class="btn-delete-confirm" @click="deleteDoctor" :disabled="deleting">
              <span v-if="!deleting">Șterge Contul</span>
              <span v-else class="spinner-sm"></span>
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- Toast -->
    <Transition name="toast-slide">
      <div v-if="toast.show" class="toast">{{ toast.message }}</div>
    </Transition>

  </div>
</template>

<script setup>
import { ref, computed, reactive, onMounted } from 'vue'
import { adminApi } from '@/api/admin'

const doctors   = ref([])
const loading   = ref(false)
const error     = ref('')
const activeTab = ref('all')

const tabs = computed(() => [
  { value: 'all',              label: 'Toți',     count: doctors.value.length },
  { value: 'doctor_medicina',  label: 'Medicină', count: doctors.value.filter(d => d.role === 'doctor_medicina').length },
  { value: 'doctor_laborator', label: 'Laborator',count: doctors.value.filter(d => d.role === 'doctor_laborator').length },
])

const filteredDoctors = computed(() =>
  activeTab.value === 'all' ? doctors.value : doctors.value.filter(d => d.role === activeTab.value)
)

function initials(d) {
  return ((d.firstName?.[0] ?? '') + (d.lastName?.[0] ?? '')).toUpperCase()
}

async function fetchDoctors() {
  loading.value = true
  error.value   = ''
  try {
    const { data } = await adminApi.getDoctors()
    doctors.value = data
  } catch (e) {
    error.value = e.response?.data?.message || 'Eroare la încărcarea doctorilor.'
  } finally {
    loading.value = false
  }
}

onMounted(fetchDoctors)

// ── Create modal ──────────────────────────────────────────
const showModal = ref(false)
const creating  = ref(false)
const formError = ref('')
const form      = reactive({ firstName: '', lastName: '', specialization: '', role: '' })
const errors    = reactive({ firstName: '', lastName: '', role: '' })

function removeDiacritics(s) {
  return s.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase()
}

const generatedEmail = computed(() => {
  const fn = removeDiacritics(form.firstName.trim()).replace(/\s+/g, '.')
  const ln = removeDiacritics(form.lastName.trim()).replace(/\s+/g, '.')
  if (!fn && !ln) return 'prenume.nume@medicalex.ro'
  return `${fn || '—'}.${ln || '—'}@medicalex.ro`
})

function openCreateModal() {
  Object.assign(form, { firstName: '', lastName: '', specialization: '', role: '' })
  Object.assign(errors, { firstName: '', lastName: '', role: '' })
  formError.value = ''
  showModal.value = true
}
function closeModal() { showModal.value = false }

function validateForm() {
  let ok = true
  if (!form.firstName.trim()) { errors.firstName = 'Prenumele este obligatoriu'; ok = false }
  if (!form.lastName.trim())  { errors.lastName  = 'Numele este obligatoriu';    ok = false }
  if (!form.role)             { errors.role      = 'Selectați tipul contului';   ok = false }
  return ok
}

async function createDoctor() {
  formError.value = ''
  if (!validateForm()) return
  creating.value = true
  try {
    const { data } = await adminApi.createDoctor({
      firstName:      form.firstName,
      lastName:       form.lastName,
      specialization: form.specialization,
      role:           form.role
    })
    await fetchDoctors()
    showModal.value = false
    showToast(`✅ Cont creat: ${data.email} — Parolă: ${data.defaultPassword}`)
  } catch (e) {
    formError.value = e.response?.data?.message || 'Eroare la crearea contului.'
  } finally {
    creating.value = false
  }
}

// ── Delete modal ──────────────────────────────────────────
const showDeleteModal = ref(false)
const doctorToDelete  = ref(null)
const deleting        = ref(false)

function confirmDelete(doctor) { doctorToDelete.value = doctor; showDeleteModal.value = true }

async function deleteDoctor() {
  deleting.value = true
  try {
    await adminApi.deleteDoctor(doctorToDelete.value.id)
    await fetchDoctors()
    showDeleteModal.value = false
    showToast('🗑️ Cont șters cu succes')
  } catch (e) {
    showToast('⚠️ ' + (e.response?.data?.message || 'Eroare la ștergere'))
  } finally {
    deleting.value = false
  }
}

// ── Toast ─────────────────────────────────────────────────
const toast = reactive({ show: false, message: '' })
function showToast(msg) { toast.message = msg; toast.show = true; setTimeout(() => toast.show = false, 4000) }
</script>

<style scoped>
.doctori-page { display: flex; flex-direction: column; gap: 1.5rem; }
.page-header { display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem; }
.page-title { font-family: 'DM Serif Display', serif; font-size: 1.5rem; color: #07111f; }
.page-sub { font-size: 0.85rem; color: #64748b; margin-top: 0.2rem; }
.btn-add { display: inline-flex; align-items: center; gap: 0.5rem; padding: 0.65rem 1.25rem; background: #07111f; color: #fff; border: none; border-radius: 10px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 600; cursor: pointer; transition: all 0.2s; white-space: nowrap; }
.btn-add:hover { background: #1a3560; transform: translateY(-1px); }
.filter-tabs { display: flex; gap: 0.5rem; }
.tab-btn { display: flex; align-items: center; gap: 0.5rem; padding: 0.55rem 1.1rem; background: #fff; border: 1.5px solid #e8edf5; border-radius: 8px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.85rem; font-weight: 500; color: #64748b; cursor: pointer; transition: all 0.18s; }
.tab-btn:hover { border-color: #0088cc; color: #0088cc; }
.tab-btn.active { background: #07111f; border-color: #07111f; color: #fff; }
.tab-count { padding: 0.1rem 0.45rem; border-radius: 100px; font-size: 0.72rem; font-weight: 700; background: rgba(255,255,255,.15); }
.tab-btn:not(.active) .tab-count { background: #f1f5f9; color: #64748b; }
.loading-state { display: flex; flex-direction: column; align-items: center; gap: 1rem; padding: 3rem; color: #64748b; }
.spinner-lg { width: 36px; height: 36px; border: 3px solid #e2e8f0; border-top-color: #0088cc; border-radius: 50%; animation: spin 0.8s linear infinite; }
.error-state { display: flex; flex-direction: column; align-items: center; gap: 1rem; padding: 2rem; background: #fef2f2; border-radius: 12px; color: #dc2626; }
.doctors-grid { display: flex; flex-direction: column; gap: 0.6rem; }
.doctor-card { background: #fff; border: 1px solid #e8edf5; border-radius: 12px; padding: 1rem 1.25rem; display: flex; align-items: center; gap: 1rem; box-shadow: 0 1px 2px rgba(15,34,64,.04); transition: all 0.18s; }
.doctor-card:hover { box-shadow: 0 4px 12px rgba(15,34,64,.08); border-color: #cbd5e1; }
.dc-avatar { width: 42px; height: 42px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-family: 'DM Serif Display', serif; font-size: 1rem; font-weight: 700; color: #fff; flex-shrink: 0; }
.av-med { background: linear-gradient(135deg, #0088cc, #00b4d8); }
.av-lab { background: linear-gradient(135deg, #7c3aed, #a855f7); }
.dc-info { flex: 1; min-width: 0; }
.dc-name { font-size: 0.92rem; font-weight: 700; color: #07111f; }
.dc-email { font-size: 0.78rem; color: #64748b; margin-top: 0.1rem; }
.dc-spec { font-size: 0.78rem; color: #0088cc; font-weight: 600; margin-top: 0.15rem; }
.dc-badge-wrap { flex-shrink: 0; }
.dc-role-badge { padding: 0.3rem 0.75rem; border-radius: 100px; font-size: 0.75rem; font-weight: 700; }
.badge-med { background: rgba(0,136,204,.08); color: #0088cc; border: 1px solid rgba(0,136,204,.2); }
.badge-lab { background: rgba(124,58,237,.08); color: #7c3aed; border: 1px solid rgba(124,58,237,.2); }
.btn-delete { background: none; border: none; cursor: pointer; color: #cbd5e1; padding: 0.4rem; border-radius: 7px; display: flex; align-items: center; transition: all 0.18s; }
.btn-delete:hover { background: #fef2f2; color: #dc2626; }
.empty-state { text-align: center; padding: 3rem; display: flex; flex-direction: column; align-items: center; gap: 0.5rem; }
.es-icon { font-size: 2.5rem; opacity: 0.4; }
.es-title { font-family: 'DM Serif Display', serif; font-size: 1.1rem; color: #64748b; }
.es-sub { font-size: 0.82rem; color: #94a3b8; }
.card-list-enter-active { transition: all 0.25s ease; }
.card-list-leave-active { transition: all 0.2s ease; }
.card-list-enter-from { opacity: 0; transform: translateY(-8px); }
.card-list-leave-to { opacity: 0; transform: translateX(16px); }
.modal-overlay { position: fixed; inset: 0; background: rgba(7,17,31,.55); backdrop-filter: blur(4px); z-index: 200; display: flex; align-items: center; justify-content: center; padding: 1rem; }
.modal { background: #fff; border-radius: 16px; box-shadow: 0 24px 48px rgba(7,17,31,.2); width: 100%; max-width: 500px; max-height: 90vh; overflow-y: auto; }
.modal-sm { max-width: 400px; }
.modal-header { padding: 1.4rem 1.5rem; border-bottom: 1px solid #f1f5f9; display: flex; align-items: center; justify-content: space-between; }
.modal-title { font-family: 'DM Serif Display', serif; font-size: 1.15rem; color: #07111f; }
.modal-close { background: none; border: none; cursor: pointer; color: #94a3b8; padding: 0.3rem; border-radius: 6px; display: flex; align-items: center; transition: all 0.15s; }
.modal-close:hover { background: #f1f5f9; color: #334155; }
.modal-body { padding: 1.4rem 1.5rem; display: flex; flex-direction: column; gap: 1rem; }
.modal-footer { padding: 1rem 1.5rem; border-top: 1px solid #f1f5f9; display: flex; justify-content: flex-end; gap: 0.75rem; }
.form-alert { padding: 0.8rem 1rem; background: #fef2f2; border: 1px solid rgba(220,38,38,.2); border-radius: 8px; font-size: 0.85rem; color: #dc2626; }
.email-preview { display: flex; align-items: center; gap: 0.6rem; flex-wrap: wrap; padding: 0.7rem 1rem; background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; }
.ep-label { font-size: 0.75rem; font-weight: 700; color: #64748b; text-transform: uppercase; letter-spacing: 0.05em; }
.ep-value { font-size: 0.88rem; font-weight: 600; color: #0088cc; font-family: monospace; }
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 0.9rem; }
.field { display: flex; flex-direction: column; gap: 0.4rem; }
.field-label { font-size: 0.78rem; font-weight: 700; color: #64748b; text-transform: uppercase; letter-spacing: 0.05em; }
.field-input { padding: 0.65rem 0.9rem; border: 1.5px solid #e2e8f0; border-radius: 8px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.9rem; color: #07111f; outline: none; transition: border-color 0.2s, box-shadow 0.2s; }
.field-input:focus { border-color: #0088cc; box-shadow: 0 0 0 3px rgba(0,136,204,.1); }
.field-input.error { border-color: #dc2626; }
.field-err { font-size: 0.75rem; color: #dc2626; }
.role-selector { display: flex; flex-direction: column; gap: 0.6rem; }
.role-option { display: flex; align-items: center; gap: 0.9rem; padding: 0.9rem 1rem; border: 1.5px solid #e2e8f0; border-radius: 10px; cursor: pointer; transition: all 0.18s; }
.role-option:hover, .role-option.selected { border-color: #0088cc; background: #f0f9ff; }
.role-option input { display: none; }
.ro-icon { font-size: 1.4rem; }
.ro-text { display: flex; flex-direction: column; }
.ro-title { font-size: 0.88rem; font-weight: 700; color: #07111f; }
.ro-sub { font-size: 0.75rem; color: #64748b; }
.pw-notice { display: flex; align-items: center; gap: 0.4rem; font-size: 0.8rem; color: #64748b; padding: 0.6rem 0.9rem; background: #fafafa; border-radius: 7px; }
.pw-notice strong { color: #07111f; }
.btn-cancel { padding: 0.6rem 1.2rem; background: #f1f5f9; border: none; border-radius: 8px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 600; color: #64748b; cursor: pointer; }
.btn-cancel:hover { background: #e2e8f0; }
.btn-confirm { padding: 0.6rem 1.4rem; background: #07111f; border: none; border-radius: 8px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 700; color: #fff; cursor: pointer; transition: all 0.18s; display: flex; align-items: center; min-width: 120px; justify-content: center; }
.btn-confirm:hover:not(:disabled) { background: #1a3560; }
.btn-confirm:disabled { opacity: 0.55; cursor: not-allowed; }
.btn-delete-confirm { padding: 0.6rem 1.4rem; background: #dc2626; border: none; border-radius: 8px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 700; color: #fff; cursor: pointer; display: flex; align-items: center; min-width: 120px; justify-content: center; }
.btn-delete-confirm:hover:not(:disabled) { background: #b91c1c; }
.btn-delete-confirm:disabled { opacity: 0.55; cursor: not-allowed; }
.spinner-sm { display: inline-block; width: 16px; height: 16px; border: 2px solid rgba(255,255,255,.3); border-top-color: #fff; border-radius: 50%; animation: spin 0.7s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
.delete-confirm-body { display: flex; flex-direction: column; align-items: center; gap: 0.6rem; padding: 0.5rem 0; text-align: center; }
.dc-warn-icon { font-size: 2.5rem; }
.dc-warn-text { font-size: 0.95rem; color: #334155; line-height: 1.6; }
.dc-warn-sub { font-size: 0.8rem; color: #94a3b8; }
.toast { position: fixed; bottom: 2rem; right: 2rem; z-index: 999; padding: 0.85rem 1.25rem; border-radius: 10px; background: #07111f; color: #fff; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 600; box-shadow: 0 8px 24px rgba(0,0,0,.15); max-width: 400px; }
.toast-slide-enter-active { animation: ti .3s ease; }
.toast-slide-leave-active { animation: ti .25s ease reverse; }
@keyframes ti { from { opacity: 0; transform: translateY(12px); } to { opacity: 1; transform: translateY(0); } }
.modal-fade-enter-active { animation: mIn .22s ease; }
.modal-fade-leave-active { animation: mIn .18s ease reverse; }
@keyframes mIn { from { opacity: 0; transform: scale(.96) translateY(8px); } to { opacity: 1; transform: scale(1) translateY(0); } }
</style>
