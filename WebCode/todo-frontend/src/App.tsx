import React, { Suspense } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { App as AntdApp } from 'antd';

import ErrorBoundary from './components/ErrorBoundary';
import { LoadingPage } from './components/LoadingSpinner';
import { ThemeProvider } from './contexts/ThemeContext';
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

// 加载组件
const PageLoading: React.FC = () => (
  <LoadingPage
    title="页面加载中"
    description="正在为您准备页面内容..."
    timeout={8000}
  />
);

// 简单的认证检查
const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const token = localStorage.getItem('token');
  return token ? <>{children}</> : <Navigate to="/login" replace />;
};

const App: React.FC = () => {
  return (
    <ErrorBoundary>
      <ThemeProvider>
        <AntdApp>
          <Router>
            <div className="App">
              <Suspense fallback={<PageLoading />}>
              <Routes>
              {/* 公开路由 */}
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />

              {/* 受保护的路由 */}
              <Route
                path="/dashboard"
                element={
                  <ProtectedRoute>
                    <DashboardPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/profile"
                element={
                  <ProtectedRoute>
                    <ProfilePage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/gantt"
                element={
                  <ProtectedRoute>
                    <GanttPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/templates"
                element={
                  <ProtectedRoute>
                    <TemplatesPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/task/:taskId"
                element={
                  <ProtectedRoute>
                    <TaskDetailsPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/activity"
                element={
                  <ProtectedRoute>
                    <ActivityPage />
                  </ProtectedRoute>
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
    </ErrorBoundary>
  );
};

export default App;
