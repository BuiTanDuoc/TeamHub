import type { WorkItemStatus } from '@/shared/api/types'

/**
 * Bản sao phía client của Domain/WorkItems/WorkItemWorkflow.cs — chỉ để tô sáng cột được phép thả khi kéo thẻ.
 * Backend vẫn là nơi quyết định (trả 409 nếu chuyển sai); khi đổi workflow ở backend phải cập nhật bảng này.
 */
export const TRANSITIONS: Record<WorkItemStatus, readonly WorkItemStatus[]> = {
  Todo: ['InProgress'],
  InProgress: ['Todo', 'InReview', 'Testing'],
  InReview: ['InProgress', 'Testing'],
  Testing: ['InProgress', 'Done'],
  Done: ['InProgress'], // mở lại task
}

export const canTransition = (from: WorkItemStatus, to: WorkItemStatus): boolean => TRANSITIONS[from].includes(to)
