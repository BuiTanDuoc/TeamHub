import { useEffect, useRef } from 'react'
import { useTranslation } from 'react-i18next'

const GOOGLE_CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID

interface GoogleId {
  initialize: (config: { client_id: string; callback: (response: { credential: string }) => void }) => void
  renderButton: (el: HTMLElement, options: Record<string, unknown>) => void
}
declare global {
  interface Window {
    google?: { accounts: { id: GoogleId } }
  }
}

let scriptPromise: Promise<void> | null = null
const loadGoogleScript = (): Promise<void> => {
  scriptPromise ??= new Promise((resolve, reject) => {
    const script = document.createElement('script')
    script.src = 'https://accounts.google.com/gsi/client'
    script.async = true
    script.onload = () => resolve()
    script.onerror = () => {
      scriptPromise = null
      reject(new Error('Không tải được Google Identity Services'))
    }
    document.head.appendChild(script)
  })
  return scriptPromise
}

interface Props {
  onCredential: (idToken: string) => void
}

/**
 * Nút Google: client lấy ID token rồi gửi POST /api/v1/auth/google (CLAUDE.md §5).
 * Không hiển thị gì nếu chưa cấu hình VITE_GOOGLE_CLIENT_ID.
 */
export function GoogleSignInButton({ onCredential }: Props) {
  const { i18n } = useTranslation()
  const ref = useRef<HTMLDivElement>(null)
  const callbackRef = useRef(onCredential)
  callbackRef.current = onCredential

  useEffect(() => {
    if (!GOOGLE_CLIENT_ID) return
    let cancelled = false
    loadGoogleScript()
      .then(() => {
        if (cancelled || !ref.current || !window.google) return
        window.google.accounts.id.initialize({
          client_id: GOOGLE_CLIENT_ID,
          callback: (response) => callbackRef.current(response.credential),
        })
        window.google.accounts.id.renderButton(ref.current, {
          type: 'standard',
          theme: 'outline',
          size: 'large',
          width: 320,
          locale: i18n.language.startsWith('en') ? 'en' : 'vi',
        })
      })
      .catch(() => {})
    return () => {
      cancelled = true
    }
  }, [i18n.language])

  if (!GOOGLE_CLIENT_ID) return null
  return <div ref={ref} className="d-flex justify-content-center" />
}

export const isGoogleConfigured = Boolean(GOOGLE_CLIENT_ID)
