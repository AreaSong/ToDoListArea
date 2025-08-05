// API响应基础类型
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors: string[];
}

// 用户相关类型
export interface User {
  id: string;
  email: string;
  name: string;
  phone?: string;
  avatarUrl?: string;
  status: string;
  role: string;
  emailVerified: boolean;
  phoneVerified: boolean;
  lastLoginAt?: string;
  createdAt: string;
}

export interface UserRegisterDto {
  email: string;
  password: string;
  name: string;
  phone?: string;
  invitationCode: string;
}

export interface UserLoginDto {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: User;
  expiresAt: string;
}

// 邀请码相关类型
export interface InvitationCode {
  id: string;
  code: string;
  maxUses: number;
  usedCount: number;
  remainingUses: number;
  expiresAt?: string;
  isExpired: boolean;
  isAvailable: boolean;
  status: string;
  createdBy: string;
  createdByName: string;
  createdAt: string;
  updatedAt: string;
}

export interface InvitationCodeValidation {
  isValid: boolean;
  message: string;
  invitationCode?: InvitationCode;
}

export interface ValidateInvitationCodeDto {
  code: string;
}

// 管理员相关类型
export interface AdminUser {
  id: string;
  email: string;
  name: string;
  phone?: string;
  avatarUrl?: string;
  status: string;
  role: string;
  emailVerified: boolean;
  phoneVerified: boolean;
  lastLoginAt?: string;
  createdAt: string;
  updatedAt: string;
  taskCount: number;
}

export interface AdminUserDetail extends AdminUser {
  completedTaskCount: number;
  invitationCodeUsed?: string;
  permissions: string[];
}

export interface AdminTask {
  id: string;
  title: string;
  description?: string;
  status: string;
  priority: string;
  dueDate?: string;
  createdAt: string;
  updatedAt: string;
}

export interface UpdateUserRoleDto {
  role: string;
}

export interface UpdateUserStatusDto {
  status: string;
}

export interface AdminStats {
  totalUsers: number;
  activeUsers: number;
  adminUsers: number;
  totalTasks: number;
  completedTasks: number;
  todayRegistrations: number;
  weekRegistrations: number;
  monthRegistrations: number;
  invitationCodeStats?: InvitationCodeStats;
}

export interface InvitationCodeStats {
  totalCodes: number;
  activeCodes: number;
  disabledCodes: number;
  expiredCodes: number;
  totalUsages: number;
  todayUsages: number;
  weekUsages: number;
  monthUsages: number;
}

