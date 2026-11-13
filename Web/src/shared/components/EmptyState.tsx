import CIcon from '@coreui/icons-react'
import { cilInbox } from '@coreui/icons'
import type { ReactNode } from 'react'

interface Props {
  title: string
  description?: string
  action?: ReactNode
}

export function EmptyState({ title, description, action }: Props) {
  return (
    <div className="text-center text-body-secondary py-5">
      <CIcon icon={cilInbox} size="3xl" className="mb-3 opacity-50" />
      <div className="fw-semibold">{title}</div>
      {description && <div className="small mt-1">{description}</div>}
      {action && <div className="mt-3">{action}</div>}
    </div>
  )
}
