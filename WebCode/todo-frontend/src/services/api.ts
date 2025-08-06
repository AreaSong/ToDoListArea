import axios from 'axios';
import type {
  ApiResponse,
  User,
  UserRegisterDto,
  UserLoginDto,
  LoginResponse,
  Task,
  TaskCreateDto,
  TaskUpdateDto,
  TaskQuery,
  PagedResponse,
  TaskCategory,
  TaskCategoryCreateDto,
  UserActivity,
  CreateUserActivity,
  UserActivityStats,
  UserActivityQuery,
  InvitationCodeValidation,
  ValidateInvitationCodeDto,
  AdminUser,
  AdminUserDetail,
  AdminTask,
  AdminStats,
  AdminUserQuery,
  UpdateUserRoleDto,
  UpdateUserStatusDto,
  PagedResult,
  UserProfileDetail,
  UserProfileUpdate,
  GanttDataItem,
  GanttDataUpdate,
  GanttSyncResult,
  GanttConsistencyCheck,
  TaskTemplate,
  TaskTemplateCreate,
  TaskTemplateUpdate,
  CreateTaskFromTemplate,
  TemplateUsageStats,
  TaskDetail,
  ChecklistItem,
  CreateChecklistItem,
  UpdateChecklistItem,
  TaskNote,
  CreateTaskNote,
  TaskLink,
  CreateTaskLink,
  TaskDetailsStats,
  BatchOperationResult
} from '../types/api';

// 创建axios实例
const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// 请求拦截器 - 添加token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// 响应拦截器 - 统一错误处理
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// 用户API
export const userApi = {
  // 用户注册
  register: (data: UserRegisterDto): Promise<ApiResponse<User>> =>
    api.post('/user/register', data).then(res => res.data),

  // 用户登录
  login: (data: UserLoginDto): Promise<ApiResponse<LoginResponse>> =>
    api.post('/user/login', data).then(res => res.data),

  // 获取用户信息
  getProfile: (userId: string): Promise<ApiResponse<User>> =>
    api.get(`/user/profile/${userId}`).then(res => res.data),

  // 更新用户信息
  updateProfile: (userId: string, data: Partial<User>): Promise<ApiResponse<User>> =>
    api.put(`/user/profile/${userId}`, data).then(res => res.data),
};

// 邀请码API
export const invitationCodeApi = {
  // 验证邀请码（公开接口）
  validate: (data: ValidateInvitationCodeDto): Promise<ApiResponse<InvitationCodeValidation>> =>
    api.post('/invitationcode/validate', data).then(res => res.data),

  // 获取邀请码列表（管理员）
  getList: (params?: {
    page?: number;
    pageSize?: number;
    status?: string;
    search?: string;
  }): Promise<ApiResponse<any>> =>
    api.get('/invitationcode', { params }).then(res => res.data),

  // 创建邀请码（管理员）
  create: (data: {
    code?: string;
    maxUses: number;
    expiresAt?: string;
  }): Promise<ApiResponse<any>> =>
    api.post('/invitationcode', data).then(res => res.data),

  // 更新邀请码（管理员）
  update: (id: string, data: {
    maxUses?: number;
    expiresAt?: string;
  }): Promise<ApiResponse<any>> =>
    api.put(`/invitationcode/${id}`, data).then(res => res.data),

  // 删除邀请码（管理员）
  delete: (id: string): Promise<ApiResponse<any>> =>
    api.delete(`/invitationcode/${id}`).then(res => res.data),

  // 启用/禁用邀请码（管理员）
  setStatus: (id: string, enabled: boolean): Promise<ApiResponse<any>> =>
    api.patch(`/invitationcode/${id}/status`, enabled).then(res => res.data),

  // 获取邀请码统计信息（管理员）
  getStats: (): Promise<ApiResponse<any>> =>
    api.get('/invitationcode/stats').then(res => res.data),

  // 获取邀请码使用记录（管理员）
  getUsages: (id: string, params?: {
    page?: number;
    pageSize?: number;
  }): Promise<ApiResponse<any>> =>
    api.get(`/invitationcode/${id}/usages`, { params }).then(res => res.data),

  // 获取所有使用记录（管理员）
  getAllUsages: (params?: {
    page?: number;
    pageSize?: number;
    invitationCodeId?: string;
    userId?: string;
  }): Promise<ApiResponse<any>> =>
    api.get('/invitationcode/usages', { params }).then(res => res.data),
};

// 用户详细资料API
export const userProfileApi = {
  // 获取用户详细资料
  getProfile: (userId: string): Promise<ApiResponse<UserProfileDetail>> =>
    api.get(`/userprofile/${userId}`).then(res => res.data),

  // 更新用户详细资料
  updateProfile: (userId: string, data: UserProfileUpdate): Promise<ApiResponse<UserProfileDetail>> =>
    api.put(`/userprofile/${userId}`, data).then(res => res.data),
};

