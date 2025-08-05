import React, { useState } from 'react';
import { Form, Input, Button, Card, Typography, message, Divider, App } from 'antd';
import { LockOutlined, MailOutlined } from '@ant-design/icons';
import { Link, useNavigate } from 'react-router-dom';
import { userApi } from '../services/api';
import type { UserLoginDto } from '../types/api';
import { EnhancedInput, EnhancedForm } from '../components/EnhancedForm';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { feedback } from '../utils/feedbackManager';

const { Title, Text } = Typography;

const LoginPage: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const [form] = Form.useForm();
  const navigate = useNavigate();
  const { message: messageApi } = App.useApp();

  const onFinish = async (values: UserLoginDto) => {
    setLoading(true);
    feedback.showLoading('正在登录...');

    try {
      const response = await userApi.login(values);

      if (response.success && response.data) {
        // 保存token和用户信息
        localStorage.setItem('token', response.data.token);
        localStorage.setItem('user', JSON.stringify(response.data.user));

        feedback.hideLoading();
        feedback.operationSuccess('登录');
        navigate('/dashboard');
      } else {
        feedback.hideLoading();
        // 使用更具体的错误消息
        const errorMsg = response.message || '登录失败，请检查用户名和密码';
        messageApi.error(errorMsg);
      }
    } catch (error: any) {
      console.error('Login error:', error);
      feedback.hideLoading();

      // 处理不同类型的错误
      let errorMessage = '登录失败';
      if (error.response) {
        switch (error.response.status) {
          case 400:
            errorMessage = '用户名或密码错误';
            break;
          case 401:
            errorMessage = '认证失败，请检查登录信息';
            break;
          case 500:
            errorMessage = '服务器错误，请稍后重试';
            break;
          default:
            errorMessage = error.response.data?.message || '登录失败，请重试';
        }
      } else if (error.request) {
        errorMessage = '网络连接失败，请检查网络后重试';
      } else {
        errorMessage = error.message || '登录失败，请重试';
      }

      messageApi.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ 
      minHeight: '100vh', 
      display: 'flex', 
      alignItems: 'center', 
      justifyContent: 'center',
      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'
    }}>
      <Card 
        style={{ 
          width: 400, 
          boxShadow: '0 4px 12px rgba(0,0,0,0.15)',
          borderRadius: '8px'
        }}
      >
        <div style={{ textAlign: 'center', marginBottom: 24 }}>
          <Title level={2} style={{ color: '#1890ff', marginBottom: 8 }}>
            智能提醒事项
          </Title>
          <Text type="secondary">登录您的账户</Text>
        </div>

        <EnhancedForm
          form={form}
          onFinish={onFinish}
          loading={loading}
          submitText="登录"
          size="large"
          layout="vertical"
        >
          <EnhancedInput
            name="email"
            label="邮箱地址"
            type="email"
            placeholder="请输入邮箱地址"
            required
            realTimeValidation
            helpText="请使用有效的邮箱地址登录"
          />

          <EnhancedInput
            name="password"
            label="密码"
            type="password"
            placeholder="请输入密码"
            required
            rules={[
              { min: 6, message: '密码长度不能少于6位!' }
            ]}
            helpText="密码长度至少6位"
          />
        </EnhancedForm>

        <Divider>或</Divider>

        <div style={{ textAlign: 'center' }}>
          <Text type="secondary">
            还没有账户？ 
            <Link to="/register" style={{ marginLeft: 4 }}>
              立即注册
            </Link>
          </Text>
        </div>
      </Card>
    </div>
  );
};

export default LoginPage;
