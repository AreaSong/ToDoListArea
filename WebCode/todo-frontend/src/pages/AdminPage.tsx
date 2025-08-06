import React, { useState, useEffect } from 'react';
import { 
  Layout, 
  Menu, 
  Card, 
  Typography, 
  Spin,
  message,
  Button,
  Space
} from 'antd';
import {
  SafetyOutlined,
  BarChartOutlined,
  SettingOutlined,
  TeamOutlined,
  FileTextOutlined
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import UserManagement from '../components/admin/UserManagement';
import InvitationCodeManagement from '../components/admin/InvitationCodeManagement';
import SystemStats from '../components/admin/SystemStats';

const { Header, Sider, Content } = Layout;
const { Title } = Typography;

type AdminMenuItem = 'dashboard' | 'users' | 'invitations' | 'stats' | 'settings';

const AdminPage: React.FC = () => {
  const [selectedMenu, setSelectedMenu] = useState<AdminMenuItem>('dashboard');
  const [collapsed, setCollapsed] = useState(false);
  const [loading, setLoading] = useState(true);
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    // 检查用户是否为管理员
    if (!user || user.role !== 'admin') {
      message.error('您没有权限访问管理员页面');
      navigate('/dashboard');
      return;
    }
    setLoading(false);
  }, [user, navigate]);

  const handleLogout = async () => {
    try {
      await logout();
      navigate('/login');
    } catch (error) {
      message.error('退出登录失败');
    }
  };

  const menuItems = [
    {
      key: 'dashboard',
      icon: <BarChartOutlined />,
      label: '仪表板',
    },
    {
      key: 'users',
      icon: <TeamOutlined />,
      label: '用户管理',
    },
    {
      key: 'invitations',
      icon: <SafetyOutlined />,
      label: '邀请码管理',
    },
    {
      key: 'stats',
      icon: <FileTextOutlined />,
      label: '统计报告',
    },
    {
      key: 'settings',
      icon: <SettingOutlined />,
      label: '系统设置',
    },
  ];

  const renderContent = () => {
    switch (selectedMenu) {
      case 'dashboard':
        return <SystemStats />;
      case 'users':
        return <UserManagement />;
      case 'invitations':
        return <InvitationCodeManagement />;
      case 'stats':
        return (
          <Card>
            <Title level={3}>统计报告</Title>
            <p>统计报告功能开发中...</p>
          </Card>
        );
      case 'settings':
        return (
          <Card>
            <Title level={3}>系统设置</Title>
            <p>系统设置功能开发中...</p>
          </Card>
        );
      default:
        return <SystemStats />;
    }
  };

  if (loading) {
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

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider 
        collapsible 
        collapsed={collapsed} 
        onCollapse={setCollapsed}
        theme="dark"
        width={250}
      >
        <div style={{ 
          height: 64, 
          display: 'flex', 
          alignItems: 'center', 
          justifyContent: 'center',
          color: 'white',
          fontSize: '18px',
          fontWeight: 'bold'
        }}>
          {collapsed ? '管理' : '管理员控制面板'}
        </div>
        
        <Menu
          theme="dark"
          mode="inline"
          selectedKeys={[selectedMenu]}
          items={menuItems}
          onClick={({ key }) => setSelectedMenu(key as AdminMenuItem)}
        />
      </Sider>
      
      <Layout>
        <Header style={{ 
          background: '#fff', 
          padding: '0 24px',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
        }}>
          <Title level={4} style={{ margin: 0 }}>
            {menuItems.find(item => item.key === selectedMenu)?.label || '仪表板'}
          </Title>
          
          <Space>
            <span>欢迎，{user?.name}</span>
            <Button type="link" onClick={() => navigate('/dashboard')}>
              返回主页
            </Button>
            <Button type="link" onClick={handleLogout}>
              退出登录
            </Button>
          </Space>
        </Header>
        
        <Content style={{ 
          margin: '24px',
          background: '#f0f2f5',
          minHeight: 'calc(100vh - 112px)'
        }}>
          {renderContent()}
        </Content>
      </Layout>
    </Layout>
  );
};

export default AdminPage;
