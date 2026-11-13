import { useCallback } from 'react'
import type { FieldValues, Path, UseFormSetError } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { ApiError } from '../api/client'
import { toast } from '../stores/uiStore'

/** Hiển thị lỗi API bằng toast; lỗi không phải ApiError (mạng, ...) dùng thông điệp chung đã dịch. */
export const useApiError = () => {
  const { t } = useTranslation()

  const showError = useCallback(
    (error: unknown) => {
      if (error instanceof ApiError) {
        if (error.status === 403) return toast.error(error.detail ?? t('errors.forbidden'))
        if (error.status === 409 && !error.detail) return toast.error(t('errors.conflict'))
        if (error.status === 429) return toast.error(t('errors.tooManyRequests'))
        return toast.error(error.displayMessage)
      }
      toast.error(t('errors.network'))
    },
    [t],
  )

  /**
   * Lỗi validate của backend (400 + errors theo field, tên property dạng PascalCase) → gắn vào field của form.
   * Trả về true nếu có ít nhất một lỗi được gắn; ngược lại gọi showError.
   */
  const applyFieldErrors = useCallback(
    <T extends FieldValues>(error: unknown, setError: UseFormSetError<T>, known: readonly string[]): void => {
      if (error instanceof ApiError && error.errors) {
        let applied = false
        for (const [key, messages] of Object.entries(error.errors)) {
          const name = key.charAt(0).toLowerCase() + key.slice(1)
          const message = messages[0]
          if (message && known.includes(name)) {
            setError(name as Path<T>, { type: 'server', message })
            applied = true
          }
        }
        if (applied) return
      }
      showError(error)
    },
    [showError],
  )

  return { showError, applyFieldErrors }
}
