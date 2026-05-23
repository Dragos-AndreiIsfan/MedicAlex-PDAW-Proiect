<template>
  <div class="page">

    <div class="page-header">
      <div>
        <h2 class="page-title">Pacienții Mei</h2>
        <p class="page-sub">Pacienți adăugați de dvs. — {{ patients.length }} înregistrați</p>
      </div>
      <button class="btn-add" @click="openCreateModal">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="15" height="15"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
        Pacient Nou
      </button>
    </div>

    <!-- Patient cards -->
    <div v-if="patients.length > 0" class="patients-grid">
      <div class="patient-card" v-for="p in patients" :key="p.id" @click="goToPatient(p.id)">
        <div class="pc-top">
          <div class="pc-avatar">{{ initials(p) }}</div>
          <div class="pc-actions" @click.stop>
            <button class="pc-btn-edit" @click="openEditModal(p)" title="Editează">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="14" height="14"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4z"/></svg>
            </button>
            <button class="pc-btn-del" @click="confirmDelete(p)" title="Șterge">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="14" height="14"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/><path d="M10 11v6"/><path d="M14 11v6"/></svg>
            </button>
          </div>
        </div>
        <h3 class="pc-name">{{ p.lastName }} {{ p.firstName }}</h3>
        <div class="pc-meta">
          <span>{{ p.age }} ani</span>
          <span class="meta-dot">·</span>
          <span>{{ p.gender }}</span>
        </div>
        <p class="pc-desc" v-if="p.medicalDescriptionription">{{ p.medicalDescriptionription }}</p>
        <div class="pc-footer">
          <button class="pc-btn-analize" @click.stop="openAnalizaModal(p)">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="13" height="13"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="12" y1="18" x2="12" y2="12"/><line x1="9" y1="15" x2="15" y2="15"/></svg>
            Cerere Analize
          </button>
          <span class="pc-detail-hint">Click pentru detalii →</span>
        </div>
      </div>
    </div>

    <!-- Empty state -->
    <div v-else class="empty-state">
      <div class="es-icon">🏥</div>
      <p class="es-title">Nu există pacienți</p>
      <p class="es-sub">Apasă „Pacient Nou" pentru a adăuga primul pacient</p>
    </div>

    <!-- ═══ MODAL: Creare/Editare Pacient ═══ -->
    <Transition name="mfade">
      <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
        <div class="modal">
          <div class="modal-header">
            <h3 class="modal-title">{{ editing ? 'Editează Pacient' : 'Pacient Nou' }}</h3>
            <button class="modal-close" @click="closeModal">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="18" height="18"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
          </div>
          <div class="modal-body">
            <div class="form-row">
              <div class="field">
                <label class="flbl">Prenume *</label>
                <input v-model="form.firstName" class="finput" :class="{err: errors.firstName}" placeholder="ex: Gheorghe" @input="errors.firstName=''" />
                <span v-if="errors.firstName" class="ferr">{{ errors.firstName }}</span>
              </div>
              <div class="field">
                <label class="flbl">Nume *</label>
                <input v-model="form.lastName" class="finput" :class="{err: errors.lastName}" placeholder="ex: Constantin" @input="errors.lastName=''" />
                <span v-if="errors.lastName" class="ferr">{{ errors.lastName }}</span>
              </div>
            </div>
            <div class="form-row">
              <div class="field">
                <label class="flbl">Vârstă *</label>
                <input v-model.number="form.age" type="number" min="1" max="149" class="finput" :class="{err: errors.age}" placeholder="ex: 45" @input="errors.age=''" />
                <span v-if="errors.age" class="ferr">{{ errors.age }}</span>
              </div>
              <div class="field">
                <label class="flbl">Gen *</label>
                <select v-model="form.gender" class="finput" :class="{err: errors.gender}" @change="errors.gender=''">
                  <option value="" disabled>Selectați...</option>
                  <option>Masculin</option>
                  <option>Feminin</option>
                  <option>Altul</option>
                </select>
                <span v-if="errors.gender" class="ferr">{{ errors.gender }}</span>
              </div>
            </div>
            <div class="field">
              <label class="flbl">Descriere Medicală</label>
              <textarea v-model="form.medicalDescription" class="finput ftextarea" rows="4" placeholder="Motivul internării, boli cunoscute, riscuri de sănătate..."></textarea>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn-cancel" @click="closeModal">Anulează</button>
            <button class="btn-save" @click="savePatient" :disabled="saving">
              <span v-if="!saving">{{ editing ? 'Salvează' : 'Adaugă Pacient' }}</span>
              <span v-else class="spin"></span>
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- ═══ MODAL: Cerere Analize ═══ -->
    <Transition name="mfade">
      <div v-if="showAnalizaModal" class="modal-overlay" @click.self="showAnalizaModal = false">
        <div class="modal modal-sm">
          <div class="modal-header">
            <h3 class="modal-title">Cerere Analize</h3>
            <button class="modal-close" @click="showAnalizaModal = false">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="18" height="18"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
          </div>
          <div class="modal-body">
            <div class="analiza-patient-badge">
              <div class="apb-avatar">{{ analizaPatient ? initials(analizaPatient) : '' }}</div>
              <div>
                <p class="apb-name">{{ analizaPatient?.lastName }} {{ analizaPatient?.firstName }}</p>
                <p class="apb-meta">{{ analizaPatient?.age }} ani · {{ analizaPatient?.gender }}</p>
              </div>
            </div>

            <div class="field">
              <label class="flbl">Tip Analiză *</label>
              <div class="analiza-chips">
                <button
                  v-for="tip in tipuriAnalize" :key="tip"
                  class="chip" :class="{ 'chip-active': analizaForm.type === tip }"
                  @click="analizaForm.type = tip; analizaErr = ''"
                >{{ tip }}</button>
              </div>
              <input
                v-model="analizaForm.customType"
                class="finput mt-1"
                placeholder="Sau scrieți un alt tip de analiză..."
                @input="analizaForm.type = ''; analizaErr = ''"
              />
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

    <!-- ═══ MODAL: Confirmare Ștergere ═══ -->
    <Transition name="mfade">
      <div v-if="showDeleteModal" class="modal-overlay" @click.self="showDeleteModal = false">
        <div class="modal modal-sm">
          <div class="modal-header">
            <h3 class="modal-title">Confirmare Ștergere</h3>
            <button class="modal-close" @click="showDeleteModal = false">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="18" height="18"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
          </div>
          <div class="modal-body">
            <div class="del-body">
              <span class="del-icon">⚠️</span>
              <p class="del-text">Ești sigur că vrei să ștergi profilul lui<br/><strong>{{ patientToDelete?.lastName }} {{ patientToDelete?.firstName }}</strong>?</p>
              <p class="del-sub">Această acțiune este ireversibilă.</p>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn-cancel" @click="showDeleteModal = false">Anulează</button>
            <button class="btn-delete" @click="deletePatient">Șterge Profilul</button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- Toast -->
    <Transition name="toast-slide">
      <div v-if="toast.show" class="toast" :class="'toast-' + toast.type">
        <span>{{ toast.icon }}</span> {{ toast.message }}
      </div>
    </Transition>

  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { medicinaApi } from '@/api/medicina'

