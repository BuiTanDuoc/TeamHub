import type { ReactNode } from 'react'

interface Props {
  title: ReactNode
  subtitle?: ReactNode
  actions?: ReactNode
}

export function PageHeader({ title, subtitle, actions }: Props) {
  return (
    <div className="d-flex flex-wrap align-items-start justify-content-between gap-2 mb-4">
      <div>
        <h2 className="h3 mb-1">{title}</h2>
        {subtitle && <div className="text-body-secondary">{subtitle}</div>}
      </div>
      {actions && <div className="d-flex flex-wrap gap-2">{actions}</div>}
    </div>
  )
}
