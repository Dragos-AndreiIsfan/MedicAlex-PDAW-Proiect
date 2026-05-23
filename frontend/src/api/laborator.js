import api from './index'

export const laboratorApi = {
  // ── Pacienți ─────────────────────────────
  getPatients: () =>
    api.get('/laborator/patients'),

  getPatient: (id) =>
    api.get(`/laborator/patients/${id}`),

  // ── Cereri ───────────────────────────────
  getPendingRequests: () =>
    api.get('/laborator/requests/pending'),

  getMyRequests: () =>
    api.get('/laborator/requests/mine'),

  acceptRequest: (id) =>
    api.put(`/laborator/requests/${id}/accept`),

  // ── Rezultate PDF ─────────────────────────
  uploadResult: (formData) =>
    api.post('/laborator/results', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    }),

  getPatientResults: (patientId) =>
    api.get(`/laborator/results/patient/${patientId}`),

  downloadPdf: (filename) =>
    api.get(`/laborator/results/${filename}`, { responseType: 'blob' }),

  // ── Notificări ───────────────────────────
  getNotifications: () =>
    api.get('/laborator/notifications'),

  markRead: (id) =>
    api.put(`/laborator/notifications/${id}/read`),

  markAllRead: () =>
    api.put('/laborator/notifications/read-all'),

  // ── Doctori ──────────────────────────────
  getMedicinaDoctors: () =>
    api.get('/laborator/doctors/medicina'),

  getLaboratorDoctors: () =>
    api.get('/laborator/doctors/laborator')
}
