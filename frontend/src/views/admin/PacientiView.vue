<template>
  <div class="pacienti-page">
    <div class="page-header">
      <div>
        <h2 class="page-title">Gestionare Pacienți</h2>
        <p class="page-sub">Adaugă, editează și administrează profilurile pacienților</p>
      </div>
      <button class="btn-add" @click="openCreateModal">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="16" height="16"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
        Pacient Nou
      </button>
    </div>

    <div class="search-bar">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16" class="search-icon"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
      <input v-model="searchQuery" class="search-input" placeholder="Caută după nume, doctor..." />
    </div>

    <div v-if="loading" class="loading-state"><div class="spinner-lg"></div><p>Se încarcă...</p></div>
    <div v-else-if="error" class="error-state"><p>⚠️ {{ error }}</p><button class="btn-add" @click="fetchData">Reîncearcă</button></div>

    <div v-else class="table-card">
      <div class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Pacient</th><th>Vârstă / Gen</th><th>Doctor responsabil</th><th>Specializare</th><th class="th-actions">Acțiuni</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="patient in filteredPatients" :key="patient.id">
              <td>
                <div class="patient-name-cell">
                  <div class="p-avatar">{{ initials(patient) }}</div>
                  <p class="p-name">{{ patient.lastName }} {{ patient.firstName }}</p>
                </div>
              </td>
              <td><span class="p-age">{{ patient.age }} ani</span> <span class="p-gender">{{ patient.gender }}</span></td>
              <td class="td-doctor">{{ patient.doctorName || 'Neasignat' }}</td>
              <td><span class="p-spec">{{ patient.doctorSpecialization || '—' }}</span></td>
              <td>
                <div class="action-btns">
                  <button class="btn-edit" @click="openEditModal(patient)" title="Editează">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="14" height="14"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4z"/></svg>
                  </button>
                  <button class="btn-del" @click="confirmDelete(patient)" title="Șterge">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="14" height="14"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/><path d="M10 11v6"/><path d="M14 11v6"/></svg>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="filteredPatients.length === 0" class="empty-state">
          <div class="es-icon">🏥</div>
          <p class="es-title">{{ searchQuery ? 'Niciun rezultat' : 'Nu există pacienți' }}</p>
          <p class="es-sub">{{ searchQuery ? 'Încearcă alt termen' : 'Apasă „Pacient Nou" pentru a adăuga' }}</p>
        </div>
      </div>
    </div>

    <!-- MODAL: Creare/Editare -->
    <Transition name="modal-fade">
      <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
        <div class="modal">
          <div class="modal-header">
            <h3 class="modal-title">{{ editingPatient ? 'Editează Pacient' : 'Pacient Nou' }}</h3>
            <button class="modal-close" @click="closeModal"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="18" height="18"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg></button>
          </div>
          <div class="modal-body">
            <div v-if="formError" class="form-alert">⚠️ {{ formError }}</div>
            <div class="form-row">
              <div class="field"><label class="field-label">Prenume *</label><input v-model="form.firstName" class="field-input" :class="{error: errors.firstName}" placeholder="ex: Gheorghe" @input="errors.firstName=''" /><span v-if="errors.firstName" class="field-err">{{ errors.firstName }}</span></div>
              <div class="field"><label class="field-label">Nume *</label><input v-model="form.lastName" class="field-input" :class="{error: errors.lastName}" placeholder="ex: Constantin" @input="errors.lastName=''" /><span v-if="errors.lastName" class="field-err">{{ errors.lastName }}</span></div>
            </div>
            <div class="form-row">
              <div class="field"><label class="field-label">Vârstă *</label><input v-model.number="form.age" type="number" min="1" max="149" class="field-input" :class="{error: errors.age}" placeholder="ex: 45" @input="errors.age=''" /><span v-if="errors.age" class="field-err">{{ errors.age }}</span></div>
              <div class="field"><label class="field-label">Gen *</label><select v-model="form.gender" class="field-input" :class="{error: errors.gender}" @change="errors.gender=''"><option value="" disabled>Selectați...</option><option>Masculin</option><option>Feminin</option><option>Altul</option></select><span v-if="errors.gender" class="field-err">{{ errors.gender }}</span></div>
            </div>
            <div class="field">
              <label class="field-label">Doctor Responsabil</label>
              <select v-model="form.doctorId" class="field-input">
                <option :value="null">— Neasignat —</option>
                <option v-for="d in medicinaDoctors" :key="d.id" :value="d.id">Dr. {{ d.lastName }} {{ d.firstName }}{{ d.specialization ? ' — ' + d.specialization : '' }}</option>
              </select>
            </div>
            <div class="field"><label class="field-label">Descriere Medicală</label><textarea v-model="form.medicalDescription" class="field-input field-textarea" rows="4" placeholder="Motivul internării, boli cunoscute, riscuri..."></textarea></div>
          </div>
          <div class="modal-footer">
            <button class="btn-cancel" @click="closeModal">Anulează</button>
            <button class="btn-confirm" @click="savePatient" :disabled="saving"><span v-if="!saving">{{ editingPatient ? 'Salvează' : 'Adaugă Pacient' }}</span><span v-else class="spinner-sm"></span></button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- MODAL: Confirmare Ștergere -->
    <Transition name="modal-fade">
      <div v-if="showDeleteModal" class="modal-overlay" @click.self="showDeleteModal = false">
        <div class="modal modal-sm">
          <div class="modal-header"><h3 class="modal-title">Confirmare Ștergere</h3><button class="modal-close" @click="showDeleteModal = false"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="18" height="18"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg></button></div>
          <div class="modal-body">
            <div class="delete-confirm-body">
              <div class="dc-warn-icon">⚠️</div>
              <p class="dc-warn-text">Ștergi profilul lui<br/><strong>{{ patientToDelete?.lastName }} {{ patientToDelete?.firstName }}</strong>?</p>
              <p class="dc-warn-sub">Toate datele asociate vor fi șterse permanent.</p>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn-cancel" @click="showDeleteModal = false">Anulează</button>
            <button class="btn-delete-confirm" @click="deletePatient" :disabled="deleting"><span v-if="!deleting">Șterge</span><span v-else class="spinner-sm"></span></button>
          </div>
        </div>
      </div>
    </Transition>

    <Transition name="toast-slide">
      <div v-if="toast.show" class="toast">{{ toast.message }}</div>
    </Transition>
  </div>
