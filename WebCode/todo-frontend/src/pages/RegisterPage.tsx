import React, { useState } from 'react';
import { Card, Form, Input, Button, Typography, message } from 'antd';
import { UserOutlined, MailOutlined, LockOutlined, PhoneOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { userApi } from '../services/api';
import type { UserRegisterDto } from '../types/api';

const { Title, Text } = Typography;

const RegisterPage: React.FC = () => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (values: any) => {
    setLoading(true);
    try {
      const registerData: UserRegisterDto = {
        name: values.name,
        email: values.email,
        password: values.password,
        phone: values.phone,
        invitationCode: values.invitationCode || ''
      };

      const response = await userApi.register(registerData);
      if (response.success) {
        message.success('注册成功！请登录您的账户');
        navigate('/login');
      } else {
        message.error(response.message || '注册失败，请重试');
      }
    } catch (error: any) {
      console.error('Registration error:', error);
      message.error(error.response?.data?.message || '注册失败，请重试');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{
      minHeight: '100vh',
      background: 'linear-gradient(135deg, #f093fb 0%, #f5576c 50%, #4facfe 100%)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      padding: '20px',
      position: 'relative',
      overflow: 'hidden'
    }}>
      <Card
        style={{
          width: '100%',
          maxWidth: '480px',
          borderRadius: '28px',
          boxShadow: '0 40px 80px rgba(0, 0, 0, 0.25), 0 20px 40px rgba(0, 0, 0, 0.15)',
          border: '1px solid rgba(255, 255, 255, 0.25)',
          backdropFilter: 'blur(40px)',
          background: 'linear-gradient(135deg, rgba(255, 255, 255, 0.95) 0%, rgba(255, 255, 255, 0.88) 100%)',
          position: 'relative',
          overflow: 'hidden'
        }}
        bodyStyle={{ padding: '48px 40px' }}
      >
        <div style={{ textAlign: 'center', marginBottom: '40px' }}>
          <div style={{
            width: '110px',
            height: '110px',
            borderRadius: '28px',
            background: 'linear-gradient(135deg, #f093fb 0%, #f5576c 50%, #4facfe 100%)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            margin: '0 auto 24px',
            boxShadow: '0 20px 50px rgba(240, 147, 251, 0.5)',
            position: 'relative',
            overflow: 'hidden'
          }}>
            <span style={{ 
              color: 'white', 
              fontSize: '44px', 
              fontWeight: 'bold',
              textShadow: '0 4px 12px rgba(0,0,0,0.3)',
              zIndex: 1,
              position: 'relative'
            }}>T</span>
          </div>
          
          <Title level={2} style={{ 
            margin: 0, 
            marginBottom: '8px',
            background: 'linear-gradient(135deg, #f093fb 0%, #f5576c 50%, #4facfe 100%)',
            WebkitBackgroundClip: 'text',
            WebkitTextFillColor: 'transparent',
            fontSize: '30px',
            fontWeight: 700
          }}>创建新账户</Title>
          
          <Text style={{ 
            fontSize: '16px',
            color: '#64748b',
            fontWeight: 500,
            display: 'block',
            marginBottom: '8px'
          }}>加入我们，开启高效管理之旅</Text>
        </div>

        <Form
          form={form}
          name="register"
          onFinish={handleSubmit}
          autoComplete="off"
          layout="vertical"
          size="large"
        >
          <Form.Item
            label="👤 用户名"
            name="name"
            rules={[
              { required: true, message: '请输入用户名' },
              { min: 2, message: '用户名至少2个字符' }
            ]}
          >
            <Input
              prefix={<UserOutlined style={{ color: '#9ca3af', fontSize: '16px' }} />}
              placeholder="请输入您的用户名"
              style={{ 
                borderRadius: '12px', 
                height: '52px',
                border: '2px solid #e5e7eb',
                fontSize: '15px',
                transition: 'all 0.3s ease',
                background: 'rgba(255,255,255,0.8)'
              }}
            />
          </Form.Item>

          <Form.Item
            label="📧 邮箱地址"
            name="email"
            rules={[
              { required: true, message: '请输入邮箱地址' },
              { type: 'email', message: '请输入有效的邮箱地址' }
            ]}
          >
            <Input
              prefix={<MailOutlined style={{ color: '#9ca3af', fontSize: '16px' }} />}
              placeholder="请输入您的邮箱地址"
              style={{ 
                borderRadius: '12px', 
                height: '52px',
                border: '2px solid #e5e7eb',
                fontSize: '15px',
                transition: 'all 0.3s ease',
                background: 'rgba(255,255,255,0.8)'
              }}
            />
          </Form.Item>

          <Form.Item
            label="📱 手机号码"
            name="phone"
            rules={[
              { pattern: /^1[3-9]\d{9}$/, message: '请输入有效的手机号码' }
            ]}
          >
            <Input
              prefix={<PhoneOutlined style={{ color: '#9ca3af', fontSize: '16px' }} />}
              placeholder="请输入您的手机号码（可选）"
              style={{ 
                borderRadius: '12px', 
                height: '52px',
                border: '2px solid #e5e7eb',
                fontSize: '15px',
                transition: 'all 0.3s ease',
                background: 'rgba(255,255,255,0.8)'
              }}
            />
          </Form.Item>

          <Form.Item
            label="🎫 邀请码"
            name="invitationCode"
            rules={[
              { required: true, message: '请输入邀请码' }
            ]}
          >
            <Input
              placeholder="请输入邀请码"
              style={{ 
                borderRadius: '12px', 
                height: '52px',
                border: '2px solid #e5e7eb',
                fontSize: '15px',
                transition: 'all 0.3s ease',
                background: 'rgba(255,255,255,0.8)'
              }}
            />
          </Form.Item>

          <Form.Item
            label="🔒 密码"
            name="password"
            rules={[
              { required: true, message: '请输入密码' },
              { min: 6, message: '密码至少6个字符' }
            ]}
          >
            <Input.Password
              prefix={<LockOutlined style={{ color: '#9ca3af', fontSize: '16px' }} />}
              placeholder="请输入您的密码"
              style={{ 
                borderRadius: '12px', 
                height: '52px',
                border: '2px solid #e5e7eb',
                fontSize: '15px',
                transition: 'all 0.3s ease',
                background: 'rgba(255,255,255,0.8)'
              }}
            />
          </Form.Item>

          <Form.Item
            label="🔐 确认密码"
            name="confirmPassword"
            dependencies={['password']}
            rules={[
              { required: true, message: '请确认密码' },
              ({ getFieldValue }) => ({
                validator(_, value) {
                  if (!value || getFieldValue('password') === value) {
                    return Promise.resolve();
                  }
                  return Promise.reject(new Error('两次输入的密码不一致'));
                },
              }),
            ]}
          >
            <Input.Password
              prefix={<LockOutlined style={{ color: '#9ca3af', fontSize: '16px' }} />}
              placeholder="请再次输入密码"
              style={{ 
                borderRadius: '12px', 
                height: '52px',
                border: '2px solid #e5e7eb',
                fontSize: '15px',
                transition: 'all 0.3s ease',
                background: 'rgba(255,255,255,0.8)'
              }}
            />
          </Form.Item>

          <Form.Item style={{ marginBottom: '24px' }}>
            <div style={{ display: 'flex', alignItems: 'flex-start', gap: '12px' }}>
              <input 
                type="checkbox" 
                required
                style={{ 
                  width: '18px', 
                  height: '18px',
                  accentColor: '#f093fb',
                  marginTop: '2px'
                }} 
              />
              <Text style={{ fontSize: '14px', color: '#6b7280', lineHeight: '1.5' }}>
                我已阅读并同意{' '}
                <Button type="link" style={{ 
                  padding: 0, 
                  fontSize: '14px',
                  color: '#f093fb',
                  fontWeight: 500,
                  height: 'auto'
                }}>
                  用户协议
                </Button>
                {' '}和{' '}
                <Button type="link" style={{ 
                  padding: 0, 
                  fontSize: '14px',
                  color: '#f5576c',
                  fontWeight: 500,
                  height: 'auto'
                }}>
                  隐私政策
                </Button>
              </Text>
            </div>
          </Form.Item>

          <Form.Item>
            <Button
              type="primary"
              htmlType="submit"
              loading={loading}
              style={{
                width: '100%',
                height: '52px',
                borderRadius: '12px',
                background: 'linear-gradient(135deg, #f093fb 0%, #f5576c 50%, #4facfe 100%)',
                border: 'none',
                fontSize: '16px',
                fontWeight: '600',
                boxShadow: '0 8px 24px rgba(240, 147, 251, 0.4)',
                transition: 'all 0.3s ease',
                position: 'relative',
                overflow: 'hidden'
              }}
            >
              <span style={{ position: 'relative', zIndex: 1 }}>🎉 创建账户</span>
            </Button>
          </Form.Item>
        </Form>

        <div style={{ textAlign: 'center' }}>
          <Text style={{ 
            fontSize: '15px',
            color: '#6b7280'
          }}>
            已有账户？{' '}
            <Button 
              type="link" 
              onClick={() => navigate('/login')} 
              style={{ 
                padding: 0,
                fontSize: '15px',
                fontWeight: 600,
                background: 'linear-gradient(135deg, #f093fb 0%, #f5576c 50%, #4facfe 100%)',
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent'
              }}
            >
              立即登录 →
            </Button>
          </Text>
        </div>
      </Card>
    </div>
  );
};

export default RegisterPage;