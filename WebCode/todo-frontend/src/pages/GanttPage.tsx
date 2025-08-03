import React, { useState, useEffect } from 'react';
import { Layout, Card, Typography, Button, Space, message } from 'antd';
import { ArrowLeftOutlined, ReloadOutlined } from '@ant-design/icons';
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
  const navigate = useNavigate();

  // 获取当前用户信息
  const currentUser: User = JSON.parse(localStorage.getItem('user') || '{}');

  // 将API任务数据转换为甘特图格式
  const convertToGanttTasks = (tasks: ApiTask[]): GanttTask[] => {
    return tasks.map((task) => {
      // 如果任务没有开始时间，设置为今天
      const startDate = task.startTime ? new Date(task.startTime) : new Date();
      
      // 如果任务没有结束时间，根据预计时长计算
      let endDate: Date;
      if (task.endTime) {
        endDate = new Date(task.endTime);
      } else if (task.estimatedDuration) {
        endDate = new Date(startDate.getTime() + task.estimatedDuration * 60 * 1000);
      } else {
        // 默认1天
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
        name: task.title,
        id: task.id,
        type: 'task' as const,
        progress: progress,
        isDisabled: false,
        styles: {
          progressColor: getProgressColor(task.priority),
          progressSelectedColor: getProgressSelectedColor(task.priority),
          backgroundColor: getBackgroundColor(task.status),
          backgroundSelectedColor: getBackgroundSelectedColor(task.status),
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
    if (!currentUser.id) return;
    
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
      }
    } catch (error) {
      message.error('获取任务数据失败');
    } finally {
      setLoading(false);
    }
  };

  // 处理任务日期变更
  const handleTaskChange = (task: GanttTask) => {
    console.log('Task changed:', task);
    // TODO: 调用API更新任务时间
    message.info('任务时间已更新');
  };

  // 处理任务进度变更
  const handleProgressChange = (task: GanttTask) => {
    console.log('Progress changed:', task);
    // TODO: 调用API更新任务进度
    message.info('任务进度已更新');
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
        <Card>
          {ganttTasks.length > 0 ? (
            <div style={{ height: '600px', overflow: 'auto' }}>
              <Gantt
                tasks={ganttTasks}
                viewMode={viewMode}
                onDateChange={handleTaskChange}
                onProgressChange={handleProgressChange}
                onClick={handleTaskClick}
                onDoubleClick={handleTaskDoubleClick}
              />
            </div>
          ) : (
            <div style={{ 
              textAlign: 'center', 
              padding: '60px 0',
              color: '#999'
            }}>
              {loading ? '正在加载任务数据...' : '暂无任务数据，请先在任务管理页面创建任务'}
            </div>
          )}
        </Card>
      </Content>
    </Layout>
  );
};

export default GanttPage;
