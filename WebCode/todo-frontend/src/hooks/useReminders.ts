import { useState, useEffect, useCallback } from 'react';
import axios from 'axios';
import { NotificationService, handleApiError, handleApiSuccess } from '../utils/notification';

// 内联类型定义，避免导入问题
interface Reminder {
  id: string;
  userId: string;
  taskId?: string;
  taskTitle?: string;
  title: string;
  message?: string;
  reminderTime: string;
  status: string;
  channels: string[];
  snoozeUntil?: string;
  createdAt: string;
  updatedAt: string;
}

interface CreateReminderRequest {
  taskId?: string;
  title: string;
  message?: string;
  reminderTime: string;
  channels?: string[];
}

interface ReminderStats {
  totalReminders: number;
  pendingReminders: number;
  completedReminders: number;
  snoozedReminders: number;
  todayReminders: number;
  weekReminders: number;
}

// API基础URL
const API_BASE_URL = 'http://localhost:5006';

// 创建axios实例
const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
});

// 添加请求拦截器
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// 添加响应拦截器
api.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error('API请求失败:', error);
    if (error.response?.status === 401) {
      // 处理未授权错误
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

/**
 * 提醒管理Hook
 */
export const useReminders = (taskId?: string) => {
  const [reminders, setReminders] = useState<Reminder[]>([]);
  const [stats, setStats] = useState<ReminderStats | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // 获取提醒列表
  const fetchReminders = useCallback(async () => {
    if (!taskId) return;
    
    setLoading(true);
    setError(null);
    
    try {
      console.log('获取任务提醒，taskId:', taskId);
      const response = await api.get(`/api/Reminder/task/${taskId}`);
      const data = response.data;
      
      console.log('提醒数据获取成功:', data);
      setReminders(Array.isArray(data) ? data : []);
    } catch (err: any) {
      console.error('获取提醒失败:', err);
      setError(err.response?.data?.message || '获取提醒失败');
      setReminders([]);
    } finally {
      setLoading(false);
    }
  }, [taskId]);

  // 获取提醒统计
  const fetchStats = useCallback(async () => {
    try {
      console.log('获取提醒统计');
      const response = await api.get('/api/Reminder/stats');
      const data = response.data;
      
      console.log('统计数据获取成功:', data);
      setStats(data);
    } catch (err: any) {
      console.error('获取统计失败:', err);
      // 统计失败不影响主要功能，只记录错误
    }
  }, []);

  // 创建提醒
  const createReminder = useCallback(async (reminderData: CreateReminderRequest) => {
    setLoading(true);
    setError(null);
    
    try {
      console.log('创建提醒:', reminderData);
      const response = await api.post('/api/Reminder', reminderData);
      const newReminder = response.data;
      
      console.log('提醒创建成功:', newReminder);
      setReminders(prev => [...prev, newReminder]);
      handleApiSuccess('提醒创建成功', '提醒已成功添加到您的任务中');
      
      // 刷新统计数据
      fetchStats();
      
      return newReminder;
    } catch (err: any) {
      console.error('创建提醒失败:', err);
      const errorMsg = err.response?.data?.message || '创建提醒失败';
      setError(errorMsg);
      handleApiError(err, '创建提醒失败');
      throw err;
    } finally {
      setLoading(false);
    }
  }, [fetchStats]);

  // 删除提醒
  const deleteReminder = useCallback(async (reminderId: string) => {
    setLoading(true);
    setError(null);
    
    try {
      console.log('删除提醒:', reminderId);
      await api.delete(`/api/Reminder/${reminderId}`);
      
      console.log('提醒删除成功');
      setReminders(prev => prev.filter(r => r.id !== reminderId));
      NotificationService.success('提醒删除成功');
      
      // 刷新统计数据
      fetchStats();
    } catch (err: any) {
      console.error('删除提醒失败:', err);
      const errorMsg = err.response?.data?.message || '删除提醒失败';
      setError(errorMsg);
      handleApiError(err, '删除提醒失败');
      throw err;
    } finally {
      setLoading(false);
    }
  }, [fetchStats]);

  // 完成提醒
  const completeReminder = useCallback(async (reminderId: string) => {
    setLoading(true);
    setError(null);
    
    try {
      console.log('完成提醒:', reminderId);
      await api.post(`/api/Reminder/${reminderId}/complete`);
      
      console.log('提醒完成成功');
      // 更新本地状态
      setReminders(prev => prev.map(r => 
        r.id === reminderId ? { ...r, status: 'completed' } : r
      ));
      NotificationService.success('提醒已标记为完成');
      
      // 刷新统计数据
      fetchStats();
    } catch (err: any) {
      console.error('完成提醒失败:', err);
      const errorMsg = err.response?.data?.message || '完成提醒失败';
      setError(errorMsg);
      handleApiError(err, '完成提醒失败');
      throw err;
    } finally {
      setLoading(false);
    }
  }, [fetchStats]);

  // 初始化数据加载
  useEffect(() => {
    if (taskId) {
      fetchReminders();
    }
    fetchStats();
  }, [taskId, fetchReminders, fetchStats]);

  return {
    // 数据
    reminders,
    stats,
    loading,
    error,
    
    // 操作方法
    createReminder,
    deleteReminder,
    completeReminder,
    fetchReminders,
    fetchStats,
    
    // 工具方法
    refresh: () => {
      fetchReminders();
      fetchStats();
    }
  };
};
