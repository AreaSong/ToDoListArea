import React, { useState, useEffect } from 'react';
import { Layout, Card, Typography, Button, Space, message, Modal, Form, Input, Select, DatePicker, Statistic, Row, Col, Result } from 'antd';
import { ArrowLeftOutlined, ReloadOutlined, PlusOutlined, CheckCircleOutlined, ClockCircleOutlined, ExclamationCircleOutlined, FileTextOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { Gantt, ViewMode } from 'gantt-task-react';
import type { Task as GanttTask } from 'gantt-task-react';
import 'gantt-task-react/dist/index.css';
import { taskApi } from '../services/api';
import type { Task as ApiTask, User } from '../types/api';

const { Header, Content } = Layout;
const { Title } = Typography;

const GanttPage: React.FC = () => {
  const [ganttTasks, setGanttTasks] = useState<GanttTask[]>([]);
  const [viewMode, setViewMode] = useState<ViewMode>(ViewMode.Day);
  const [loading, setLoading] = useState(false);
  const [createModalVisible, setCreateModalVisible] = useState(false);
  const [form] = Form.useForm();
  const navigate = useNavigate();

  // 获取当前用户信息
  const currentUser: User = JSON.parse(localStorage.getItem('user') || '{}');

  // 将API任务数据转换为甘特图格式
  const convertToGanttTasks = (tasks: ApiTask[]): GanttTask[] => {
    if (!tasks || tasks.length === 0) {
      return [];
    }

    return tasks.map((task, index) => {
      // 确保任务有有效的开始时间
      let startDate: Date;
      if (task.startTime) {
        startDate = new Date(task.startTime);
        // 验证日期是否有效
        if (isNaN(startDate.getTime())) {
          startDate = new Date();
        }
      } else {
        // 如果没有开始时间，设置为今天加上索引天数，避免重叠
        startDate = new Date();
        startDate.setDate(startDate.getDate() + index);
      }

      // 确保任务有有效的结束时间
      let endDate: Date;
      if (task.endTime) {
        endDate = new Date(task.endTime);
        // 验证日期是否有效
        if (isNaN(endDate.getTime())) {
          endDate = new Date(startDate.getTime() + 24 * 60 * 60 * 1000);
        }
      } else if (task.estimatedDuration && task.estimatedDuration > 0) {
        endDate = new Date(startDate.getTime() + task.estimatedDuration * 60 * 1000);
      } else {
        // 默认1天
        endDate = new Date(startDate.getTime() + 24 * 60 * 60 * 1000);
      }

      // 确保结束时间不早于开始时间
      if (endDate <= startDate) {
        endDate = new Date(startDate.getTime() + 24 * 60 * 60 * 1000);
      }

      // 计算进度百分比
      let progress = 0;
      if (task.status === 'Completed') {
        progress = 100;
      } else if (task.status === 'InProgress') {
        progress = 50; // 可以根据实际需求调整
      }

      return {
        start: startDate,
        end: endDate,
        name: task.title || `任务 ${index + 1}`,
        id: task.id,
        type: 'task' as const,
        progress: progress,
        isDisabled: false,
        styles: {
          progressColor: getProgressColor(task.priority || 'Medium'),
          progressSelectedColor: getProgressSelectedColor(task.priority || 'Medium'),
          backgroundColor: getBackgroundColor(task.status || 'Pending'),
          backgroundSelectedColor: getBackgroundSelectedColor(task.status || 'Pending'),
        },
      };
    });
  };

  // 根据优先级获取进度条颜色
  const getProgressColor = (priority: string) => {
    switch (priority) {
      case 'High': return '#ff4d4f';
      case 'Medium': return '#faad14';
      case 'Low': return '#52c41a';
      default: return '#1890ff';
    }
  };

  const getProgressSelectedColor = (priority: string) => {
    switch (priority) {
      case 'High': return '#cf1322';
      case 'Medium': return '#d48806';
      case 'Low': return '#389e0d';
      default: return '#096dd9';
    }
  };

  // 根据状态获取背景颜色
  const getBackgroundColor = (status: string) => {
    switch (status) {
      case 'Completed': return '#f6ffed';
      case 'InProgress': return '#fff7e6';
      case 'Pending': return '#f0f5ff';
      default: return '#fafafa';
    }
  };

  const getBackgroundSelectedColor = (status: string) => {
    switch (status) {
      case 'Completed': return '#d9f7be';
      case 'InProgress': return '#ffd591';
      case 'Pending': return '#adc6ff';
      default: return '#d9d9d9';
    }
  };

  // 获取任务数据
  const fetchTasks = async () => {
    if (!currentUser.id) {
      message.warning('请先登录后再使用甘特图功能');
      navigate('/login');
      return;
    }

    setLoading(true);
    try {
      const response = await taskApi.getTasks(currentUser.id, {
        pageNumber: 1,
        pageSize: 100, // 获取更多任务用于甘特图显示
        sortBy: 'StartTime',
        sortOrder: 'asc'
      });

      if (response.success && response.data) {
        const ganttData = convertToGanttTasks(response.data.items);
        setGanttTasks(ganttData);

        if (ganttData.length === 0) {
          console.log('没有找到任务数据，显示空状态');
        } else {
          console.log(`成功加载 ${ganttData.length} 个任务到甘特图`);
        }
      } else {
        message.error(response.message || '获取任务数据失败');
        setGanttTasks([]);
      }
    } catch (error) {
      console.error('获取任务数据失败:', error);
      message.error('获取任务数据失败，请检查网络连接');
      setGanttTasks([]);
    } finally {
      setLoading(false);
    }
  };

  // 处理任务日期变更
  const handleTaskChange = async (task: GanttTask) => {
    try {
      const response = await taskApi.updateTask(task.id, {
        startTime: task.start.toISOString(),
        endTime: task.end.toISOString(),
      });

      if (response.success) {
        message.success('任务时间已更新');
        // 更新本地状态
        setGanttTasks(prev =>
          prev.map(t => t.id === task.id ? task : t)
        );
      } else {
        message.error('任务时间更新失败');
        // 重新获取数据以恢复原状态
        fetchTasks();
      }
    } catch (error) {
      message.error('任务时间更新失败');
      fetchTasks();
    }
  };

  // 处理任务进度变更
  const handleProgressChange = async (task: GanttTask) => {
    try {
      // 根据进度计算状态
      let status = 'Pending';
      if (task.progress === 100) {
        status = 'Completed';
      } else if (task.progress > 0) {
        status = 'InProgress';
      }

      const response = await taskApi.updateTask(task.id, {
        status: status,
      });

      if (response.success) {
        message.success('任务进度已更新');
        // 更新本地状态
        setGanttTasks(prev =>
          prev.map(t => t.id === task.id ? task : t)
        );
      } else {
        message.error('任务进度更新失败');
        fetchTasks();
      }
    } catch (error) {
      message.error('任务进度更新失败');
      fetchTasks();
    }
  };

  // 处理任务点击
  const handleTaskClick = (task: GanttTask) => {
    console.log('Task clicked:', task);
    // 可以跳转到任务详情或编辑页面
  };

  // 处理任务双击
  const handleTaskDoubleClick = (task: GanttTask) => {
    console.log('Task double clicked:', task);
    // 跳转回任务管理页面并编辑该任务
    navigate('/dashboard');
  };

  // 创建新任务
  const handleCreateTask = async (values: any) => {
    try {
      const response = await taskApi.createTask(currentUser.id, {
        title: values.title,
        description: values.description,
        status: values.status || 'Pending',
        priority: values.priority || 'Medium',
        startTime: values.startTime?.toISOString(),
        endTime: values.endTime?.toISOString(),
      });

      if (response.success) {
        message.success('任务创建成功');
        setCreateModalVisible(false);
        form.resetFields();
        fetchTasks(); // 重新获取数据
      } else {
        message.error('任务创建失败');
      }
    } catch (error) {
      message.error('任务创建失败');
    }
  };

  useEffect(() => {
    fetchTasks();
  }, []);

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
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => setCreateModalVisible(true)}
          >
            新建任务
          </Button>
          <Button
            type="text"
            icon={<ReloadOutlined />}
            onClick={fetchTasks}
            loading={loading}
          >
            刷新
          </Button>
          <Space.Compact>
            <Button 
              type={viewMode === ViewMode.Day ? 'primary' : 'default'}
              onClick={() => setViewMode(ViewMode.Day)}
            >
              日视图
            </Button>
            <Button 
              type={viewMode === ViewMode.Week ? 'primary' : 'default'}
              onClick={() => setViewMode(ViewMode.Week)}
            >
              周视图
            </Button>
            <Button 
              type={viewMode === ViewMode.Month ? 'primary' : 'default'}
              onClick={() => setViewMode(ViewMode.Month)}
            >
              月视图
            </Button>
          </Space.Compact>
        </Space>
      </Header>

      <Content style={{ padding: '24px' }}>
        {/* 统计信息面板 */}
        <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
          <Col xs={12} sm={12} md={6} lg={6}>
            <Card size="small">
              <Statistic
                title="总任务数"
                value={ganttTasks.length}
                prefix={<ExclamationCircleOutlined />}
                valueStyle={{ fontSize: '20px' }}
              />
            </Card>
          </Col>
          <Col xs={12} sm={12} md={6} lg={6}>
            <Card size="small">
              <Statistic
                title="已完成"
                value={ganttTasks.filter(t => t.progress === 100).length}
                prefix={<CheckCircleOutlined />}
                valueStyle={{ color: '#3f8600', fontSize: '20px' }}
              />
            </Card>
          </Col>
          <Col xs={12} sm={12} md={6} lg={6}>
            <Card size="small">
              <Statistic
                title="进行中"
                value={ganttTasks.filter(t => t.progress > 0 && t.progress < 100).length}
                prefix={<ClockCircleOutlined />}
                valueStyle={{ color: '#faad14', fontSize: '20px' }}
              />
            </Card>
          </Col>
          <Col xs={12} sm={12} md={6} lg={6}>
            <Card size="small">
              <Statistic
                title="完成率"
                value={ganttTasks.length > 0 ? Math.round((ganttTasks.filter(t => t.progress === 100).length / ganttTasks.length) * 100) : 0}
                suffix="%"
                valueStyle={{ color: '#1890ff', fontSize: '20px' }}
              />
            </Card>
          </Col>
        </Row>

        <Card>
          {ganttTasks.length > 0 ? (
            <div style={{
              height: '600px',
              overflow: 'auto',
              border: '1px solid #f0f0f0',
              borderRadius: '6px',
              backgroundColor: '#fafafa'
            }}>
              <Gantt
                tasks={ganttTasks}
                viewMode={viewMode}
                onDateChange={handleTaskChange}
                onProgressChange={handleProgressChange}
                onClick={handleTaskClick}
                onDoubleClick={handleTaskDoubleClick}
                listCellWidth="200px"
                columnWidth={viewMode === ViewMode.Day ? 65 : viewMode === ViewMode.Week ? 250 : 300}
              />
            </div>
          ) : (
            <div style={{ padding: '40px 0' }}>
              {loading ? (
                <div style={{ textAlign: 'center' }}>
                  <Space direction="vertical" size="large">
                    <div style={{ fontSize: '16px', color: '#666' }}>正在加载任务数据...</div>
                    <div style={{ fontSize: '12px', color: '#999' }}>
                      正在从服务器获取您的任务信息
                    </div>
                  </Space>
                </div>
              ) : (
                <Result
                  icon={<FileTextOutlined style={{ color: '#1890ff' }} />}
                  title="暂无任务数据"
                  subTitle={
                    <div>
                      <p>您还没有创建任何任务，创建第一个任务来开始使用甘特图功能吧！</p>
                      <p style={{ fontSize: '12px', color: '#999', marginTop: '8px' }}>
                        甘特图可以帮助您可视化任务时间线，拖拽调整时间和进度
                      </p>
                    </div>
                  }
                  extra={[
                    <Button
                      type="primary"
                      key="create"
                      icon={<PlusOutlined />}
                      onClick={() => setCreateModalVisible(true)}
                      size="large"
                    >
                      创建第一个任务
                    </Button>,
                    <Button
                      key="dashboard"
                      onClick={() => navigate('/dashboard')}
                      size="large"
                    >
                      前往任务管理
                    </Button>,
                  ]}
                />
              )}
            </div>
          )}
        </Card>

        {/* 创建任务模态框 */}
        <Modal
          title="新建任务"
          open={createModalVisible}
          onCancel={() => {
            setCreateModalVisible(false);
            form.resetFields();
          }}
          footer={null}
          width={500}
        >
          <Form
            form={form}
            layout="vertical"
            onFinish={handleCreateTask}
          >
            <Form.Item
              name="title"
              label="任务标题"
              rules={[{ required: true, message: '请输入任务标题' }]}
            >
              <Input placeholder="请输入任务标题" />
            </Form.Item>

            <Form.Item
              name="description"
              label="任务描述"
            >
              <Input.TextArea rows={3} placeholder="请输入任务描述" />
            </Form.Item>

            <Form.Item
              name="status"
              label="任务状态"
              initialValue="Pending"
            >
              <Select>
                <Select.Option value="Pending">待处理</Select.Option>
                <Select.Option value="InProgress">进行中</Select.Option>
                <Select.Option value="Completed">已完成</Select.Option>
              </Select>
            </Form.Item>

            <Form.Item
              name="priority"
              label="优先级"
              initialValue="Medium"
            >
              <Select>
                <Select.Option value="High">高</Select.Option>
                <Select.Option value="Medium">中</Select.Option>
                <Select.Option value="Low">低</Select.Option>
              </Select>
            </Form.Item>

            <Form.Item
              name="startTime"
              label="开始时间"
            >
              <DatePicker showTime style={{ width: '100%' }} />
            </Form.Item>

            <Form.Item
              name="endTime"
              label="结束时间"
            >
              <DatePicker showTime style={{ width: '100%' }} />
            </Form.Item>

            <Form.Item>
              <Space>
                <Button type="primary" htmlType="submit">
                  创建任务
                </Button>
                <Button onClick={() => {
                  setCreateModalVisible(false);
                  form.resetFields();
                }}>
                  取消
                </Button>
              </Space>
            </Form.Item>
          </Form>
        </Modal>
      </Content>
    </Layout>
  );
};

export default GanttPage;
