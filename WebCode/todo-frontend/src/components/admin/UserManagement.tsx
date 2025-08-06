import React, { useState, useEffect } from 'react';
import {
  Card,
  Table,
  Button,
  Input,
  Select,
  Space,
  Tag,
  Modal,
  Form,
  message,
  Row,
  Col,
  Statistic
} from 'antd';
import {
  UserOutlined,
  EditOutlined,
  EyeOutlined,
  ReloadOutlined
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { adminApi } from '../../services/api';

const { Option } = Select;

interface AdminUser {
  id: string;
  email: string;
  name: string;
  phone?: string;
  status: string;
  role: string;
  emailVerified: boolean;
  phoneVerified: boolean;
  lastLoginAt?: string;
  createdAt: string;
  taskCount: number;
}

interface UserStats {
  totalUsers: number;
  activeUsers: number;
  adminUsers: number;
  todayRegistrations: number;
}

const UserManagement: React.FC = () => {
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [loading, setLoading] = useState(false);
  const [stats, setStats] = useState<UserStats>({
    totalUsers: 0,
    activeUsers: 0,
    adminUsers: 0,
    todayRegistrations: 0
  });
  const [pagination, setPagination] = useState({
    current: 1,
    pageSize: 20,
    total: 0
  });
  const [filters, setFilters] = useState({
    search: '',
    role: '',
    status: ''
  });
  const [editModalVisible, setEditModalVisible] = useState(false);
  const [selectedUser, setSelectedUser] = useState<AdminUser | null>(null);
  const [form] = Form.useForm();

  // 加载用户列表
  const loadUsers = async () => {
    setLoading(true);
    try {
      const response = await adminApi.getUsers({
        page: pagination.current,
        pageSize: pagination.pageSize,
        search: filters.search || undefined,
        role: filters.role || undefined,
        status: filters.status || undefined
      });

      if (response.success && response.data) {
        setUsers(response.data.items);
        setPagination(prev => ({
          ...prev,
          total: response.data!.totalCount
        }));
      }
    } catch (error) {
      message.error('加载用户列表失败');
    } finally {
      setLoading(false);
    }
  };

  // 加载统计信息
  const loadStats = async () => {
    try {
      const response = await adminApi.getStats();
      if (response.success && response.data) {
        setStats({
          totalUsers: response.data.totalUsers,
          activeUsers: response.data.activeUsers,
          adminUsers: response.data.adminUsers,
          todayRegistrations: response.data.todayRegistrations
        });
      }
    } catch (error) {
      console.error('加载统计信息失败:', error);
    }
  };

  useEffect(() => {
    loadUsers();
    loadStats();
  }, [pagination.current, pagination.pageSize, filters]);

  // 搜索处理
  const handleSearch = (value: string) => {
    setFilters(prev => ({ ...prev, search: value }));
    setPagination(prev => ({ ...prev, current: 1 }));
  };

  // 筛选处理
  const handleFilter = (key: string, value: string) => {
    setFilters(prev => ({ ...prev, [key]: value }));
    setPagination(prev => ({ ...prev, current: 1 }));
  };

  // 编辑用户
  const handleEditUser = (user: AdminUser) => {
    setSelectedUser(user);
    form.setFieldsValue({
      role: user.role,
      status: user.status
    });
    setEditModalVisible(true);
  };

  // 保存用户编辑
  const handleSaveUser = async () => {
    if (!selectedUser) return;

    try {
      const values = await form.validateFields();
      
      // 更新角色
      if (values.role !== selectedUser.role) {
        await adminApi.updateUserRole(selectedUser.id, { role: values.role });
      }
      
      // 更新状态
      if (values.status !== selectedUser.status) {
        await adminApi.updateUserStatus(selectedUser.id, { status: values.status });
      }

      message.success('用户信息更新成功');
      setEditModalVisible(false);
      loadUsers();
    } catch (error) {
      message.error('更新用户信息失败');
    }
  };

  // 查看用户详情
  const handleViewUser = async (userId: string) => {
    try {
      const response = await adminApi.getUserDetail(userId);
      if (response.success && response.data) {
        Modal.info({
          title: '用户详情',
          width: 600,
          content: (
            <div>
              <p><strong>用户ID:</strong> {response.data.id}</p>
              <p><strong>邮箱:</strong> {response.data.email}</p>
              <p><strong>姓名:</strong> {response.data.name}</p>
              <p><strong>手机:</strong> {response.data.phone || '未设置'}</p>
              <p><strong>角色:</strong> <Tag color={response.data.role === 'admin' ? 'red' : 'blue'}>{response.data.role}</Tag></p>
              <p><strong>状态:</strong> <Tag color={response.data.status?.toLowerCase() === 'active' ? 'green' : 'red'}>{response.data.status?.toLowerCase() === 'active' ? '活跃' : '禁用'}</Tag></p>
              <p><strong>任务数量:</strong> {response.data.taskCount}</p>
              <p><strong>已完成任务:</strong> {response.data.completedTaskCount}</p>
              <p><strong>使用的邀请码:</strong> {response.data.invitationCodeUsed || '无'}</p>
              <p><strong>注册时间:</strong> {new Date(response.data.createdAt).toLocaleString()}</p>
              <p><strong>最后登录:</strong> {response.data.lastLoginAt ? new Date(response.data.lastLoginAt).toLocaleString() : '从未登录'}</p>
            </div>
          )
        });
      }
    } catch (error) {
      message.error('获取用户详情失败');
    }
  };

  const columns: ColumnsType<AdminUser> = [
    {
      title: '用户信息',
      key: 'userInfo',
      render: (_, record) => (
        <div>
          <div style={{ fontWeight: 'bold' }}>{record.name}</div>
          <div style={{ color: '#666', fontSize: '12px' }}>{record.email}</div>
        </div>
      ),
    },
    {
      title: '角色',
      dataIndex: 'role',
      key: 'role',
      render: (role: string) => (
        <Tag color={role === 'admin' ? 'red' : 'blue'}>
          {role === 'admin' ? '管理员' : '普通用户'}
        </Tag>
      ),
      filters: [
        { text: '管理员', value: 'admin' },
        { text: '普通用户', value: 'user' }
      ],
    },
    {
      title: '状态',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => {
        const normalizedStatus = status?.toLowerCase();
        return (
          <Tag color={normalizedStatus === 'active' ? 'green' : 'red'}>
            {normalizedStatus === 'active' ? '活跃' : '禁用'}
          </Tag>
        );
      },
      filters: [
        { text: '活跃', value: 'active' },
        { text: '禁用', value: 'inactive' }
      ],
    },
    {
      title: '任务数',
      dataIndex: 'taskCount',
      key: 'taskCount',
      sorter: true,
    },
    {
      title: '注册时间',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (date: string) => new Date(date).toLocaleDateString(),
      sorter: true,
    },
    {
      title: '最后登录',
      dataIndex: 'lastLoginAt',
      key: 'lastLoginAt',
      render: (date?: string) => date ? new Date(date).toLocaleDateString() : '从未登录',
    },
    {
      title: '操作',
      key: 'actions',
      render: (_, record) => (
        <Space>
          <Button
            type="link"
            icon={<EyeOutlined />}
            onClick={() => handleViewUser(record.id)}
          >
            查看
          </Button>
          <Button
            type="link"
            icon={<EditOutlined />}
            onClick={() => handleEditUser(record)}
          >
            编辑
          </Button>
        </Space>
      ),
    },
  ];

  return (
    <div>
      {/* 统计卡片 */}
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={6}>
          <Card>
            <Statistic
              title="总用户数"
              value={stats.totalUsers}
              prefix={<UserOutlined />}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="活跃用户"
              value={stats.activeUsers}
              valueStyle={{ color: '#3f8600' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="管理员"
              value={stats.adminUsers}
              valueStyle={{ color: '#cf1322' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="今日注册"
              value={stats.todayRegistrations}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
      </Row>

      {/* 用户列表 */}
      <Card>
        <div style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col span={8}>
              <Input.Search
                placeholder="搜索用户名或邮箱"
                allowClear
                onSearch={handleSearch}
                style={{ width: '100%' }}
              />
            </Col>
            <Col span={4}>
              <Select
                placeholder="角色筛选"
                allowClear
                style={{ width: '100%' }}
                onChange={(value) => handleFilter('role', value || '')}
              >
                <Option value="admin">管理员</Option>
                <Option value="user">普通用户</Option>
              </Select>
            </Col>
            <Col span={4}>
              <Select
                placeholder="状态筛选"
                allowClear
                style={{ width: '100%' }}
                onChange={(value) => handleFilter('status', value || '')}
              >
                <Option value="active">活跃</Option>
                <Option value="inactive">禁用</Option>
              </Select>
            </Col>
            <Col span={4}>
              <Button
                icon={<ReloadOutlined />}
                onClick={() => {
                  loadUsers();
                  loadStats();
                }}
              >
                刷新
              </Button>
            </Col>
          </Row>
        </div>

        <Table
          columns={columns}
          dataSource={users}
          rowKey="id"
          loading={loading}
          pagination={{
            current: pagination.current,
            pageSize: pagination.pageSize,
            total: pagination.total,
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total, range) => `第 ${range[0]}-${range[1]} 条，共 ${total} 条`,
            onChange: (page, pageSize) => {
              setPagination(prev => ({
                ...prev,
                current: page,
                pageSize: pageSize || 20
              }));
            }
          }}
        />
      </Card>

      {/* 编辑用户模态框 */}
      <Modal
        title="编辑用户"
        open={editModalVisible}
        onOk={handleSaveUser}
        onCancel={() => setEditModalVisible(false)}
        okText="保存"
        cancelText="取消"
      >
        <Form form={form} layout="vertical">
          <Form.Item
            name="role"
            label="角色"
            rules={[{ required: true, message: '请选择角色' }]}
          >
            <Select>
              <Option value="user">普通用户</Option>
              <Option value="admin">管理员</Option>
            </Select>
          </Form.Item>
          <Form.Item
            name="status"
            label="状态"
            rules={[{ required: true, message: '请选择状态' }]}
          >
            <Select>
              <Option value="active">活跃</Option>
              <Option value="inactive">禁用</Option>
            </Select>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default UserManagement;
