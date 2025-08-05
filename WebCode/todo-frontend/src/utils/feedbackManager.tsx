import React from 'react';
import { message, notification, Modal } from 'antd';
import { ExclamationCircleOutlined } from '@ant-design/icons';
import { NotificationService, ApiErrorHandler } from './notification';

// 扩展现有的通知服务
export class FeedbackManager extends NotificationService {
  // 操作成功反馈
  static operationSuccess(operation: string, details?: string) {
    return this.success({
      content: `${operation}成功！`,
      duration: 2,
    });
  }

  // 操作失败反馈
  static operationError(operation: string, error?: any) {
    const errorMessage = error?.response?.data?.message || error?.message || '操作失败';
    return this.error({
      content: `${operation}失败：${errorMessage}`,
      duration: 4,
    });
  }

  // 加载状态管理
  static showLoading(message: string = '处理中...') {
    return this.loading({
      content: message,
      key: 'loading',
    });
  }

  static hideLoading() {
    this.destroy('loading');
  }

  // 确认对话框
  static confirm(options: {
    title: string;
    content: string;
    onOk: () => void | Promise<void>;
    onCancel?: () => void;
    okText?: string;
    cancelText?: string;
    type?: 'warning' | 'info' | 'error';
  }) {
    const { title, content, onOk, onCancel, okText = '确认', cancelText = '取消', type = 'warning' } = options;

    return Modal.confirm({
      title,
      content,
      icon: <ExclamationCircleOutlined />,
      okText,
      cancelText,
      onOk,
      onCancel,
      centered: true,
      okButtonProps: {
        danger: type === 'error'
      }
    });
  }

  // 删除确认
  static confirmDelete(itemName: string, onConfirm: () => void | Promise<void>) {
    return this.confirm({
      title: '确认删除',
      content: `确定要删除"${itemName}"吗？此操作不可恢复。`,
      onOk: onConfirm,
      okText: '删除',
      cancelText: '取消',
      type: 'error'
    });
  }

  // 批量操作确认
  static confirmBatchOperation(operation: string, count: number, onConfirm: () => void | Promise<void>) {
    return this.confirm({
      title: `确认${operation}`,
      content: `确定要${operation} ${count} 个项目吗？`,
      onOk: onConfirm,
      okText: operation,
      cancelText: '取消'
    });
  }

  // 表单验证错误
  static formValidationError(errors: string[]) {
    const content = errors.length === 1 
      ? errors[0] 
      : `发现 ${errors.length} 个错误：${errors.slice(0, 3).join('、')}${errors.length > 3 ? '等' : ''}`;
    
    return this.warning({
      content,
      duration: 4,
    });
  }

  // 网络错误处理
  static networkError(action?: string) {
    return this.error({
      content: `网络连接失败${action ? `，无法${action}` : ''}，请检查网络后重试`,
      duration: 5,
    });
  }

  // 权限错误
  static permissionError(action?: string) {
    return this.error({
      content: `权限不足${action ? `，无法${action}` : ''}，请联系管理员`,
      duration: 5,
    });
  }

  // 数据保存成功
  static saveSuccess(itemType: string = '数据') {
    return this.success({
      content: `${itemType}保存成功`,
      duration: 2,
    });
  }

  // 数据加载失败
  static loadError(itemType: string = '数据') {
    return this.error({
      content: `${itemType}加载失败，请刷新重试`,
      duration: 4,
    });
  }

  // 复制成功
  static copySuccess() {
    return this.success({
      content: '已复制到剪贴板',
      duration: 1.5,
    });
  }

  // 功能开发中
  static featureInDevelopment() {
    return this.info({
      content: '该功能正在开发中，敬请期待',
      duration: 3,
    });
  }

  // 操作取消
  static operationCancelled() {
    return this.info({
      content: '操作已取消',
      duration: 1.5,
    });
  }

  // 自动保存提示
  static autoSaved() {
    return this.success({
      content: '已自动保存',
      duration: 1,
    });
  }

  // 离线提示
  static offlineWarning() {
    return this.warning({
      content: '网络连接已断开，部分功能可能无法使用',
      duration: 0, // 不自动消失
      key: 'offline'
    });
  }

  // 在线恢复
  static onlineRecovered() {
    this.destroy('offline');
    return this.success({
      content: '网络连接已恢复',
      duration: 2,
    });
  }

  // 批量操作结果
  static batchOperationResult(operation: string, success: number, failed: number) {
    if (failed === 0) {
      return this.success({
        content: `${operation}完成，成功处理 ${success} 个项目`,
        duration: 3,
      });
    } else {
      return this.warning({
        content: `${operation}完成，成功 ${success} 个，失败 ${failed} 个`,
        duration: 4,
      });
    }
  }

  // 数据同步状态
  static syncInProgress() {
    return this.loading({
      content: '正在同步数据...',
      key: 'sync',
    });
  }

  static syncSuccess() {
    this.destroy('sync');
    return this.success({
      content: '数据同步完成',
      duration: 2,
    });
  }

  static syncError() {
    this.destroy('sync');
    return this.error({
      content: '数据同步失败，请重试',
      duration: 4,
    });
  }
}

// 常用操作的快捷方法
export const feedback = {
  // 基础操作反馈
  operationSuccess: (operation: string, details?: string) => FeedbackManager.operationSuccess(operation, details),
  operationError: (operation: string, error?: any) => FeedbackManager.operationError(operation, error),

  // 任务相关
  taskCreated: () => FeedbackManager.operationSuccess('任务创建'),
  taskUpdated: () => FeedbackManager.operationSuccess('任务更新'),
  taskDeleted: () => FeedbackManager.operationSuccess('任务删除'),
  taskCompleted: () => FeedbackManager.operationSuccess('任务完成'),

  // 模板相关
  templateCreated: () => FeedbackManager.operationSuccess('模板创建'),
  templateUpdated: () => FeedbackManager.operationSuccess('模板更新'),
  templateDeleted: () => FeedbackManager.operationSuccess('模板删除'),

  // 分类相关
  categoryCreated: () => FeedbackManager.operationSuccess('分类创建'),
  categoryUpdated: () => FeedbackManager.operationSuccess('分类更新'),
  categoryDeleted: () => FeedbackManager.operationSuccess('分类删除'),

  // 用户相关
  profileUpdated: () => FeedbackManager.operationSuccess('个人资料更新'),
  passwordChanged: () => FeedbackManager.operationSuccess('密码修改'),

  // 通用操作
  saved: () => FeedbackManager.saveSuccess(),
  copied: () => FeedbackManager.copySuccess(),
  cancelled: () => FeedbackManager.operationCancelled(),

  // 错误处理
  networkError: (action?: string) => FeedbackManager.networkError(action),
  permissionError: (action?: string) => FeedbackManager.permissionError(action),
  loadError: (itemType?: string) => FeedbackManager.loadError(itemType),

  // 确认对话框
  confirmDelete: (itemName: string, onConfirm: () => void | Promise<void>) =>
    FeedbackManager.confirmDelete(itemName, onConfirm),

  // 加载状态
  showLoading: (message?: string) => FeedbackManager.showLoading(message),
  hideLoading: () => FeedbackManager.hideLoading(),
};

// 默认导出
export default FeedbackManager;
