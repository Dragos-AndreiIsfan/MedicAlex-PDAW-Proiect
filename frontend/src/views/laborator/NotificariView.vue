<template>
  <div class="page">

    <div class="page-header">
      <div>
        <h2 class="page-title">Notificări</h2>
        <p class="page-sub">{{ unread.length }} necitite · {{ notifications.length }} total</p>
      </div>
      <button v-if="unread.length > 0" class="btn-mark-all" @click="markAllRead">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="14" height="14"><polyline points="20 6 9 17 4 12"/></svg>
        Marchează toate ca citite
      </button>
    </div>

    <!-- Filter -->
    <div class="filter-tabs">
      <button class="tab" :class="{ active: filter === 'all' }"    @click="filter = 'all'">Toate <span class="tc">{{ notifications.length }}</span></button>
      <button class="tab" :class="{ active: filter === 'unread' }" @click="filter = 'unread'">Necitite <span class="tc">{{ unread.length }}</span></button>
    </div>

    <!-- List -->
    <div v-if="filtered.length > 0" class="notif-list">
      <TransitionGroup name="notif-fade">
        <div
          v-for="n in filtered" :key="n.id"
          class="notif-card"
          :class="{ 'notif-unread': !n.isRead }"
        >
          <div class="nc-indicator" :class="{ 'ind-unread': !n.isRead }"></div>
          <div class="nc-icon">{{ n.isRead ? '📋' : '🔔' }}</div>
          <div class="nc-body">
            <p class="nc-msg">{{ n.message }}</p>
            <div class="nc-meta">
              <span class="nc-time">{{ formatTime(n.createdAt) }}</span>
              <span v-if="n.relatedRequestId" class="nc-req-link" @click="goToCereri">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="11" height="11"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
                Vezi cererea
              </span>
            </div>
          </div>
          <div class="nc-actions">
            <button v-if="!n.isRead" class="btn-read" @click="markRead(n.id)" title="Marchează ca citit">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="14" height="14"><polyline points="20 6 9 17 4 12"/></svg>
            </button>
            <span v-else class="nc-read-tag">Citit</span>
          </div>
        </div>
      </TransitionGroup>
    </div>

    <div v-else class="empty-state">
      <div class="es-icon">🔔</div>
      <p class="es-title">{{ filter === 'unread' ? 'Nicio notificare necitită' : 'Nicio notificare' }}</p>
      <p class="es-sub">{{ filter === 'unread' ? 'Toate notificările au fost citite' : 'Vei fi notificat când apar cereri noi de analize' }}</p>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { laboratorApi } from '@/api/laborator'

const router        = useRouter()
const filter        = ref('all')
const notifications = ref([])
const loading       = ref(false)

function formatTime(d) {
  if (!d) return ''
  const date = new Date(d)
  const now  = new Date()
  const diff = Math.floor((now - date) / 1000)
  if (diff < 60)   return 'acum câteva secunde'
  if (diff < 3600) return `acum ${Math.floor(diff / 60)} min`
  if (diff < 86400) return `acum ${Math.floor(diff / 3600)} ore`
  return date.toLocaleDateString('ro-RO', { day: 'numeric', month: 'short' })
}

async function fetchNotifications() {
  loading.value = true
  try {
    const { data } = await laboratorApi.getNotifications()
    notifications.value = data
  } catch (e) {
    console.error('Eroare:', e)
  } finally {
    loading.value = false
  }
}

onMounted(fetchNotifications)

const unread   = computed(() => notifications.value.filter(n => !n.isRead))
const filtered = computed(() => filter.value === 'unread' ? unread.value : notifications.value)

async function markRead(id) {
  try {
    await laboratorApi.markRead(id)
    const n = notifications.value.find(x => x.id === id)
    if (n) n.isRead = true
  } catch (e) { console.error(e) }
}

async function markAllRead() {
  try {
    await laboratorApi.markAllRead()
    notifications.value.forEach(n => n.isRead = true)
  } catch (e) { console.error(e) }
}

function goToCereri() { router.push('/laborator/cereri') }
</script>

