import { getApp, getApps, initializeApp, type FirebaseApp } from 'firebase/app'

// Cấu hình web app của Firebase (Project settings → General → Your apps). Đây là giá trị công khai, không phải bí mật.
const env = import.meta.env

const config = {
  apiKey: env.VITE_FIREBASE_API_KEY ?? '',
  authDomain: env.VITE_FIREBASE_AUTH_DOMAIN ?? '',
  projectId: env.VITE_FIREBASE_PROJECT_ID ?? '',
  messagingSenderId: env.VITE_FIREBASE_MESSAGING_SENDER_ID ?? '',
  appId: env.VITE_FIREBASE_APP_ID ?? '',
}

/** Web Push certificate (key pair): Project settings → Cloud Messaging → Web configuration. */
export const VAPID_KEY = env.VITE_FIREBASE_VAPID_KEY ?? ''

/** Thiếu bất kỳ giá trị nào thì tắt tính năng push trình duyệt (UI hiển thị hướng dẫn cấu hình). */
export const isFirebaseConfigured = Boolean(config.apiKey && config.projectId && config.messagingSenderId && config.appId && VAPID_KEY)

export const getFirebaseApp = (): FirebaseApp => (getApps().length > 0 ? getApp() : initializeApp(config))
