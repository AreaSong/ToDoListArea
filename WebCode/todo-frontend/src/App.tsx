import React, { Suspense } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { App as AntdApp } from 'antd';

import ErrorBoundary from './components/ErrorBoundary';
import { LoadingPage } from './components/LoadingSpinner';
import { ThemeProvider } from './contexts/ThemeContext';
import { AuthProvider } from './contexts/AuthContext';
import { ProtectedRoute as AuthProtectedRoute, PublicRoute, AdminRoute } from './components/auth/ProtectedRoute';
import './App.css';
import './styles/theme.css';

// 懒加载页面组件
const LoginPage = React.lazy(() => import('./pages/LoginPage'));
const RegisterPage = React.lazy(() => import('./pages/RegisterPage'));
const DashboardPage = React.lazy(() => import('./pages/DashboardPage'));
const ProfilePage = React.lazy(() => import('./pages/ProfilePage'));
const GanttPage = React.lazy(() => import('./pages/GanttPage'));
const TemplatesPage = React.lazy(() => import('./pages/TemplatesPage'));
const TaskDetailsPage = React.lazy(() => import('./pages/TaskDetailsPage'));
const ActivityPage = React.lazy(() => import('./pages/ActivityPage'));
const AdminPage = React.lazy(() => import('./pages/AdminPage'));

// 加载组件
const PageLoading: React.FC = () => (
  <LoadingPage
    title="页面加载中"
    description="正在为您准备页面内容..."
    timeout={8000}
  />
);

const App: React.FC = () => {
  return (
    <ErrorBoundary>
      <AuthProvider>
        <ThemeProvider>
          <AntdApp>
          <Router>
            <div className="App">
              <Suspense fallback={<PageLoading />}>
              <Routes>
              {/* 公开路由 */}
              <Route
                path="/login"
                element={
                  <PublicRoute redirectIfAuthenticated>
                    <LoginPage />
                  </PublicRoute>
                }
              />
              <Route
                path="/register"
                element={
                  <PublicRoute redirectIfAuthenticated>
                    <RegisterPage />
                  </PublicRoute>
                }
              />

              {/* 受保护的路由 */}
              <Route
                path="/dashboard"
                element={
                  <AuthProtectedRoute>
                    <DashboardPage />
                  </AuthProtectedRoute>
                }
              />
              <Route
                path="/profile"
                element={
                  <AuthProtectedRoute>
                    <ProfilePage />
                  </AuthProtectedRoute>
                }
              />
              <Route
                path="/gantt"
                element={
                  <AuthProtectedRoute>
                    <GanttPage />
                  </AuthProtectedRoute>
                }
              />
              <Route
                path="/templates"
                element={
                  <AuthProtectedRoute>
                    <TemplatesPage />
                  </AuthProtectedRoute>
                }
              />
              <Route
                path="/task/:taskId"
                element={
                  <AuthProtectedRoute>
                    <TaskDetailsPage />
                  </AuthProtectedRoute>
                }
              />
              <Route
                path="/activity"
                element={
                  <AuthProtectedRoute>
                    <ActivityPage />
                  </AuthProtectedRoute>
                }
              />

              {/* 管理员专用路由 */}
              <Route
                path="/admin"
                element={
                  <AdminRoute>
                    <AdminPage />
                  </AdminRoute>
                }
              />

              {/* 默认重定向 */}
              <Route path="/" element={<Navigate to="/dashboard" replace />} />
            </Routes>
          </Suspense>
        </div>
      </Router>
          </AntdApp>
        </ThemeProvider>
      </AuthProvider>
    </ErrorBoundary>
  );
};

export default App;
