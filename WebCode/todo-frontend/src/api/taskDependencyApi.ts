import api from '../services/api';

// 任务依赖关系相关的类型定义
export interface TaskDependency {
  id: string;
  taskId: string;
  taskTitle?: string;
  dependsOnTaskId: string;
  dependsOnTaskTitle?: string;
  dependencyType: string;
  lagTime: number;
  createdAt: string;
}

export interface CreateTaskDependency {
  dependsOnTaskId: string;
  lagTime?: number;
}

export interface TaskConflictCheck {
  taskId: string;
  hasConflicts: boolean;
  conflicts: string[];
  checkedAt: string;
}

export interface TaskDependencySummary {
  taskId: string;
  taskTitle: string;
  dependenciesCount: number;
  dependentsCount: number;
  hasCircularDependency: boolean;
  hasTimeConflict: boolean;
}

// 任务依赖关系API接口
export const taskDependencyApi = {
  /**
   * 获取任务的所有依赖关系
   * @param taskId 任务ID
   * @returns 依赖关系列表
   */
  getTaskDependencies: async (taskId: string): Promise<TaskDependency[]> => {
    const response = await api.get(`/TaskDependency/task/${taskId}/dependencies`);
    return response.data;
  },

  /**
   * 获取依赖于指定任务的所有任务
   * @param taskId 任务ID
   * @returns 依赖任务列表
   */
  getTaskDependents: async (taskId: string): Promise<TaskDependency[]> => {
    const response = await api.get(`/TaskDependency/task/${taskId}/dependents`);
    return response.data;
  },

  /**
   * 为任务添加依赖关系
   * @param taskId 任务ID
   * @param dependency 依赖关系数据
   * @returns 创建的依赖关系
   */
  createTaskDependency: async (taskId: string, dependency: CreateTaskDependency): Promise<TaskDependency> => {
    const response = await api.post(`/TaskDependency/task/${taskId}/dependency`, dependency);
    return response.data;
  },

  /**
   * 删除依赖关系
   * @param dependencyId 依赖关系ID
   */
  deleteTaskDependency: async (dependencyId: string): Promise<void> => {
    await api.delete(`/TaskDependency/${dependencyId}`);
  },

  /**
   * 检查任务的依赖冲突
   * @param taskId 任务ID
   * @returns 冲突检查结果
   */
  checkTaskConflicts: async (taskId: string): Promise<TaskConflictCheck> => {
    const response = await api.get(`/TaskDependency/task/${taskId}/conflicts`);
    return response.data;
  },

  /**
   * 获取用户所有任务的依赖关系摘要
   * @param userId 用户ID
   * @returns 依赖关系摘要列表
   */
  getTaskDependenciesSummary: async (userId: string): Promise<TaskDependencySummary[]> => {
    // 这个API需要在后端实现，暂时返回空数组
    // const response = await apiClient.get(`/api/TaskDependency/user/${userId}/summary`);
    // return response.data;
    console.log(`获取用户 ${userId} 的任务依赖摘要`);
    return [];
  },

  /**
   * 批量检查多个任务的冲突
   * @param taskIds 任务ID列表
   * @returns 冲突检查结果列表
   */
  batchCheckConflicts: async (taskIds: string[]): Promise<TaskConflictCheck[]> => {
    const results: TaskConflictCheck[] = [];
    
    // 并发检查所有任务的冲突
    const promises = taskIds.map(taskId => 
      taskDependencyApi.checkTaskConflicts(taskId).catch(error => {
        console.error(`检查任务 ${taskId} 冲突失败:`, error);
        return {
          taskId,
          hasConflicts: false,
          conflicts: [],
          checkedAt: new Date().toISOString()
        };
      })
    );

    const conflictResults = await Promise.all(promises);
    results.push(...conflictResults);

    return results;
  },

  /**
   * 获取任务的完整依赖信息（包括依赖和被依赖）
   * @param taskId 任务ID
   * @returns 完整依赖信息
   */
  getTaskFullDependencyInfo: async (taskId: string) => {
    try {
      const [dependencies, dependents, conflicts] = await Promise.all([
        taskDependencyApi.getTaskDependencies(taskId),
        taskDependencyApi.getTaskDependents(taskId),
        taskDependencyApi.checkTaskConflicts(taskId)
      ]);

      return {
        taskId,
        dependencies,
        dependents,
        conflicts,
        summary: {
          dependenciesCount: dependencies.length,
          dependentsCount: dependents.length,
          hasConflicts: conflicts.hasConflicts
        }
      };
    } catch (error) {
      console.error('获取任务完整依赖信息失败:', error);
      throw error;
    }
  }
};

// 导出类型和API
export default taskDependencyApi;
