// Lưu token ở localStorage (web). Lưu ý bảo mật: access token ngắn hạn (15 phút), refresh token xoay vòng
// và bị thu hồi toàn bộ phiên nếu dùng lại — giảm thiểu rủi ro khi bị XSS so với token sống lâu.
const KEY = 'teamhub-auth'

export interface StoredTokens {
  accessToken: string
  refreshToken: string
  accessTokenExpiresAt: string
}

let cache: StoredTokens | null | undefined

export const tokenStorage = {
  get(): StoredTokens | null {
    if (cache !== undefined) return cache
    try {
      const raw = localStorage.getItem(KEY)
      cache = raw ? (JSON.parse(raw) as StoredTokens) : null
    } catch {
      cache = null
    }
    return cache
  },
  set(tokens: StoredTokens): void {
    cache = tokens
    try {
      localStorage.setItem(KEY, JSON.stringify(tokens))
    } catch {
      /* bỏ qua: chế độ riêng tư có thể chặn localStorage */
    }
  },
  clear(): void {
    cache = null
    try {
      localStorage.removeItem(KEY)
    } catch {
      /* noop */
    }
  },
}
