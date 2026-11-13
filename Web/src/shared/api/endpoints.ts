import { api } from './client'
import type {
  AddTeamMemberRequest, AppNotification, AuthResult, CreatedTask, CreateProjectRequest, CreateTaskRequest,
  CreateTeamRequest, CursorPagedResult, IdResponse, KanbanBoard, MeResponse, PagedResult, Project, ProjectMember,
  ProjectStatus, Task, TaskComment, TaskDetail, TaskHistoryEntry, Team, TeamMember, TeamRole, UpdateProjectRequest,
  UpdateTaskRequest, UpdateTeamRequest, UserSummary, WorkItemStatus, WorkItemType,
} from './types'

const teamPath = (teamId: string) => `/api/v1/teams/${teamId}`
const projectPath = (teamId: string, projectId: string) => `${teamPath(teamId)}/projects/${projectId}`
const taskPath = (teamId: string, projectId: string, taskId: string) => `${projectPath(teamId, projectId)}/tasks/${taskId}`

export interface TaskFilter {
  status?: WorkItemStatus
  type?: WorkItemType
  assigneeId?: string
  search?: string
  cursor?: number
  pageSize?: number
}

export const authApi = {
  login: (email: string, password: string) => api.post<AuthResult>('/api/v1/auth/login', { email, password }, false),
  google: (idToken: string) => api.post<AuthResult>('/api/v1/auth/google', { idToken }, false),
  logout: (refreshToken: string) => api.post<void>('/api/v1/auth/logout', { refreshToken }, false),
  me: () => api.get<MeResponse>('/api/v1/auth/me'),
}

export const usersApi = {
  /** Chỉ SystemAdmin. */
  search: (search: string, pageSize = 10) => api.get<PagedResult<UserSummary>>('/api/v1/users', { search, page: 1, pageSize }),
}

export const teamsApi = {
  list: (page = 1, pageSize = 50) => api.get<PagedResult<Team>>('/api/v1/teams', { page, pageSize }),
  get: (teamId: string) => api.get<Team>(teamPath(teamId)),
  create: (body: CreateTeamRequest) => api.post<IdResponse>('/api/v1/teams', body),
  update: (teamId: string, body: UpdateTeamRequest) => api.put(teamPath(teamId), body),
  setActive: (teamId: string, isActive: boolean) => api.put(`${teamPath(teamId)}/active`, { isActive }),
  members: (teamId: string, page = 1, pageSize = 100) =>
    api.get<PagedResult<TeamMember>>(`${teamPath(teamId)}/members`, { page, pageSize }),
  addMember: (teamId: string, body: AddTeamMemberRequest) => api.post<IdResponse>(`${teamPath(teamId)}/members`, body),
  changeRole: (teamId: string, userId: string, role: TeamRole) => api.put(`${teamPath(teamId)}/members/${userId}/role`, { role }),
  changeCapacity: (teamId: string, userId: string, weeklyCapacityHours: number) =>
    api.put(`${teamPath(teamId)}/members/${userId}/capacity`, { weeklyCapacityHours }),
  removeMember: (teamId: string, userId: string) => api.delete(`${teamPath(teamId)}/members/${userId}`),
  leave: (teamId: string) => api.delete(`${teamPath(teamId)}/members/me`),
}

export const projectsApi = {
  list: (teamId: string, status?: ProjectStatus, page = 1, pageSize = 100) =>
    api.get<PagedResult<Project>>(`${teamPath(teamId)}/projects`, { status, page, pageSize }),
  get: (teamId: string, projectId: string) => api.get<Project>(projectPath(teamId, projectId)),
  create: (teamId: string, body: CreateProjectRequest) => api.post<IdResponse>(`${teamPath(teamId)}/projects`, body),
  update: (teamId: string, projectId: string, body: UpdateProjectRequest) => api.put(projectPath(teamId, projectId), body),
  changeStatus: (teamId: string, projectId: string, status: ProjectStatus) =>
    api.put(`${projectPath(teamId, projectId)}/status`, { status }),
  remove: (teamId: string, projectId: string) => api.delete(projectPath(teamId, projectId)),
  members: (teamId: string, projectId: string, page = 1, pageSize = 100) =>
    api.get<PagedResult<ProjectMember>>(`${projectPath(teamId, projectId)}/members`, { page, pageSize }),
  addMember: (teamId: string, projectId: string, userId: string) =>
    api.post<void>(`${projectPath(teamId, projectId)}/members`, { userId }),
  removeMember: (teamId: string, projectId: string, userId: string) =>
    api.delete(`${projectPath(teamId, projectId)}/members/${userId}`),
}

export const tasksApi = {
  board: (teamId: string, projectId: string, assigneeId?: string) =>
    api.get<KanbanBoard>(`${projectPath(teamId, projectId)}/board`, { assigneeId }),
  list: (teamId: string, projectId: string, filter: TaskFilter = {}) =>
    api.get<CursorPagedResult<Task>>(`${projectPath(teamId, projectId)}/tasks`, { ...filter }),
  get: (teamId: string, projectId: string, taskId: string) => api.get<TaskDetail>(taskPath(teamId, projectId, taskId)),
  create: (teamId: string, projectId: string, body: CreateTaskRequest) =>
    api.post<CreatedTask>(`${projectPath(teamId, projectId)}/tasks`, body),
  update: (teamId: string, projectId: string, taskId: string, body: UpdateTaskRequest) =>
    api.put(taskPath(teamId, projectId, taskId), body),
  changeStatus: (teamId: string, projectId: string, taskId: string, status: WorkItemStatus) =>
    api.put(`${taskPath(teamId, projectId, taskId)}/status`, { status }),
  assign: (teamId: string, projectId: string, taskId: string, assigneeId: string | null) =>
    api.put(`${taskPath(teamId, projectId, taskId)}/assignee`, { assigneeId }),
  remove: (teamId: string, projectId: string, taskId: string) => api.delete(taskPath(teamId, projectId, taskId)),
  history: (teamId: string, projectId: string, taskId: string, page = 1, pageSize = 50) =>
    api.get<PagedResult<TaskHistoryEntry>>(`${taskPath(teamId, projectId, taskId)}/history`, { page, pageSize }),
}

export const commentsApi = {
  list: (teamId: string, projectId: string, taskId: string, page = 1, pageSize = 100) =>
    api.get<PagedResult<TaskComment>>(`${taskPath(teamId, projectId, taskId)}/comments`, { page, pageSize }),
  add: (teamId: string, projectId: string, taskId: string, body: string) =>
    api.post<IdResponse>(`${taskPath(teamId, projectId, taskId)}/comments`, { body }),
  edit: (teamId: string, projectId: string, taskId: string, commentId: string, body: string) =>
    api.put(`${taskPath(teamId, projectId, taskId)}/comments/${commentId}`, { body }),
  remove: (teamId: string, projectId: string, taskId: string, commentId: string) =>
    api.delete(`${taskPath(teamId, projectId, taskId)}/comments/${commentId}`),
}

export const notificationsApi = {
  list: (unreadOnly = false, page = 1, pageSize = 20) =>
    api.get<PagedResult<AppNotification>>('/api/v1/notifications', { unreadOnly, page, pageSize }),
  unreadCount: () => api.get<{ count: number }>('/api/v1/notifications/unread-count'),
  markRead: (id: string) => api.put(`/api/v1/notifications/${id}/read`),
  markAllRead: () => api.put<{ updated: number }>('/api/v1/notifications/read-all'),
}
