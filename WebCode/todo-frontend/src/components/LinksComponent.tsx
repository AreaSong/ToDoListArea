import React, { useState, useEffect } from 'react';
import {
  List,
  Button,
  Input,
  Space,
  message,
  Popconfirm,
  Empty,
  Typography,
  Card,
  Modal,
  Form,
  Tooltip,
} from 'antd';
import {
  PlusOutlined,
  DeleteOutlined,
  LinkOutlined,
  GlobalOutlined,
} from '@ant-design/icons';
import { taskDetailsApi } from '../services/api';
import type { TaskLink, CreateTaskLink } from '../types/api';
import dayjs from 'dayjs';

const { Text, Link } = Typography;

interface LinksComponentProps {
  taskId: string;
  onStatsChange?: () => void;
}

const LinksComponent: React.FC<LinksComponentProps> = ({ taskId, onStatsChange }) => {
  const [links, setLinks] = useState<TaskLink[]>([]);
  const [loading, setLoading] = useState(false);
  const [createModalVisible, setCreateModalVisible] = useState(false);
  const [form] = Form.useForm();

  // 获取链接列表
  const fetchLinks = async () => {
    setLoading(true);
    try {
      const response = await taskDetailsApi.getTaskLinks(taskId);
      if (response.success && response.data) {
        setLinks(response.data);
      } else {
        message.error(response.message || '获取链接列表失败');
      }
    } catch (error) {
      console.error('获取链接列表失败:', error);
      message.error('获取链接列表失败，请重试');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchLinks();
  }, [taskId]);

  // 添加链接
  const handleCreateLink = async (values: any) => {
    try {
      const createData: CreateTaskLink = {
        title: values.title,
        url: values.url
      };

      const response = await taskDetailsApi.addTaskLink(taskId, createData);
      if (response.success && response.data) {
        setLinks(prev => [...prev, response.data!]);
        setCreateModalVisible(false);
        form.resetFields();
        message.success('链接添加成功');
        onStatsChange?.();
      } else {
        message.error(response.message || '添加链接失败');
      }
    } catch (error) {
      console.error('添加链接失败:', error);
      message.error('添加链接失败，请重试');
    }
  };

  // 删除链接
  const handleDeleteLink = async (linkId: string) => {
    try {
      const response = await taskDetailsApi.deleteTaskDetail(linkId);
      if (response.success) {
        setLinks(prev => prev.filter(l => l.id !== linkId));
        message.success('链接删除成功');
        onStatsChange?.();
      } else {
        message.error(response.message || '删除链接失败');
      }
    } catch (error) {
      console.error('删除链接失败:', error);
      message.error('删除链接失败，请重试');
    }
  };

  // 获取域名
  const getDomain = (url: string) => {
    try {
      const urlObj = new URL(url);
      return urlObj.hostname;
    } catch {
      return url;
    }
  };

  return (
    <div>
      {/* 操作按钮 */}
      <div style={{ marginBottom: 16 }}>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={() => setCreateModalVisible(true)}
        >
          添加链接
        </Button>
      </div>

      {/* 链接列表 */}
      {links.length === 0 ? (
        <Empty
          description="暂无链接，点击上方按钮添加第一个链接"
          image={Empty.PRESENTED_IMAGE_SIMPLE}
        />
      ) : (
        <List
          loading={loading}
          dataSource={links}
          renderItem={(link) => (
            <List.Item
              style={{ padding: '12px 0' }}
              actions={[
                <Tooltip title="在新窗口打开">
                  <Button
                    type="text"
                    size="small"
                    icon={<GlobalOutlined />}
                    onClick={() => window.open(link.url, '_blank')}
                  />
                </Tooltip>,
                <Popconfirm
                  title="确定要删除这个链接吗？"
                  onConfirm={() => handleDeleteLink(link.id)}
                  okText="确定"
                  cancelText="取消"
                >
                  <Tooltip title="删除">
                    <Button
                      type="text"
                      size="small"
                      danger
                      icon={<DeleteOutlined />}
                    />
                  </Tooltip>
                </Popconfirm>
              ]}
            >
              <List.Item.Meta
                avatar={<LinkOutlined style={{ fontSize: '16px', color: '#1890ff' }} />}
                title={
                  <Link
                    href={link.url}
                    target="_blank"
                    rel="noopener noreferrer"
                    style={{ fontSize: '14px' }}
                  >
                    {link.title}
                  </Link>
                }
                description={
                  <div>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      {getDomain(link.url)}
                    </Text>
                    <br />
                    <Text type="secondary" style={{ fontSize: '11px' }}>
                      添加于 {dayjs(link.createdAt).format('YYYY-MM-DD HH:mm')}
                    </Text>
                  </div>
                }
              />
            </List.Item>
          )}
        />
      )}

      {/* 创建链接模态框 */}
      <Modal
        title="添加链接"
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
          onFinish={handleCreateLink}
        >
          <Form.Item
            name="title"
            label="链接标题"
            rules={[
              { required: true, message: '请输入链接标题' },
              { max: 200, message: '标题长度不能超过200个字符' }
            ]}
          >
            <Input placeholder="请输入链接标题" />
          </Form.Item>

          <Form.Item
            name="url"
            label="链接地址"
            rules={[
              { required: true, message: '请输入链接地址' },
              { type: 'url', message: '请输入有效的URL地址' },
              { max: 2000, message: 'URL长度不能超过2000个字符' }
            ]}
          >
            <Input
              placeholder="请输入链接地址，如：https://example.com"
              addonBefore={<LinkOutlined />}
            />
          </Form.Item>

          <Form.Item style={{ textAlign: 'right', marginBottom: 0 }}>
            <Space>
              <Button onClick={() => {
                setCreateModalVisible(false);
                form.resetFields();
              }}>
                取消
              </Button>
              <Button type="primary" htmlType="submit">
                添加链接
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default LinksComponent;