const router = useRouter()

const patients     = ref([])
const loadingData  = ref(false)

async function fetchPatients() {
  loadingData.value = true
  try {
    const { data } = await medicinaApi.getMyPatients()
    patients.value = data
  } catch (e) {
    console.error('Eroare la încărcarea pacienților:', e)
  } finally {
    loadingData.value = false
  }
}

onMounted(fetchPatients)

function initials(p) { return ((p.firstName?.[0] ?? '') + (p.lastName?.[0] ?? '')).toUpperCase() }
function goToPatient(id) { router.push(`/medicina/pacient/${id}`) }

// ── Toast ─────────────────────────────────────────────────
const toast = reactive({ show: false, message: '', type: 'success', icon: '✅' })
function showToast(message, type = 'success') {
  toast.message = message
  toast.type    = type
  toast.icon    = type === 'success' ? '✅' : '⚠️'
  toast.show    = true
  setTimeout(() => toast.show = false, 3000)
}

// ── Create/Edit modal ─────────────────────────────────────
const showModal = ref(false)
const saving    = ref(false)
const editing   = ref(null)
const form      = reactive({ firstName: '', lastName: '', age: '', gender: '', medicalDescription: '' })
const errors    = reactive({ firstName: '', lastName: '', age: '', gender: '' })

