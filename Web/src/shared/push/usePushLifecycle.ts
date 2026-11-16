import { useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { notificationsApi } from '../api/endpoints'
import { qk } from '../api/queryKeys'
import { syncPushRegistration } from './push'

/** Khớp CLICK_MESSAGE_TYPE trong public/firebase-messaging-sw.js. */
const CLICK_MESSAGE_TYPE = 'teamhub:push-click'

interface PushClickMessage {
  type: typeof CLICK_MESSAGE_TYPE
  url: string
  notificationId: string | null
}

const isPushClick = (data: unknown): data is PushClickMessage =>
  typeof data === 'object' && data !== null && (data as { type?: unknown }).type === CLICK_MESSAGE_TYPE && typeof (data as { url?: unknown }).url === 'string'

/**
 * Vòng đời push trình duyệt trong layout đã đăng nhập:
 * - làm mới đăng ký token mỗi lần mở app (nếu người dùng đã bật);
 * - khi bấm thông báo hệ thống mà tab TeamHub đang mở: điều hướng trong SPA tới đúng task/bug và đánh dấu đã đọc.
 */
export const usePushLifecycle = (): void => {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  useEffect(() => {
    void syncPushRegistration()
  }, [])

  useEffect(() => {
    if (!('serviceWorker' in navigator)) return

    const onMessage = (event: MessageEvent<unknown>) => {
      if (!isPushClick(event.data)) return
      const { url, notificationId } = event.data
      if (notificationId) {
        void notificationsApi
          .markRead(notificationId)
          .then(() => queryClient.invalidateQueries({ queryKey: qk.notifications }))
          .catch(() => {})
      }
      navigate(url)
    }

    navigator.serviceWorker.addEventListener('message', onMessage)
    return () => navigator.serviceWorker.removeEventListener('message', onMessage)
  }, [navigate, queryClient])
}
