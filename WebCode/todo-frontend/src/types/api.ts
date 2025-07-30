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

// 任务相关类型
export interface Task {
  id: string;
  userId: string;
  parentTaskId?: string;
  categoryId?: string;
  title: string;
  description?: string;
  status: string;
  priority: string;
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
