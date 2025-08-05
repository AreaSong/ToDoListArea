import React from 'react';
import { Result, Button } from 'antd';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';

interface PermissionGuardProps {
  children: React.ReactNode;
  permission?: string;
  role?: string;
  requireAdmin?: boolean;
  fallback?: React.ReactNode;
  redirectTo?: string;
}

/**
 * 权限保护组件
 * 用于保护需要特定权限或角色的组件
 */
export const PermissionGuard: React.FC<PermissionGuardProps> = ({
  children,
  permission,
  role,
  requireAdmin = false,
  fallback,
  redirectTo
}) => {
  const { user, hasPermission, hasRole, isAdmin } = useAuth();
  const navigate = useNavigate();

  // 检查用户是否已登录
  if (!user) {
    if (redirectTo) {
      navigate(redirectTo);
      return null;
    }
    
    return fallback || (
      <Result
        status="403"
        title="需要登录"
        subTitle="请先登录后再访问此页面"
        extra={
          <Button type="primary" onClick={() => navigate('/login')}>
            去登录
          </Button>
        }
      />
    );
  }

  // 检查管理员权限
  if (requireAdmin && !isAdmin()) {
    if (redirectTo) {
      navigate(redirectTo);
      return null;
    }
    
    return fallback || (
      <Result
        status="403"
        title="权限不足"
        subTitle="您需要管理员权限才能访问此页面"
        extra={
          <Button type="primary" onClick={() => navigate('/dashboard')}>
            返回首页
          </Button>
        }
      />
    );
  }

  // 检查特定角色
  if (role && !hasRole(role)) {
    if (redirectTo) {
      navigate(redirectTo);
      return null;
    }
    
    return fallback || (
      <Result
        status="403"
        title="权限不足"
        subTitle={`您需要 ${role} 角色权限才能访问此页面`}
        extra={
          <Button type="primary" onClick={() => navigate('/dashboard')}>
            返回首页
          </Button>
        }
      />
    );
  }

  // 检查特定权限
  if (permission && !hasPermission(permission)) {
    if (redirectTo) {
      navigate(redirectTo);
      return null;
    }
    
    return fallback || (
      <Result
        status="403"
        title="权限不足"
        subTitle={`您需要 ${permission} 权限才能访问此页面`}
        extra={
          <Button type="primary" onClick={() => navigate('/dashboard')}>
            返回首页
          </Button>
        }
      />
    );
  }

  // 权限检查通过，渲染子组件
  return <>{children}</>;
};

/**
 * 管理员专用组件保护
 */
export const AdminGuard: React.FC<{ children: React.ReactNode; fallback?: React.ReactNode }> = ({
  children,
  fallback
}) => {
  return (
    <PermissionGuard requireAdmin fallback={fallback}>
      {children}
    </PermissionGuard>
  );
};

/**
 * 角色保护组件
 */
export const RoleGuard: React.FC<{ 
  children: React.ReactNode; 
  role: string; 
  fallback?: React.ReactNode 
}> = ({ children, role, fallback }) => {
  return (
    <PermissionGuard role={role} fallback={fallback}>
      {children}
    </PermissionGuard>
  );
};

/**
 * 权限保护组件（用于特定权限）
 */
export const PermissionProtected: React.FC<{ 
  children: React.ReactNode; 
  permission: string; 
  fallback?: React.ReactNode 
}> = ({ children, permission, fallback }) => {
  return (
    <PermissionGuard permission={permission} fallback={fallback}>
      {children}
    </PermissionGuard>
  );
};

/**
 * 条件渲染组件 - 根据权限显示或隐藏内容
 */
export const ConditionalRender: React.FC<{
  children: React.ReactNode;
  permission?: string;
  role?: string;
  requireAdmin?: boolean;
  showFallback?: boolean;
  fallback?: React.ReactNode;
}> = ({
  children,
  permission,
  role,
  requireAdmin = false,
  showFallback = false,
  fallback = null
}) => {
  const { user, hasPermission, hasRole, isAdmin } = useAuth();

  // 检查用户是否已登录
  if (!user) {
    return showFallback ? fallback : null;
  }

  // 检查管理员权限
  if (requireAdmin && !isAdmin()) {
    return showFallback ? fallback : null;
  }

  // 检查特定角色
  if (role && !hasRole(role)) {
    return showFallback ? fallback : null;
  }

  // 检查特定权限
  if (permission && !hasPermission(permission)) {
    return showFallback ? fallback : null;
  }

  // 权限检查通过，渲染子组件
  return <>{children}</>;
};

/**
 * 管理员专用条件渲染
 */
export const AdminOnly: React.FC<{ 
  children: React.ReactNode; 
  fallback?: React.ReactNode 
}> = ({ children, fallback }) => {
  return (
    <ConditionalRender requireAdmin showFallback={!!fallback} fallback={fallback}>
      {children}
    </ConditionalRender>
  );
};

/**
 * 用户角色条件渲染
 */
export const RoleOnly: React.FC<{ 
  children: React.ReactNode; 
  role: string; 
  fallback?: React.ReactNode 
}> = ({ children, role, fallback }) => {
  return (
    <ConditionalRender role={role} showFallback={!!fallback} fallback={fallback}>
      {children}
    </ConditionalRender>
  );
};

/**
 * 权限条件渲染
 */
export const PermissionOnly: React.FC<{ 
  children: React.ReactNode; 
  permission: string; 
  fallback?: React.ReactNode 
}> = ({ children, permission, fallback }) => {
  return (
    <ConditionalRender permission={permission} showFallback={!!fallback} fallback={fallback}>
      {children}
    </ConditionalRender>
  );
};