// 甘特图数据API
export const ganttDataApi = {
  // 获取用户甘特图数据
  getGanttData: (userId: string): Promise<ApiResponse<GanttDataItem[]>> =>
    api.get(`/ganttdata/user/${userId}`).then(res => res.data),

  // 更新甘特图数据
  updateGanttData: (id: string, data: GanttDataUpdate): Promise<ApiResponse<GanttDataItem>> =>
    api.put(`/ganttdata/${id}`, data).then(res => res.data),

  // 从任务同步到甘特图
  syncFromTasks: (userId: string): Promise<ApiResponse<GanttSyncResult>> =>
    api.post(`/ganttdata/sync/${userId}`).then(res => res.data),

  // 删除甘特图数据
  deleteGanttData: (id: string): Promise<ApiResponse<any>> =>
    api.delete(`/ganttdata/${id}`).then(res => res.data),

  // 检查数据一致性
  checkConsistency: (userId: string): Promise<ApiResponse<GanttConsistencyCheck>> =>
    api.get(`/ganttdata/consistency-check/${userId}`).then(res => res.data),
};

// 任务模板API
export const taskTemplateApi = {
  // 获取用户模板列表
  getUserTemplates: (userId: string, category?: string, sortBy?: string): Promise<ApiResponse<TaskTemplate[]>> =>
    api.get(`/tasktemplate/user/${userId}`, {
      params: { category, sortBy }
    }).then(res => res.data),

  // 获取模板详情
  getTemplate: (id: string): Promise<ApiResponse<TaskTemplate>> =>
    api.get(`/tasktemplate/${id}`).then(res => res.data),

  // 创建模板
  createTemplate: (data: TaskTemplateCreate): Promise<ApiResponse<TaskTemplate>> =>
    api.post('/tasktemplate', data).then(res => res.data),

  // 更新模板
  updateTemplate: (id: string, data: TaskTemplateUpdate): Promise<ApiResponse<TaskTemplate>> =>
    api.put(`/tasktemplate/${id}`, data).then(res => res.data),

  // 删除模板
  deleteTemplate: (id: string): Promise<ApiResponse<any>> =>
    api.delete(`/tasktemplate/${id}`).then(res => res.data),

  // 从模板创建任务
  createTaskFromTemplate: (templateId: string, data: CreateTaskFromTemplate): Promise<ApiResponse<Task>> =>
    api.post(`/tasktemplate/${templateId}/use`, data).then(res => res.data),

  // 获取模板分类
  getTemplateCategories: (userId: string): Promise<ApiResponse<string[]>> =>
    api.get(`/tasktemplate/categories/${userId}`).then(res => res.data),

  // 获取模板使用统计
  getTemplateStats: (userId: string): Promise<ApiResponse<TemplateUsageStats>> =>
    api.get(`/tasktemplate/stats/${userId}`).then(res => res.data),
};

// 任务详情API
export const taskDetailsApi = {
  // 获取任务所有详情
  getTaskDetails: (taskId: string, detailType?: string): Promise<ApiResponse<TaskDetail[]>> =>
    api.get(`/taskdetails/task/${taskId}`, {
      params: { detailType }
    }).then(res => res.data),

  // 获取任务检查清单
  getTaskChecklist: (taskId: string): Promise<ApiResponse<ChecklistItem[]>> =>
    api.get(`/taskdetails/task/${taskId}/checklist`).then(res => res.data),

  // 获取任务笔记
  getTaskNotes: (taskId: string): Promise<ApiResponse<TaskNote[]>> =>
    api.get(`/taskdetails/task/${taskId}/notes`).then(res => res.data),

  // 获取任务链接
  getTaskLinks: (taskId: string): Promise<ApiResponse<TaskLink[]>> =>
    api.get(`/taskdetails/task/${taskId}/links`).then(res => res.data),

  // 添加检查清单项
  addChecklistItem: (taskId: string, data: CreateChecklistItem): Promise<ApiResponse<ChecklistItem>> =>
    api.post(`/taskdetails/task/${taskId}/checklist`, data).then(res => res.data),

  // 更新检查清单项
  updateChecklistItem: (id: string, data: UpdateChecklistItem): Promise<ApiResponse<ChecklistItem>> =>
    api.put(`/taskdetails/checklist/${id}`, data).then(res => res.data),

  // 添加任务笔记
  addTaskNote: (taskId: string, data: CreateTaskNote): Promise<ApiResponse<TaskNote>> =>
    api.post(`/taskdetails/task/${taskId}/notes`, data).then(res => res.data),

  // 添加任务链接
  addTaskLink: (taskId: string, data: CreateTaskLink): Promise<ApiResponse<TaskLink>> =>
    api.post(`/taskdetails/task/${taskId}/links`, data).then(res => res.data),

  // 删除任务详情项
  deleteTaskDetail: (id: string): Promise<ApiResponse<any>> =>
    api.delete(`/taskdetails/${id}`).then(res => res.data),

  // 获取任务详情统计
  getTaskDetailsStats: (taskId: string): Promise<ApiResponse<TaskDetailsStats>> =>
    api.get(`/taskdetails/task/${taskId}/stats`).then(res => res.data),

  // 批量更新检查清单状态
  batchUpdateChecklistStatus: (taskId: string, isCompleted: boolean): Promise<ApiResponse<BatchOperationResult>> =>
    api.put(`/taskdetails/task/${taskId}/checklist/batch-status`, null, {
      params: { isCompleted }
    }).then(res => res.data),
};