</template>

<script setup>
import { ref, computed, reactive, onMounted } from 'vue'
import { adminApi } from '@/api/admin'

const patients       = ref([])
const medicinaDoctors = ref([])
const loading        = ref(false)
const error          = ref('')
const searchQuery    = ref('')

const filteredPatients = computed(() => {
  const q = searchQuery.value.toLowerCase().trim()
  if (!q) return patients.value
  return patients.value.filter(p =>
    `${p.firstName} ${p.lastName} ${p.doctorName}`.toLowerCase().includes(q)
  )
})

function initials(p) { return ((p.firstName?.[0] ?? '') + (p.lastName?.[0] ?? '')).toUpperCase() }

async function fetchData() {
  loading.value = true; error.value = ''
  try {
    const [pRes, dRes] = await Promise.all([adminApi.getPatients(), adminApi.getDoctors()])
    patients.value = pRes.data
    medicinaDoctors.value = dRes.data.filter(d => d.role === 'doctor_medicina')
  } catch (e) {
    error.value = e.response?.data?.message || 'Eroare la încărcarea datelor.'
  } finally {
    loading.value = false
  }
}

onMounted(fetchData)

const showModal      = ref(false)
const saving         = ref(false)
const editingPatient = ref(null)
const formError      = ref('')
const form           = reactive({ firstName: '', lastName: '', age: '', gender: '', doctorId: null, medicalDescription: '' })
const errors         = reactive({ firstName: '', lastName: '', age: '', gender: '' })

