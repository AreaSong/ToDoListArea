import React, { useState, useEffect } from 'react';
import {
  Layout,
  Card,
  Button,
  Table,
  Space,
  message,
  Modal,
  Form,
  Input,
  Select,
  Tag,
  Statistic,
  Row,
  Col,
  Popconfirm,
  Tooltip,
  Empty,
  DatePicker,
  Typography,
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  CopyOutlined,
  BarChartOutlined,
  ReloadOutlined,
  ArrowLeftOutlined,
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { taskTemplateApi } from '../services/api';
import type { TaskTemplate, TaskTemplateCreate, TemplateUsageStats, CreateTaskFromTemplate, TaskTemplateData } from '../types/api';
import { useTheme } from '../contexts/ThemeContext';
import dayjs from 'dayjs';

const { Header, Content } = Layout;
const { Title, Text } = Typography;
const { Option } = Select;
const { TextArea } = Input;

interface User {
  id: string;
  name: string;
  email: string;
}

const TemplatesPage: React.FC = () => {
  const [templates, setTemplates] = useState<TaskTemplate[]>([]);
  const [categories, setCategories] = useState<string[]>([]);
  const [stats, setStats] = useState<TemplateUsageStats | null>(null);
  const [loading, setLoading] = useState(false);
  const [createModalVisible, setCreateModalVisible] = useState(false);
  const [editModalVisible, setEditModalVisible] = useState(false);
  const [useTemplateModalVisible, setUseTemplateModalVisible] = useState(false);
  const [selectedTemplate, setSelectedTemplate] = useState<TaskTemplate | null>(null);
  const [selectedCategory, setSelectedCategory] = useState<string>('');
  const [sortBy, setSortBy] = useState<string>('usage');
  const [createForm] = Form.useForm();
  const [editForm] = Form.useForm();
  const [useTemplateForm] = Form.useForm();
  const navigate = useNavigate();
  const { isDarkMode } = useTheme();

  // 获取当前用户信息
  const currentUser: User = JSON.parse(localStorage.getItem('user') || '{}');

  // 获取模板列表
  const fetchTemplates = async () => {
    if (!currentUser.id) return;

    setLoading(true);
    try {
      const response = await taskTemplateApi.getUserTemplates(currentUser.id, selectedCategory || undefined, sortBy);
      if (response.success && response.data) {
        setTemplates(response.data);
      } else {
        message.error(response.message || '获取模板列表失败');
      }
    } catch (error) {
      console.error('获取模板列表失败:', error);
      message.error('获取模板列表失败，请重试');
    } finally {
      setLoading(false);
    }
  };

  // 获取模板分类
  const fetchCategories = async () => {
    if (!currentUser.id) return;

    try {
      const response = await taskTemplateApi.getTemplateCategories(currentUser.id);
      if (response.success && response.data) {
        setCategories(response.data);
      }
    } catch (error) {
      console.error('获取模板分类失败:', error);
    }
  };

  // 获取使用统计
  const fetchStats = async () => {
    if (!currentUser.id) return;

    try {
      const response = await taskTemplateApi.getTemplateStats(currentUser.id);
      if (response.success && response.data) {
        setStats(response.data);
      }
    } catch (error) {
      console.error('获取统计信息失败:', error);
    }
  };

  useEffect(() => {
    if (currentUser.id) {
      fetchTemplates();
      fetchCategories();
      fetchStats();
    }
  }, [currentUser.id, selectedCategory, sortBy]);

  // 创建模板
  const handleCreateTemplate = async (values: any) => {
    try {
      const templateData: TaskTemplateData = {
        title: values.title,
        description: values.description,
        priority: values.priority || 'Medium',
        categoryId: values.categoryId,
        startTime: values.startTime?.toISOString(),
        endTime: values.endTime?.toISOString(),
        tags: values.tags || [],
        customFields: {}
      };

      const createData: TaskTemplateCreate = {
        userId: currentUser.id,
        name: values.name,
        description: values.templateDescription,
        templateData: JSON.stringify(templateData),
        category: values.category
      };

      const response = await taskTemplateApi.createTemplate(createData);
      if (response.success) {
        message.success('模板创建成功');
        setCreateModalVisible(false);
        createForm.resetFields();
        fetchTemplates();
        fetchStats();
      } else {
        message.error(response.message || '创建模板失败');
      }
    } catch (error) {
      console.error('创建模板失败:', error);
      message.error('创建模板失败，请重试');
    }
  };

  // 编辑模板
  const handleEditTemplate = async (values: any) => {
    if (!selectedTemplate) return;

    try {
      const templateData: TaskTemplateData = {
        title: values.title,
        description: values.description,
        priority: values.priority || 'Medium',
        categoryId: values.categoryId,
        startTime: values.startTime?.toISOString(),
        endTime: values.endTime?.toISOString(),
        tags: values.tags || [],
        customFields: {}
      };

      const updateData = {
        name: values.name,
        description: values.templateDescription,
        templateData: JSON.stringify(templateData),
        category: values.category
      };

      const response = await taskTemplateApi.updateTemplate(selectedTemplate.id, updateData);
      if (response.success) {
        message.success('模板更新成功');
        setEditModalVisible(false);
        editForm.resetFields();
        setSelectedTemplate(null);
        fetchTemplates();
      } else {
        message.error(response.message || '更新模板失败');
      }
    } catch (error) {
      console.error('更新模板失败:', error);
      message.error('更新模板失败，请重试');
    }
  };

  // 使用模板创建任务
  const handleUseTemplate = async (values: any) => {
    if (!selectedTemplate) return;

    try {
      const createData: CreateTaskFromTemplate = {
        userId: currentUser.id,
        customTitle: values.customTitle,
        customDescription: values.customDescription,
        startTime: values.startTime?.toISOString(),
        endTime: values.endTime?.toISOString()
      };

      const response = await taskTemplateApi.createTaskFromTemplate(selectedTemplate.id, createData);
      if (response.success) {
        message.success('从模板创建任务成功');
        setUseTemplateModalVisible(false);
        useTemplateForm.resetFields();
        setSelectedTemplate(null);
        fetchStats(); // 更新统计信息
      } else {
        message.error(response.message || '创建任务失败');
      }
    } catch (error) {
      console.error('从模板创建任务失败:', error);
      message.error('创建任务失败，请重试');
    }
  };

  // 打开编辑模板对话框
  const openEditModal = (template: TaskTemplate) => {
    setSelectedTemplate(template);

    try {
      const templateData: TaskTemplateData = JSON.parse(template.templateData);
      editForm.setFieldsValue({
        name: template.name,
        templateDescription: template.description,
        category: template.category,
        title: templateData.title,
        description: templateData.description,
        priority: templateData.priority,
        categoryId: templateData.categoryId,
        startTime: templateData.startTime ? dayjs(templateData.startTime) : undefined,
        endTime: templateData.endTime ? dayjs(templateData.endTime) : undefined,
        tags: templateData.tags
      });
    } catch (error) {
      console.error('解析模板数据失败:', error);
      message.error('模板数据格式错误');
      return;
    }

    setEditModalVisible(true);
  };

  // 打开使用模板对话框
  const openUseTemplateModal = (template: TaskTemplate) => {
    setSelectedTemplate(template);

    try {
      const templateData: TaskTemplateData = JSON.parse(template.templateData);
      useTemplateForm.setFieldsValue({
        customTitle: templateData.title,
        customDescription: templateData.description,
        startTime: templateData.startTime ? dayjs(templateData.startTime) : undefined,
        endTime: templateData.endTime ? dayjs(templateData.endTime) : undefined
      });
    } catch (error) {
      console.error('解析模板数据失败:', error);
      message.error('模板数据格式错误');
      return;
    }

    setUseTemplateModalVisible(true);
  };

  // 表格列定义
  const columns = [
    {
      title: '模板名称',
      dataIndex: 'name',
      key: 'name',
      render: (text: string, record: TaskTemplate) => (
        <div>
          <Text strong>{text}</Text>
          {record.description && (
            <div style={{ fontSize: '12px', color: '#666', marginTop: '4px' }}>
              {record.description}
            </div>
          )}
        </div>
      ),
    },
    {
      title: '分类',
      dataIndex: 'category',
      key: 'category',
      render: (category: string) => category ? <Tag color="blue">{category}</Tag> : <Text type="secondary">无分类</Text>,
    },
    {
      title: '使用次数',
      dataIndex: 'usageCount',
      key: 'usageCount',
      sorter: (a: TaskTemplate, b: TaskTemplate) => a.usageCount - b.usageCount,
      render: (count: number) => (
        <Tag color={count > 0 ? 'green' : 'default'}>
          {count} 次
        </Tag>
      ),
    },
    {
      title: '创建时间',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (date: string) => dayjs(date).format('YYYY-MM-DD HH:mm'),
    },
    {
      title: '操作',
      key: 'actions',
      render: (_: any, record: TaskTemplate) => (
        <Space>
          <Tooltip title="使用模板">
            <Button
              type="primary"
              size="small"
              icon={<CopyOutlined />}
              onClick={() => openUseTemplateModal(record)}
            >
              使用
            </Button>
          </Tooltip>
          <Tooltip title="编辑模板">
            <Button
              size="small"
              icon={<EditOutlined />}
              onClick={() => openEditModal(record)}
            />
          </Tooltip>
          <Popconfirm
            title="确定要删除这个模板吗？"
            onConfirm={() => handleDeleteTemplate(record.id)}
            okText="确定"
            cancelText="取消"
          >
            <Tooltip title="删除模板">
              <Button
                size="small"
                danger
                icon={<DeleteOutlined />}
              />
            </Tooltip>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  // 删除模板
  const handleDeleteTemplate = async (templateId: string) => {
    try {
      const response = await taskTemplateApi.deleteTemplate(templateId);
      if (response.success) {
        message.success('模板删除成功');
        fetchTemplates();
        fetchStats();
      } else {
        message.error(response.message || '删除模板失败');
      }
    } catch (error) {
      console.error('删除模板失败:', error);
      message.error('删除模板失败，请重试');
    }
  };

  if (!currentUser.id) {
    return (
      <Layout style={{ minHeight: '100vh' }}>
        <Content style={{ display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
          <div>请先登录后再使用模板功能</div>
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
            任务模板管理
          </Title>
        </div>

        <Space>
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => setCreateModalVisible(true)}
          >
            新建模板
          </Button>
          <Button
            icon={<ReloadOutlined />}
            onClick={() => {
              fetchTemplates();
              fetchStats();
            }}
            loading={loading}
          >
            刷新
          </Button>
        </Space>
      </Header>

      <Content style={{ padding: '24px' }}>
        {/* 统计信息 */}
        {stats && (
          <Row gutter={16} style={{ marginBottom: 24 }}>
            <Col span={6}>
              <Card>
                <Statistic
                  title="总模板数"
                  value={stats.totalTemplates}
                  prefix={<BarChartOutlined />}
                />
              </Card>
            </Col>
            <Col span={6}>
              <Card>
                <Statistic
                  title="总使用次数"
                  value={stats.totalUsage}
                  prefix={<CopyOutlined />}
                />
              </Card>
            </Col>
            <Col span={6}>
              <Card>
                <Statistic
                  title="平均使用次数"
                  value={stats.averageUsage}
                  precision={1}
                />
              </Card>
            </Col>
            <Col span={6}>
              <Card>
                <Statistic
                  title="分类数量"
                  value={stats.categoriesCount}
                />
              </Card>
            </Col>
          </Row>
        )}

        {/* 筛选和排序 */}
        <Card style={{ marginBottom: 16 }}>
          <Row gutter={16} align="middle">
            <Col>
              <Text strong>筛选和排序：</Text>
            </Col>
            <Col>
              <Select
                placeholder="选择分类"
                style={{ width: 150 }}
                value={selectedCategory}
                onChange={setSelectedCategory}
                allowClear
              >
                {categories.map(category => (
                  <Option key={category} value={category}>{category}</Option>
                ))}
              </Select>
            </Col>
            <Col>
              <Select
                value={sortBy}
                onChange={setSortBy}
                style={{ width: 150 }}
              >
                <Option value="usage">按使用频率</Option>
                <Option value="created">按创建时间</Option>
                <Option value="name">按名称</Option>
              </Select>
            </Col>
          </Row>
        </Card>

        {/* 模板列表 */}
        <Card title="模板列表">
          {templates.length === 0 ? (
            <Empty
              description="暂无模板，点击上方按钮创建第一个模板"
              image={Empty.PRESENTED_IMAGE_SIMPLE}
            />
          ) : (
            <Table
              columns={columns}
              dataSource={templates}
              rowKey="id"
              loading={loading}
              pagination={{
                pageSize: 10,
                showSizeChanger: true,
                showQuickJumper: true,
                showTotal: (total) => `共 ${total} 个模板`,
              }}
            />
          )}
        </Card>

        {/* 创建模板模态框 */}
        <Modal
          title="创建任务模板"
          open={createModalVisible}
          onCancel={() => {
            setCreateModalVisible(false);
            createForm.resetFields();
          }}
          footer={null}
          width={600}
        >
          <Form
            form={createForm}
            layout="vertical"
            onFinish={handleCreateTemplate}
          >
            <Form.Item
              name="name"
              label="模板名称"
              rules={[
                { required: true, message: '请输入模板名称' },
                { max: 100, message: '模板名称不能超过100个字符' }
              ]}
            >
              <Input placeholder="请输入模板名称" />
            </Form.Item>

            <Form.Item
              name="templateDescription"
              label="模板描述"
              rules={[{ max: 500, message: '模板描述不能超过500个字符' }]}
            >
              <TextArea rows={3} placeholder="请输入模板描述（可选）" />
            </Form.Item>

            <Form.Item
              name="category"
              label="模板分类"
            >
              <Select placeholder="选择或输入分类" allowClear>
                {categories.map(category => (
                  <Option key={category} value={category}>{category}</Option>
                ))}
              </Select>
            </Form.Item>

            <Form.Item
              name="title"
              label="任务标题模板"
              rules={[{ required: true, message: '请输入任务标题模板' }]}
            >
              <Input placeholder="请输入任务标题模板" />
            </Form.Item>

            <Form.Item
              name="description"
              label="任务描述模板"
            >
              <TextArea rows={3} placeholder="请输入任务描述模板（可选）" />
            </Form.Item>

            <Form.Item
              name="priority"
              label="优先级"
              initialValue="Medium"
            >
              <Select>
                <Option value="Low">低</Option>
                <Option value="Medium">中</Option>
                <Option value="High">高</Option>
                <Option value="Urgent">紧急</Option>
              </Select>
            </Form.Item>

            <Row gutter={16}>
              <Col span={12}>
                <Form.Item
                  name="startTime"
                  label="默认开始时间"
                >
                  <DatePicker
                    showTime
                    style={{ width: '100%' }}
                    placeholder="选择开始时间"
                  />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item
                  name="endTime"
                  label="默认结束时间"
                >
                  <DatePicker
                    showTime
                    style={{ width: '100%' }}
                    placeholder="选择结束时间"
                  />
                </Form.Item>
              </Col>
            </Row>

            <Form.Item style={{ textAlign: 'right', marginBottom: 0 }}>
              <Space>
                <Button onClick={() => {
                  setCreateModalVisible(false);
                  createForm.resetFields();
                }}>
                  取消
                </Button>
                <Button type="primary" htmlType="submit">
                  创建模板
                </Button>
              </Space>
            </Form.Item>
          </Form>
        </Modal>

        {/* 编辑模板模态框 */}
        <Modal
          title="编辑任务模板"
          open={editModalVisible}
          onCancel={() => {
            setEditModalVisible(false);
            editForm.resetFields();
            setSelectedTemplate(null);
          }}
          footer={null}
          width={600}
        >
          <Form
            form={editForm}
            layout="vertical"
            onFinish={handleEditTemplate}
          >
            <Form.Item
              name="name"
              label="模板名称"
              rules={[
                { required: true, message: '请输入模板名称' },
                { max: 100, message: '模板名称不能超过100个字符' }
              ]}
            >
              <Input placeholder="请输入模板名称" />
            </Form.Item>

            <Form.Item
              name="templateDescription"
              label="模板描述"
              rules={[{ max: 500, message: '模板描述不能超过500个字符' }]}
            >
              <TextArea rows={3} placeholder="请输入模板描述（可选）" />
            </Form.Item>

            <Form.Item
              name="category"
              label="模板分类"
            >
              <Select placeholder="选择或输入分类" allowClear>
                {categories.map(category => (
                  <Option key={category} value={category}>{category}</Option>
                ))}
              </Select>
            </Form.Item>

            <Form.Item
              name="title"
              label="任务标题模板"
              rules={[{ required: true, message: '请输入任务标题模板' }]}
            >
              <Input placeholder="请输入任务标题模板" />
            </Form.Item>

            <Form.Item
              name="description"
              label="任务描述模板"
            >
              <TextArea rows={3} placeholder="请输入任务描述模板（可选）" />
            </Form.Item>

            <Form.Item
              name="priority"
              label="优先级"
            >
              <Select>
                <Option value="Low">低</Option>
                <Option value="Medium">中</Option>
                <Option value="High">高</Option>
                <Option value="Urgent">紧急</Option>
              </Select>
            </Form.Item>

            <Row gutter={16}>
              <Col span={12}>
                <Form.Item
                  name="startTime"
                  label="默认开始时间"
                >
                  <DatePicker
                    showTime
                    style={{ width: '100%' }}
                    placeholder="选择开始时间"
                  />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item
                  name="endTime"
                  label="默认结束时间"
                >
                  <DatePicker
                    showTime
                    style={{ width: '100%' }}
                    placeholder="选择结束时间"
                  />
                </Form.Item>
              </Col>
            </Row>

            <Form.Item style={{ textAlign: 'right', marginBottom: 0 }}>
              <Space>
                <Button onClick={() => {
                  setEditModalVisible(false);
                  editForm.resetFields();
                  setSelectedTemplate(null);
                }}>
                  取消
                </Button>
                <Button type="primary" htmlType="submit">
                  更新模板
                </Button>
              </Space>
            </Form.Item>
          </Form>
        </Modal>

        {/* 使用模板创建任务模态框 */}
        <Modal
          title={`使用模板：${selectedTemplate?.name}`}
          open={useTemplateModalVisible}
          onCancel={() => {
            setUseTemplateModalVisible(false);
            useTemplateForm.resetFields();
            setSelectedTemplate(null);
          }}
          footer={null}
          width={500}
        >
          <Form
            form={useTemplateForm}
            layout="vertical"
            onFinish={handleUseTemplate}
          >
            <Form.Item
              name="customTitle"
              label="任务标题"
              rules={[{ required: true, message: '请输入任务标题' }]}
            >
              <Input placeholder="请输入任务标题" />
            </Form.Item>

            <Form.Item
              name="customDescription"
              label="任务描述"
            >
              <TextArea rows={3} placeholder="请输入任务描述（可选）" />
            </Form.Item>

            <Row gutter={16}>
              <Col span={12}>
                <Form.Item
                  name="startTime"
                  label="开始时间"
                >
                  <DatePicker
                    showTime
                    style={{ width: '100%' }}
                    placeholder="选择开始时间"
                  />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item
                  name="endTime"
                  label="结束时间"
                >
                  <DatePicker
                    showTime
                    style={{ width: '100%' }}
                    placeholder="选择结束时间"
                  />
                </Form.Item>
              </Col>
            </Row>

            <Form.Item style={{ textAlign: 'right', marginBottom: 0 }}>
              <Space>
                <Button onClick={() => {
                  setUseTemplateModalVisible(false);
                  useTemplateForm.resetFields();
                  setSelectedTemplate(null);
                }}>
                  取消
                </Button>
                <Button type="primary" htmlType="submit">
                  创建任务
                </Button>
              </Space>
            </Form.Item>
          </Form>
        </Modal>
      </Content>
    </Layout>
  );
};

export default TemplatesPage;
