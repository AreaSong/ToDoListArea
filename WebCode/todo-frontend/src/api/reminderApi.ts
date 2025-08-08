import api from '../services/api';

// 提醒相关的类型定义
export interface Reminder {
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

export interface CreateReminderRequest {
  taskId?: string;
  title: string;
  message?: string;
  reminderTime: string;
  channels?: string[];
}

export interface ReminderStats {
  totalReminders: number;
  pendingReminders: number;
  completedReminders: number;
  snoozedReminders: number;
  todayReminders: number;
  weekReminders: number;
}

// 提醒API接口
export const reminderApi = {
  /**
   * 获取用户的提醒列表
   */
  getReminders: async (params?: any): Promise<Reminder[]> => {
    const response = await api.get('/Reminder', { params });
    return response.data.data?.items || [];
  },

  /**
   * 获取任务的提醒列表
   */
  getTaskReminders: async (taskId: string): Promise<Reminder[]> => {
    const response = await api.get(`/Reminder/task/${taskId}`);
    return response.data || [];
  },

  /**
   * 创建提醒
   */
  createReminder: async (data: CreateReminderRequest): Promise<Reminder> => {
    const response = await api.post('/Reminder', data);
    return response.data;
  },

  /**
   * 删除提醒
   */
  deleteReminder: async (reminderId: string): Promise<void> => {
    await api.delete(`/Reminder/${reminderId}`);
  },

  /**
   * 完成提醒
   */
  completeReminder: async (reminderId: string): Promise<void> => {
    await api.post(`/Reminder/${reminderId}/complete`);
  },

  /**
   * 获取提醒统计
   */
  getReminderStats: async (): Promise<ReminderStats> => {
    const response = await api.get('/Reminder/stats');
    return response.data || {
      totalReminders: 0,
      pendingReminders: 0,
      completedReminders: 0,
      snoozedReminders: 0,
      todayReminders: 0,
      weekReminders: 0
    };
  }
};

export default reminderApi;
