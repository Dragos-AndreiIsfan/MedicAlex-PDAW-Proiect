import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '@/api/auth'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('token') || null)
  const user  = ref(JSON.parse(localStorage.getItem('user') || 'null'))

  const isLoggedIn = computed(() => !!token.value)
  const role       = computed(() => user.value?.role || null)
  const fullName   = computed(() =>
    user.value ? `${user.value.firstName} ${user.value.lastName}` : ''
  )

  function setAuth(newToken, newUser) {
    token.value = newToken
    user.value  = newUser
    localStorage.setItem('token', newToken)
    localStorage.setItem('user', JSON.stringify(newUser))
  }

  function logout() {
    token.value = null
    user.value  = null
    localStorage.removeItem('token')
    localStorage.removeItem('user')
  }

  async function fetchMe() {
    try {
      const { data } = await authApi.getMe()
      user.value = data
      localStorage.setItem('user', JSON.stringify(data))
    } catch {
      logout()
    }
  }

  return { token, user, isLoggedIn, role, fullName, setAuth, logout, fetchMe }
})