function openCreateModal() {
  editingPatient.value = null
  Object.assign(form, { firstName: '', lastName: '', age: '', gender: '', doctorId: null, medicalDescription: '' })
  Object.assign(errors, { firstName: '', lastName: '', age: '', gender: '' })
  formError.value = ''; showModal.value = true
}
function openEditModal(p) {
  editingPatient.value = p
  Object.assign(form, { firstName: p.firstName, lastName: p.lastName, age: p.age, gender: p.gender, doctorId: p.doctorId, medicalDescription: p.medicalDescription || '' })
  Object.assign(errors, { firstName: '', lastName: '', age: '', gender: '' })
  formError.value = ''; showModal.value = true
}
function closeModal() { showModal.value = false }

function validate() {
  let ok = true
  if (!form.firstName.trim()) { errors.firstName = 'Prenumele este obligatoriu'; ok = false }
  if (!form.lastName.trim())  { errors.lastName  = 'Numele este obligatoriu'; ok = false }
  if (!form.age || form.age < 1 || form.age > 149) { errors.age = 'Vârstă invalidă'; ok = false }
  if (!form.gender) { errors.gender = 'Genul este obligatoriu'; ok = false }
  return ok
}

async function savePatient() {
  if (!validate()) return
  saving.value = true; formError.value = ''
  try {
    const payload = { firstName: form.firstName, lastName: form.lastName, age: form.age, gender: form.gender, medicalDescription: form.medicalDescription, doctorId: form.doctorId }
    if (editingPatient.value) {
      await adminApi.updatePatient(editingPatient.value.id, payload)
      showToast('✅ Profil actualizat')
    } else {
      await adminApi.createPatient(payload)
      showToast('✅ Pacient adăugat')
    }
    await fetchData(); showModal.value = false
  } catch (e) {
    formError.value = e.response?.data?.message || 'Eroare la salvare.'
  } finally {
    saving.value = false
  }
}

const showDeleteModal = ref(false)
const patientToDelete = ref(null)
const deleting        = ref(false)
function confirmDelete(p) { patientToDelete.value = p; showDeleteModal.value = true }
async function deletePatient() {
  deleting.value = true
  try {
    await adminApi.deletePatient(patientToDelete.value.id)
    await fetchData(); showDeleteModal.value = false; showToast('🗑️ Pacient șters')
  } catch (e) {
    showToast('⚠️ ' + (e.response?.data?.message || 'Eroare la ștergere'))
  } finally { deleting.value = false }
}

const toast = reactive({ show: false, message: '' })
function showToast(msg) { toast.message = msg; toast.show = true; setTimeout(() => toast.show = false, 3500) }
</script>

