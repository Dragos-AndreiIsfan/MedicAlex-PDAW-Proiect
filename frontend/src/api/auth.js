import api from './index'

export const authApi = {
  login: (email, password) =>
    api.post('/auth/login', { email, password }),

  getMe: () =>
    api.get('/auth/me'),

  changePassword: (currentPassword, newPassword, confirmNewPassword) =>
    api.post('/profile/change-password', {
      currentPassword,
      newPassword,
      confirmNewPassword
    })
}
