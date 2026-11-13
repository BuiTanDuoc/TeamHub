import i18n from '@/i18n'

const locale = (): string => (i18n.language?.startsWith('en') ? 'en-US' : 'vi-VN')

/** Thời gian lưu UTC (DateTimeOffset); hiển thị theo múi giờ trình duyệt (CLAUDE.md §7). */
export const formatDateTime = (iso: string | null | undefined): string =>
  iso ? new Intl.DateTimeFormat(locale(), { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(iso)) : '—'

/** DateOnly 'yyyy-MM-dd' → ngày hiển thị; tránh lệch múi giờ bằng cách dựng ngày theo giờ địa phương. */
export const formatDate = (dateOnly: string | null | undefined): string => {
  if (!dateOnly) return '—'
  const [y, m, d] = dateOnly.split('-').map(Number)
  if (!y || !m || !d) return dateOnly
  return new Intl.DateTimeFormat(locale(), { dateStyle: 'medium' }).format(new Date(y, m - 1, d))
}

export const formatRelative = (iso: string): string => {
  const diffSec = Math.round((new Date(iso).getTime() - Date.now()) / 1000)
  const rtf = new Intl.RelativeTimeFormat(locale(), { numeric: 'auto' })
  const abs = Math.abs(diffSec)
  if (abs < 60) return rtf.format(diffSec, 'second')
  if (abs < 3600) return rtf.format(Math.round(diffSec / 60), 'minute')
  if (abs < 86400) return rtf.format(Math.round(diffSec / 3600), 'hour')
  if (abs < 86400 * 30) return rtf.format(Math.round(diffSec / 86400), 'day')
  return formatDateTime(iso)
}

export const todayDateOnly = (): string => {
  const d = new Date()
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`
}

export const initials = (name: string): string =>
  name
    .split(/\s+/)
    .filter(Boolean)
    .slice(-2)
    .map((p) => p[0]?.toUpperCase() ?? '')
    .join('')
