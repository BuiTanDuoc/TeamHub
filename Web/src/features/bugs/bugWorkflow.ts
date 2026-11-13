import type { BugStatus } from '@/shared/api/types'

/** Bản sao của BugWorkflow.RequiresNote (Domain/Bugs) chỉ để bắt buộc nhập lý do trên form; backend vẫn kiểm tra (400). */
export const requiresNote = (to: BugStatus): boolean =>
  to === 'Reopened' || to === 'WontFix' || to === 'Duplicate' || to === 'CannotReproduce'

/** Bug đã xử lý xong/bị loại bỏ — không còn tính SLA (BugWorkflow.IsFinal). */
export const isFinal = (status: BugStatus): boolean =>
  status === 'Closed' || status === 'WontFix' || status === 'Duplicate' || status === 'CannotReproduce'
