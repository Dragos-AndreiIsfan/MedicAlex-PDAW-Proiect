import api from './index'

export const medicinaApi = {
  // ── Pacienți ─────────────────────────────
  getMyPatients: () =>
    api.get('/medicina/patients/mine'),

  getOtherPatients: () =>
    api.get('/medicina/patients/others'),

  getPatient: (id) =>
    api.get(`/medicina/patients/${id}`),

  createPatient: (data) =>
    api.post('/medicina/patients', data),

  updatePatient: (id, data) =>
    api.put(`/medicina/patients/${id}`, data),

  deletePatient: (id) =>
    api.delete(`/medicina/patients/${id}`),

  // ── Cereri analize ────────────────────────
  createAnalysisRequest: (patientId, analysisType) =>
    api.post('/medicina/analysis-requests', { patientId, analysisType }),

  getPatientRequests: (patientId) =>
    api.get(`/medicina/analysis-requests/patient/${patientId}`),

  getPatientResults: (patientId) =>
    api.get(`/medicina/results/patient/${patientId}`),

  // ── Doctori ──────────────────────────────
  getMedicinaDoctors: () =>
    api.get('/medicina/doctors/medicina'),

  getLaboratorDoctors: () =>
    api.get('/medicina/doctors/laborator')
}
