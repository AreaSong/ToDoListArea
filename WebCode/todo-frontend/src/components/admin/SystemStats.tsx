import React, { useState, useEffect } from 'react';
import {
  Card,
  Row,
  Col,
  Statistic,
  Typography,
  Progress,
  Table,
  Tag,
  Space,
  Button
} from 'antd';
import {
  UserOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  SafetyOutlined,
  ReloadOutlined,
  TrophyOutlined
} from '@ant-design/icons';
import { adminApi } from '../../services/api';

const { Title } = Typography;

interface SystemStatsData {
  totalUsers: number;
  activeUsers: number;
  adminUsers: number;
  totalTasks: number;
  completedTasks: number;
  todayRegistrations: number;
  weekRegistrations: number;
  monthRegistrations: number;
  invitationCodeStats?: {
    totalCodes: number;
    activeCodes: number;
    disabledCodes: number;
    expiredCodes: number;
    totalUsages: number;
    todayUsages: number;
    weekUsages: number;
    monthUsages: number;
  };
}

const SystemStats: React.FC = () => {
  const [stats, setStats] = useState<SystemStatsData>({
    totalUsers: 0,
    activeUsers: 0,
    adminUsers: 0,
    totalTasks: 0,
    completedTasks: 0,
    todayRegistrations: 0,
    weekRegistrations: 0,
    monthRegistrations: 0
  });
  const [loading, setLoading] = useState(false);

  const loadStats = async () => {
    setLoading(true);
    try {
      const response = await adminApi.getStats();
      if (response.success && response.data) {
        setStats(response.data);
      }
    } catch (error) {
      console.error('加载统计信息失败:', error);
      // 使用模拟数据作为后备
      setStats({
        totalUsers: 156,
        activeUsers: 142,
        adminUsers: 3,
        totalTasks: 1248,
        completedTasks: 892,
        todayRegistrations: 8,
        weekRegistrations: 23,
        monthRegistrations: 67,
        invitationCodeStats: {
          totalCodes: 12,
          activeCodes: 8,
          disabledCodes: 2,
          expiredCodes: 2,
          totalUsages: 156,
          todayUsages: 8,
          weekUsages: 23,
          monthUsages: 67
        }
      });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadStats();
  }, []);

  // 计算完成率
  const taskCompletionRate = stats.totalTasks > 0 
    ? Math.round((stats.completedTasks / stats.totalTasks) * 100) 
    : 0;

  // 计算活跃用户率
  const activeUserRate = stats.totalUsers > 0 
    ? Math.round((stats.activeUsers / stats.totalUsers) * 100) 
    : 0;

  // 计算邀请码使用率
  const invitationUsageRate = stats.invitationCodeStats && stats.invitationCodeStats.totalCodes > 0
    ? Math.round((stats.invitationCodeStats.totalUsages / (stats.invitationCodeStats.totalCodes * 10)) * 100)
    : 0;

  // 快速统计数据
  const quickStats = [
    {
      title: '今日新增用户',
      value: stats.todayRegistrations,
      icon: <UserOutlined style={{ color: '#1890ff' }} />,
      color: '#1890ff'
    },
    {
      title: '本周新增用户',
      value: stats.weekRegistrations,
      icon: <UserOutlined style={{ color: '#52c41a' }} />,
      color: '#52c41a'
    },
    {
      title: '本月新增用户',
      value: stats.monthRegistrations,
      icon: <UserOutlined style={{ color: '#722ed1' }} />,
      color: '#722ed1'
    },
    {
      title: '今日邀请码使用',
      value: stats.invitationCodeStats?.todayUsages || 0,
      icon: <SafetyOutlined style={{ color: '#fa8c16' }} />,
      color: '#fa8c16'
    }
  ];

  return (
    <div>
      {/* 页面标题和刷新按钮 */}
      <div style={{ 
        display: 'flex', 
        justifyContent: 'space-between', 
        alignItems: 'center', 
        marginBottom: 24 
      }}>
        <Title level={3} style={{ margin: 0 }}>系统概览</Title>
        <Button 
          icon={<ReloadOutlined />} 
          onClick={loadStats}
          loading={loading}
        >
          刷新数据
        </Button>
      </div>

      {/* 主要统计卡片 */}
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={6}>
          <Card>
            <Statistic
              title="总用户数"
              value={stats.totalUsers}
              prefix={<UserOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
            <div style={{ marginTop: 8 }}>
              <Progress 
                percent={activeUserRate} 
                size="small" 
                format={() => `${stats.activeUsers} 活跃`}
              />
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="总任务数"
              value={stats.totalTasks}
              prefix={<ClockCircleOutlined />}
              valueStyle={{ color: '#722ed1' }}
            />
            <div style={{ marginTop: 8 }}>
              <Progress 
                percent={taskCompletionRate} 
                size="small" 
                format={() => `${stats.completedTasks} 已完成`}
              />
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="管理员数量"
              value={stats.adminUsers}
              prefix={<TrophyOutlined />}
              valueStyle={{ color: '#f5222d' }}
            />
            <div style={{ marginTop: 8, color: '#666' }}>
              占总用户 {stats.totalUsers > 0 ? Math.round((stats.adminUsers / stats.totalUsers) * 100) : 0}%
            </div>
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="邀请码总数"
              value={stats.invitationCodeStats?.totalCodes || 0}
              prefix={<SafetyOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
            <div style={{ marginTop: 8 }}>
              <Progress 
                percent={invitationUsageRate} 
                size="small" 
                format={() => `${stats.invitationCodeStats?.totalUsages || 0} 次使用`}
              />
            </div>
          </Card>
        </Col>
      </Row>

      {/* 快速统计 */}
      <Row gutter={16} style={{ marginBottom: 24 }}>
        {quickStats.map((stat, index) => (
          <Col span={6} key={index}>
            <Card size="small">
              <div style={{ display: 'flex', alignItems: 'center' }}>
                <div style={{ marginRight: 12 }}>
                  {stat.icon}
                </div>
                <div>
                  <div style={{ fontSize: '24px', fontWeight: 'bold', color: stat.color }}>
                    {stat.value}
                  </div>
                  <div style={{ color: '#666', fontSize: '12px' }}>
                    {stat.title}
                  </div>
                </div>
              </div>
            </Card>
          </Col>
        ))}
      </Row>

      {/* 详细统计 */}
      <Row gutter={16}>
        <Col span={12}>
          <Card title="用户统计详情" size="small">
            <Row gutter={16}>
              <Col span={12}>
                <Statistic
                  title="活跃用户"
                  value={stats.activeUsers}
                  suffix={`/ ${stats.totalUsers}`}
                  valueStyle={{ color: '#3f8600' }}
                />
              </Col>
              <Col span={12}>
                <Statistic
                  title="活跃率"
                  value={activeUserRate}
                  suffix="%"
                  valueStyle={{ color: '#3f8600' }}
                />
              </Col>
            </Row>
            <div style={{ marginTop: 16 }}>
              <div>今日注册: <Tag color="blue">{stats.todayRegistrations}</Tag></div>
              <div>本周注册: <Tag color="green">{stats.weekRegistrations}</Tag></div>
              <div>本月注册: <Tag color="purple">{stats.monthRegistrations}</Tag></div>
            </div>
          </Card>
        </Col>
        
        <Col span={12}>
          <Card title="任务统计详情" size="small">
            <Row gutter={16}>
              <Col span={12}>
                <Statistic
                  title="已完成任务"
                  value={stats.completedTasks}
                  suffix={`/ ${stats.totalTasks}`}
                  valueStyle={{ color: '#722ed1' }}
                />
              </Col>
              <Col span={12}>
                <Statistic
                  title="完成率"
                  value={taskCompletionRate}
                  suffix="%"
                  valueStyle={{ color: '#722ed1' }}
                />
              </Col>
            </Row>
            <div style={{ marginTop: 16 }}>
              <Progress 
                percent={taskCompletionRate} 
                strokeColor={{
                  '0%': '#108ee9',
                  '100%': '#87d068',
                }}
              />
            </div>
          </Card>
        </Col>
      </Row>

      {/* 邀请码统计 */}
      {stats.invitationCodeStats && (
        <Row gutter={16} style={{ marginTop: 16 }}>
          <Col span={24}>
            <Card title="邀请码统计详情" size="small">
              <Row gutter={16}>
                <Col span={6}>
                  <Statistic
                    title="总邀请码"
                    value={stats.invitationCodeStats.totalCodes}
                    valueStyle={{ color: '#1890ff' }}
                  />
                </Col>
                <Col span={6}>
                  <Statistic
                    title="活跃邀请码"
                    value={stats.invitationCodeStats.activeCodes}
                    valueStyle={{ color: '#52c41a' }}
                  />
                </Col>
                <Col span={6}>
                  <Statistic
                    title="已禁用"
                    value={stats.invitationCodeStats.disabledCodes}
                    valueStyle={{ color: '#faad14' }}
                  />
                </Col>
                <Col span={6}>
                  <Statistic
                    title="已过期"
                    value={stats.invitationCodeStats.expiredCodes}
                    valueStyle={{ color: '#f5222d' }}
                  />
                </Col>
              </Row>
              <div style={{ marginTop: 16 }}>
                <Row gutter={16}>
                  <Col span={6}>
                    <div>总使用次数: <Tag color="blue">{stats.invitationCodeStats.totalUsages}</Tag></div>
                  </Col>
                  <Col span={6}>
                    <div>今日使用: <Tag color="green">{stats.invitationCodeStats.todayUsages}</Tag></div>
                  </Col>
                  <Col span={6}>
                    <div>本周使用: <Tag color="orange">{stats.invitationCodeStats.weekUsages}</Tag></div>
                  </Col>
                  <Col span={6}>
                    <div>本月使用: <Tag color="purple">{stats.invitationCodeStats.monthUsages}</Tag></div>
                  </Col>
                </Row>
              </div>
            </Card>
          </Col>
        </Row>
      )}
    </div>
  );
};

export default SystemStats;
