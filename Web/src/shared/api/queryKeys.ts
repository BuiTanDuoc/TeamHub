import type { TaskFilter } from './endpoints'

// Khóa query phân cấp theo team → project nên invalidate ở cấp cha sẽ làm mới toàn bộ nhánh con.
export const qk = {
  teams: ['teams'] as const,
  team: (teamId: string) => ['teams', teamId] as const,
  teamMembers: (teamId: string) => ['teams', teamId, 'members'] as const,
  projects: (teamId: string) => ['teams', teamId, 'projects'] as const,
  project: (teamId: string, projectId: string) => ['teams', teamId, 'projects', projectId] as const,
  projectMembers: (teamId: string, projectId: string) => ['teams', teamId, 'projects', projectId, 'members'] as const,
  board: (teamId: string, projectId: string, assigneeId?: string) =>
    ['teams', teamId, 'projects', projectId, 'board', assigneeId ?? 'all'] as const,
  tasks: (teamId: string, projectId: string, filter: TaskFilter) =>
    ['teams', teamId, 'projects', projectId, 'tasks', filter] as const,
  task: (teamId: string, projectId: string, taskId: string) =>
    ['teams', teamId, 'projects', projectId, 'task', taskId] as const,
  taskHistory: (teamId: string, projectId: string, taskId: string) =>
    ['teams', teamId, 'projects', projectId, 'task', taskId, 'history'] as const,
  taskComments: (teamId: string, projectId: string, taskId: string) =>
    ['teams', teamId, 'projects', projectId, 'task', taskId, 'comments'] as const,
  notifications: ['notifications'] as const,
  notificationList: (unreadOnly: boolean) => ['notifications', 'list', unreadOnly] as const,
  unreadCount: ['notifications', 'unread-count'] as const,
  users: (search: string) => ['users', search] as const,
}
