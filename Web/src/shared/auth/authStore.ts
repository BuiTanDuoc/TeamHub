import { create } from 'zustand'
import { setUnauthorizedHandler } from '../api/client'
import { authApi } from '../api/endpoints'
import { queryClient } from '../api/queryClient'
import { tokenStorage } from '../api/tokens'
import type { AuthResult, AuthUser, MyTeam } from '../api/types'

type Status = 'loading' | 'authenticated' | 'anonymous'

interface AuthState {
  status: Status
  user: AuthUser | null
  teams: MyTeam[]
  init: () => Promise<void>
  login: (email: string, password: string) => Promise<void>
  loginWithGoogle: (idToken: string) => Promise<void>
  logout: () => Promise<void>
  /** Tải lại user + danh sách team (sau khi tạo team, đổi vai trò, rời team...). */
  reloadMe: () => Promise<void>
}

const saveTokens = (result: AuthResult): void =>
  tokenStorage.set({
    accessToken: result.accessToken,
    refreshToken: result.refreshToken,
    accessTokenExpiresAt: result.accessTokenExpiresAt,
  })

export const useAuthStore = create<AuthState>((set, get) => ({
  status: 'loading',
  user: null,
  teams: [],

  init: async () => {
    if (!tokenStorage.get()) {
      set({ status: 'anonymous' })
      return
    }
    try {
      const me = await authApi.me()
      set({ status: 'authenticated', user: me.user, teams: me.teams })
    } catch {
      tokenStorage.clear()
      set({ status: 'anonymous', user: null, teams: [] })
    }
  },

  login: async (email, password) => {
    saveTokens(await authApi.login(email, password))
    await get().reloadMe()
  },

  loginWithGoogle: async (idToken) => {
    saveTokens(await authApi.google(idToken))
    await get().reloadMe()
  },

  logout: async () => {
    const refreshToken = tokenStorage.get()?.refreshToken
    tokenStorage.clear()
    set({ status: 'anonymous', user: null, teams: [] })
    queryClient.clear()
    if (refreshToken) {
      try {
        await authApi.logout(refreshToken)
      } catch {
        /* thu hồi phía server thất bại thì bỏ qua: token đã bị xóa ở client */
      }
    }
  },

  reloadMe: async () => {
    const me = await authApi.me()
    set({ status: 'authenticated', user: me.user, teams: me.teams })
  },
}))

// Khi refresh token thất bại / hết hạn: đưa về trạng thái chưa đăng nhập
setUnauthorizedHandler(() => {
  if (useAuthStore.getState().status !== 'anonymous') {
    useAuthStore.setState({ status: 'anonymous', user: null, teams: [] })
    queryClient.clear()
  }
})
