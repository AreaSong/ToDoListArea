import React, { useState, useEffect } from 'react';
import { 
  Layout, 
  Card, 
  Form, 
  Input, 
  Button, 
  Avatar, 
  Typography, 
  Space, 
  message,
  Divider,
  Upload,
  type UploadProps
} from 'antd';
import { 
  UserOutlined, 
  MailOutlined, 
  PhoneOutlined, 
  ArrowLeftOutlined,
  CameraOutlined
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { userApi } from '../services/api';
import type { User } from '../types/api';

const { Header, Content } = Layout;
const { Title, Text } = Typography;

const ProfilePage: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const [user, setUser] = useState<User | null>(null);
  const [form] = Form.useForm();
  const navigate = useNavigate();

  // 获取用户信息
  const fetchUserProfile = async () => {
    const currentUser = JSON.parse(localStorage.getItem('user') || '{}');
    if (!currentUser.id) return;

    try {
      const response = await userApi.getProfile(currentUser.id);
      if (response.success && response.data) {
        setUser(response.data);
        form.setFieldsValue({
          name: response.data.name,
          email: response.data.email,
          phone: response.data.phone,
        });
      }
    } catch (error) {
      message.error('获取用户信息失败');
    }
  };

  // 更新用户信息
  const handleSubmit = async (values: any) => {
    if (!user) return;

    setLoading(true);
    try {
      const response = await userApi.updateProfile(user.id, {
        name: values.name,
        phone: values.phone,
      });

      if (response.success && response.data) {
        // 更新本地存储的用户信息
        localStorage.setItem('user', JSON.stringify(response.data));
        setUser(response.data);
        message.success('个人信息更新成功');
      }
    } catch (error: any) {
      message.error(error.response?.data?.message || '更新失败');
    } finally {
      setLoading(false);
    }
  };

  // 头像上传配置
  const uploadProps: UploadProps = {
    name: 'avatar',
    listType: 'picture-card',
    className: 'avatar-uploader',
    showUploadList: false,
    beforeUpload: (file) => {
      const isJpgOrPng = file.type === 'image/jpeg' || file.type === 'image/png';
      if (!isJpgOrPng) {
        message.error('只能上传 JPG/PNG 格式的图片!');
      }
      const isLt2M = file.size / 1024 / 1024 < 2;
      if (!isLt2M) {
        message.error('图片大小不能超过 2MB!');
      }
      return isJpgOrPng && isLt2M;
    },
    onChange: (info) => {
      if (info.file.status === 'done') {
        message.success('头像上传成功');
        // 这里应该处理头像上传成功后的逻辑
      } else if (info.file.status === 'error') {
        message.error('头像上传失败');
      }
    },
  };

  useEffect(() => {
    fetchUserProfile();
  }, []);

  if (!user) {
    return <div>加载中...</div>;
  }

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Header style={{ 
        background: '#fff', 
        padding: '0 24px', 
        display: 'flex', 
        alignItems: 'center',
        boxShadow: '0 2px 8px rgba(0,0,0,0.1)'
      }}>
        <Button 
          type="text" 
          icon={<ArrowLeftOutlined />} 
          onClick={() => navigate('/dashboard')}
          style={{ marginRight: 16 }}
        >
          返回
        </Button>
        <Title level={3} style={{ margin: 0, color: '#1890ff' }}>
          个人设置
        </Title>
      </Header>

      <Content style={{ padding: '24px' }}>
        <div style={{ maxWidth: 600, margin: '0 auto' }}>
          <Card>
            {/* 头像部分 */}
            <div style={{ textAlign: 'center', marginBottom: 32 }}>
              <div style={{ position: 'relative', display: 'inline-block' }}>
                <Avatar 
                  size={100} 
                  src={user.avatarUrl} 
                  icon={<UserOutlined />}
                  style={{ marginBottom: 16 }}
                />
                <Upload {...uploadProps}>
                  <Button 
                    type="primary" 
                    shape="circle" 
                    icon={<CameraOutlined />}
                    size="small"
                    style={{ 
                      position: 'absolute', 
                      bottom: 16, 
                      right: -8,
                      border: '2px solid #fff'
                    }}
                  />
                </Upload>
              </div>
              <div>
                <Title level={4} style={{ marginBottom: 4 }}>
                  {user.name}
                </Title>
                <Text type="secondary">{user.email}</Text>
              </div>
            </div>

            <Divider />

            {/* 基本信息表单 */}
            <Title level={5} style={{ marginBottom: 16 }}>
              基本信息
            </Title>

            <Form
              form={form}
              layout="vertical"
              onFinish={handleSubmit}
            >
              <Form.Item
                name="name"
                label="用户名"
                rules={[
                  { required: true, message: '请输入用户名' },
                  { min: 2, message: '用户名长度不能少于2位' },
                  { max: 50, message: '用户名长度不能超过50位' }
                ]}
              >
                <Input 
                  prefix={<UserOutlined />} 
                  placeholder="请输入用户名" 
                />
              </Form.Item>

              <Form.Item
                name="email"
                label="邮箱地址"
              >
                <Input 
                  prefix={<MailOutlined />} 
                  disabled
                  placeholder="邮箱地址不可修改"
                />
              </Form.Item>

              <Form.Item
                name="phone"
                label="手机号码"
                rules={[
                  { pattern: /^1[3-9]\d{9}$/, message: '请输入有效的手机号码' }
                ]}
              >
                <Input 
                  prefix={<PhoneOutlined />} 
                  placeholder="请输入手机号码" 
                />
              </Form.Item>

              <Form.Item>
                <Button 
                  type="primary" 
                  htmlType="submit" 
                  loading={loading}
                  block
                >
                  保存修改
                </Button>
              </Form.Item>
            </Form>

            <Divider />

            {/* 账户信息 */}
            <Title level={5} style={{ marginBottom: 16 }}>
              账户信息
            </Title>

            <Space direction="vertical" style={{ width: '100%' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <Text>账户状态：</Text>
                <Text strong style={{ color: user.status === 'Active' ? '#52c41a' : '#ff4d4f' }}>
                  {user.status === 'Active' ? '正常' : '异常'}
                </Text>
              </div>

              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <Text>邮箱验证：</Text>
                <Text strong style={{ color: user.emailVerified ? '#52c41a' : '#ff4d4f' }}>
                  {user.emailVerified ? '已验证' : '未验证'}
                </Text>
              </div>

              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <Text>手机验证：</Text>
                <Text strong style={{ color: user.phoneVerified ? '#52c41a' : '#ff4d4f' }}>
                  {user.phoneVerified ? '已验证' : '未验证'}
                </Text>
              </div>

              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <Text>注册时间：</Text>
                <Text>{new Date(user.createdAt).toLocaleDateString()}</Text>
              </div>

              {user.lastLoginAt && (
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Text>最后登录：</Text>
                  <Text>{new Date(user.lastLoginAt).toLocaleString()}</Text>
                </div>
              )}
            </Space>
          </Card>
        </div>
      </Content>
    </Layout>
  );
};

export default ProfilePage;
