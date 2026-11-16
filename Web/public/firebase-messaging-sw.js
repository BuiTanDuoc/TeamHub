/* Service worker nhận push FCM khi người dùng không đang xem TeamHub.
 *
 * Cố ý KHÔNG nạp Firebase SDK ở đây: backend gửi tin chỉ có "data" (title, body, type, teamId, projectId,
 * workItemId, bugId, notificationId), nên chỉ cần tự dựng thông báo từ payload. Token vẫn do Firebase SDK
 * ở trang lấy (src/shared/push/push.ts) và đăng ký service worker này với scope riêng bên dưới.
 *
 * Phần dựng đường dẫn phải khớp notificationTarget() trong src/layout/NotificationBell.tsx.
 */

const CLICK_MESSAGE_TYPE = 'teamhub:push-click'

const targetFor = (data) =>
  data.teamId && data.projectId
    ? `/teams/${data.teamId}/projects/${data.projectId}${
        data.bugId ? `?tab=bugs&bug=${data.bugId}` : data.workItemId ? `?task=${data.workItemId}` : ''
      }`
    : '/notifications'

const readPayload = (event) => {
  if (!event.data) return null
  try {
    const payload = event.data.json()
    // FCM bọc tin data-only thành { data: {...}, from, fcmMessageId }; chấp nhận cả dạng phẳng cho chắc
    return { ...(payload.notification ?? {}), ...(payload.data ?? payload) }
  } catch {
    return null
  }
}

self.addEventListener('install', () => self.skipWaiting())
self.addEventListener('activate', (event) => event.waitUntil(self.clients.claim()))

self.addEventListener('push', (event) => {
  const data = readPayload(event)
  if (!data) return

  event.waitUntil(
    (async () => {
      // Đang xem TeamHub thì SignalR đã báo trong app (toast + chuông), không hiện thêm thông báo hệ thống
      const windows = await self.clients.matchAll({ type: 'window', includeUncontrolled: true })
      if (windows.some((w) => w.visibilityState === 'visible')) return

      await self.registration.showNotification(data.title || 'TeamHub', {
        body: data.body || '',
        icon: '/favicon.svg',
        badge: '/favicon.svg',
        // Cùng notificationId thì thay thế chứ không xếp chồng
        tag: data.notificationId || undefined,
        data,
      })
    })(),
  )
})

self.addEventListener('notificationclick', (event) => {
  event.notification.close()
  const data = event.notification.data || {}
  const url = targetFor(data)

  event.waitUntil(
    (async () => {
      const windows = await self.clients.matchAll({ type: 'window', includeUncontrolled: true })
      const existing = windows.find((w) => new URL(w.url).origin === self.location.origin)
      if (existing) {
        // Trang chưa được SW này kiểm soát nên không navigate() được: nhờ trang tự điều hướng (SPA, không tải lại)
        await existing.focus()
        existing.postMessage({ type: CLICK_MESSAGE_TYPE, url, notificationId: data.notificationId || null })
        return
      }
      await self.clients.openWindow(url)
    })(),
  )
})
