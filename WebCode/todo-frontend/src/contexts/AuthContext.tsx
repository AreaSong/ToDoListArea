import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { message } from 'antd';
import { userApi } from '../services/api';
import type { User, UserLoginDto } from '../types/api';

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: UserLoginDto) => Promise<void>;
  logout: () => Promise<void>;
  refreshUser: () => Promise<void>;
  hasRole: (role: string) => boolean;
  isAdmin: () => boolean;
  hasPermission: (permission: string) => boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // 检查用户是否已认证
  const isAuthenticated = !!user;

  // 初始化认证状态
  useEffect(() => {
    const initAuth = async () => {
      try {
        const token = localStorage.getItem('token');
        const savedUser = localStorage.getItem('user');

        if (token && savedUser) {
          try {
            const userData = JSON.parse(savedUser);
            setUser(userData);
          } catch (error) {
            console.error('Failed to parse saved user data:', error);
            localStorage.removeItem('token');
            localStorage.removeItem('user');
          }
        }
      } catch (error) {
        console.error('Failed to initialize auth:', error);
      } finally {
        setIsLoading(false);
      }
    };

    initAuth();
  }, []);

  // 登录
  const login = async (credentials: UserLoginDto) => {
    try {
      const response = await userApi.login(credentials);
      
      if (response.success && response.data) {
        const { token, user: userData } = response.data;
        
        // 保存到localStorage
        localStorage.setItem('token', token);
        localStorage.setItem('user', JSON.stringify(userData));
        
        // 更新状态
        setUser(userData);
        
        message.success('登录成功');
      } else {
        throw new Error(response.message || '登录失败');
      }
    } catch (error: any) {
      const errorMessage = error.response?.data?.message || error.message || '登录失败';
      message.error(errorMessage);
      throw error;
    }
  };

  // 登出
  const logout = async () => {
    try {
      // 清除本地存储
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      
      // 清除状态
      setUser(null);
      
      message.success('已退出登录');
    } catch (error) {
      console.error('Logout error:', error);
      message.error('退出登录失败');
    }
  };

  // 刷新用户信息
  const refreshUser = async () => {
    try {
      if (!user) return;
      
      const response = await userApi.getProfile(user.id);
      if (response.success && response.data) {
        const updatedUser = response.data;
        setUser(updatedUser);
        localStorage.setItem('user', JSON.stringify(updatedUser));
      }
    } catch (error) {
      console.error('Failed to refresh user:', error);
    }
  };

  // 检查用户是否具有指定角色
  const hasRole = (role: string): boolean => {
    if (!user) return false;
    return user.role === role;
  };

  // 检查用户是否为管理员
  const isAdmin = (): boolean => {
    return hasRole('admin');
  };

  // 检查用户是否具有指定权限
  const hasPermission = (permission: string): boolean => {
    if (!user) return false;
    
    // 管理员拥有所有权限
    if (isAdmin()) return true;
    
    // 根据角色定义权限
    const rolePermissions: Record<string, string[]> = {
      admin: ['*'], // 管理员拥有所有权限
      user: [
        'task:read',
        'task:write', 
        'task:delete',
        'profile:read',
        'profile:write'
      ]
    };
    
    const userPermissions = rolePermissions[user.role] || [];
    return userPermissions.includes('*') || userPermissions.includes(permission);
  };

  const value: AuthContextType = {
    user,
    isAuthenticated,
    isLoading,
    login,
    logout,
    refreshUser,
    hasRole,
    isAdmin,
    hasPermission
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

// 权限检查Hook
export const usePermission = (permission: string) => {
  const { hasPermission } = useAuth();
  return hasPermission(permission);
};

// 角色检查Hook
export const useRole = (role: string) => {
  const { hasRole } = useAuth();
  return hasRole(role);
};

// 管理员检查Hook
export const useIsAdmin = () => {
  const { isAdmin } = useAuth();
  return isAdmin();
};
