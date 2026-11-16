import { devicesApi } from '../api/endpoints'
import { getFirebaseApp, isFirebaseConfigured, VAPID_KEY } from './firebase'

// Push trình duyệt qua Firebase Cloud Messaging (FCM).
// Luồng: xin quyền → đăng ký service worker → lấy FCM token → gửi token lên backend (POST /api/v1/devices, platform Web).
// Backend gửi tin tới token đó khi có thông báo; service worker (public/firebase-messaging-sw.js) hiển thị.

export type PushSupport = 'ok' | 'unsupported' | 'not-configured'

/** Người dùng đã bật push trên trình duyệt này. Giữ qua đăng xuất để lần đăng nhập sau tự đăng ký lại. */
const OPT_IN_KEY = 'teamhub-push-optin'
/** Token đang đăng ký, để gỡ khỏi backend khi đăng xuất/tắt mà không cần hỏi lại FCM. */
const TOKEN_KEY = 'teamhub-push-token'

const SW_URL = '/firebase-messaging-sw.js'
// Scope riêng (mặc định của Firebase) để service worker này không chiếm quyền kiểm soát trang
const SW_SCOPE = '/firebase-cloud-messaging-push-scope'

/** Người dùng chặn quyền thông báo (hoặc đóng hộp thoại) khi bật push. */
export class PushPermissionError extends Error {
  constructor(readonly permission: NotificationPermission) {
    super(`Notification permission ${permission}`)
    this.name = 'PushPermissionError'
  }
}

const storage = {
  get: (key: string): string | null => {
    try {
      return localStorage.getItem(key)
    } catch {
      return null
    }
  },
  set: (key: string, value: string): void => {
    try {
      localStorage.setItem(key, value)
    } catch {
      /* chế độ riêng tư/đầy bộ nhớ: bỏ qua, lần sau chỉ phải bật lại */
    }
  },
  remove: (key: string): void => {
    try {
      localStorage.removeItem(key)
    } catch {
      /* như trên */
    }
  },
}

export const getPushSupport = async (): Promise<PushSupport> => {
  if (!isFirebaseConfigured) return 'not-configured'
  if (typeof Notification === 'undefined' || !('serviceWorker' in navigator)) return 'unsupported'
  // Nạp muộn: firebase/messaging khá nặng, chỉ cần khi người dùng dùng tính năng này
  const { isSupported } = await import('firebase/messaging')
  return (await isSupported()) ? 'ok' : 'unsupported'
}

export const getNotificationPermission = (): NotificationPermission | 'unsupported' =>
  typeof Notification === 'undefined' ? 'unsupported' : Notification.permission

/** Đã bật trên trình duyệt này (người dùng chọn bật và quyền vẫn còn). */
export const isPushEnabled = (): boolean => storage.get(OPT_IN_KEY) === '1' && getNotificationPermission() === 'granted'

const waitForActive = (registration: ServiceWorkerRegistration): Promise<void> =>
  new Promise((resolve, reject) => {
    const worker = registration.installing ?? registration.waiting ?? registration.active
    if (!worker || worker.state === 'activated') {
      resolve()
      return
    }
    worker.addEventListener('statechange', () => {
      if (worker.state === 'activated') resolve()
      else if (worker.state === 'redundant') reject(new Error('Service worker bị thay thế trước khi kích hoạt'))
    })
  })

/** Lấy FCM token hiện tại (tạo mới nếu chưa có) và gửi lên backend cho tài khoản đang đăng nhập. */
const registerCurrentToken = async (): Promise<void> => {
  const { getMessaging, getToken } = await import('firebase/messaging')
  const registration = await navigator.serviceWorker.register(SW_URL, { scope: SW_SCOPE })
  // getToken cần service worker đã active mới subscribe được
  await waitForActive(registration)

  const token = await getToken(getMessaging(getFirebaseApp()), { vapidKey: VAPID_KEY, serviceWorkerRegistration: registration })
  if (!token) throw new Error('FCM không trả về token')

  // Backend chuyển token sang người dùng hiện tại nếu máy từng đăng nhập bằng tài khoản khác; gọi lại mỗi lần mở app là an toàn
  await devicesApi.register(token, 'Web')

  // Token đổi (FCM luân chuyển) thì gỡ token cũ để không còn gửi vào chỗ chết
  const previous = storage.get(TOKEN_KEY)
  storage.set(TOKEN_KEY, token)
  if (previous && previous !== token) await devicesApi.unregister(previous).catch(() => {})
}

/** Bật push: xin quyền rồi đăng ký. Ném PushPermissionError nếu người dùng không cho phép. */
export const enablePush = async (): Promise<void> => {
  const permission = await Notification.requestPermission()
  if (permission !== 'granted') throw new PushPermissionError(permission)
  storage.set(OPT_IN_KEY, '1')
  await registerCurrentToken()
}

/** Tắt push trên trình duyệt này: gỡ token khỏi backend và hủy token ở FCM. */
export const disablePush = async (): Promise<void> => {
  const token = storage.get(TOKEN_KEY)
  storage.remove(OPT_IN_KEY)
  storage.remove(TOKEN_KEY)
  if (token) await devicesApi.unregister(token).catch(() => {})
  try {
    const { deleteToken, getMessaging } = await import('firebase/messaging')
    await deleteToken(getMessaging(getFirebaseApp()))
  } catch {
    /* token đã bị hủy hoặc FCM không truy cập được: backend đã gỡ nên không còn gửi tới */
  }
}

/** Gọi khi mở app lúc đã đăng nhập: làm mới đăng ký nếu người dùng đã bật trước đó. Không ném lỗi. */
export const syncPushRegistration = async (): Promise<void> => {
  if (storage.get(OPT_IN_KEY) !== '1') return
  if (getNotificationPermission() !== 'granted') {
    // Người dùng thu hồi quyền trong cài đặt trình duyệt: coi như đã tắt
    storage.remove(OPT_IN_KEY)
    storage.remove(TOKEN_KEY)
    return
  }
  if ((await getPushSupport()) !== 'ok') return
  try {
    await registerCurrentToken()
  } catch {
    /* lỗi mạng/FCM: lần mở app sau sẽ thử lại */
  }
}

/**
 * Gọi TRƯỚC khi xóa access token lúc đăng xuất để máy này không nhận push của tài khoản cũ.
 * Giữ cờ bật push: người dùng kế tiếp đăng nhập sẽ được đăng ký lại tự động. Không ném lỗi, tối đa vài giây.
 */
export const unregisterPushForLogout = async (): Promise<void> => {
  const token = storage.get(TOKEN_KEY)
  if (!token) return
  storage.remove(TOKEN_KEY)
  const timeout = new Promise<void>((resolve) => setTimeout(resolve, 3000))
  await Promise.race([devicesApi.unregister(token).catch(() => {}), timeout])
}
