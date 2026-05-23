import api from './index'

export const adminApi = {
  // ── Doctori ──────────────────────────────
  getDoctors: () =>
    api.get('/admin/doctors'),

  createDoctor: (data) =>
    api.post('/admin/doctors', data),

  deleteDoctor: (id) =>
    api.delete(`/admin/doctors/${id}`),

  // ── Pacienți ─────────────────────────────
  getPatients: () =>
    api.get('/admin/patients'),

  createPatient: (data) =>
    api.post('/admin/patients', data),

  updatePatient: (id, data) =>
    api.put(`/admin/patients/${id}`, data),

  deletePatient: (id) =>
    api.delete(`/admin/patients/${id}`)
}
