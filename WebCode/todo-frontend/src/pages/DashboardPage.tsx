import React, { useState, useEffect } from 'react';
import {
  Layout,
  Card,
  Button,
  Table,
  Tag,
  Space,
  Modal,
  Form,
  Input,
  Select,
  DatePicker,
  message,
  Typography,
  Avatar,
  Dropdown,
  Empty,
  type MenuProps
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  UserOutlined,
  LogoutOutlined,
  SettingOutlined,
  BellOutlined,
  BarChartOutlined,
  FileTextOutlined,
  ClockCircleOutlined,
  EyeOutlined
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { taskApi, categoryApi } from '../services/api';
import type { Task, TaskCreateDto, TaskCategory, User, TaskStatus, TaskPriority } from '../types/api';
import { TaskPriorityLabels } from '../types/api';
import dayjs from 'dayjs';
import { LoadingSpinner } from '../components/LoadingSpinner';
import ConfirmDialog from '../components/ConfirmDialog';
import SuccessFeedback, { SuccessTypes } from '../components/SuccessFeedback';
import EnhancedEmpty, { EmptyPresets } from '../components/EnhancedEmpty';
import { feedback } from '../utils/feedbackManager';
import { useAuth } from '../contexts/AuthContext';
import { AdminOnly } from '../components/auth/PermissionGuard';

const { Header, Content } = Layout;
const { Title } = Typography;
const { Option } = Select;

const DashboardPage: React.FC = () => {
  const [tasks, setTasks] = useState<Task[]>([]);
  const [categories, setCategories] = useState<TaskCategory[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [editingTask, setEditingTask] = useState<Task | null>(null);
  const [form] = Form.useForm();
  const [searchKeyword, setSearchKeyword] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [priorityFilter, setPriorityFilter] = useState<string>('');
  const [categoryFilter, setCategoryFilter] = useState<string>('');
  const [confirmDialogVisible, setConfirmDialogVisible] = useState(false);
  const [successFeedbackVisible, setSuccessFeedbackVisible] = useState(false);
  const [successFeedbackConfig, setSuccessFeedbackConfig] = useState<any>({});
  const [taskToDelete, setTaskToDelete] = useState<string | null>(null);
  const navigate = useNavigate();
  const { user: currentUser, logout, isAdmin } = useAuth();

  // 页面加载时获取数据
  useEffect(() => {
    fetchTasks();
    fetchCategories();
  }, []);

  // 过滤器变化时重新获取数据
  useEffect(() => {
    fetchTasks();
  }, [statusFilter, priorityFilter, categoryFilter]);

  // 退出登录
  const handleLogout = async () => {
    try {
      await logout();
      navigate('/login');
    } catch (error) {
      console.error('Logout error:', error);
    }
  };

  // 用户菜单
  const userMenuItems: MenuProps['items'] = [
    {
      key: 'profile',
      icon: <SettingOutlined />,
      label: '个人设置',
      onClick: () => navigate('/profile')
    },
    ...(isAdmin() ? [{
      key: 'admin',
      icon: <BarChartOutlined />,
      label: '管理员控制面板',
      onClick: () => navigate('/admin')
    }] : []),
    {
      key: 'logout',
      icon: <LogoutOutlined />,
      label: '退出登录',
      onClick: handleLogout
    }
  ];

  // 获取任务列表
  const fetchTasks = async () => {
    if (!currentUser?.id) return;

    setLoading(true);
    try {
      const response = await taskApi.getTasks(currentUser.id, {
        pageNumber: 1,
        pageSize: 50,
        sortBy: 'CreatedAt',
        sortOrder: 'desc',
        searchKeyword: searchKeyword || undefined,
        status: statusFilter || undefined,
        priority: priorityFilter || undefined,
        categoryId: categoryFilter || undefined
      });

      if (response.success && response.data) {
        setTasks(response.data.items);
      }
    } catch (error) {
      feedback.loadError('任务列表');
    } finally {
      setLoading(false);
    }
  };

  // 获取分类列表
  const fetchCategories = async () => {
    try {
      const response = await categoryApi.getCategories();
      if (response.success && response.data) {
        setCategories(response.data);
      }
    } catch (error) {
      feedback.loadError('分类列表');
    }
  };

  // 创建/更新任务
  const handleSubmit = async (values: any) => {
    try {
      const taskData: TaskCreateDto = {
        title: values.title,
        description: values.description,
        status: values.status || 'Pending',
        priority: values.priority || 'Medium',
        startTime: values.startTime?.toISOString(),
        endTime: values.endTime?.toISOString(),
        estimatedDuration: values.estimatedDuration,
        categoryId: values.categoryId
      };

      if (editingTask) {
        // 更新任务
        const response = await taskApi.updateTask(editingTask.id, taskData);
        if (response.success) {
          feedback.taskUpdated();
          fetchTasks();
        }
      } else {
        // 创建任务
        const response = await taskApi.createTask(currentUser.id, taskData);
        if (response.success) {
          // 显示成功反馈
          setSuccessFeedbackConfig({
            ...SuccessTypes.taskCreated,
            onClose: () => setSuccessFeedbackVisible(false)
          });
          setSuccessFeedbackVisible(true);
          fetchTasks();
        }
      }

      setModalVisible(false);
      setEditingTask(null);
      form.resetFields();
    } catch (error) {
      feedback.operationError(editingTask ? '任务更新' : '任务创建', error);
    }
  };

  // 删除任务
  const handleDelete = async (taskId: string, taskTitle: string) => {
    setTaskToDelete(taskId);
    setConfirmDialogVisible(true);
  };

  // 确认删除任务
  const confirmDeleteTask = async () => {
    if (!taskToDelete) return;

    try {
      const response = await taskApi.deleteTask(taskToDelete);
      if (response.success) {
        feedback.taskDeleted();
        fetchTasks();
      }
    } catch (error) {
      feedback.operationError('任务删除', error);
    } finally {
      setConfirmDialogVisible(false);
      setTaskToDelete(null);
    }
  };

  // 编辑任务
  const handleEdit = (task: Task) => {
    setEditingTask(task);
    form.setFieldsValue({
      title: task.title,
      description: task.description,
      status: task.status,
      priority: task.priority,
      startTime: task.startTime ? dayjs(task.startTime) : null,
      endTime: task.endTime ? dayjs(task.endTime) : null,
      estimatedDuration: task.estimatedDuration,
      categoryId: task.categoryId
    });
    setModalVisible(true);
  };

  // 新建任务
  const handleCreate = () => {
    setEditingTask(null);
    form.resetFields();
    setModalVisible(true);
  };

  // 快速切换任务状态
  const handleStatusChange = async (taskId: string, newStatus: TaskStatus) => {
    try {
      const response = await taskApi.updateTask(taskId, { status: newStatus });
      if (response.success) {
        if (newStatus === 'Completed') {
          // 任务完成时显示庆祝反馈
          setSuccessFeedbackConfig({
            ...SuccessTypes.taskCompleted,
            onClose: () => setSuccessFeedbackVisible(false)
          });
          setSuccessFeedbackVisible(true);
        } else {
          feedback.operationSuccess('状态更新');
        }
        fetchTasks();
      }
    } catch (error) {
      feedback.operationError('状态更新', error);
    }
  };

  // 获取优先级标签颜色
  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'High': return 'red';
      case 'Medium': return 'orange';
      case 'Low': return 'green';
      default: return 'default';
    }
  };

  // 获取状态标签颜色
  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Completed': return 'success';
      case 'InProgress': return 'processing';
      case 'Pending': return 'warning';
      default: return 'default';
    }
  };

  // 表格列定义
  const columns = [
    {
      title: '任务标题',
      dataIndex: 'title',
      key: 'title',
      ellipsis: true,
    },
    {
      title: '状态',
      dataIndex: 'status',
      key: 'status',
      render: (status: TaskStatus, record: Task) => (
        <Select
          value={status}
          style={{ width: 100 }}
          size="small"
          onChange={(newStatus) => handleStatusChange(record.id, newStatus)}
        >
          <Option value="Pending">
            <Tag color={getStatusColor('Pending')}>待处理</Tag>
          </Option>
          <Option value="InProgress">
            <Tag color={getStatusColor('InProgress')}>进行中</Tag>
          </Option>
          <Option value="Completed">
            <Tag color={getStatusColor('Completed')}>已完成</Tag>
          </Option>
        </Select>
      ),
    },
    {
      title: '优先级',
      dataIndex: 'priority',
      key: 'priority',
      render: (priority: TaskPriority) => (
        <Tag color={getPriorityColor(priority)}>
          {TaskPriorityLabels[priority]}
        </Tag>
      ),
    },
    {
      title: '分类',
      dataIndex: 'categoryName',
      key: 'categoryName',
      render: (categoryName: string) => categoryName || '无分类',
    },
    {
      title: '创建时间',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (date: string) => dayjs(date).format('YYYY-MM-DD HH:mm'),
    },
    {
      title: '操作',
      key: 'action',
      render: (_: any, record: Task) => (
        <Space size="middle">
          <Button
            type="link"
            icon={<EyeOutlined />}
            onClick={() => navigate(`/task/${record.id}`)}
          >
            详情
          </Button>
          <Button
            type="link"
            icon={<EditOutlined />}
            onClick={() => handleEdit(record)}
          >
            编辑
          </Button>
          <Button
            type="link"
            danger
            icon={<DeleteOutlined />}
            onClick={() => handleDelete(record.id, record.title)}
          >
            删除
          </Button>
        </Space>
      ),
    },
  ];

  useEffect(() => {
    fetchTasks();
    fetchCategories();
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
        <Title level={3} style={{ margin: 0, color: '#1890ff' }}>
          智能提醒事项管理
        </Title>
        
        <Space>
          <Button type="text" icon={<BellOutlined />} />
          <AdminOnly>
            <Button
              type="primary"
              icon={<BarChartOutlined />}
              onClick={() => navigate('/admin')}
            >
              管理员控制面板
            </Button>
          </AdminOnly>
          <Dropdown menu={{ items: userMenuItems }} placement="bottomRight">
            <Space style={{ cursor: 'pointer' }}>
              <Avatar icon={<UserOutlined />} />
              <span>{currentUser?.name}</span>
            </Space>
          </Dropdown>
        </Space>
      </Header>

      <Content style={{ padding: '24px' }}>
        <Card>
          <div style={{ marginBottom: 16, display: 'flex', justifyContent: 'space-between' }}>
            <Title level={4}>任务列表</Title>
            <Space>
              <Button
                icon={<BarChartOutlined />}
                onClick={() => navigate('/gantt')}
              >
                甘特图
              </Button>
              <Button
                icon={<ClockCircleOutlined />}
                onClick={() => navigate('/activity')}
              >
                活动记录
              </Button>
              <Button
                icon={<FileTextOutlined />}
                onClick={() => navigate('/templates')}
              >
                模板管理
              </Button>
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={handleCreate}
              >
                新建任务
              </Button>
            </Space>
          </div>

          {/* 搜索和过滤器 */}
          <div style={{ marginBottom: 16, padding: '16px', background: '#fafafa', borderRadius: '6px' }}>
            <Space wrap size="middle">
              <Input.Search
                placeholder="搜索任务标题或描述"
                style={{ width: 250 }}
                value={searchKeyword}
                onChange={(e) => setSearchKeyword(e.target.value)}
                onSearch={fetchTasks}
                allowClear
              />

              <Select
                placeholder="状态筛选"
                style={{ width: 120 }}
                value={statusFilter}
                onChange={setStatusFilter}
                allowClear
              >
                <Option value="Pending">待处理</Option>
                <Option value="InProgress">进行中</Option>
                <Option value="Completed">已完成</Option>
              </Select>

              <Select
                placeholder="优先级筛选"
                style={{ width: 120 }}
                value={priorityFilter}
                onChange={setPriorityFilter}
                allowClear
              >
                <Option value="High">高</Option>
                <Option value="Medium">中</Option>
                <Option value="Low">低</Option>
              </Select>

              <Select
                placeholder="分类筛选"
                style={{ width: 150 }}
                value={categoryFilter}
                onChange={setCategoryFilter}
                allowClear
              >
                {categories.map(category => (
                  <Option key={category.id} value={category.id}>
                    {category.name}
                  </Option>
                ))}
              </Select>

              <Button onClick={fetchTasks}>刷新</Button>
            </Space>
          </div>

          <LoadingSpinner spinning={loading} tip="加载任务列表中...">
            <Table
              columns={columns}
              dataSource={tasks}
              rowKey="id"
              pagination={{
                pageSize: 10,
                showSizeChanger: true,
                showQuickJumper: true,
                showTotal: (total) => `共 ${total} 条记录`,
              }}
              locale={{
                emptyText: (
                  <EnhancedEmpty
                    {...EmptyPresets.noTasks(handleCreate)}
                    showTips={true}
                  />
                )
              }}
            />
          </LoadingSpinner>
        </Card>

        {/* 任务创建/编辑模态框 */}
        <Modal
          title={editingTask ? '编辑任务' : '新建任务'}
          open={modalVisible}
          onCancel={() => {
            setModalVisible(false);
            setEditingTask(null);
            form.resetFields();
          }}
          footer={null}
          width={600}
        >
          <Form
            form={form}
            layout="vertical"
            onFinish={handleSubmit}
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
                <Option value="Pending">待处理</Option>
                <Option value="InProgress">进行中</Option>
                <Option value="Completed">已完成</Option>
              </Select>
            </Form.Item>

            <Form.Item
              name="priority"
              label="优先级"
              initialValue="Medium"
            >
              <Select>
                <Option value="High">高</Option>
                <Option value="Medium">中</Option>
                <Option value="Low">低</Option>
              </Select>
            </Form.Item>

            <Form.Item
              name="categoryId"
              label="任务分类"
            >
              <Select placeholder="请选择任务分类" allowClear>
                {categories.map(category => (
                  <Option key={category.id} value={category.id}>
                    {category.name}
                  </Option>
                ))}
              </Select>
            </Form.Item>

            <Space>
              <Form.Item
                name="startTime"
                label="开始时间"
              >
                <DatePicker showTime placeholder="选择开始时间" />
              </Form.Item>

              <Form.Item
                name="endTime"
                label="结束时间"
              >
                <DatePicker showTime placeholder="选择结束时间" />
              </Form.Item>
            </Space>

            <Form.Item
              name="estimatedDuration"
              label="预计时长（分钟）"
            >
              <Input type="number" placeholder="请输入预计时长" />
            </Form.Item>

            <Form.Item style={{ marginBottom: 0, textAlign: 'right' }}>
              <Space>
                <Button onClick={() => setModalVisible(false)}>
                  取消
                </Button>
                <Button type="primary" htmlType="submit">
                  {editingTask ? '更新' : '创建'}
                </Button>
              </Space>
            </Form.Item>
          </Form>
        </Modal>

        {/* 确认删除对话框 */}
        <ConfirmDialog
          visible={confirmDialogVisible}
          title="确认删除任务"
          content="确定要删除这个任务吗？此操作不可恢复。"
          type="danger"
          confirmText="删除"
          cancelText="取消"
          onConfirm={confirmDeleteTask}
          onCancel={() => {
            setConfirmDialogVisible(false);
            setTaskToDelete(null);
          }}
          consequences={[
            '任务及其所有相关数据将被永久删除',
            '任务的历史记录将无法恢复',
            '相关的提醒和依赖关系将被清除'
          ]}
        />

        {/* 成功反馈对话框 */}
        <SuccessFeedback
          visible={successFeedbackVisible}
          {...successFeedbackConfig}
        />
      </Content>
    </Layout>
  );
};

export default DashboardPage;
