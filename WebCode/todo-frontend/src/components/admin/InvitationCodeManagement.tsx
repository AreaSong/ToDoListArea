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
  Popconfirm,
  Row,
  Col,
  Statistic,
  DatePicker,
  InputNumber
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  ReloadOutlined,
  CopyOutlined
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import { invitationCodeApi } from '../../services/api';

const { Option } = Select;

interface InvitationCodeItem {
  id: string;
  code: string;
  maxUses: number;
  usedCount: number;
  remainingUses: number;
  expiresAt?: string;
  isExpired: boolean;
  isAvailable: boolean;
  status: string;
  createdBy: string;
  createdByName: string;
  createdAt: string;
  updatedAt: string;
}

interface InvitationCodeStats {
  totalCodes: number;
  activeCodes: number;
  disabledCodes: number;
  expiredCodes: number;
  totalUsages: number;
  todayUsages: number;
  weekUsages: number;
  monthUsages: number;
}

const InvitationCodeManagement: React.FC = () => {
  const [codes, setCodes] = useState<InvitationCodeItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [stats, setStats] = useState<InvitationCodeStats>({
    totalCodes: 0,
    activeCodes: 0,
    disabledCodes: 0,
    expiredCodes: 0,
    totalUsages: 0,
    todayUsages: 0,
    weekUsages: 0,
    monthUsages: 0
  });
  const [pagination, setPagination] = useState({
    current: 1,
    pageSize: 20,
    total: 0
  });
  const [filters, setFilters] = useState({
    search: '',
    status: ''
  });
  const [createModalVisible, setCreateModalVisible] = useState(false);
  const [editModalVisible, setEditModalVisible] = useState(false);
  const [selectedCode, setSelectedCode] = useState<InvitationCodeItem | null>(null);
  const [form] = Form.useForm();

  // 加载邀请码列表
  const loadCodes = async () => {
    setLoading(true);
    try {
      const response = await invitationCodeApi.getList({
        page: pagination.current,
        pageSize: pagination.pageSize,
        status: filters.status,
        search: filters.search
      });

      if (response.success && response.data) {
        setCodes(response.data.items || []);
        setPagination(prev => ({
          ...prev,
          total: response.data.totalCount || 0
        }));
      } else {
        message.error(response.message || '加载邀请码列表失败');
      }
    } catch (error: any) {
      console.error('加载邀请码列表失败:', error);
      message.error('加载邀请码列表失败，请检查网络连接');
    } finally {
      setLoading(false);
    }
  };

  const loadStats = async () => {
    try {
      const response = await invitationCodeApi.getStats();

      if (response.success && response.data) {
        setStats(response.data);
      } else {
        console.error('加载统计信息失败:', response.message);
      }
    } catch (error: any) {
      console.error('加载统计信息失败:', error);
      // 如果统计信息加载失败，不显示错误消息，因为这不是关键功能
    }
  };

  useEffect(() => {
    loadCodes();
    loadStats();
  }, [pagination.current, pagination.pageSize, filters]);

  // 创建邀请码
  const handleCreateCode = async () => {
    try {
      const values = await form.validateFields();

      const createData = {
        code: values.code,
        maxUses: values.maxUses,
        expiresAt: values.expiresAt ? values.expiresAt.toISOString() : undefined
      };

      const response = await invitationCodeApi.create(createData);

      if (response.success) {
        message.success('邀请码创建成功');
        setCreateModalVisible(false);
        form.resetFields();
        loadCodes();
        loadStats();
      } else {
        message.error(response.message || '创建邀请码失败');
      }
    } catch (error: any) {
      console.error('创建邀请码失败:', error);
      message.error('创建邀请码失败，请检查网络连接');
    }
  };

  // 编辑邀请码
  const handleEditCode = (code: InvitationCodeItem) => {
    setSelectedCode(code);
    form.setFieldsValue({
      maxUses: code.maxUses,
      expiresAt: code.expiresAt ? dayjs(code.expiresAt) : undefined,
      status: code.status
    });
    setEditModalVisible(true);
  };

  // 保存编辑
  const handleSaveEdit = async () => {
    if (!selectedCode) return;

    try {
      const values = await form.validateFields();

      const updateData = {
        maxUses: values.maxUses,
        expiresAt: values.expiresAt ? values.expiresAt.toISOString() : undefined
      };

      const response = await invitationCodeApi.update(selectedCode.id, updateData);

      if (response.success) {
        message.success('邀请码更新成功');
        setEditModalVisible(false);
        loadCodes();
        loadStats();
      } else {
        message.error(response.message || '更新邀请码失败');
      }
    } catch (error: any) {
      console.error('更新邀请码失败:', error);
      message.error('更新邀请码失败，请检查网络连接');
    }
  };

  // 删除邀请码
  const handleDeleteCode = async (codeId: string) => {
    try {
      const response = await invitationCodeApi.delete(codeId);

      if (response.success) {
        message.success('邀请码删除成功');
        loadCodes();
        loadStats();
      } else {
        message.error(response.message || '删除邀请码失败');
      }
    } catch (error: any) {
      console.error('删除邀请码失败:', error);
      message.error('删除邀请码失败，请检查网络连接');
    }
  };

  // 复制邀请码
  const handleCopyCode = (code: string) => {
    navigator.clipboard.writeText(code).then(() => {
      message.success('邀请码已复制到剪贴板');
    }).catch(() => {
      message.error('复制失败');
    });
  };

  // 切换状态
  const handleToggleStatus = async (codeId: string, enabled: boolean) => {
    try {
      const response = await invitationCodeApi.setStatus(codeId, enabled);

      if (response.success) {
        message.success(`邀请码已${enabled ? '启用' : '禁用'}`);
        loadCodes();
        loadStats();
      } else {
        message.error(response.message || '操作失败');
      }
    } catch (error: any) {
      console.error('切换邀请码状态失败:', error);
      message.error('操作失败，请检查网络连接');
    }
  };

  const columns: ColumnsType<InvitationCodeItem> = [
    {
      title: '邀请码',
      dataIndex: 'code',
      key: 'code',
      render: (code: string) => (
        <Space>
          <code style={{ 
            background: '#f5f5f5', 
            padding: '2px 6px', 
            borderRadius: '4px',
            fontFamily: 'monospace'
          }}>
            {code}
          </code>
          <Button
            type="link"
            size="small"
            icon={<CopyOutlined />}
            onClick={() => handleCopyCode(code)}
          />
        </Space>
      ),
    },
    {
      title: '使用情况',
      key: 'usage',
      render: (_, record) => (
        <div>
          <div>已用: {record.usedCount} / {record.maxUses}</div>
          <div style={{ color: '#666', fontSize: '12px' }}>
            剩余: {record.remainingUses}
          </div>
        </div>
      ),
    },
    {
      title: '状态',
      key: 'status',
      render: (_, record) => {
        if (record.isExpired) {
          return <Tag color="red">已过期</Tag>;
        }
        if (!record.isAvailable) {
          return <Tag color="orange">已用完</Tag>;
        }
        return (
          <Tag color={record.status === 'active' ? 'green' : 'red'}>
            {record.status === 'active' ? '启用' : '禁用'}
          </Tag>
        );
      },
    },
    {
      title: '过期时间',
      dataIndex: 'expiresAt',
      key: 'expiresAt',
      render: (date?: string) => date ? new Date(date).toLocaleDateString() : '永不过期',
    },
    {
      title: '创建者',
      dataIndex: 'createdByName',
      key: 'createdByName',
    },
    {
      title: '创建时间',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (date: string) => new Date(date).toLocaleDateString(),
    },
    {
      title: '操作',
      key: 'actions',
      render: (_, record) => (
        <Space>
          <Button
            type="link"
            icon={<EditOutlined />}
            onClick={() => handleEditCode(record)}
          >
            编辑
          </Button>
          <Button
            type="link"
            onClick={() => handleToggleStatus(record.id, record.status !== 'active')}
          >
            {record.status === 'active' ? '禁用' : '启用'}
          </Button>
          <Popconfirm
            title="确定要删除这个邀请码吗？"
            onConfirm={() => handleDeleteCode(record.id)}
            okText="确定"
            cancelText="取消"
          >
            <Button
              type="link"
              danger
              icon={<DeleteOutlined />}
            >
              删除
            </Button>
          </Popconfirm>
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
              title="总邀请码"
              value={stats.totalCodes}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="活跃邀请码"
              value={stats.activeCodes}
              valueStyle={{ color: '#3f8600' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="总使用次数"
              value={stats.totalUsages}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="今日使用"
              value={stats.todayUsages}
              valueStyle={{ color: '#722ed1' }}
            />
          </Card>
        </Col>
      </Row>

      {/* 邀请码列表 */}
      <Card>
        <div style={{ marginBottom: 16 }}>
          <Row gutter={16}>
            <Col span={8}>
              <Input.Search
                placeholder="搜索邀请码"
                allowClear
                onSearch={(value) => setFilters(prev => ({ ...prev, search: value }))}
                style={{ width: '100%' }}
              />
            </Col>
            <Col span={4}>
              <Select
                placeholder="状态筛选"
                allowClear
                style={{ width: '100%' }}
                onChange={(value) => setFilters(prev => ({ ...prev, status: value || '' }))}
              >
                <Option value="active">启用</Option>
                <Option value="disabled">禁用</Option>
              </Select>
            </Col>
            <Col span={4}>
              <Button
                icon={<ReloadOutlined />}
                onClick={() => {
                  loadCodes();
                  loadStats();
                }}
              >
                刷新
              </Button>
            </Col>
            <Col span={4}>
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={() => setCreateModalVisible(true)}
              >
                创建邀请码
              </Button>
            </Col>
          </Row>
        </div>

        <Table
          columns={columns}
          dataSource={codes}
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

      {/* 创建邀请码模态框 */}
      <Modal
        title="创建邀请码"
        open={createModalVisible}
        onOk={handleCreateCode}
        onCancel={() => {
          setCreateModalVisible(false);
          form.resetFields();
        }}
        okText="创建"
        cancelText="取消"
      >
        <Form form={form} layout="vertical">
          <Form.Item
            name="code"
            label="邀请码（可选，留空自动生成）"
          >
            <Input placeholder="留空将自动生成" />
          </Form.Item>
          <Form.Item
            name="maxUses"
            label="最大使用次数"
            rules={[{ required: true, message: '请输入最大使用次数' }]}
            initialValue={1}
          >
            <InputNumber min={1} max={10000} style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item
            name="expiresAt"
            label="过期时间（可选）"
          >
            <DatePicker 
              showTime 
              style={{ width: '100%' }} 
              placeholder="留空表示永不过期"
            />
          </Form.Item>
        </Form>
      </Modal>

      {/* 编辑邀请码模态框 */}
      <Modal
        title="编辑邀请码"
        open={editModalVisible}
        onOk={handleSaveEdit}
        onCancel={() => setEditModalVisible(false)}
        okText="保存"
        cancelText="取消"
      >
        <Form form={form} layout="vertical">
          <Form.Item
            name="maxUses"
            label="最大使用次数"
            rules={[{ required: true, message: '请输入最大使用次数' }]}
          >
            <InputNumber min={1} max={10000} style={{ width: '100%' }} />
          </Form.Item>
          <Form.Item
            name="expiresAt"
            label="过期时间"
          >
            <DatePicker 
              showTime 
              style={{ width: '100%' }} 
              placeholder="留空表示永不过期"
            />
          </Form.Item>
          <Form.Item
            name="status"
            label="状态"
            rules={[{ required: true, message: '请选择状态' }]}
          >
            <Select>
              <Option value="active">启用</Option>
              <Option value="disabled">禁用</Option>
            </Select>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default InvitationCodeManagement;