function openCreateModal() {
  editing.value = null
  Object.assign(form, { firstName: '', lastName: '', age: '', gender: '', medicalDescription: '' })
  Object.assign(errors, { firstName: '', lastName: '', age: '', gender: '' })
  showModal.value = true
}
function openEditModal(p) {
  editing.value = p
  Object.assign(form, { firstName: p.firstName, lastName: p.lastName, age: p.age, gender: p.gender, medicalDescription: p.medicalDescriptionription })
  Object.assign(errors, { firstName: '', lastName: '', age: '', gender: '' })
  showModal.value = true
}
function closeModal() { showModal.value = false }

function validate() {
  let ok = true
  if (!form.firstName.trim()) { errors.firstName = 'Prenumele este obligatoriu'; ok = false }
  if (!form.lastName.trim())  { errors.lastName  = 'Numele este obligatoriu';    ok = false }
  if (!form.age || form.age < 1 || form.age > 149) { errors.age = 'Vârstă invalidă'; ok = false }
  if (!form.gender) { errors.gender = 'Genul este obligatoriu'; ok = false }
  return ok
}

async function savePatient() {
  if (!validate()) return
  saving.value = true
  try {
    const payload = { firstName: form.firstName, lastName: form.lastName, age: form.age, gender: form.gender, medicalDescription: form.medicalDescription }
    if (editing.value) {
      await medicinaApi.updatePatient(editing.value.id, payload)
      showToast('Profil actualizat cu succes')
    } else {
      await medicinaApi.createPatient(payload)
      showToast('Pacient adăugat cu succes')
    }
    await fetchPatients()
    saving.value = false
    showModal.value = false
  } catch (e) {
    showToast('⚠️ ' + (e.response?.data?.message || 'Eroare la salvare'))
    saving.value = false
  }
}

// ── Delete ─────────────────────────────────────────────────
const showDeleteModal = ref(false)
const patientToDelete = ref(null)
function confirmDelete(p) { patientToDelete.value = p; showDeleteModal.value = true }
async function deletePatient() {
  try {
    await medicinaApi.deletePatient(patientToDelete.value.id)
    await fetchPatients()
    showDeleteModal.value = false
    showToast('Profil șters')
  } catch (e) {
    showToast('⚠️ ' + (e.response?.data?.message || 'Eroare la ștergere'))
  }
}

// ── Cerere Analize ─────────────────────────────────────────
const showAnalizaModal  = ref(false)
const sendingAnaliza    = ref(false)
const analizaPatient    = ref(null)
const analizaErr        = ref('')
const analizaForm       = reactive({ type: '', customType: '' })
const tipuriAnalize     = ['Sânge', 'Colesterol', 'Glucoză', 'Urină', 'Coagulare', 'Ecografie', 'Inimă', 'Hepatită']

function openAnalizaModal(p) {
  analizaPatient.value = p
  analizaForm.type       = ''
  analizaForm.customType = ''
  analizaErr.value       = ''
  showAnalizaModal.value = true
}

async function submitAnaliza() {
  const finalType = analizaForm.type || analizaForm.customType.trim()
  if (!finalType) { analizaErr.value = 'Selectați sau scrieți tipul de analiză'; return }
  sendingAnaliza.value = true
  try {
    await medicinaApi.createAnalysisRequest(analizaPatient.value.id, finalType)
    sendingAnaliza.value   = false
    showAnalizaModal.value = false
    showToast(`Cerere „${finalType}" trimisă cu succes`)
  } catch (e) {
    analizaErr.value = e.response?.data?.message || 'Eroare la trimiterea cererii'
    sendingAnaliza.value = false
  }
}
</script>