// 用户活动API
export const userActivityApi = {
  // 获取用户活动列表
  getUserActivities: (userId: string, params?: UserActivityQuery): Promise<ApiResponse<UserActivity[]>> =>
    api.get(`/useractivity/user/${userId}`, { params }).then(res => res.data),

  // 获取用户活动统计
  getUserActivityStats: (userId: string, startDate?: string, endDate?: string): Promise<ApiResponse<UserActivityStats>> =>
    api.get(`/useractivity/user/${userId}/stats`, {
      params: { startDate, endDate }
    }).then(res => res.data),

  // 手动记录用户活动
  createUserActivity: (data: CreateUserActivity): Promise<ApiResponse<UserActivity>> =>
    api.post('/useractivity', data).then(res => res.data),

  // 获取活动类型列表
  getActivityTypes: (): Promise<ApiResponse<string[]>> =>
    api.get('/useractivity/activity-types').then(res => res.data),

  // 删除用户活动记录
  deleteUserActivity: (id: string): Promise<ApiResponse<any>> =>
    api.delete(`/useractivity/${id}`).then(res => res.data),
};

// 任务API
export const taskApi = {
  // 获取任务列表
  getTasks: (userId: string, query: TaskQuery): Promise<ApiResponse<PagedResponse<Task>>> =>
    api.get(`/task/user/${userId}`, { params: query }).then(res => res.data),

  // 获取任务详情
  getTask: (taskId: string): Promise<ApiResponse<Task>> =>
    api.get(`/task/${taskId}`).then(res => res.data),

  // 创建任务
  createTask: (userId: string, data: TaskCreateDto): Promise<ApiResponse<Task>> =>
    api.post(`/task/user/${userId}`, data).then(res => res.data),

  // 更新任务
  updateTask: (taskId: string, data: TaskUpdateDto): Promise<ApiResponse<Task>> =>
    api.put(`/task/${taskId}`, data).then(res => res.data),

  // 删除任务
  deleteTask: (taskId: string): Promise<ApiResponse<null>> =>
    api.delete(`/task/${taskId}`).then(res => res.data),
};

// 任务分类API
export const categoryApi = {
  // 获取所有分类
  getCategories: (): Promise<ApiResponse<TaskCategory[]>> =>
    api.get('/taskcategory').then(res => res.data),

  // 获取分类详情
  getCategory: (categoryId: string): Promise<ApiResponse<TaskCategory>> =>
    api.get(`/taskcategory/${categoryId}`).then(res => res.data),

  // 创建分类
  createCategory: (data: TaskCategoryCreateDto): Promise<ApiResponse<TaskCategory>> =>
    api.post('/taskcategory', data).then(res => res.data),

  // 更新分类
  updateCategory: (categoryId: string, data: Partial<TaskCategoryCreateDto>): Promise<ApiResponse<TaskCategory>> =>
    api.put(`/taskcategory/${categoryId}`, data).then(res => res.data),

  // 删除分类
  deleteCategory: (categoryId: string): Promise<ApiResponse<null>> =>
    api.delete(`/taskcategory/${categoryId}`).then(res => res.data),
};

// 管理员API
export const adminApi = {
  // 获取用户列表
  getUsers: (query: AdminUserQuery): Promise<ApiResponse<PagedResult<AdminUser>>> =>
    api.get('/admin/users', { params: query }).then(res => res.data),

  // 获取用户详情
  getUserDetail: (userId: string): Promise<ApiResponse<AdminUserDetail>> =>
    api.get(`/admin/users/${userId}`).then(res => res.data),

  // 更新用户角色
  updateUserRole: (userId: string, data: UpdateUserRoleDto): Promise<ApiResponse<boolean>> =>
    api.put(`/admin/users/${userId}/role`, data).then(res => res.data),

  // 更新用户状态
  updateUserStatus: (userId: string, data: UpdateUserStatusDto): Promise<ApiResponse<boolean>> =>
    api.put(`/admin/users/${userId}/status`, data).then(res => res.data),

  // 获取用户的待办事项
  getUserTasks: (userId: string, page?: number, pageSize?: number): Promise<ApiResponse<PagedResult<AdminTask>>> =>
    api.get(`/admin/users/${userId}/tasks`, { params: { page, pageSize } }).then(res => res.data),

  // 获取系统统计信息
  getStats: (): Promise<ApiResponse<AdminStats>> =>
    api.get('/admin/stats').then(res => res.data),
};

export default api;