export interface AdminUserQuery {
  page?: number;
  pageSize?: number;
  search?: string;
  role?: string;
  status?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

// 任务状态枚举（与后端TodoTaskStatus对应）
export type TodoTaskStatus = 'Pending' | 'InProgress' | 'Completed';

// 任务优先级枚举（与后端TodoTaskPriority对应）
export type TodoTaskPriority = 'Low' | 'Medium' | 'High';

// 状态显示映射
export const TodoTaskStatusLabels: Record<TodoTaskStatus, string> = {
  'Pending': '待处理',
  'InProgress': '进行中',
  'Completed': '已完成'
};

// 优先级显示映射
export const TodoTaskPriorityLabels: Record<TodoTaskPriority, string> = {
  'Low': '低',
  'Medium': '中',
  'High': '高'
};

// 为了向后兼容，保留旧的类型别名
export type TaskStatus = TodoTaskStatus;
export type TaskPriority = TodoTaskPriority;
export const TaskStatusLabels = TodoTaskStatusLabels;
export const TaskPriorityLabels = TodoTaskPriorityLabels;

// 任务相关类型
export interface Task {
  id: string;
  userId: string;
  parentTaskId?: string;
  categoryId?: string;
  title: string;
  description?: string;
  status: TodoTaskStatus;
  priority: TodoTaskPriority;
  startTime?: string;
  endTime?: string;
  estimatedDuration?: number;
  actualDuration?: number;
  createdAt: string;
  updatedAt: string;
  categoryName?: string;
  parentTaskTitle?: string;
  subTasks: Task[];
}

export interface TaskCreateDto {
  title: string;
  description?: string;
  status: string;
  priority: string;
  startTime?: string;
  endTime?: string;
  estimatedDuration?: number;
  categoryId?: string;
  parentTaskId?: string;
}

export interface TaskUpdateDto {
  title?: string;
  description?: string;
  status?: string;
  priority?: string;
  startTime?: string;
  endTime?: string;
  estimatedDuration?: number;
  actualDuration?: number;
  categoryId?: string;
  parentTaskId?: string;
}

export interface TaskQuery {
  status?: string;
  priority?: string;
  categoryId?: string;
  startDate?: string;
  endDate?: string;
  searchKeyword?: string;
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortOrder?: string;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// 任务分类类型
export interface TaskCategory {
  id: string;
  name: string;
  color: string;
  icon?: string;
  description?: string;
  isSystem: boolean;
  sortOrder: number;
  createdAt: string;
}

export interface TaskCategoryCreateDto {
  name: string;
  color?: string;
  icon?: string;
  description?: string;
  sortOrder?: number;
}

// 用户更新DTO
export interface UserUpdateDto {
  name?: string;
  phone?: string;
  avatarUrl?: string;
}

// 用户详细资料相关类型
export interface UserProfileDetail {
  userId: string;
  firstName?: string;
  lastName?: string;
  timezone: string;
  language: string;
  dateFormat: string;
  timeFormat: string;
  notificationPreferences: NotificationPreferences;
  themePreferences: ThemePreferences;
}

export interface UserProfileUpdate {
  firstName?: string;
  lastName?: string;
  timezone: string;
  language: string;
  dateFormat: string;
  timeFormat: string;
  notificationPreferences?: NotificationPreferences;
  themePreferences?: ThemePreferences;
}

export interface NotificationPreferences {
  email: boolean;
  push: boolean;
  desktop: boolean;
  quietHours: QuietHours;
}

export interface QuietHours {
  start: string;
  end: string;
}

export interface ThemePreferences {
  theme: 'light' | 'dark' | 'auto';
  primaryColor: string;
  compactMode: boolean;
}

// 甘特图数据相关类型
export interface GanttDataItem {
  id: string;
  userId: string;
  taskId: string;
  taskTitle: string;
  taskDescription: string;
  startDate: string;
  endDate: string;
  progress: number;
  dependencies: string[];
  resources: string[];
  categoryColor: string;
  categoryName: string;
  createdAt: string;
  updatedAt?: string;
}

export interface GanttDataUpdate {
  startDate: string;
  endDate: string;
  progress: number;
  dependencies?: string[];
  resources?: string[];
}

export interface GanttSyncResult {
  syncedCount: number;
  updatedCount: number;
  skippedCount: number;
  cleanedCount: number;
  totalTasks: number;
  successRate: number;
}

export interface GanttConsistencyCheck {
  isConsistent: boolean;
  inconsistencies: GanttInconsistency[];
  checkTime: string;
}

export interface GanttInconsistency {
  ganttDataId: string;
  taskId: string;
  taskTitle: string;
  inconsistencyType: string;
  description: string;
  expectedValue?: any;
  actualValue?: any;
}

// 任务模板相关类型
export interface TaskTemplate {
  id: string;
  userId: string;
  name: string;
  description?: string;
  templateData: string;
  category?: string;
  isPublic: boolean;
  usageCount: number;
  createdAt: string;
  updatedAt?: string;
}

export interface TaskTemplateCreate {
  userId: string;
  name: string;
  description?: string;
  templateData: string;
  category?: string;
}

export interface TaskTemplateUpdate {
  name?: string;
  description?: string;
  templateData?: string;
  category?: string;
}

export interface CreateTaskFromTemplate {
  userId: string;
  customTitle?: string;
  customDescription?: string;
  startTime?: string;
  endTime?: string;
}

export interface TemplateUsageStats {
  totalTemplates: number;
  totalUsage: number;
  mostUsedTemplate?: string;
  averageUsage: number;
  categoriesCount: number;
  recentlyCreated: RecentTemplate[];
}

export interface RecentTemplate {
  id: string;
  name: string;
  category?: string;
  usageCount: number;
  createdAt: string;
}

export interface TaskTemplateData {
  title: string;
  description?: string;
  priority: string;
  categoryId?: string;
  startTime?: string;
  endTime?: string;
  tags: string[];
  customFields: Record<string, any>;
}

// 任务详情相关类型
export interface TaskDetail {
  id: string;
  taskId: string;
  detailType: string;
  detailKey: string;
  detailValue?: string;
  sortOrder: number;
  createdAt: string;
  updatedAt: string;
}

export interface ChecklistItem {
  id: string;
  taskId: string;
  title: string;
  isCompleted: boolean;
  sortOrder: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateChecklistItem {
  title: string;
}

export interface UpdateChecklistItem {
  title?: string;
  isCompleted?: boolean;
  sortOrder?: number;
}

export interface TaskNote {
  id: string;
  taskId: string;
  noteType: string; // "note" 或 "comment"
  title: string;
  content: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTaskNote {
  noteType: string;
  title: string;
  content: string;
}

export interface UpdateTaskNote {
  title?: string;
  content?: string;
}

export interface TaskLink {
  id: string;
  taskId: string;
  title: string;
  url: string;
  sortOrder: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTaskLink {
  title: string;
  url: string;
}

export interface UpdateTaskLink {
  title?: string;
  url?: string;
  sortOrder?: number;
}

export interface TaskDetailsStats {
  taskId: string;
  checklistTotal: number;
  checklistCompleted: number;
  checklistCompletionRate: number;
  notesCount: number;
  commentsCount: number;
  linksCount: number;
  lastUpdated?: string;
}

export interface BatchOperationResult {
  successCount: number;
  failureCount: number;
  totalCount: number;
  errors: string[];
  successRate: number;
}

// 用户活动相关类型
export interface UserActivity {
  id: string;
  userId: string;
  activityType: string;
  activityDescription?: string;
  entityType?: string;
  entityId?: string;
  metadata?: string;
  ipAddress?: string;
  userAgent?: string;
  createdAt: string;
}

export interface CreateUserActivity {
  userId: string;
  activityType: string;
  activityDescription?: string;
  entityType?: string;
  entityId?: string;
  metadata?: string;
}

export interface UserActivityStats {
  totalActivities: number;
  todayActivities: number;
  lastActivityTime?: string;
  activityTypeStats: ActivityTypeCount[];
}

export interface ActivityTypeCount {
  activityType: string;
  count: number;
}

export interface UserActivityQuery {
  page?: number;
  pageSize?: number;
  activityType?: string;
  startDate?: string;
  endDate?: string;
}

// 活动类型常量
export const ActivityTypes = {
  LOGIN: 'login',
  LOGOUT: 'logout',
  CREATE_TASK: 'create_task',
  UPDATE_TASK: 'update_task',
  DELETE_TASK: 'delete_task',
  VIEW_TASK: 'view_task',
  CREATE_TASK_DETAIL: 'create_task_detail',
  UPDATE_TASK_DETAIL: 'update_task_detail',
  DELETE_TASK_DETAIL: 'delete_task_detail',
  CREATE_TEMPLATE: 'create_template',
  UPDATE_TEMPLATE: 'update_template',
  DELETE_TEMPLATE: 'delete_template',
  USE_TEMPLATE: 'use_template',
  UPDATE_PROFILE: 'update_profile',
  VIEW_DASHBOARD: 'view_dashboard',
  VIEW_GANTT: 'view_gantt',
  VIEW_TEMPLATES: 'view_templates',
  VIEW_PROFILE: 'view_profile',
  SEARCH: 'search',
  EXPORT: 'export',
  IMPORT: 'import'
} as const;

// 实体类型常量
export const EntityTypes = {
  TASK: 'task',
  TASK_DETAIL: 'task_detail',
  TASK_TEMPLATE: 'task_template',
  TASK_CATEGORY: 'task_category',
  USER: 'user',
  USER_PROFILE: 'user_profile',
  GANTT_DATA: 'gantt_data'
} as const;
