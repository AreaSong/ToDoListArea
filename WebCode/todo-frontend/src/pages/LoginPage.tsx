import React, { useState, useEffect } from 'react';
import { Form, Input, Button, Card, Typography, App, Checkbox } from 'antd';
import { LockOutlined, MailOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import type { UserLoginDto } from '../types/api';
// import { EnhancedInput, EnhancedForm } from '../components/EnhancedForm';
// import { LoadingSpinner } from '../components/LoadingSpinner';
// import { feedback } from '../utils/feedbackManager';

const { Title, Text } = Typography;

const LoginPage: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const [rememberMe, setRememberMe] = useState(false);
  const [form] = Form.useForm();
  const navigate = useNavigate();
  const { message: messageApi } = App.useApp();
  const { login } = useAuth();

  // 组件挂载时检查是否有保存的登录信息
  useEffect(() => {
    const savedCredentials = localStorage.getItem('rememberedCredentials');
    if (savedCredentials) {
      try {
        const { email, password, remember } = JSON.parse(savedCredentials);
        form.setFieldsValue({ email, password });
        setRememberMe(remember);
      } catch (error) {
        console.error('解析保存的登录信息失败:', error);
        localStorage.removeItem('rememberedCredentials');
      }
    }
  }, [form]);

  const onFinish = async (values: UserLoginDto) => {
    setLoading(true);
    messageApi.loading('正在登录...', 0);

    try {
      // 使用AuthContext的login方法
      await login(values);

      // 处理"记住我"功能
      if (rememberMe) {
        // 保存登录信息到localStorage
        const credentialsToSave = {
          email: values.email,
          password: values.password,
          remember: true
        };
        localStorage.setItem('rememberedCredentials', JSON.stringify(credentialsToSave));
      } else {
        // 如果未选择记住我，清除保存的信息
        localStorage.removeItem('rememberedCredentials');
      }

      messageApi.destroy();
      messageApi.success('登录成功！');
      navigate('/dashboard');
    } catch (error: any) {
      console.error('Login error:', error);
      messageApi.destroy();

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
      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 50%, #f093fb 100%)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      padding: '20px',
      position: 'relative',
      overflow: 'hidden'
    }}>
      {/* 动态背景粒子效果 */}
      <div style={{
        position: 'absolute',
        top: 0,
        left: 0,
        width: '100%',
        height: '100%',
        background: `
          radial-gradient(circle at 20% 80%, rgba(120, 119, 198, 0.3) 0%, transparent 50%),
          radial-gradient(circle at 80% 20%, rgba(255, 119, 198, 0.3) 0%, transparent 50%),
          radial-gradient(circle at 40% 40%, rgba(120, 219, 255, 0.3) 0%, transparent 50%)
        `,
        animation: 'gradientShift 8s ease-in-out infinite'
      }} />

      {/* 浮动装饰元素 */}
      <div style={{
        position: 'absolute',
        top: '15%',
        left: '10%',
        width: '120px',
        height: '120px',
        borderRadius: '50%',
        background: 'linear-gradient(135deg, rgba(255, 255, 255, 0.2), rgba(255, 255, 255, 0.05))',
        filter: 'blur(1px)',
        animation: 'float 6s ease-in-out infinite',
        boxShadow: '0 8px 32px rgba(255, 255, 255, 0.1)'
      }} />
      
      <div style={{
        position: 'absolute',
        top: '60%',
        right: '15%',
        width: '80px',
        height: '80px',
        borderRadius: '30%',
        background: 'linear-gradient(45deg, rgba(255, 255, 255, 0.15), rgba(255, 255, 255, 0.05))',
        animation: 'float 4s ease-in-out infinite reverse',
        transform: 'rotate(45deg)'
      }} />

      <div style={{
        position: 'absolute',
        bottom: '20%',
        left: '20%',
        width: '60px',
        height: '60px',
        borderRadius: '50%',
        background: 'linear-gradient(135deg, rgba(255, 255, 255, 0.25), rgba(255, 255, 255, 0.1))',
        animation: 'float 5s ease-in-out infinite',
        filter: 'blur(0.5px)'
      }} />

      {/* 几何装饰元素 */}
      <div style={{
        position: 'absolute',
        top: '25%',
        right: '25%',
        width: '40px',
        height: '40px',
        background: 'linear-gradient(45deg, rgba(255, 255, 255, 0.2), transparent)',
        transform: 'rotate(45deg)',
        animation: 'spin 20s linear infinite'
      }} />

      <div style={{
        position: 'absolute',
        bottom: '30%',
        right: '10%',
        width: '0',
        height: '0',
        borderLeft: '25px solid transparent',
        borderRight: '25px solid transparent',
        borderBottom: '43px solid rgba(255, 255, 255, 0.1)',
        animation: 'float 7s ease-in-out infinite reverse'
      }} />
      
      <Card
        style={{
          width: '100%',
          maxWidth: '450px',
          borderRadius: '24px',
          boxShadow: '0 32px 64px rgba(0, 0, 0, 0.2), 0 16px 32px rgba(0, 0, 0, 0.1)',
          border: '1px solid rgba(255, 255, 255, 0.2)',
          backdropFilter: 'blur(40px)',
          background: 'linear-gradient(135deg, rgba(255, 255, 255, 0.95) 0%, rgba(255, 255, 255, 0.85) 100%)',
          position: 'relative',
          overflow: 'hidden'
        }}
        bodyStyle={{ padding: '48px 40px' }}
      >
        {/* 卡片内部装饰 */}
        <div style={{
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0,
          height: '4px',
          background: 'linear-gradient(90deg, #667eea 0%, #764ba2 50%, #f093fb 100%)',
          animation: 'shimmer 3s ease-in-out infinite'
        }} />

        <div style={{
          position: 'absolute',
          top: '-50%',
          right: '-50%',
          width: '100%',
          height: '100%',
          background: 'linear-gradient(45deg, transparent 30%, rgba(255,255,255,0.1) 50%, transparent 70%)',
          transform: 'rotate(45deg)',
          animation: 'cardShimmer 4s ease-in-out infinite'
        }} />

        <div style={{ textAlign: 'center', marginBottom: '40px' }}>
          <div style={{
            width: '100px',
            height: '100px',
            borderRadius: '24px',
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 50%, #f093fb 100%)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            margin: '0 auto 24px',
            boxShadow: '0 16px 40px rgba(102, 126, 234, 0.4)',
            position: 'relative',
            overflow: 'hidden'
          }}>
            <div style={{
              position: 'absolute',
              top: '-50%',
              left: '-50%',
              width: '200%',
              height: '200%',
              background: 'linear-gradient(45deg, transparent 30%, rgba(255,255,255,0.3) 50%, transparent 70%)',
              transform: 'rotate(45deg)',
              animation: 'logoShimmer 2s ease-in-out infinite'
            }} />
            <span style={{ 
              color: 'white', 
              fontSize: '40px', 
              fontWeight: 'bold',
              textShadow: '0 2px 8px rgba(0,0,0,0.3)',
              zIndex: 1,
              position: 'relative'
            }}>T</span>
          </div>
          
          <Title level={2} style={{ 
            margin: 0, 
            marginBottom: '8px',
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            WebkitBackgroundClip: 'text',
            WebkitTextFillColor: 'transparent',
            fontSize: '28px',
            fontWeight: 700
          }}>智能提醒事项</Title>
          
          <Text style={{ 
            fontSize: '16px',
            color: '#64748b',
            fontWeight: 500,
            display: 'block',
            marginBottom: '8px'
          }}>登录您的账户，开始高效管理</Text>
          
          <div style={{
            display: 'flex',
            justifyContent: 'center',
            gap: '8px',
            marginTop: '16px'
          }}>
            <div style={{
              width: '8px',
              height: '8px',
              borderRadius: '50%',
              background: '#667eea',
              animation: 'pulse 2s ease-in-out infinite'
            }} />
            <div style={{
              width: '8px',
              height: '8px',
              borderRadius: '50%',
              background: '#764ba2',
              animation: 'pulse 2s ease-in-out infinite 0.3s'
            }} />
            <div style={{
              width: '8px',
              height: '8px',
              borderRadius: '50%',
              background: '#f093fb',
              animation: 'pulse 2s ease-in-out infinite 0.6s'
            }} />
          </div>
        </div>

        <Form
          form={form}
          name="login"
          onFinish={onFinish}
          autoComplete="off"
          layout="vertical"
          size="large"
        >
          <Form.Item
            label={
              <span style={{ 
                fontWeight: 600, 
                color: '#374151',
                fontSize: '14px',
                display: 'flex',
                alignItems: 'center',
                gap: '6px'
              }}>
                📧 邮箱地址
              </span>
            }
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
              onFocus={(e) => {
                const target = e.target as HTMLInputElement;
                target.style.borderColor = '#667eea';
                target.style.boxShadow = '0 0 0 3px rgba(102, 126, 234, 0.1)';
                target.style.transform = 'translateY(-1px)';
              }}
              onBlur={(e) => {
                const target = e.target as HTMLInputElement;
                target.style.borderColor = '#e5e7eb';
                target.style.boxShadow = 'none';
                target.style.transform = 'translateY(0)';
              }}
            />
          </Form.Item>

          <Form.Item
            label={
              <span style={{ 
                fontWeight: 600, 
                color: '#374151',
                fontSize: '14px',
                display: 'flex',
                alignItems: 'center',
                gap: '6px'
              }}>
                🔒 密码
              </span>
            }
            name="password"
            rules={[{ required: true, message: '请输入密码' }]}
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
              onFocus={(e) => {
                const target = e.target as HTMLInputElement;
                target.style.borderColor = '#667eea';
                target.style.boxShadow = '0 0 0 3px rgba(102, 126, 234, 0.1)';
                target.style.transform = 'translateY(-1px)';
              }}
              onBlur={(e) => {
                const target = e.target as HTMLInputElement;
                target.style.borderColor = '#e5e7eb';
                target.style.boxShadow = 'none';
                target.style.transform = 'translateY(0)';
              }}
            />
          </Form.Item>

          <Form.Item style={{ marginBottom: '24px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Checkbox
                checked={rememberMe}
                onChange={(e) => setRememberMe(e.target.checked)}
                style={{
                  fontSize: '14px',
                  color: '#6b7280'
                }}
              >
                <span style={{ fontSize: '14px', color: '#6b7280', userSelect: 'none' }}>
                  记住我
                </span>
              </Checkbox>
              <Button type="link" style={{ 
                padding: 0, 
                fontSize: '14px',
                color: '#667eea',
                fontWeight: 500
              }}>
                忘记密码？
              </Button>
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
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 50%, #f093fb 100%)',
                border: 'none',
                fontSize: '16px',
                fontWeight: '600',
                boxShadow: '0 8px 24px rgba(102, 126, 234, 0.3)',
                transition: 'all 0.3s ease',
                position: 'relative',
                overflow: 'hidden'
              }}
              onMouseEnter={(e) => {
                const target = e.target as HTMLButtonElement;
                target.style.transform = 'translateY(-2px)';
                target.style.boxShadow = '0 12px 32px rgba(102, 126, 234, 0.4)';
              }}
              onMouseLeave={(e) => {
                const target = e.target as HTMLButtonElement;
                target.style.transform = 'translateY(0)';
                target.style.boxShadow = '0 8px 24px rgba(102, 126, 234, 0.3)';
              }}
            >
              <span style={{ position: 'relative', zIndex: 1 }}>🚀 立即登录</span>
            </Button>
          </Form.Item>
        </Form>

        <div style={{ 
          textAlign: 'center', 
          margin: '32px 0 24px',
          position: 'relative'
        }}>
          <div style={{
            height: '1px',
            background: 'linear-gradient(90deg, transparent, #e5e7eb, transparent)',
            position: 'relative'
          }}>
            <span style={{
              position: 'absolute',
              top: '50%',
              left: '50%',
              transform: 'translate(-50%, -50%)',
              background: 'white',
              padding: '0 16px',
              color: '#9ca3af',
              fontSize: '14px',
              fontWeight: 500
            }}>或者</span>
          </div>
        </div>

        {/* 社交登录按钮 */}
        <div style={{ 
          display: 'grid', 
          gridTemplateColumns: '1fr 1fr', 
          gap: '12px',
          marginBottom: '24px'
        }}>
          <Button style={{
            height: '48px',
            borderRadius: '12px',
            border: '2px solid #e5e7eb',
            background: 'rgba(255,255,255,0.8)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            gap: '8px',
            fontWeight: 500,
            transition: 'all 0.3s ease'
          }}
          onMouseEnter={(e) => {
            const target = e.target as HTMLButtonElement;
            target.style.borderColor = '#667eea';
            target.style.transform = 'translateY(-1px)';
            target.style.boxShadow = '0 4px 12px rgba(102, 126, 234, 0.15)';
          }}
          onMouseLeave={(e) => {
            const target = e.target as HTMLButtonElement;
            target.style.borderColor = '#e5e7eb';
            target.style.transform = 'translateY(0)';
            target.style.boxShadow = 'none';
          }}>
            <span style={{ fontSize: '18px' }}>🔍</span>
            Google
          </Button>
          
          <Button style={{
            height: '48px',
            borderRadius: '12px',
            border: '2px solid #e5e7eb',
            background: 'rgba(255,255,255,0.8)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            gap: '8px',
            fontWeight: 500,
            transition: 'all 0.3s ease'
          }}
          onMouseEnter={(e) => {
            const target = e.target as HTMLButtonElement;
            target.style.borderColor = '#667eea';
            target.style.transform = 'translateY(-1px)';
            target.style.boxShadow = '0 4px 12px rgba(102, 126, 234, 0.15)';
          }}
          onMouseLeave={(e) => {
            const target = e.target as HTMLButtonElement;
            target.style.borderColor = '#e5e7eb';
            target.style.transform = 'translateY(0)';
            target.style.boxShadow = 'none';
          }}>
            <span style={{ fontSize: '18px' }}>📱</span>
            微信
          </Button>
        </div>

        <div style={{ textAlign: 'center' }}>
          <Text style={{ 
            fontSize: '15px',
            color: '#6b7280'
          }}>
            还没有账户？{' '}
            <Button 
              type="link" 
              onClick={() => navigate('/register')} 
              style={{ 
                padding: 0,
                fontSize: '15px',
                fontWeight: 600,
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent'
              }}
            >
              立即注册 →
            </Button>
          </Text>
        </div>

        {/* 底部装饰 */}
        <div style={{
          position: 'absolute',
          bottom: 0,
          left: 0,
          right: 0,
          height: '4px',
          background: 'linear-gradient(90deg, #667eea 0%, #764ba2 50%, #f093fb 100%)',
          opacity: 0.6
        }} />
      </Card>

      <style>{`
        @keyframes float {
          0%, 100% { transform: translateY(0px) rotate(0deg); }
          50% { transform: translateY(-20px) rotate(5deg); }
        }
        
        @keyframes spin {
          from { transform: rotate(0deg); }
          to { transform: rotate(360deg); }
        }
        
        @keyframes gradientShift {
          0%, 100% { opacity: 1; }
          50% { opacity: 0.8; }
        }
        
        @keyframes shimmer {
          0% { background-position: -200% 0; }
          100% { background-position: 200% 0; }
        }
        
        @keyframes cardShimmer {
          0% { transform: translateX(-100%) rotate(45deg); }
          100% { transform: translateX(200%) rotate(45deg); }
        }
        
        @keyframes logoShimmer {
          0% { transform: translateX(-100%) rotate(45deg); }
          100% { transform: translateX(200%) rotate(45deg); }
        }
        
        @keyframes pulse {
          0%, 100% { opacity: 1; transform: scale(1); }
          50% { opacity: 0.5; transform: scale(1.2); }
        }
      `}</style>
    </div>
  );
};

export default LoginPage;