<style scoped>
.pacienti-page { display: flex; flex-direction: column; gap: 1.5rem; }
.page-header { display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem; }
.page-title { font-family: 'DM Serif Display', serif; font-size: 1.5rem; color: #07111f; }
.page-sub { font-size: 0.85rem; color: #64748b; margin-top: 0.2rem; }
.btn-add { display: inline-flex; align-items: center; gap: 0.5rem; padding: 0.65rem 1.25rem; background: #07111f; color: #fff; border: none; border-radius: 10px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 600; cursor: pointer; transition: all 0.2s; white-space: nowrap; }
.btn-add:hover { background: #1a3560; transform: translateY(-1px); }
.search-bar { position: relative; display: flex; align-items: center; }
.search-icon { position: absolute; left: 1rem; color: #94a3b8; pointer-events: none; }
.search-input { width: 100%; padding: 0.7rem 1rem 0.7rem 2.6rem; border: 1.5px solid #e2e8f0; border-radius: 10px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.9rem; outline: none; transition: border-color 0.2s; background: #fff; color: #07111f; }
.search-input:focus { border-color: #0088cc; box-shadow: 0 0 0 3px rgba(0,136,204,.1); }
.loading-state { display: flex; flex-direction: column; align-items: center; gap: 1rem; padding: 3rem; color: #64748b; }
.spinner-lg { width: 36px; height: 36px; border: 3px solid #e2e8f0; border-top-color: #0088cc; border-radius: 50%; animation: spin 0.8s linear infinite; }
.error-state { display: flex; flex-direction: column; align-items: center; gap: 1rem; padding: 2rem; background: #fef2f2; border-radius: 12px; color: #dc2626; }
.table-card { background: #fff; border-radius: 12px; border: 1px solid #e8edf5; overflow: hidden; box-shadow: 0 1px 3px rgba(15,34,64,.06); }
.table-wrap { overflow-x: auto; }
table { width: 100%; border-collapse: collapse; font-size: 0.875rem; }
th { padding: 0.8rem 1rem; text-align: left; font-size: 0.72rem; font-weight: 800; text-transform: uppercase; letter-spacing: 0.07em; color: #64748b; background: #f8fafc; border-bottom: 1.5px solid #e8edf5; }
.th-actions { text-align: center; }
td { padding: 0.9rem 1rem; border-bottom: 1px solid #f1f5f9; vertical-align: middle; }
tr:last-child td { border-bottom: none; }
tr:hover td { background: #fafbfc; }
.patient-name-cell { display: flex; align-items: center; gap: 0.75rem; }
.p-avatar { width: 36px; height: 36px; border-radius: 50%; background: linear-gradient(135deg, #059669, #34d399); display: flex; align-items: center; justify-content: center; font-family: 'DM Serif Display', serif; font-size: 0.85rem; font-weight: 700; color: #fff; flex-shrink: 0; }
.p-name { font-weight: 700; color: #07111f; font-size: 0.88rem; }
.p-age { font-weight: 600; color: #334155; margin-right: 0.4rem; }
.p-gender { font-size: 0.78rem; color: #94a3b8; }
.td-doctor { font-weight: 600; color: #0088cc; font-size: 0.85rem; }
.p-spec { font-size: 0.75rem; color: #64748b; background: #f1f5f9; padding: 0.2rem 0.55rem; border-radius: 100px; font-weight: 600; }
.action-btns { display: flex; align-items: center; justify-content: center; gap: 0.4rem; }
.btn-edit { background: #f0f9ff; border: 1px solid #bae6fd; border-radius: 6px; color: #0088cc; cursor: pointer; padding: 0.4rem; display: flex; align-items: center; transition: all 0.15s; }
.btn-edit:hover { background: #0088cc; color: #fff; }
.btn-del { background: #fef2f2; border: 1px solid #fecaca; border-radius: 6px; color: #dc2626; cursor: pointer; padding: 0.4rem; display: flex; align-items: center; transition: all 0.15s; }
.btn-del:hover { background: #dc2626; color: #fff; }
.empty-state { text-align: center; padding: 3rem; }
.es-icon { font-size: 2.5rem; opacity: 0.4; }
.es-title { font-family: 'DM Serif Display', serif; font-size: 1.1rem; color: #64748b; margin-top: 0.5rem; }
.es-sub { font-size: 0.82rem; color: #94a3b8; margin-top: 0.25rem; }
.modal-overlay { position: fixed; inset: 0; background: rgba(7,17,31,.55); backdrop-filter: blur(4px); z-index: 200; display: flex; align-items: center; justify-content: center; padding: 1rem; }
.modal { background: #fff; border-radius: 16px; box-shadow: 0 24px 48px rgba(7,17,31,.2); width: 100%; max-width: 520px; max-height: 90vh; overflow-y: auto; }
.modal-sm { max-width: 400px; }
.modal-header { padding: 1.4rem 1.5rem; border-bottom: 1px solid #f1f5f9; display: flex; align-items: center; justify-content: space-between; }
.modal-title { font-family: 'DM Serif Display', serif; font-size: 1.15rem; color: #07111f; }
.modal-close { background: none; border: none; cursor: pointer; color: #94a3b8; padding: 0.3rem; border-radius: 6px; display: flex; align-items: center; }
.modal-close:hover { background: #f1f5f9; }
.modal-body { padding: 1.4rem 1.5rem; display: flex; flex-direction: column; gap: 1rem; }
.modal-footer { padding: 1rem 1.5rem; border-top: 1px solid #f1f5f9; display: flex; justify-content: flex-end; gap: 0.75rem; }
.form-alert { padding: 0.8rem 1rem; background: #fef2f2; border: 1px solid rgba(220,38,38,.2); border-radius: 8px; font-size: 0.85rem; color: #dc2626; }
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 0.9rem; }
.field { display: flex; flex-direction: column; gap: 0.4rem; }
.field-label { font-size: 0.78rem; font-weight: 700; color: #64748b; text-transform: uppercase; letter-spacing: 0.05em; }
.field-input { padding: 0.65rem 0.9rem; border: 1.5px solid #e2e8f0; border-radius: 8px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.9rem; color: #07111f; outline: none; transition: border-color 0.2s; background: #fff; }
.field-input:focus { border-color: #0088cc; box-shadow: 0 0 0 3px rgba(0,136,204,.1); }
.field-input.error { border-color: #dc2626; }
.field-textarea { resize: vertical; min-height: 90px; }
.field-err { font-size: 0.75rem; color: #dc2626; }
.btn-cancel { padding: 0.6rem 1.2rem; background: #f1f5f9; border: none; border-radius: 8px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 600; color: #64748b; cursor: pointer; }
.btn-confirm { padding: 0.6rem 1.4rem; background: #07111f; border: none; border-radius: 8px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 700; color: #fff; cursor: pointer; display: flex; align-items: center; min-width: 140px; justify-content: center; }
.btn-confirm:hover:not(:disabled) { background: #1a3560; }
.btn-confirm:disabled { opacity: 0.55; cursor: not-allowed; }
.btn-delete-confirm { padding: 0.6rem 1.4rem; background: #dc2626; border: none; border-radius: 8px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 700; color: #fff; cursor: pointer; display: flex; align-items: center; min-width: 100px; justify-content: center; }
.btn-delete-confirm:hover:not(:disabled) { background: #b91c1c; }
.btn-delete-confirm:disabled { opacity: 0.55; cursor: not-allowed; }
.spinner-sm { display: inline-block; width: 16px; height: 16px; border: 2px solid rgba(255,255,255,.3); border-top-color: #fff; border-radius: 50%; animation: spin 0.7s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
.delete-confirm-body { display: flex; flex-direction: column; align-items: center; gap: 0.6rem; padding: 0.5rem 0; text-align: center; }
.dc-warn-icon { font-size: 2.5rem; }
.dc-warn-text { font-size: 0.95rem; color: #334155; line-height: 1.6; }
.dc-warn-sub { font-size: 0.8rem; color: #94a3b8; }
.toast { position: fixed; bottom: 2rem; right: 2rem; z-index: 999; padding: 0.85rem 1.25rem; border-radius: 10px; background: #07111f; color: #fff; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 600; box-shadow: 0 8px 24px rgba(0,0,0,.15); }
.toast-slide-enter-active { animation: ti .3s ease; }
.toast-slide-leave-active { animation: ti .25s ease reverse; }
@keyframes ti { from { opacity: 0; transform: translateY(12px); } to { opacity: 1; transform: translateY(0); } }
.modal-fade-enter-active { animation: mIn .22s ease; }
.modal-fade-leave-active { animation: mIn .18s ease reverse; }
@keyframes mIn { from { opacity: 0; transform: scale(.96) translateY(8px); } to { opacity: 1; transform: scale(1) translateY(0); } }
</style>
