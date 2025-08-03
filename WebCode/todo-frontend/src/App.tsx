import React, { Suspense } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ConfigProvider, Spin } from 'antd';
import zhCN from 'antd/locale/zh_CN';
import ErrorBoundary from './components/ErrorBoundary';
import './App.css';

// 懒加载页面组件
const LoginPage = React.lazy(() => import('./pages/LoginPage'));
const RegisterPage = React.lazy(() => import('./pages/RegisterPage'));
const DashboardPage = React.lazy(() => import('./pages/DashboardPage'));
const ProfilePage = React.lazy(() => import('./pages/ProfilePage'));
const GanttPage = React.lazy(() => import('./pages/GanttPage'));

// 加载组件
const PageLoading: React.FC = () => (
  <div style={{
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    height: '100vh',
    flexDirection: 'column',
    gap: '16px'
  }}>
    <Spin size="large" />
    <div style={{ color: '#666' }}>页面加载中...</div>
  </div>
);

// 简单的认证检查
const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const token = localStorage.getItem('token');
  return token ? <>{children}</> : <Navigate to="/login" replace />;
};

const App: React.FC = () => {
  return (
    <ErrorBoundary>
      <ConfigProvider
        locale={zhCN}
        theme={{
          token: {
            fontSize: 14,
            borderRadius: 6,
          },
          components: {
            Layout: {
              bodyBg: '#f5f5f5',
              headerBg: '#ffffff',
            },
          },
        }}
      >
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

              {/* 默认重定向 */}
              <Route path="/" element={<Navigate to="/dashboard" replace />} />
            </Routes>
          </Suspense>
        </div>
      </Router>
    </ConfigProvider>
    </ErrorBoundary>
  );
};

export default App;
