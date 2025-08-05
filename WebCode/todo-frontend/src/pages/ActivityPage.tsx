import React, { useState, useEffect } from 'react';
import {
  Layout,
  Card,
  List,
  Button,
  Space,
  message,
  Typography,
  Statistic,
  Row,
  Col,
  Select,
  DatePicker,
  Tag,
  Empty,
  Spin,
  Timeline,
  Avatar,
  Tooltip,
  Divider,
} from 'antd';
import {
  ArrowLeftOutlined,
  ReloadOutlined,
  UserOutlined,
  ClockCircleOutlined,
  EyeOutlined,
  EditOutlined,
  PlusOutlined,
  DeleteOutlined,
  LoginOutlined,
  LogoutOutlined,
  DashboardOutlined,
  FileTextOutlined,
  LinkOutlined,
  SettingOutlined,
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { userActivityApi } from '../services/api';
import type { UserActivity, UserActivityStats, UserActivityQuery, ActivityTypeCount } from '../types/api';
import { ActivityTypes } from '../types/api';
import { useTheme } from '../contexts/ThemeContext';
import dayjs from 'dayjs';

const { Content } = Layout;
const { Title, Text } = Typography;
const { RangePicker } = DatePicker;

const ActivityPage: React.FC = () => {
  const navigate = useNavigate();
  const { isDarkMode } = useTheme();
  const [activities, setActivities] = useState<UserActivity[]>([]);
  const [stats, setStats] = useState<UserActivityStats | null>(null);
  const [loading, setLoading] = useState(false);
  const [statsLoading, setStatsLoading] = useState(false);
  const [query, setQuery] = useState<UserActivityQuery>({
    page: 1,
    pageSize: 20
  });

  // 从JWT token中获取用户ID
  const getCurrentUserId = () => {
    const token = localStorage.getItem('token');
    if (!token) return '00000000-0000-0000-0000-000000000000';

    try {
      // JWT token格式: header.payload.signature
      const parts = token.split('.');
      if (parts.length !== 3) {
        console.error('Invalid JWT token format');
        return '00000000-0000-0000-0000-000000000000';
      }

      // 解码payload部分，处理base64url编码
      let base64 = parts[1];
      // 将base64url转换为标准base64
      base64 = base64.replace(/-/g, '+').replace(/_/g, '/');
      // 添加必要的填充
      while (base64.length % 4) {
        base64 += '=';
      }

      const payload = JSON.parse(atob(base64));
      return payload.nameid || payload.user_id || '00000000-0000-0000-0000-000000000000';
    } catch (error) {
      console.error('解析token失败:', error);
      return '00000000-0000-0000-0000-000000000000';
    }
  };

  const currentUserId = getCurrentUserId();

  // 获取活动列表
  const fetchActivities = async () => {
    setLoading(true);
    try {
      const response = await userActivityApi.getUserActivities(currentUserId, query);
      if (response.success && response.data) {
        setActivities(response.data);
      }
    } catch (error) {
      console.error('获取活动列表失败:', error);
      message.error('获取活动列表失败');
    } finally {
      setLoading(false);
    }
  };

  // 获取活动统计
  const fetchStats = async () => {
    setStatsLoading(true);
    try {
      const response = await userActivityApi.getUserActivityStats(currentUserId);
      if (response.success && response.data) {
        setStats(response.data);
      }
    } catch (error) {
      console.error('获取活动统计失败:', error);
      message.error('获取活动统计失败');
    } finally {
      setStatsLoading(false);
    }
  };

  // 获取活动类型的图标
  const getActivityIcon = (activityType: string) => {
    switch (activityType) {
      case ActivityTypes.LOGIN:
        return <LoginOutlined style={{ color: '#52c41a' }} />;
      case ActivityTypes.LOGOUT:
        return <LogoutOutlined style={{ color: '#ff4d4f' }} />;
      case ActivityTypes.CREATE_TASK:
        return <PlusOutlined style={{ color: '#1890ff' }} />;
      case ActivityTypes.UPDATE_TASK:
        return <EditOutlined style={{ color: '#faad14' }} />;
      case ActivityTypes.DELETE_TASK:
        return <DeleteOutlined style={{ color: '#ff4d4f' }} />;
      case ActivityTypes.VIEW_TASK:
        return <EyeOutlined style={{ color: '#722ed1' }} />;
      case ActivityTypes.VIEW_DASHBOARD:
        return <DashboardOutlined style={{ color: '#13c2c2' }} />;
      case ActivityTypes.CREATE_TASK_DETAIL:
      case ActivityTypes.UPDATE_TASK_DETAIL:
      case ActivityTypes.DELETE_TASK_DETAIL:
        return <FileTextOutlined style={{ color: '#eb2f96' }} />;
      case ActivityTypes.UPDATE_PROFILE:
        return <SettingOutlined style={{ color: '#fa8c16' }} />;
      default:
        return <ClockCircleOutlined style={{ color: '#8c8c8c' }} />;
    }
  };

  // 获取活动类型的颜色
  const getActivityColor = (activityType: string) => {
    switch (activityType) {
      case ActivityTypes.LOGIN:
        return 'green';
      case ActivityTypes.LOGOUT:
        return 'red';
      case ActivityTypes.CREATE_TASK:
      case ActivityTypes.CREATE_TASK_DETAIL:
      case ActivityTypes.CREATE_TEMPLATE:
        return 'blue';
      case ActivityTypes.UPDATE_TASK:
      case ActivityTypes.UPDATE_TASK_DETAIL:
      case ActivityTypes.UPDATE_TEMPLATE:
      case ActivityTypes.UPDATE_PROFILE:
        return 'orange';
      case ActivityTypes.DELETE_TASK:
      case ActivityTypes.DELETE_TASK_DETAIL:
      case ActivityTypes.DELETE_TEMPLATE:
        return 'red';
      case ActivityTypes.VIEW_TASK:
      case ActivityTypes.VIEW_DASHBOARD:
      case ActivityTypes.VIEW_GANTT:
      case ActivityTypes.VIEW_TEMPLATES:
      case ActivityTypes.VIEW_PROFILE:
        return 'purple';
      default:
        return 'default';
    }
  };

  // 格式化活动类型显示名称
  const formatActivityType = (activityType: string) => {
    const typeMap: Record<string, string> = {
      [ActivityTypes.LOGIN]: '登录',
      [ActivityTypes.LOGOUT]: '退出',
      [ActivityTypes.CREATE_TASK]: '创建任务',
      [ActivityTypes.UPDATE_TASK]: '更新任务',
      [ActivityTypes.DELETE_TASK]: '删除任务',
      [ActivityTypes.VIEW_TASK]: '查看任务',
      [ActivityTypes.CREATE_TASK_DETAIL]: '添加详情',
      [ActivityTypes.UPDATE_TASK_DETAIL]: '更新详情',
      [ActivityTypes.DELETE_TASK_DETAIL]: '删除详情',
      [ActivityTypes.CREATE_TEMPLATE]: '创建模板',
      [ActivityTypes.UPDATE_TEMPLATE]: '更新模板',
      [ActivityTypes.DELETE_TEMPLATE]: '删除模板',
      [ActivityTypes.USE_TEMPLATE]: '使用模板',
      [ActivityTypes.UPDATE_PROFILE]: '更新资料',
      [ActivityTypes.VIEW_DASHBOARD]: '访问控制台',
      [ActivityTypes.VIEW_GANTT]: '查看甘特图',
      [ActivityTypes.VIEW_TEMPLATES]: '查看模板',
      [ActivityTypes.VIEW_PROFILE]: '查看资料',
    };
    return typeMap[activityType] || activityType;
  };

  // 处理筛选变化
  const handleFilterChange = (field: keyof UserActivityQuery, value: any) => {
    setQuery(prev => ({
      ...prev,
      [field]: value,
      page: 1 // 重置页码
    }));
  };

  // 处理日期范围变化
  const handleDateRangeChange = (dates: any) => {
    if (dates && dates.length === 2) {
      setQuery(prev => ({
        ...prev,
        startDate: dates[0].format('YYYY-MM-DD'),
        endDate: dates[1].format('YYYY-MM-DD'),
        page: 1
      }));
    } else {
      setQuery(prev => ({
        ...prev,
        startDate: undefined,
        endDate: undefined,
        page: 1
      }));
    }
  };

  useEffect(() => {
    fetchActivities();
  }, [query]);

  useEffect(() => {
    fetchStats();
  }, []);

  return (
    <Layout style={{ minHeight: '100vh', backgroundColor: isDarkMode ? '#141414' : '#f0f2f5' }}>
      <Content style={{ padding: '24px' }}>
        {/* 页面头部 */}
        <div style={{ marginBottom: 24 }}>
          <Space>
            <Button 
              icon={<ArrowLeftOutlined />} 
              onClick={() => navigate('/dashboard')}
            >
              返回
            </Button>
            <Title level={2} style={{ margin: 0 }}>
              用户活动记录
            </Title>
            <Button 
              icon={<ReloadOutlined />} 
              onClick={() => {
                fetchActivities();
                fetchStats();
              }}
            >
              刷新
            </Button>
          </Space>
        </div>

        {/* 统计信息 */}
        <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
          <Col xs={24} sm={12} md={6}>
            <Card>
              <Statistic
                title="总活动数"
                value={stats?.totalActivities || 0}
                prefix={<ClockCircleOutlined />}
                loading={statsLoading}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Card>
              <Statistic
                title="今日活动"
                value={stats?.todayActivities || 0}
                prefix={<UserOutlined />}
                loading={statsLoading}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Card>
              <Statistic
                title="最后活动"
                value={stats?.lastActivityTime ? dayjs(stats.lastActivityTime).format('MM-DD HH:mm') : '无'}
                loading={statsLoading}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Card>
              <Statistic
                title="活动类型"
                value={stats?.activityTypeStats.length || 0}
                suffix="种"
                loading={statsLoading}
              />
            </Card>
          </Col>
        </Row>

        {/* 筛选器 */}
        <Card style={{ marginBottom: 24 }}>
          <Space wrap>
            <Select
              placeholder="选择活动类型"
              style={{ width: 150 }}
              allowClear
              value={query.activityType}
              onChange={(value) => handleFilterChange('activityType', value)}
            >
              <Select.Option value={ActivityTypes.LOGIN}>登录</Select.Option>
              <Select.Option value={ActivityTypes.CREATE_TASK}>创建任务</Select.Option>
              <Select.Option value={ActivityTypes.UPDATE_TASK}>更新任务</Select.Option>
              <Select.Option value={ActivityTypes.VIEW_TASK}>查看任务</Select.Option>
              <Select.Option value={ActivityTypes.VIEW_DASHBOARD}>访问控制台</Select.Option>
              <Select.Option value={ActivityTypes.UPDATE_PROFILE}>更新资料</Select.Option>
            </Select>
            
            <RangePicker
              placeholder={['开始日期', '结束日期']}
              onChange={handleDateRangeChange}
            />
            
            <Select
              placeholder="每页显示"
              style={{ width: 120 }}
              value={query.pageSize}
              onChange={(value) => handleFilterChange('pageSize', value)}
            >
              <Select.Option value={10}>10条</Select.Option>
              <Select.Option value={20}>20条</Select.Option>
              <Select.Option value={50}>50条</Select.Option>
            </Select>
          </Space>
        </Card>

        {/* 活动列表 */}
        <Card title="活动时间线">
          <Spin spinning={loading}>
            {activities.length === 0 ? (
              <Empty description="暂无活动记录" />
            ) : (
              <Timeline mode="left">
                {activities.map((activity) => (
                  <Timeline.Item
                    key={activity.id}
                    dot={
                      <Avatar 
                        size="small" 
                        icon={getActivityIcon(activity.activityType)}
                        style={{ backgroundColor: 'transparent' }}
                      />
                    }
                  >
                    <div style={{ paddingLeft: 16 }}>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 4 }}>
                        <Tag color={getActivityColor(activity.activityType)}>
                          {formatActivityType(activity.activityType)}
                        </Tag>
                        <Text type="secondary">
                          {dayjs(activity.createdAt).format('YYYY-MM-DD HH:mm:ss')}
                        </Text>
                      </div>
                      
                      <div style={{ marginBottom: 8 }}>
                        <Text>{activity.activityDescription || '无描述'}</Text>
                      </div>
                      
                      {(activity.entityType || activity.ipAddress) && (
                        <div style={{ fontSize: '12px', color: '#8c8c8c' }}>
                          {activity.entityType && (
                            <span>实体类型: {activity.entityType} </span>
                          )}
                          {activity.ipAddress && (
                            <span>IP: {activity.ipAddress}</span>
                          )}
                        </div>
                      )}
                    </div>
                  </Timeline.Item>
                ))}
              </Timeline>
            )}
          </Spin>
        </Card>
      </Content>
    </Layout>
  );
};

export default ActivityPage;
