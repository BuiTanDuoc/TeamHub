import { CNav, CNavItem, CNavLink } from '@coreui/react'
import type { ReactNode } from 'react'

export interface TabDef<K extends string> {
  key: K
  label: ReactNode
}

interface Props<K extends string> {
  tabs: TabDef<K>[]
  active: K
  onChange: (key: K) => void
}

export function TabNav<K extends string>({ tabs, active, onChange }: Props<K>) {
  return (
    <CNav variant="tabs" className="mb-3" role="tablist">
      {tabs.map((tab) => (
        <CNavItem key={tab.key}>
          <CNavLink as="button" type="button" role="tab" active={tab.key === active} onClick={() => onChange(tab.key)}>
            {tab.label}
          </CNavLink>
        </CNavItem>
      ))}
    </CNav>
  )
}
