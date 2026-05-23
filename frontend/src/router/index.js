import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '@/views/LoginView.vue'

const routes = [
  { path: '/', redirect: '/login' },
  { path: '/login', name: 'Login', component: LoginView, meta: { guest: true } },

  // ──── Admin ────────────────────────────────────────────
  {
    path: '/admin',
    component: () => import('@/views/admin/AdminLayout.vue'),
    meta: { requiresAuth: true, role: 'admin' },
    children: [
      { path: '', name: 'AdminDashboard', component: () => import('@/views/admin/DashboardView.vue') },
      { path: 'doctori',  name: 'AdminDoctori',  component: () => import('@/views/admin/DoctoriView.vue') },
      { path: 'pacienti', name: 'AdminPacienti', component: () => import('@/views/admin/PacientiView.vue') },
    ]
  },

  // ──── Doctor Medicina ──────────────────────────────────
  {
    path: '/medicina',
    component: () => import('@/views/medicina/MedicinaLayout.vue'),
    meta: { requiresAuth: true, role: 'doctor_medicina' },
    children: [
      { path: '', name: 'MedicinaDashboard', component: () => import('@/views/medicina/MedicinaDashboard.vue') },
      { path: 'pacientii-mei', name: 'PacientiiMei', component: () => import('@/views/medicina/PacientiiMeiView.vue') },
      { path: 'pacienti',      name: 'PacientiAlti', component: () => import('@/views/medicina/PacientiView.vue') },
      { path: 'pacient/:id',   name: 'PacientDetail', component: () => import('@/views/medicina/PacientDetailView.vue') },
      { path: 'doctori-medicina',  name: 'MedDoctoriMed', component: () => import('@/views/shared/DoctoriListView.vue'), props: { type: 'medicina' } },
      { path: 'doctori-laborator', name: 'MedDoctoriLab', component: () => import('@/views/shared/DoctoriListView.vue'), props: { type: 'laborator' } },
    ]
  },

  // ──── Doctor Laborator ─────────────────────────────────
  {
    path: '/laborator',
    component: () => import('@/views/laborator/LaboratorLayout.vue'),
    meta: { requiresAuth: true, role: 'doctor_laborator' },
    children: [
      { path: '', name: 'LaboratorDashboard', component: () => import('@/views/laborator/LaboratorDashboard.vue') },
      { path: 'notificari',        name: 'Notificari',       component: () => import('@/views/laborator/NotificariView.vue') },
      { path: 'cereri',            name: 'CereriAnalize',    component: () => import('@/views/laborator/CereriView.vue') },
      { path: 'pacienti',          name: 'LabPacienti',      component: () => import('@/views/laborator/LabPacientiView.vue') },
      { path: 'pacient/:id',       name: 'LabPacientDetail', component: () => import('@/views/laborator/LabPacientDetailView.vue') },
      { path: 'doctori-medicina',  name: 'LabDoctoriMed',    component: () => import('@/views/shared/DoctoriListView.vue'), props: { type: 'medicina' } },
      { path: 'doctori-laborator', name: 'LabDoctoriLab',    component: () => import('@/views/shared/DoctoriListView.vue'), props: { type: 'laborator' } },
    ]
  },

  { path: '/:pathMatch(.*)*', redirect: '/login' }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  // Citim direct din localStorage — NU facem apel API în guard
  const token = localStorage.getItem('token')
  const user  = JSON.parse(localStorage.getItem('user') || 'null')

  // Rută protejată fără token → login
  if (to.meta.requiresAuth && !token) {
    console.log('Guard: no token, redirect to login')
    return next('/login')
  }

  // Deja logat și merge la /login → redirect la dashboard
  if (to.meta.guest && token && user) {
    const dest = user.role === 'admin'
      ? '/admin'
      : user.role === 'doctor_medicina'
        ? '/medicina'
        : '/laborator'
    console.log('Guard: already logged in, redirect to', dest)
    return next(dest)
  }

  // Rol nepotrivit → login
  if (to.meta.role && user && user.role !== to.meta.role) {
    console.log('Guard: wrong role', user.role, '!= ', to.meta.role)
    return next('/login')
  }

  next()
})

export default router
