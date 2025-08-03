import React, { useState, useEffect } from 'react';
import { Layout, Card, Typography, Button, Space, message } from 'antd';
import { ArrowLeftOutlined, ReloadOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';

const { Header, Content } = Layout;
const { Title } = Typography;

const SimpleGanttPage: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Header style={{ 
        background: '#fff', 
        padding: '0 24px', 
        display: 'flex', 
        justifyContent: 'space-between', 
        alignItems: 'center',
        boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
      }}>
        <Space>
          <Button 
            type="text" 
            icon={<ArrowLeftOutlined />} 
            onClick={() => navigate('/dashboard')}
          >
            返回任务列表
          </Button>
          <Title level={3} style={{ margin: 0, color: '#1890ff' }}>
            甘特图 - 项目时间线
          </Title>
        </Space>
        
        <Space>
          <Button 
            type="text" 
            icon={<ReloadOutlined />} 
            loading={loading}
          >
            刷新
          </Button>
        </Space>
      </Header>

      <Content style={{ padding: '24px' }}>
        <Card>
          <div style={{ 
            textAlign: 'center', 
            padding: '60px 0',
            color: '#666'
          }}>
            <Title level={4}>甘特图功能开发中</Title>
            <p>甘特图组件正在集成中，请稍后再试。</p>
            <p>当前可以使用任务列表功能管理您的任务。</p>
            <Button type="primary" onClick={() => navigate('/dashboard')}>
              返回任务管理
            </Button>
          </div>
        </Card>
      </Content>
    </Layout>
  );
};

export default SimpleGanttPage;
