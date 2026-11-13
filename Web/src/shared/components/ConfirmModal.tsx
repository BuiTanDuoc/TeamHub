import { CButton, CModal, CModalBody, CModalFooter, CModalHeader, CModalTitle, CSpinner } from '@coreui/react'
import type { ReactNode } from 'react'
import { useTranslation } from 'react-i18next'

interface Props {
  visible: boolean
  title: string
  message: ReactNode
  confirmLabel?: string
  danger?: boolean
  busy?: boolean
  onConfirm: () => void
  onClose: () => void
}

export function ConfirmModal({ visible, title, message, confirmLabel, danger = true, busy = false, onConfirm, onClose }: Props) {
  const { t } = useTranslation()
  return (
    <CModal visible={visible} onClose={onClose} alignment="center">
      <CModalHeader>
        <CModalTitle>{title}</CModalTitle>
      </CModalHeader>
      <CModalBody>{message}</CModalBody>
      <CModalFooter>
        <CButton color="secondary" variant="outline" onClick={onClose} disabled={busy}>
          {t('common.cancel')}
        </CButton>
        <CButton color={danger ? 'danger' : 'primary'} onClick={onConfirm} disabled={busy}>
          {busy && <CSpinner size="sm" className="me-2" />}
          {confirmLabel ?? t('common.confirm')}
        </CButton>
      </CModalFooter>
    </CModal>
  )
}
