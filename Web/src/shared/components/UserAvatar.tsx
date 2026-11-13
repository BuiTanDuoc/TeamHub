import { CAvatar } from '@coreui/react'
import { initials } from '../utils/format'

const COLORS = ['primary', 'success', 'info', 'warning', 'danger', 'secondary'] as const

const colorFor = (seed: string) => COLORS[[...seed].reduce((sum, ch) => sum + ch.charCodeAt(0), 0) % COLORS.length]

interface Props {
  name: string
  src?: string | null
  size?: 'sm' | 'md' | 'lg' | 'xl'
}

export function UserAvatar({ name, src, size = 'md' }: Props) {
  return src ? (
    <CAvatar src={src} size={size} title={name} />
  ) : (
    <CAvatar color={colorFor(name)} textColor="white" size={size} title={name}>
      {initials(name) || '?'}
    </CAvatar>
  )
}
