import { CToast, CToastBody, CToastClose } from '@coreui/react'
import { useUiStore } from '../stores/uiStore'

/** Hàng đợi toast tự quản (Zustand) — không dùng CToaster.push để tránh trạng thái kép. */
export function ToastHost() {
  const toasts = useUiStore((s) => s.toasts)
  const dismiss = useUiStore((s) => s.dismissToast)
  return (
    <div className="toast-container position-fixed top-0 end-0 p-3" style={{ zIndex: 2000 }} aria-live="polite">
      {toasts.map((item) => (
        <CToast key={item.id} visible autohide={false} color={item.color} className="text-white align-items-center border-0 mb-2" onClose={() => dismiss(item.id)}>
          <div className="d-flex">
            <CToastBody>{item.message}</CToastBody>
            <CToastClose dark className="me-2 m-auto" />
          </div>
        </CToast>
      ))}
    </div>
  )
}
