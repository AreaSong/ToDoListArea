import { message, notification } from 'antd';

// 配置全局消息设置
message.config({
  top: 100,
  duration: 3,
  maxCount: 3,
  rtl: false,
});

// 配置全局通知设置
notification.config({
  placement: 'topRight',
  top: 24,
  duration: 4.5,
  rtl: false,
});

export interface NotificationOptions {
  title?: string;
  description?: string;
  duration?: number;
  placement?: 'topLeft' | 'topRight' | 'bottomLeft' | 'bottomRight';
  key?: string;
  btn?: React.ReactNode;
  onClose?: () => void;
  onClick?: () => void;
}

export interface MessageOptions {
  content: string;
  duration?: number;
  onClose?: () => void;
  key?: string;
}

/**
 * 全局消息通知工具类
 */
export class NotificationService {
  /**
   * 成功消息
   */
  static success(options: string | MessageOptions) {
    if (typeof options === 'string') {
      return message.success(options);
    }

    return message.success(options);
  }

  /**
   * 错误消息
   */
  static error(options: string | MessageOptions) {
    if (typeof options === 'string') {
      return message.error({
        content: options,
        duration: 5, // 错误消息显示时间更长
      });
    }

    return message.error({
      ...options,
      duration: options.duration || 5,
    });
  }

  /**
   * 警告消息
   */
  static warning(options: string | MessageOptions) {
    if (typeof options === 'string') {
      return message.warning(options);
    }

    return message.warning(options);
  }

  /**
   * 信息消息
   */
  static info(options: string | MessageOptions) {
    if (typeof options === 'string') {
      return message.info(options);
    }

    return message.info(options);
  }

  /**
   * 加载消息
   */
  static loading(options: string | MessageOptions) {
    if (typeof options === 'string') {
      return message.loading({
        content: options,
        duration: 0, // 加载消息不自动消失
      });
    }
    
    return message.loading({
      ...options,
      duration: 0,
    });
  }

  /**
   * 销毁所有消息
   */
  static destroyAll() {
    message.destroy();
  }

  /**
   * 销毁指定消息
   */
  static destroy(key: string) {
    message.destroy(key);
  }

  // 通知相关方法
  /**
   * 成功通知
   */
  static notifySuccess(options: NotificationOptions) {
    return notification.success({
      message: options.title || '操作成功',
      description: options.description,
      duration: options.duration,
      placement: options.placement,
      key: options.key,
      btn: options.btn,
      onClose: options.onClose,
      onClick: options.onClick,
    });
  }

  /**
   * 错误通知
   */
  static notifyError(options: NotificationOptions) {
    return notification.error({
      message: options.title || '操作失败',
      description: options.description,
      duration: options.duration || 6, // 错误通知显示时间更长
      placement: options.placement,
      key: options.key,
      btn: options.btn,
      onClose: options.onClose,
      onClick: options.onClick,
    });
  }

  /**
   * 警告通知
   */
  static notifyWarning(options: NotificationOptions) {
    return notification.warning({
      message: options.title || '注意',
      description: options.description,
      duration: options.duration,
      placement: options.placement,
      key: options.key,
      btn: options.btn,
      onClose: options.onClose,
      onClick: options.onClick,
    });
  }

  /**
   * 信息通知
   */
  static notifyInfo(options: NotificationOptions) {
    return notification.info({
      message: options.title || '提示',
      description: options.description,
      duration: options.duration,
      placement: options.placement,
      key: options.key,
      btn: options.btn,
      onClose: options.onClose,
      onClick: options.onClick,
    });
  }

  /**
   * 销毁所有通知
   */
  static destroyAllNotifications() {
    notification.destroy();
  }

  /**
   * 销毁指定通知
   */
  static destroyNotification(key: string) {
    notification.destroy(key);
  }
}

/**
 * API错误处理工具
 */
export class ApiErrorHandler {
  /**
   * 处理API错误响应
   */
  static handleError(error: any, customMessage?: string) {
    console.error('API Error:', error);

    let errorMessage = customMessage || '操作失败';
    let errorDescription = '';

    if (error.response) {
      // 服务器响应错误
      const { status, data } = error.response;
      
      switch (status) {
        case 400:
          errorMessage = '请求参数错误';
          errorDescription = data?.message || '请检查输入的信息是否正确';
          break;
        case 401:
          errorMessage = '未授权访问';
          errorDescription = '请重新登录后再试';
          // 可以在这里触发登录跳转
          setTimeout(() => {
            window.location.href = '/login';
          }, 2000);
          break;
        case 403:
          errorMessage = '权限不足';
          errorDescription = '您没有权限执行此操作';
          break;
        case 404:
          errorMessage = '资源不存在';
          errorDescription = '请求的资源未找到';
          break;
        case 422:
          errorMessage = '数据验证失败';
          errorDescription = data?.message || '请检查输入的数据格式';
          break;
        case 500:
          errorMessage = '服务器内部错误';
          errorDescription = '服务器遇到问题，请稍后重试';
          break;
        case 502:
        case 503:
        case 504:
          errorMessage = '服务暂时不可用';
          errorDescription = '服务器正在维护，请稍后重试';
          break;
        default:
          errorMessage = `请求失败 (${status})`;
          errorDescription = data?.message || '未知错误';
      }
    } else if (error.request) {
      // 网络错误
      errorMessage = '网络连接失败';
      errorDescription = '请检查网络连接后重试';
    } else {
      // 其他错误
      errorMessage = '操作失败';
      errorDescription = error.message || '未知错误';
    }

    NotificationService.notifyError({
      title: errorMessage,
      description: errorDescription,
    });

    return {
      message: errorMessage,
      description: errorDescription,
      status: error.response?.status,
    };
  }

  /**
   * 处理成功响应
   */
  static handleSuccess(message: string, description?: string) {
    NotificationService.notifySuccess({
      title: message,
      description,
    });
  }
}

// 导出便捷方法
export const notify = NotificationService;
export const handleApiError = ApiErrorHandler.handleError;
export const handleApiSuccess = ApiErrorHandler.handleSuccess;

// 默认导出
export default NotificationService;
