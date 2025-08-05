import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { Spin } from 'antd';
import { useAuth } from '../../contexts/AuthContext';

interface ProtectedRouteProps {
  children: React.ReactNode;
  requireAuth?: boolean;
  requireAdmin?: boolean;
  requiredRole?: string;
  requiredPermission?: string;
  redirectTo?: string;
}

/**
 * 路由保护组件
 * 用于保护需要认证或特定权限的路由
 */
export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  requireAuth = true,
  requireAdmin = false,
  requiredRole,
  requiredPermission,
  redirectTo = '/login'
}) => {
  const { user, isAuthenticated, isLoading, hasRole, hasPermission, isAdmin } = useAuth();
  const location = useLocation();

  // 如果正在加载认证状态，显示加载器
  if (isLoading) {
    return (
      <div style={{ 
        display: 'flex', 
        justifyContent: 'center', 
        alignItems: 'center', 
        height: '100vh' 
      }}>
        <Spin size="large" />
      </div>
    );
  }

  // 如果需要认证但用户未登录，重定向到登录页
  if (requireAuth && !isAuthenticated) {
    return <Navigate to={redirectTo} state={{ from: location }} replace />;
  }

  // 如果需要管理员权限但用户不是管理员，重定向到首页
  if (requireAdmin && !isAdmin()) {
    return <Navigate to="/dashboard" replace />;
  }

  // 如果需要特定角色但用户没有该角色，重定向到首页
  if (requiredRole && !hasRole(requiredRole)) {
    return <Navigate to="/dashboard" replace />;
  }

  // 如果需要特定权限但用户没有该权限，重定向到首页
  if (requiredPermission && !hasPermission(requiredPermission)) {
    return <Navigate to="/dashboard" replace />;
  }

  // 权限检查通过，渲染子组件
  return <>{children}</>;
};

/**
 * 管理员专用路由保护
 */
export const AdminRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  return (
    <ProtectedRoute requireAdmin>
      {children}
    </ProtectedRoute>
  );
};

/**
 * 角色保护路由
 */
export const RoleRoute: React.FC<{ children: React.ReactNode; role: string }> = ({ 
  children, 
  role 
}) => {
  return (
    <ProtectedRoute requiredRole={role}>
      {children}
    </ProtectedRoute>
  );
};

/**
 * 权限保护路由
 */
export const PermissionRoute: React.FC<{ 
  children: React.ReactNode; 
  permission: string 
}> = ({ children, permission }) => {
  return (
    <ProtectedRoute requiredPermission={permission}>
      {children}
    </ProtectedRoute>
  );
};

/**
 * 公开路由（不需要认证）
 */
export const PublicRoute: React.FC<{ 
  children: React.ReactNode;
  redirectIfAuthenticated?: boolean;
  redirectTo?: string;
}> = ({ 
  children, 
  redirectIfAuthenticated = false,
  redirectTo = '/dashboard'
}) => {
  const { isAuthenticated, isLoading } = useAuth();

  // 如果正在加载认证状态，显示加载器
  if (isLoading) {
    return (
      <div style={{ 
        display: 'flex', 
        justifyContent: 'center', 
        alignItems: 'center', 
        height: '100vh' 
      }}>
        <Spin size="large" />
      </div>
    );
  }

  // 如果已认证且需要重定向，重定向到指定页面
  if (redirectIfAuthenticated && isAuthenticated) {
    return <Navigate to={redirectTo} replace />;
  }

  // 渲染子组件
  return <>{children}</>;
};
