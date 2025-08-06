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
  Select,
  Radio,
  Checkbox,
  ColorPicker,

  type UploadProps
} from 'antd';
import {
  UserOutlined,
  ArrowLeftOutlined,
  CameraOutlined,
  SettingOutlined,
  GlobalOutlined,
  BellOutlined,
  BgColorsOutlined
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { userApi, userProfileApi } from '../services/api';
import type { User, UserProfileDetail, UserProfileUpdate } from '../types/api';
import { useTheme } from '../contexts/ThemeContext';


const { Header, Content } = Layout;
const { Title, Text } = Typography;
const { Option } = Select;

const ProfilePage: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [user, setUser] = useState<User | null>(null);
  const [userProfile, setUserProfile] = useState<UserProfileDetail | null>(null);
  const [form] = Form.useForm();
  const navigate = useNavigate();
  const { updateTheme, applyTheme, themePreferences, isDarkMode } = useTheme();

  // 获取用户基本信息
  const fetchUserInfo = async () => {
    const currentUser = JSON.parse(localStorage.getItem('user') || '{}');
    if (!currentUser.id) return;

    try {
      const response = await userApi.getProfile(currentUser.id);
      if (response.success && response.data) {
        setUser(response.data);
      }
    } catch (error) {
      message.error('获取用户基本信息失败');
    }
  };

  // 获取用户详细资料
  const fetchUserProfile = async () => {
    const currentUser = JSON.parse(localStorage.getItem('user') || '{}');
    if (!currentUser.id) return;

    setLoading(true);
    try {
      const response = await userProfileApi.getProfile(currentUser.id);
      if (response.success && response.data) {
        setUserProfile(response.data);

        // 应用主题设置到界面
        applyTheme(response.data.themePreferences);

        form.setFieldsValue({
          firstName: response.data.firstName,
          lastName: response.data.lastName,
          timezone: response.data.timezone,
          language: response.data.language,
          dateFormat: response.data.dateFormat,
          timeFormat: response.data.timeFormat,
          'notificationPreferences.email': response.data.notificationPreferences.email,
          'notificationPreferences.push': response.data.notificationPreferences.push,
          'notificationPreferences.desktop': response.data.notificationPreferences.desktop,
          'notificationPreferences.quietHours.start': response.data.notificationPreferences.quietHours.start,
          'notificationPreferences.quietHours.end': response.data.notificationPreferences.quietHours.end,
          'themePreferences.theme': response.data.themePreferences.theme,
          'themePreferences.primaryColor': response.data.themePreferences.primaryColor,
          'themePreferences.compactMode': response.data.themePreferences.compactMode,
        });

        message.success('用户资料加载完成', 1);
      }
    } catch (error) {
      message.error('获取用户详细资料失败');
    } finally {
      setLoading(false);
    }
  };

  // 更新用户详细资料
  const handleSubmit = async (values: any) => {
    if (!user || !userProfile) return;

    setSaving(true);
    try {
      const updateData: UserProfileUpdate = {
        firstName: values.firstName,
        lastName: values.lastName,
        timezone: values.timezone,
        language: values.language,
        dateFormat: values.dateFormat,
        timeFormat: values.timeFormat,
        notificationPreferences: {
          email: values.notificationPreferences?.email ?? true,
          push: values.notificationPreferences?.push ?? true,
          desktop: values.notificationPreferences?.desktop ?? true,
          quietHours: {
            start: values.notificationPreferences?.quietHours?.start ?? '22:00',
            end: values.notificationPreferences?.quietHours?.end ?? '08:00',
          }
        },
        themePreferences: {
          theme: values.themePreferences?.theme ?? 'light',
          primaryColor: values.themePreferences?.primaryColor ?? '#1890ff',
          compactMode: values.themePreferences?.compactMode ?? false,
        }
      };

      const response = await userProfileApi.updateProfile(user.id, updateData);

      if (response.success && response.data) {
        setUserProfile(response.data);

        // 立即应用新的主题设置
        applyTheme(response.data.themePreferences);

        // 显示详细的成功反馈
        message.success({
          content: '个人资料更新成功！主题设置已应用',
          duration: 3,
          style: {
            marginTop: '20vh',
          },
        });
      }
    } catch (error: any) {
      message.error(error.response?.data?.message || '更新失败');
    } finally {
      setSaving(false);
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
    fetchUserInfo();
    fetchUserProfile();
  }, []);

  if (loading || !user || !userProfile) {
    return (
      <Layout style={{ minHeight: '100vh' }}>
        <Content style={{ display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
          <div>加载中...</div>
        </Content>
      </Layout>
    );
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
          个人资料设置
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

            {/* 个人资料表单 */}
            <Form
              form={form}
              layout="vertical"
              onFinish={handleSubmit}
            >
              {/* 基本信息 */}
              <Title level={5} style={{ marginBottom: 16 }}>
                <UserOutlined style={{ marginRight: 8 }} />
                基本信息
              </Title>

              <div style={{ display: 'flex', gap: 16 }}>
                <Form.Item
                  name="firstName"
                  label="姓"
                  style={{ flex: 1 }}
                  rules={[{ max: 50, message: '姓的长度不能超过50个字符' }]}
                >
                  <Input placeholder="请输入姓" />
                </Form.Item>

                <Form.Item
                  name="lastName"
                  label="名"
                  style={{ flex: 1 }}
                  rules={[{ max: 50, message: '名的长度不能超过50个字符' }]}
                >
                  <Input placeholder="请输入名" />
                </Form.Item>
              </div>

              <Divider />

              {/* 地区和语言设置 */}
              <Title level={5} style={{ marginBottom: 16 }}>
                <GlobalOutlined style={{ marginRight: 8 }} />
                地区和语言
              </Title>

              <div style={{ display: 'flex', gap: 16 }}>
                <Form.Item
                  name="timezone"
                  label="时区"
                  style={{ flex: 1 }}
                  rules={[{ required: true, message: '请选择时区' }]}
                >
                  <Select placeholder="请选择时区">
                    <Option value="Asia/Shanghai">北京时间 (UTC+8)</Option>
                    <Option value="UTC">协调世界时 (UTC)</Option>
                    <Option value="America/New_York">纽约时间 (UTC-5)</Option>
                    <Option value="Europe/London">伦敦时间 (UTC+0)</Option>
                    <Option value="Asia/Tokyo">东京时间 (UTC+9)</Option>
                  </Select>
                </Form.Item>

                <Form.Item
                  name="language"
                  label="语言"
                  style={{ flex: 1 }}
                  rules={[{ required: true, message: '请选择语言' }]}
                >
                  <Select placeholder="请选择语言">
                    <Option value="zh-CN">简体中文</Option>
                    <Option value="en-US">English</Option>
                  </Select>
                </Form.Item>
              </div>

              <div style={{ display: 'flex', gap: 16 }}>
                <Form.Item
                  name="dateFormat"
                  label="日期格式"
                  style={{ flex: 1 }}
                  rules={[{ required: true, message: '请选择日期格式' }]}
                >
                  <Radio.Group>
                    <Radio value="YYYY-MM-DD">2025-01-01</Radio>
                    <Radio value="MM/DD/YYYY">01/01/2025</Radio>
                    <Radio value="DD/MM/YYYY">01/01/2025</Radio>
                  </Radio.Group>
                </Form.Item>

                <Form.Item
                  name="timeFormat"
                  label="时间格式"
                  style={{ flex: 1 }}
                  rules={[{ required: true, message: '请选择时间格式' }]}
                >
                  <Radio.Group>
                    <Radio value="24h">24小时制</Radio>
                    <Radio value="12h">12小时制</Radio>
                  </Radio.Group>
                </Form.Item>
              </div>

              <Divider />

            <Divider />

              {/* 通知设置 */}
              <Title level={5} style={{ marginBottom: 16 }}>
                <BellOutlined style={{ marginRight: 8 }} />
                通知设置
              </Title>

              <div style={{ display: 'flex', gap: 24, marginBottom: 16 }}>
                <Form.Item name={['notificationPreferences', 'email']} valuePropName="checked">
                  <Checkbox>邮件通知</Checkbox>
                </Form.Item>
                <Form.Item name={['notificationPreferences', 'push']} valuePropName="checked">
                  <Checkbox>推送通知</Checkbox>
                </Form.Item>
                <Form.Item name={['notificationPreferences', 'desktop']} valuePropName="checked">
                  <Checkbox>桌面通知</Checkbox>
                </Form.Item>
              </div>

              <div style={{ display: 'flex', gap: 16 }}>
                <Form.Item
                  name={['notificationPreferences', 'quietHours', 'start']}
                  label="免打扰开始时间"
                  style={{ flex: 1 }}
                  rules={[
                    { pattern: /^([01]?[0-9]|2[0-3]):[0-5][0-9]$/, message: '请输入正确的时间格式 (HH:mm)' }
                  ]}
                >
                  <Input
                    placeholder="22:00"
                    style={{ width: '100%' }}
                  />
                </Form.Item>

                <Form.Item
                  name={['notificationPreferences', 'quietHours', 'end']}
                  label="免打扰结束时间"
                  style={{ flex: 1 }}
                  rules={[
                    { pattern: /^([01]?[0-9]|2[0-3]):[0-5][0-9]$/, message: '请输入正确的时间格式 (HH:mm)' }
                  ]}
                >
                  <Input
                    placeholder="08:00"
                    style={{ width: '100%' }}
                  />
                </Form.Item>
              </div>

              <Divider />

              {/* 主题设置 */}
              <Title level={5} style={{ marginBottom: 16 }}>
                <BgColorsOutlined style={{ marginRight: 8 }} />
                主题设置
              </Title>

              {/* 当前主题状态显示 */}
              <div style={{
                padding: '12px 16px',
                backgroundColor: isDarkMode ? '#1f1f1f' : '#f5f5f5',
                borderRadius: '8px',
                marginBottom: '16px',
                border: `2px solid ${themePreferences.primaryColor}20`
              }}>
                <Text strong>当前主题状态：</Text>
                <div style={{ marginTop: '8px', display: 'flex', gap: '16px', flexWrap: 'wrap' }}>
                  <Text>
                    模式: <Text code>{themePreferences.theme === 'light' ? '浅色' : themePreferences.theme === 'dark' ? '深色' : '自动'}</Text>
                  </Text>
                  <Text>
                    主色调: <span style={{
                      display: 'inline-block',
                      width: '16px',
                      height: '16px',
                      backgroundColor: themePreferences.primaryColor,
                      borderRadius: '3px',
                      marginRight: '4px',
                      verticalAlign: 'middle'
                    }}></span>
                    <Text code>{themePreferences.primaryColor}</Text>
                  </Text>
                  <Text>
                    紧凑模式: <Text code>{themePreferences.compactMode ? '开启' : '关闭'}</Text>
                  </Text>
                </div>
              </div>

              {/* 主题预设 */}
              <div style={{ marginBottom: '16px' }}>
                <Text strong style={{ marginBottom: '8px', display: 'block' }}>快速主题预设：</Text>
                <div style={{ display: 'flex', gap: '8px', flexWrap: 'wrap' }}>
                  <Button
                    size="small"
                    onClick={() => {
                      const preset = { theme: 'light' as const, primaryColor: '#1890ff', compactMode: false };
                      updateTheme(preset);
                      form.setFieldsValue({ themePreferences: preset });
                      message.success('已应用默认浅色主题', 1);
                    }}
                  >
                    🌞 默认浅色
                  </Button>
                  <Button
                    size="small"
                    onClick={() => {
                      const preset = { theme: 'dark' as const, primaryColor: '#1890ff', compactMode: false };
                      updateTheme(preset);
                      form.setFieldsValue({ themePreferences: preset });
                      message.success('已应用默认深色主题', 1);
                    }}
                  >
                    🌙 默认深色
                  </Button>
                  <Button
                    size="small"
                    onClick={() => {
                      const preset = { theme: 'light' as const, primaryColor: '#52c41a', compactMode: true };
                      updateTheme(preset);
                      form.setFieldsValue({ themePreferences: preset });
                      message.success('已应用绿色紧凑主题', 1);
                    }}
                  >
                    🍃 绿色紧凑
                  </Button>
                  <Button
                    size="small"
                    onClick={() => {
                      const preset = { theme: 'dark' as const, primaryColor: '#722ed1', compactMode: false };
                      updateTheme(preset);
                      form.setFieldsValue({ themePreferences: preset });
                      message.success('已应用紫色深色主题', 1);
                    }}
                  >
                    🔮 紫色深色
                  </Button>
                </div>
              </div>

              <div style={{ display: 'flex', gap: 16, marginBottom: 16 }}>
                <Form.Item
                  name={['themePreferences', 'theme']}
                  label="主题模式"
                  style={{ flex: 1 }}
                  rules={[{ required: true, message: '请选择主题模式' }]}
                >
                  <Radio.Group
                    onChange={(e) => {
                      // 实时预览主题变更
                      updateTheme({ theme: e.target.value });
                      message.info(`已切换到${e.target.value === 'light' ? '浅色' : e.target.value === 'dark' ? '深色' : '自动'}模式`, 1);
                    }}
                  >
                    <Radio value="light">浅色模式</Radio>
                    <Radio value="dark">深色模式</Radio>
                    <Radio value="auto">跟随系统</Radio>
                  </Radio.Group>
                </Form.Item>

                <Form.Item
                  name={['themePreferences', 'primaryColor']}
                  label="主色调"
                  style={{ flex: 1 }}
                >
                  <ColorPicker
                    showText
                    format="hex"
                    presets={[
                      { label: '推荐', colors: ['#1890ff', '#52c41a', '#faad14', '#f5222d', '#722ed1'] }
                    ]}
                    onChange={(color) => {
                      // 实时预览颜色变更
                      const hexColor = typeof color === 'string' ? color : color.toHexString();
                      updateTheme({ primaryColor: hexColor });
                      message.info(`主色调已更新为 ${hexColor}`, 1);
                    }}
                  />
                </Form.Item>
              </div>

              <Form.Item name={['themePreferences', 'compactMode']} valuePropName="checked">
                <Checkbox
                  onChange={(e) => {
                    // 实时预览紧凑模式切换
                    updateTheme({ compactMode: e.target.checked });
                    message.info(`${e.target.checked ? '已启用' : '已关闭'}紧凑模式`, 1);
                  }}
                >
                  紧凑模式
                </Checkbox>
              </Form.Item>

              <Divider />

              {/* 保存按钮 */}
              <Form.Item>
                <div style={{ display: 'flex', gap: '12px' }}>
                  <Button
                    type="primary"
                    htmlType="submit"
                    loading={saving}
                    size="large"
                    style={{ flex: 1 }}
                    icon={saving ? undefined : <SettingOutlined />}
                  >
                    {saving ? '保存中...' : '保存所有设置'}
                  </Button>

                  <Button
                    size="large"
                    onClick={() => {
                      form.resetFields();
                      if (userProfile) {
                        applyTheme(userProfile.themePreferences);
                        message.info('已重置为原始设置', 2);
                      }
                    }}
                  >
                    重置
                  </Button>
                </div>

                <div style={{
                  marginTop: '8px',
                  fontSize: '12px',
                  color: '#666',
                  textAlign: 'center'
                }}>
                  💡 主题设置会实时预览，点击保存后永久生效
                </div>
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
