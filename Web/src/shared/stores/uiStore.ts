import { create } from 'zustand'

export type ToastColor = 'success' | 'danger' | 'warning' | 'info'
export interface ToastItem {
  id: number
  color: ToastColor
  message: string
}

interface UiState {
  sidebarShow: boolean
  sidebarUnfoldable: boolean
  toasts: ToastItem[]
  setSidebarShow: (show: boolean) => void
  setSidebarUnfoldable: (value: boolean) => void
  pushToast: (color: ToastColor, message: string) => void
  dismissToast: (id: number) => void
}

let toastSeq = 0

export const useUiStore = create<UiState>((set) => ({
  sidebarShow: true,
  sidebarUnfoldable: false,
  toasts: [],
  setSidebarShow: (sidebarShow) => set({ sidebarShow }),
  setSidebarUnfoldable: (sidebarUnfoldable) => set({ sidebarUnfoldable }),
  pushToast: (color, message) => {
    const id = ++toastSeq
    set((s) => ({ toasts: [...s.toasts.slice(-3), { id, color, message }] }))
    setTimeout(() => set((s) => ({ toasts: s.toasts.filter((t) => t.id !== id) })), 5000)
  },
  dismissToast: (id) => set((s) => ({ toasts: s.toasts.filter((t) => t.id !== id) })),
}))

export const toast = {
  success: (message: string) => useUiStore.getState().pushToast('success', message),
  error: (message: string) => useUiStore.getState().pushToast('danger', message),
  info: (message: string) => useUiStore.getState().pushToast('info', message),
}