<style scoped>
.page { display: flex; flex-direction: column; gap: 1.5rem; }
.page-header { display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem; }
.page-title { font-family: 'DM Serif Display', serif; font-size: 1.5rem; color: #071a2e; }
.page-sub { font-size: 0.85rem; color: #64748b; margin-top: 0.2rem; }
.btn-add {
  display: inline-flex; align-items: center; gap: 0.5rem;
  padding: 0.65rem 1.25rem; background: #071a2e; color: #fff;
  border: none; border-radius: 10px;
  font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 600;
  cursor: pointer; transition: all 0.2s; white-space: nowrap;
}
.btn-add:hover { background: #0c2d4a; transform: translateY(-1px); box-shadow: 0 4px 12px rgba(7,26,46,.25); }

.patients-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 1rem; }

.patient-card {
  background: #fff; border: 1px solid #e8edf5; border-radius: 14px;
  padding: 1.25rem; cursor: pointer; transition: all 0.2s;
  display: flex; flex-direction: column; gap: 0.5rem;
  box-shadow: 0 1px 3px rgba(15,34,64,.05);
}
.patient-card:hover { box-shadow: 0 6px 20px rgba(15,34,64,.1); border-color: #0ea5e9; transform: translateY(-2px); }

.pc-top { display: flex; align-items: flex-start; justify-content: space-between; }
.pc-avatar {
  width: 46px; height: 46px; border-radius: 50%;
  background: linear-gradient(135deg, #0ea5e9, #06b6d4);
  display: flex; align-items: center; justify-content: center;
  font-family: 'DM Serif Display', serif; font-size: 1.1rem; font-weight: 700; color: #fff;
}
.pc-actions { display: flex; gap: 0.4rem; }
.pc-btn-edit, .pc-btn-del {
  background: none; border: none; cursor: pointer; padding: 0.35rem;
  border-radius: 6px; display: flex; align-items: center; transition: all 0.15s;
}
.pc-btn-edit { color: #94a3b8; }
.pc-btn-edit:hover { background: #f0f9ff; color: #0ea5e9; }
.pc-btn-del { color: #94a3b8; }
.pc-btn-del:hover { background: #fef2f2; color: #dc2626; }

.pc-name { font-family: 'DM Serif Display', serif; font-size: 1.05rem; color: #071a2e; }
.pc-meta { font-size: 0.82rem; color: #64748b; display: flex; gap: 0.4rem; align-items: center; }
.meta-dot { color: #cbd5e1; }
.pc-desc {
  font-size: 0.82rem; color: #64748b; line-height: 1.5;
  display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden;
}
.pc-footer { display: flex; align-items: center; justify-content: space-between; margin-top: 0.25rem; }
.pc-btn-analize {
  display: inline-flex; align-items: center; gap: 0.4rem;
  padding: 0.4rem 0.85rem; background: rgba(14,165,233,.1); color: #0ea5e9;
  border: 1px solid rgba(14,165,233,.2); border-radius: 8px;
  font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.78rem; font-weight: 600;
  cursor: pointer; transition: all 0.18s;
}
.pc-btn-analize:hover { background: #0ea5e9; color: #fff; }
.pc-detail-hint { font-size: 0.74rem; color: #cbd5e1; }

.empty-state { text-align: center; padding: 4rem 2rem; display: flex; flex-direction: column; align-items: center; gap: 0.6rem; }
.es-icon { font-size: 3rem; opacity: 0.3; }
.es-title { font-family: 'DM Serif Display', serif; font-size: 1.15rem; color: #64748b; }
.es-sub { font-size: 0.85rem; color: #94a3b8; }

/* Analiza chips */
.analiza-chips { display: flex; flex-wrap: wrap; gap: 0.5rem; }
.chip {
  padding: 0.35rem 0.85rem; border-radius: 100px;
  border: 1.5px solid #e2e8f0; background: #f8fafc;
  font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.82rem; font-weight: 600;
  color: #64748b; cursor: pointer; transition: all 0.18s;
}
.chip:hover { border-color: #0ea5e9; color: #0ea5e9; background: #f0f9ff; }
.chip-active { border-color: #0ea5e9; color: #0ea5e9; background: rgba(14,165,233,.1); }
.mt-1 { margin-top: 0.5rem; }

/* Analiza patient badge */
.analiza-patient-badge {
  display: flex; align-items: center; gap: 0.75rem;
  padding: 0.75rem 1rem; background: #f0f9ff;
  border: 1px solid #bae6fd; border-radius: 10px;
}
.apb-avatar {
  width: 38px; height: 38px; border-radius: 50%;
  background: linear-gradient(135deg, #0ea5e9, #06b6d4);
  display: flex; align-items: center; justify-content: center;
  font-family: 'DM Serif Display', serif; font-weight: 700; font-size: 0.9rem; color: #fff;
  flex-shrink: 0;
}
.apb-name { font-size: 0.9rem; font-weight: 700; color: #071a2e; }
.apb-meta { font-size: 0.75rem; color: #64748b; margin-top: 0.1rem; }

/* Modal shared styles */
.modal-overlay {
  position: fixed; inset: 0; background: rgba(7,26,46,.55); backdrop-filter: blur(4px);
  z-index: 200; display: flex; align-items: center; justify-content: center; padding: 1rem;
}
.modal {
  background: #fff; border-radius: 16px; box-shadow: 0 24px 48px rgba(7,26,46,.2);
  width: 100%; max-width: 500px; max-height: 90vh; overflow-y: auto;
}
.modal-sm { max-width: 420px; }
.modal-header { padding: 1.4rem 1.5rem; border-bottom: 1px solid #f1f5f9; display: flex; align-items: center; justify-content: space-between; }
.modal-title { font-family: 'DM Serif Display', serif; font-size: 1.15rem; color: #071a2e; }
.modal-close { background: none; border: none; cursor: pointer; color: #94a3b8; padding: 0.3rem; border-radius: 6px; display: flex; align-items: center; transition: all 0.15s; }
.modal-close:hover { background: #f1f5f9; color: #334155; }
.modal-body { padding: 1.4rem 1.5rem; display: flex; flex-direction: column; gap: 1rem; }
.modal-footer { padding: 1rem 1.5rem; border-top: 1px solid #f1f5f9; display: flex; justify-content: flex-end; gap: 0.75rem; }

.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 0.9rem; }
.field { display: flex; flex-direction: column; gap: 0.4rem; }
.flbl { font-size: 0.78rem; font-weight: 700; color: #64748b; text-transform: uppercase; letter-spacing: 0.05em; }
.finput {
  padding: 0.65rem 0.9rem; border: 1.5px solid #e2e8f0; border-radius: 8px;
  font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.9rem; color: #071a2e;
  outline: none; transition: border-color 0.2s, box-shadow 0.2s; background: #fff; width: 100%;
}
.finput:focus { border-color: #0ea5e9; box-shadow: 0 0 0 3px rgba(14,165,233,.1); }
.finput.err { border-color: #dc2626; }
.ftextarea { resize: vertical; min-height: 90px; }
.ferr { font-size: 0.75rem; color: #dc2626; }

.btn-cancel { padding: 0.6rem 1.2rem; background: #f1f5f9; border: none; border-radius: 8px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 600; color: #64748b; cursor: pointer; transition: background 0.18s; }
.btn-cancel:hover { background: #e2e8f0; }
.btn-save { padding: 0.6rem 1.4rem; background: #0ea5e9; border: none; border-radius: 8px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 700; color: #fff; cursor: pointer; transition: all 0.18s; display: flex; align-items: center; min-width: 130px; justify-content: center; }
.btn-save:hover:not(:disabled) { background: #0284c7; }
.btn-save:disabled { opacity: 0.55; cursor: not-allowed; }
.btn-delete { padding: 0.6rem 1.4rem; background: #dc2626; border: none; border-radius: 8px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 700; color: #fff; cursor: pointer; }
.btn-delete:hover { background: #b91c1c; }

.del-body { display: flex; flex-direction: column; align-items: center; gap: 0.6rem; text-align: center; padding: 0.5rem 0; }
.del-icon { font-size: 2.5rem; }
.del-text { font-size: 0.95rem; color: #334155; line-height: 1.6; }
.del-sub  { font-size: 0.8rem; color: #94a3b8; }

.spin { display: inline-block; width: 16px; height: 16px; border: 2px solid rgba(255,255,255,.3); border-top-color: #fff; border-radius: 50%; animation: sp 0.7s linear infinite; }
@keyframes sp { to { transform: rotate(360deg); } }

/* Toast */
.toast {
  position: fixed; bottom: 2rem; right: 2rem; z-index: 999;
  padding: 0.85rem 1.25rem; border-radius: 10px;
  font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 600;
  display: flex; align-items: center; gap: 0.6rem;
  box-shadow: 0 8px 24px rgba(0,0,0,.15);
}
.toast-success { background: #071a2e; color: #fff; }
.toast-error   { background: #fef2f2; color: #dc2626; border: 1px solid #fecaca; }
.toast-slide-enter-active { animation: toastIn .3s ease; }
.toast-slide-leave-active { animation: toastIn .25s ease reverse; }
@keyframes toastIn { from { opacity: 0; transform: translateY(12px); } to { opacity: 1; transform: translateY(0); } }

/* Modal transition */
.mfade-enter-active { animation: mIn .22s ease; }
.mfade-leave-active { animation: mIn .18s ease reverse; }
@keyframes mIn { from { opacity: 0; transform: scale(.96) translateY(8px); } to { opacity: 1; transform: scale(1) translateY(0); } }
</style>