<style scoped>
.page { display: flex; flex-direction: column; gap: 1.5rem; }
.page-header { display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem; }
.page-title { font-family: 'DM Serif Display', serif; font-size: 1.5rem; color: #0f1f14; }
.page-sub { font-size: 0.85rem; color: #64748b; margin-top: 0.2rem; }
.btn-mark-all {
  display: inline-flex; align-items: center; gap: 0.4rem;
  padding: 0.55rem 1rem; background: #f0fdf4;
  border: 1.5px solid #bbf7d0; border-radius: 8px;
  font-family: 'Plus Jakarta Sans', sans-serif; font-size: 0.82rem; font-weight: 600; color: #16a34a;
  cursor: pointer; transition: all 0.18s; white-space: nowrap;
}
.btn-mark-all:hover { background: #16a34a; color: #fff; border-color: #16a34a; }

.filter-tabs { display: flex; gap: 0.5rem; }
.tab {
  display: flex; align-items: center; gap: 0.5rem;
  padding: 0.5rem 1rem; background: #fff; border: 1.5px solid #e8edf5;
  border-radius: 8px; font-family: 'Plus Jakarta Sans', sans-serif;
  font-size: 0.85rem; font-weight: 500; color: #64748b; cursor: pointer; transition: all 0.18s;
}
.tab:hover { border-color: #16a34a; color: #16a34a; }
.tab.active { background: #0f1f14; border-color: #0f1f14; color: #fff; }
.tc { padding: 0.1rem 0.45rem; border-radius: 100px; font-size: 0.72rem; font-weight: 700; }
.tab.active .tc { background: rgba(255,255,255,0.2); }
.tab:not(.active) .tc { background: #f1f5f9; color: #64748b; }

.notif-list { display: flex; flex-direction: column; gap: 0.6rem; }

.notif-card {
  background: #fff; border: 1px solid #e8edf5; border-radius: 12px;
  padding: 1rem 1.25rem 1rem 1rem;
  display: flex; align-items: flex-start; gap: 0.9rem;
  box-shadow: 0 1px 3px rgba(15,34,64,.04);
  transition: all 0.18s; position: relative; overflow: hidden;
}
.notif-card:hover { box-shadow: 0 4px 12px rgba(15,34,64,.08); }
.notif-unread { background: #f0fdf4; border-color: #bbf7d0; }

.nc-indicator {
  position: absolute; left: 0; top: 0; bottom: 0;
  width: 3px; background: #e2e8f0; border-radius: 3px 0 0 3px;
}
.ind-unread { background: #22c55e; }

.nc-icon { font-size: 1.2rem; flex-shrink: 0; margin-top: 0.1rem; }
.nc-body { flex: 1; min-width: 0; }
.nc-msg { font-size: 0.88rem; color: #334155; line-height: 1.5; }
.notif-unread .nc-msg { font-weight: 600; color: #0f1f14; }
.nc-meta { display: flex; align-items: center; gap: 1rem; margin-top: 0.35rem; }
.nc-time { font-size: 0.74rem; color: #94a3b8; }
.nc-req-link {
  display: inline-flex; align-items: center; gap: 0.3rem;
  font-size: 0.74rem; font-weight: 600; color: #16a34a;
  cursor: pointer; text-decoration: underline;
}
.nc-req-link:hover { color: #15803d; }

.nc-actions { flex-shrink: 0; display: flex; align-items: center; }
.btn-read {
  padding: 0.4rem; background: rgba(34,197,94,.1);
  border: 1px solid rgba(34,197,94,.2); border-radius: 7px;
  color: #16a34a; cursor: pointer; display: flex; align-items: center;
  transition: all 0.15s;
}
.btn-read:hover { background: #16a34a; color: #fff; }
.nc-read-tag { font-size: 0.72rem; color: #94a3b8; font-weight: 600; }

.notif-fade-enter-active { transition: all 0.25s ease; }
.notif-fade-leave-active { transition: all 0.2s ease; }
.notif-fade-enter-from, .notif-fade-leave-to { opacity: 0; transform: translateX(-8px); }

.empty-state { text-align: center; padding: 4rem; display: flex; flex-direction: column; align-items: center; gap: 0.6rem; }
.es-icon { font-size: 3rem; opacity: 0.3; }
.es-title { font-family: 'DM Serif Display', serif; font-size: 1.15rem; color: #64748b; }
.es-sub { font-size: 0.85rem; color: #94a3b8; }
</style>
