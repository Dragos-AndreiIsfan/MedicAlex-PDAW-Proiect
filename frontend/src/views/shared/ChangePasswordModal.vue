<template>
  <Transition name="mfade">
    <div v-if="show" class="modal-overlay" @click.self="$emit('close')">
      <div class="modal">
        <div class="modal-header">
          <h3 class="modal-title">🔒 Schimbă Parola</h3>
          <button class="modal-close" @click="$emit('close')">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="18" height="18">
              <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>

        <div class="modal-body">
          <div v-if="successMsg" class="alert-success">✅ {{ successMsg }}</div>
          <div v-if="errorMsg"   class="alert-error">⚠️ {{ errorMsg }}</div>

          <div class="field">
            <label class="flbl">Parola curentă *</label>
            <input v-model="form.current" type="password" class="finput" placeholder="Parola actuală" @input="errorMsg = ''" />
          </div>
          <div class="field">
            <label class="flbl">Parola nouă * <span class="hint">(minim 6 caractere)</span></label>
            <input v-model="form.new" type="password" class="finput" placeholder="Parola nouă" @input="errorMsg = ''" />
          </div>
          <div class="field">
            <label class="flbl">Confirmă parola nouă *</label>
            <input v-model="form.confirm" type="password" class="finput" :class="{ err: mismatch }" placeholder="Repetă parola nouă" @input="errorMsg = ''" />
            <span v-if="mismatch" class="ferr">Parolele nu coincid</span>
          </div>
        </div>

        <div class="modal-footer">
          <button class="btn-cancel" @click="$emit('close')">Anulează</button>
          <button class="btn-save" @click="submit" :disabled="saving || !!successMsg">
            <span v-if="!saving">Schimbă Parola</span>
            <span v-else class="spin"></span>
          </button>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { authApi } from '@/api/auth'

const props = defineProps({ show: Boolean })
const emit  = defineEmits(['close'])

const form       = ref({ current: '', new: '', confirm: '' })
const saving     = ref(false)
const errorMsg   = ref('')
const successMsg = ref('')

const mismatch = computed(() =>
  form.value.confirm.length > 0 && form.value.new !== form.value.confirm
)

// Reset form când se deschide modalul
watch(() => props.show, (val) => {
  if (val) {
    form.value   = { current: '', new: '', confirm: '' }
    errorMsg.value   = ''
    successMsg.value = ''
  }
})

async function submit() {
  errorMsg.value   = ''
  successMsg.value = ''

  if (!form.value.current) { errorMsg.value = 'Introduceți parola curentă.'; return }
  if (!form.value.new || form.value.new.length < 6) { errorMsg.value = 'Parola nouă trebuie să aibă minim 6 caractere.'; return }
  if (form.value.new !== form.value.confirm) { errorMsg.value = 'Parolele nu coincid.'; return }

  saving.value = true
  try {
    await authApi.changePassword(form.value.current, form.value.new, form.value.confirm)
    successMsg.value = 'Parola a fost schimbată cu succes!'
    setTimeout(() => emit('close'), 1800)
  } catch (e) {
    errorMsg.value = e.response?.data?.message || 'Eroare la schimbarea parolei.'
  } finally {
    saving.value = false
  }
}
</script>

<style scoped>
.modal-overlay {
  position: fixed; inset: 0; background: rgba(0,0,0,.5); backdrop-filter: blur(4px);
  z-index: 500; display: flex; align-items: center; justify-content: center; padding: 1rem;
}
.modal {
  background: #fff; border-radius: 16px; box-shadow: 0 24px 48px rgba(0,0,0,.2);
  width: 100%; max-width: 420px;
}
.modal-header {
  padding: 1.4rem 1.5rem; border-bottom: 1px solid #f1f5f9;
  display: flex; align-items: center; justify-content: space-between;
}
.modal-title { font-family: 'DM Serif Display', serif; font-size: 1.1rem; color: #0f172a; }
.modal-close { background: none; border: none; cursor: pointer; color: #94a3b8; padding: 0.3rem; border-radius: 6px; display: flex; align-items: center; }
.modal-close:hover { background: #f1f5f9; }
.modal-body { padding: 1.4rem 1.5rem; display: flex; flex-direction: column; gap: 1rem; }
.modal-footer { padding: 1rem 1.5rem; border-top: 1px solid #f1f5f9; display: flex; justify-content: flex-end; gap: 0.75rem; }

.alert-success { padding: 0.75rem 1rem; background: #f0fdf4; border: 1px solid #bbf7d0; border-radius: 8px; font-size: 0.875rem; color: #15803d; }
.alert-error   { padding: 0.75rem 1rem; background: #fef2f2; border: 1px solid #fecaca; border-radius: 8px; font-size: 0.875rem; color: #dc2626; }

.field { display: flex; flex-direction: column; gap: 0.4rem; }
.flbl { font-size: 0.78rem; font-weight: 700; color: #64748b; text-transform: uppercase; letter-spacing: 0.05em; }
.hint { font-size: 0.7rem; font-weight: 400; color: #94a3b8; text-transform: none; }
.finput {
  padding: 0.65rem 0.9rem; border: 1.5px solid #e2e8f0; border-radius: 8px;
  font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.9rem; color: #0f172a;
  outline: none; transition: border-color 0.2s;
}
.finput:focus { border-color: #0ea5e9; box-shadow: 0 0 0 3px rgba(14,165,233,.1); }
.finput.err   { border-color: #dc2626; }
.ferr { font-size: 0.75rem; color: #dc2626; }

.btn-cancel { padding: 0.6rem 1.2rem; background: #f1f5f9; border: none; border-radius: 8px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 600; color: #64748b; cursor: pointer; }
.btn-cancel:hover { background: #e2e8f0; }
.btn-save { padding: 0.6rem 1.4rem; background: #0ea5e9; border: none; border-radius: 8px; font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.88rem; font-weight: 700; color: #fff; cursor: pointer; display: flex; align-items: center; min-width: 140px; justify-content: center; transition: background 0.18s; }
.btn-save:hover:not(:disabled) { background: #0284c7; }
.btn-save:disabled { opacity: 0.55; cursor: not-allowed; }
.spin { display: inline-block; width: 16px; height: 16px; border: 2px solid rgba(255,255,255,.3); border-top-color: #fff; border-radius: 50%; animation: sp 0.7s linear infinite; }
@keyframes sp { to { transform: rotate(360deg); } }

.mfade-enter-active { animation: mIn .2s ease; }
.mfade-leave-active { animation: mIn .15s ease reverse; }
@keyframes mIn { from { opacity: 0; transform: scale(.96) translateY(8px); } to { opacity: 1; transform: scale(1) translateY(0); } }
</style>
