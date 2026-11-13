import type { AuthResult } from './types'
import { tokenStorage } from './tokens'

/** Dev: để trống → dùng proxy của Vite. Production: URL gốc của API. */
export const API_URL = (import.meta.env.VITE_API_URL as string | undefined)?.replace(/\/$/, '') ?? ''

/** Lỗi API theo ProblemDetails (RFC 9457); validate trả kèm `errors` theo field. */
export class ApiError extends Error {
  readonly status: number
  readonly title: string
  readonly detail: string | null
  readonly errors: Record<string, string[]> | null

  constructor(status: number, title: string, detail: string | null, errors: Record<string, string[]> | null) {
    super(detail ?? title)
    this.name = 'ApiError'
    this.status = status
    this.title = title
    this.detail = detail
    this.errors = errors
  }

  /** Tin nhắn dễ đọc: ưu tiên lỗi field đầu tiên, rồi detail, rồi title. */
  get displayMessage(): string {
    if (this.errors) {
      const first = Object.values(this.errors).flat()[0]
      if (first) return first
    }
    return this.detail ?? this.title
  }
}

type Query = Record<string, string | number | boolean | null | undefined>

interface RequestOptions {
  query?: Query
  body?: unknown
  /** false cho endpoint ẩn danh (login/refresh/logout). */
  auth?: boolean
  signal?: AbortSignal
}

type UnauthorizedHandler = () => void
let onUnauthorized: UnauthorizedHandler = () => {}
export const setUnauthorizedHandler = (handler: UnauthorizedHandler): void => {
  onUnauthorized = handler
}

export const buildUrl = (path: string, query?: Query): string => {
  const params = new URLSearchParams()
  if (query) {
    for (const [key, value] of Object.entries(query)) {
      if (value !== undefined && value !== null && value !== '') params.set(key, String(value))
    }
  }
  const qs = params.toString()
  return `${API_URL}${path}${qs ? `?${qs}` : ''}`
}

const parseError = async (res: Response): Promise<ApiError> => {
  let title = res.statusText || `HTTP ${res.status}`
  let detail: string | null = null
  let errors: Record<string, string[]> | null = null
  try {
    const data = (await res.json()) as { title?: string; detail?: string; errors?: Record<string, string[]> }
    title = data.title ?? title
    detail = data.detail ?? null
    errors = data.errors ?? null
  } catch {
    /* body không phải JSON */
  }
  return new ApiError(res.status, title, detail, errors)
}

// ---------- Refresh token (single-flight để nhiều request 401 cùng lúc chỉ làm mới một lần) ----------
let refreshing: Promise<boolean> | null = null

const refreshTokens = (): Promise<boolean> => {
  refreshing ??= (async () => {
    const current = tokenStorage.get()
    if (!current) return false
    try {
      const res = await fetch(buildUrl('/api/v1/auth/refresh'), {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken: current.refreshToken }),
      })
      if (!res.ok) return false
      const result = (await res.json()) as AuthResult
      tokenStorage.set({
        accessToken: result.accessToken,
        refreshToken: result.refreshToken,
        accessTokenExpiresAt: result.accessTokenExpiresAt,
      })
      return true
    } catch {
      return false
    } finally {
      // Giữ promise thêm một nhịp để các request đến sau cùng nhận kết quả, rồi mới cho phép lần làm mới mới
      setTimeout(() => {
        refreshing = null
      }, 0)
    }
  })()
  return refreshing
}

/** Dùng cho SignalR: luôn trả access token còn hạn (tự làm mới nếu sắp hết). */
export const getValidAccessToken = async (): Promise<string> => {
  const tokens = tokenStorage.get()
  if (!tokens) return ''
  const expiresInMs = new Date(tokens.accessTokenExpiresAt).getTime() - Date.now()
  if (expiresInMs < 30_000) {
    const ok = await refreshTokens()
    if (!ok) {
      onUnauthorized()
      return ''
    }
  }
  return tokenStorage.get()?.accessToken ?? ''
}

export async function request<T>(method: string, path: string, options: RequestOptions = {}): Promise<T> {
  const { query, body, auth = true, signal } = options

  const send = (): Promise<Response> => {
    const headers: Record<string, string> = { Accept: 'application/json' }
    if (body !== undefined) headers['Content-Type'] = 'application/json'
    const token = auth ? tokenStorage.get()?.accessToken : undefined
    if (token) headers.Authorization = `Bearer ${token}`
    const init: RequestInit = { method, headers }
    if (body !== undefined) init.body = JSON.stringify(body)
    if (signal) init.signal = signal
    return fetch(buildUrl(path, query), init)
  }

  let res = await send()

  if (res.status === 401 && auth && tokenStorage.get()) {
    if (await refreshTokens()) {
      res = await send()
    }
    if (res.status === 401) {
      tokenStorage.clear()
      onUnauthorized()
    }
  }

  if (!res.ok) throw await parseError(res)
  if (res.status === 204) return undefined as T
  return (await res.json()) as T
}

export const api = {
  get: <T>(path: string, query?: Query, signal?: AbortSignal) => {
    const opts: RequestOptions = {}
    if (query) opts.query = query
    if (signal) opts.signal = signal
    return request<T>('GET', path, opts)
  },
  post: <T>(path: string, body?: unknown, auth = true) => request<T>('POST', path, { body, auth }),
  put: <T = void>(path: string, body?: unknown) => request<T>('PUT', path, { body }),
  delete: <T = void>(path: string) => request<T>('DELETE', path),
}
