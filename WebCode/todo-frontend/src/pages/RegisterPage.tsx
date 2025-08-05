import React, { useState, useCallback, useRef } from 'react';
import { Form, Input, Button, Card, Typography, message, Divider } from 'antd';
import { UserOutlined, LockOutlined, MailOutlined, PhoneOutlined, SafetyOutlined } from '@ant-design/icons';
import { Link, useNavigate } from 'react-router-dom';
import { userApi, invitationCodeApi } from '../services/api';
import type { UserRegisterDto } from '../types/api';

const { Title, Text } = Typography;

const RegisterPage: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const [invitationCodeValidating, setInvitationCodeValidating] = useState(false);
  const [invitationCodeStatus, setInvitationCodeStatus] = useState<'success' | 'error' | ''>('');
  const [invitationCodeMessage, setInvitationCodeMessage] = useState('');
  const navigate = useNavigate();
  const debounceTimerRef = useRef<NodeJS.Timeout | null>(null);

  // 验证邀请码
  const validateInvitationCode = useCallback(async (code: string) => {
    if (!code || code.length < 6) {
      setInvitationCodeStatus('');
      setInvitationCodeMessage('');
      return;
    }

    setInvitationCodeValidating(true);
    try {
      const response = await invitationCodeApi.validate({ code });

      if (response.success && response.data?.isValid) {
        setInvitationCodeStatus('success');
        setInvitationCodeMessage('邀请码有效');
      } else {
        setInvitationCodeStatus('error');
        setInvitationCodeMessage(response.data?.message || '邀请码无效');
      }
    } catch (error: any) {
      setInvitationCodeStatus('error');
      setInvitationCodeMessage('验证邀请码失败，请检查网络连接');
    } finally {
      setInvitationCodeValidating(false);
    }
  }, []);

  // 防抖验证邀请码
  const debouncedValidateInvitationCode = useCallback((code: string) => {
    if (debounceTimerRef.current) {
      clearTimeout(debounceTimerRef.current);
    }

    debounceTimerRef.current = setTimeout(() => {
      validateInvitationCode(code);
    }, 500);
  }, [validateInvitationCode]);

  const onFinish = async (values: UserRegisterDto & { confirmPassword: string }) => {
    setLoading(true);
    try {
      // 移除确认密码字段
      const { confirmPassword, ...registerData } = values;
      
      const response = await userApi.register(registerData);
      
      if (response.success) {
        message.success('注册成功！请登录您的账户');
        navigate('/login');
      } else {
        message.error(response.message || '注册失败');
      }
    } catch (error: any) {
      console.error('Register error:', error);
      message.error(error.response?.data?.message || '注册失败，请检查网络连接');
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
          <Text type="secondary">创建您的账户</Text>
        </div>

        <Form
          name="register"
          onFinish={onFinish}
          autoComplete="off"
          size="large"
        >
          <Form.Item
            name="name"
            rules={[
              { required: true, message: '请输入用户名!' },
              { min: 2, message: '用户名长度不能少于2位!' },
              { max: 50, message: '用户名长度不能超过50位!' }
            ]}
          >
            <Input 
              prefix={<UserOutlined />} 
              placeholder="用户名" 
            />
          </Form.Item>

          <Form.Item
            name="email"
            rules={[
              { required: true, message: '请输入邮箱地址!' },
              { type: 'email', message: '请输入有效的邮箱地址!' }
            ]}
          >
            <Input 
              prefix={<MailOutlined />} 
              placeholder="邮箱地址" 
            />
          </Form.Item>

          <Form.Item
            name="phone"
            rules={[
              { pattern: /^1[3-9]\d{9}$/, message: '请输入有效的手机号码!' }
            ]}
          >
            <Input
              prefix={<PhoneOutlined />}
              placeholder="手机号码（可选）"
            />
          </Form.Item>

          <Form.Item
            name="invitationCode"
            rules={[
              { required: true, message: '请输入邀请码!' },
              { min: 6, message: '邀请码长度不能少于6位!' },
              { max: 32, message: '邀请码长度不能超过32位!' }
            ]}
            validateStatus={invitationCodeStatus}
            help={invitationCodeMessage}
            hasFeedback
          >
            <Input
              prefix={<SafetyOutlined />}
              placeholder="邀请码"
              onChange={(e) => {
                const value = e.target.value.trim();
                if (value) {
                  debouncedValidateInvitationCode(value);
                } else {
                  setInvitationCodeStatus('');
                  setInvitationCodeMessage('');
                }
              }}
              suffix={invitationCodeValidating ? <span>验证中...</span> : null}
            />
          </Form.Item>

          <Form.Item
            name="password"
            rules={[
              { required: true, message: '请输入密码!' },
              { min: 6, message: '密码长度不能少于6位!' }
            ]}
          >
            <Input.Password 
              prefix={<LockOutlined />} 
              placeholder="密码" 
            />
          </Form.Item>

          <Form.Item
            name="confirmPassword"
            dependencies={['password']}
            rules={[
              { required: true, message: '请确认密码!' },
              ({ getFieldValue }) => ({
                validator(_, value) {
                  if (!value || getFieldValue('password') === value) {
                    return Promise.resolve();
                  }
                  return Promise.reject(new Error('两次输入的密码不一致!'));
                },
              }),
            ]}
          >
            <Input.Password 
              prefix={<LockOutlined />} 
              placeholder="确认密码" 
            />
          </Form.Item>

          <Form.Item>
            <Button 
              type="primary" 
              htmlType="submit" 
              loading={loading}
              block
              style={{ height: 40 }}
            >
              注册
            </Button>
          </Form.Item>
        </Form>

        <Divider>或</Divider>

        <div style={{ textAlign: 'center' }}>
          <Text type="secondary">
            已有账户？ 
            <Link to="/login" style={{ marginLeft: 4 }}>
              立即登录
            </Link>
          </Text>
        </div>
      </Card>
    </div>
  );
};

export default RegisterPage;
