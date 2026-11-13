import { CFormFeedback, CFormLabel } from '@coreui/react'
import type { ReactNode } from 'react'
import { useTranslation } from 'react-i18next'

interface Props {
  label: string
  /** Khóa i18n (từ zod) hoặc thông điệp thô từ server. */
  error?: string | undefined
  required?: boolean
  hint?: string
  htmlFor?: string
  children: ReactNode
  className?: string
}

export function Field({ label, error, required, hint, htmlFor, children, className = 'mb-3' }: Props) {
  const { t } = useTranslation()
  return (
    <div className={className}>
      <CFormLabel htmlFor={htmlFor}>
        {label}
        {required && <span className="text-danger ms-1">*</span>}
      </CFormLabel>
      {children}
      {hint && !error && <div className="form-text">{hint}</div>}
      {error && (
        <CFormFeedback invalid className="d-block">
          {t(error, { defaultValue: error })}
        </CFormFeedback>
      )}
    </div>
  )
}
