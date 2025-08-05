import React, { useState, useEffect } from 'react';
import {
  Layout,
  Card,
  Button,
  Space,
  message,
  Tabs,
  Typography,
  Statistic,
  Row,
  Col,
  Progress,
  Tag,
  Spin,
} from 'antd';
import {
  ArrowLeftOutlined,
  CheckCircleOutlined,
  FileTextOutlined,
  LinkOutlined,
  ReloadOutlined,
  ApartmentOutlined,
  BellOutlined,
} from '@ant-design/icons';
import { useNavigate, useParams } from 'react-router-dom';
import { taskApi, taskDetailsApi } from '../services/api';
import type { Task, TaskDetailsStats } from '../types/api';
import { useTheme } from '../contexts/ThemeContext';
import ChecklistComponent from '../components/ChecklistComponent';
import NotesComponent from '../components/NotesComponent';
import LinksComponent from '../components/LinksComponent';
import TaskDependencyPanel from '../components/TaskDependencyPanel';
import ReminderPanel from '../components/ReminderPanel';

const { Header, Content } = Layout;
const { Title, Text } = Typography;

const TaskDetailsPage: React.FC = () => {
  const { taskId } = useParams<{ taskId: string }>();
  const [task, setTask] = useState<Task | null>(null);
  const [stats, setStats] = useState<TaskDetailsStats | null>(null);
  const [loading, setLoading] = useState(false);
  const [activeTab, setActiveTab] = useState('checklist');
  const navigate = useNavigate();
  const { isDarkMode } = useTheme();

  // 获取任务信息
  const fetchTask = async () => {
    if (!taskId) return;

    try {
      const response = await taskApi.getTask(taskId);
      if (response.success && response.data) {
        setTask(response.data);
      } else {
        message.error(response.message || '获取任务信息失败');
      }
    } catch (error) {
      console.error('获取任务信息失败:', error);
      message.error('获取任务信息失败，请重试');
    }
  };

  // 获取任务详情统计
  const fetchStats = async () => {
    if (!taskId) return;

    try {
      const response = await taskDetailsApi.getTaskDetailsStats(taskId);
      if (response.success && response.data) {
        setStats(response.data);
      }
    } catch (error) {
      console.error('获取任务详情统计失败:', error);
    }
  };

  // 刷新数据
  const handleRefresh = async () => {
    setLoading(true);
    try {
      await Promise.all([fetchTask(), fetchStats()]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (taskId) {
      handleRefresh();
    }
  }, [taskId]);

  // 获取优先级颜色
  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'Urgent': return 'red';
      case 'High': return 'orange';
      case 'Medium': return 'blue';
      case 'Low': return 'green';
      default: return 'default';
    }
  };

  // 获取状态颜色
  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Completed': return 'green';
      case 'InProgress': return 'blue';
      case 'Pending': return 'orange';
      case 'Cancelled': return 'red';
      default: return 'default';
    }
  };

  if (!taskId) {
    return (
      <Layout style={{ minHeight: '100vh' }}>
        <Content style={{ display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
          <div>任务ID无效</div>
        </Content>
      </Layout>
    );
  }

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Header style={{ 
        background: isDarkMode ? '#1f1f1f' : '#ffffff',
        borderBottom: `1px solid ${isDarkMode ? '#434343' : '#f0f0f0'}`,
        padding: '0 24px',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between'
      }}>
        <div style={{ display: 'flex', alignItems: 'center' }}>
          <Button
            type="text"
            icon={<ArrowLeftOutlined />}
            onClick={() => navigate('/dashboard')}
            style={{ marginRight: 16 }}
          >
            返回
          </Button>
          <Title level={3} style={{ margin: 0, color: '#1890ff' }}>
            任务详情
          </Title>
        </div>

        <Space>
          <Button
            icon={<ReloadOutlined />}
            onClick={handleRefresh}
            loading={loading}
          >
            刷新
          </Button>
        </Space>
      </Header>

      <Content style={{ padding: '24px' }}>
        {task ? (
          <>
            {/* 任务基本信息 */}
            <Card style={{ marginBottom: 24 }}>
              <Row gutter={[16, 16]}>
                <Col span={16}>
                  <div>
                    <Title level={4} style={{ marginBottom: 8 }}>
                      {task.title}
                    </Title>
                    {task.description && (
                      <Text type="secondary" style={{ display: 'block', marginBottom: 16 }}>
                        {task.description}
                      </Text>
                    )}
                    <Space>
                      <Tag color={getPriorityColor(task.priority)}>
                        {task.priority}
                      </Tag>
                      <Tag color={getStatusColor(task.status)}>
                        {task.status}
                      </Tag>
                      {task.categoryName && (
                        <Tag color="blue">{task.categoryName}</Tag>
                      )}
                    </Space>
                  </div>
                </Col>
                <Col span={8}>
                  {stats && (
                    <Row gutter={16}>
                      <Col span={12}>
                        <Statistic
                          title="检查清单"
                          value={stats.checklistCompleted}
                          suffix={`/ ${stats.checklistTotal}`}
                          prefix={<CheckCircleOutlined />}
                        />
                        {stats.checklistTotal > 0 && (
                          <Progress
                            percent={stats.checklistCompletionRate}
                            size="small"
                            style={{ marginTop: 8 }}
                          />
                        )}
                      </Col>
                      <Col span={12}>
                        <Row gutter={[8, 8]}>
                          <Col span={24}>
                            <Statistic
                              title="笔记"
                              value={stats.notesCount}
                              prefix={<FileTextOutlined />}
                              valueStyle={{ fontSize: '16px' }}
                            />
                          </Col>
                          <Col span={24}>
                            <Statistic
                              title="链接"
                              value={stats.linksCount}
                              prefix={<LinkOutlined />}
                              valueStyle={{ fontSize: '16px' }}
                            />
                          </Col>
                        </Row>
                      </Col>
                    </Row>
                  )}
                </Col>
              </Row>
            </Card>

            {/* 任务详情标签页 */}
            <Card>
              <Tabs
                activeKey={activeTab}
                onChange={setActiveTab}
                items={[
                  {
                    key: 'checklist',
                    label: (
                      <span>
                        <CheckCircleOutlined />
                        检查清单
                        {stats && stats.checklistTotal > 0 && (
                          <Tag color="blue" style={{ marginLeft: 8 }}>
                            {stats.checklistCompleted}/{stats.checklistTotal}
                          </Tag>
                        )}
                      </span>
                    ),
                    children: <ChecklistComponent taskId={taskId} onStatsChange={fetchStats} />
                  },
                  {
                    key: 'notes',
                    label: (
                      <span>
                        <FileTextOutlined />
                        笔记评论
                        {stats && (stats.notesCount + stats.commentsCount) > 0 && (
                          <Tag color="green" style={{ marginLeft: 8 }}>
                            {stats.notesCount + stats.commentsCount}
                          </Tag>
                        )}
                      </span>
                    ),
                    children: <NotesComponent taskId={taskId} onStatsChange={fetchStats} />
                  },
                  {
                    key: 'links',
                    label: (
                      <span>
                        <LinkOutlined />
                        链接引用
                        {stats && stats.linksCount > 0 && (
                          <Tag color="orange" style={{ marginLeft: 8 }}>
                            {stats.linksCount}
                          </Tag>
                        )}
                      </span>
                    ),
                    children: <LinksComponent taskId={taskId} onStatsChange={fetchStats} />
                  },
                  {
                    key: 'dependencies',
                    label: (
                      <span>
                        <ApartmentOutlined />
                        依赖关系
                      </span>
                    ),
                    children: <TaskDependencyPanel taskId={taskId} onDependencyChange={fetchStats} />
                  },
                  {
                    key: 'reminders',
                    label: (
                      <span>
                        <BellOutlined />
                        提醒
                      </span>
                    ),
                    children: <ReminderPanel taskId={taskId} onReminderChange={fetchStats} />
                  }
                ]}
              />
            </Card>
          </>
        ) : (
          <div style={{ textAlign: 'center', padding: '50px' }}>
            <Spin size="large" />
            <div style={{ marginTop: 16 }}>加载任务信息中...</div>
          </div>
        )}
      </Content>
    </Layout>
  );
};

export default TaskDetailsPage;
