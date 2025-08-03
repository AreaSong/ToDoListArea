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
  TaskCategoryCreateDto
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

export default api;
