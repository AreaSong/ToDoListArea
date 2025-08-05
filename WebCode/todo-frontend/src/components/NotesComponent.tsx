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
  Tag,
  Modal,
  Form,
  Select,
} from 'antd';
import {
  PlusOutlined,
  DeleteOutlined,
  EditOutlined,
  FileTextOutlined,
  CommentOutlined,
} from '@ant-design/icons';
import { taskDetailsApi } from '../services/api';
import type { TaskNote, CreateTaskNote } from '../types/api';
import dayjs from 'dayjs';

const { Text, Paragraph } = Typography;
const { TextArea } = Input;
const { Option } = Select;

interface NotesComponentProps {
  taskId: string;
  onStatsChange?: () => void;
}

const NotesComponent: React.FC<NotesComponentProps> = ({ taskId, onStatsChange }) => {
  const [notes, setNotes] = useState<TaskNote[]>([]);
  const [loading, setLoading] = useState(false);
  const [createModalVisible, setCreateModalVisible] = useState(false);
  const [form] = Form.useForm();

  // 获取笔记列表
  const fetchNotes = async () => {
    setLoading(true);
    try {
      const response = await taskDetailsApi.getTaskNotes(taskId);
      if (response.success && response.data) {
        setNotes(response.data);
      } else {
        message.error(response.message || '获取笔记列表失败');
      }
    } catch (error) {
      console.error('获取笔记列表失败:', error);
      message.error('获取笔记列表失败，请重试');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchNotes();
  }, [taskId]);

  // 添加笔记
  const handleCreateNote = async (values: any) => {
    try {
      const createData: CreateTaskNote = {
        noteType: values.noteType,
        title: values.title,
        content: values.content
      };

      const response = await taskDetailsApi.addTaskNote(taskId, createData);
      if (response.success && response.data) {
        setNotes(prev => [response.data!, ...prev]);
        setCreateModalVisible(false);
        form.resetFields();
        message.success('笔记添加成功');
        onStatsChange?.();
      } else {
        message.error(response.message || '添加笔记失败');
      }
    } catch (error) {
      console.error('添加笔记失败:', error);
      message.error('添加笔记失败，请重试');
    }
  };

  // 删除笔记
  const handleDeleteNote = async (noteId: string) => {
    try {
      const response = await taskDetailsApi.deleteTaskDetail(noteId);
      if (response.success) {
        setNotes(prev => prev.filter(n => n.id !== noteId));
        message.success('笔记删除成功');
        onStatsChange?.();
      } else {
        message.error(response.message || '删除笔记失败');
      }
    } catch (error) {
      console.error('删除笔记失败:', error);
      message.error('删除笔记失败，请重试');
    }
  };

  // 获取笔记类型图标
  const getNoteTypeIcon = (noteType: string) => {
    return noteType === 'comment' ? <CommentOutlined /> : <FileTextOutlined />;
  };

  // 获取笔记类型颜色
  const getNoteTypeColor = (noteType: string) => {
    return noteType === 'comment' ? 'green' : 'blue';
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
          添加笔记
        </Button>
      </div>

      {/* 笔记列表 */}
      {notes.length === 0 ? (
        <Empty
          description="暂无笔记，点击上方按钮添加第一个笔记"
          image={Empty.PRESENTED_IMAGE_SIMPLE}
        />
      ) : (
        <List
          loading={loading}
          dataSource={notes}
          renderItem={(note) => (
            <List.Item style={{ padding: 0, marginBottom: 16 }}>
              <Card
                size="small"
                style={{ width: '100%' }}
                title={
                  <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                    <div style={{ display: 'flex', alignItems: 'center' }}>
                      <Tag color={getNoteTypeColor(note.noteType)} icon={getNoteTypeIcon(note.noteType)}>
                        {note.noteType === 'comment' ? '评论' : '笔记'}
                      </Tag>
                      <Text strong>{note.title}</Text>
                    </div>
                    <Text type="secondary" style={{ fontSize: '12px' }}>
                      {dayjs(note.createdAt).format('YYYY-MM-DD HH:mm')}
                    </Text>
                  </div>
                }
                extra={
                  <Popconfirm
                    title="确定要删除这个笔记吗？"
                    onConfirm={() => handleDeleteNote(note.id)}
                    okText="确定"
                    cancelText="取消"
                  >
                    <Button
                      type="text"
                      size="small"
                      danger
                      icon={<DeleteOutlined />}
                    />
                  </Popconfirm>
                }
              >
                <Paragraph
                  style={{ marginBottom: 0, whiteSpace: 'pre-wrap' }}
                >
                  {note.content}
                </Paragraph>
              </Card>
            </List.Item>
          )}
        />
      )}

      {/* 创建笔记模态框 */}
      <Modal
        title="添加笔记"
        open={createModalVisible}
        onCancel={() => {
          setCreateModalVisible(false);
          form.resetFields();
        }}
        footer={null}
        width={600}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleCreateNote}
        >
          <Form.Item
            name="noteType"
            label="类型"
            initialValue="note"
            rules={[{ required: true, message: '请选择笔记类型' }]}
          >
            <Select>
              <Option value="note">
                <FileTextOutlined /> 笔记
              </Option>
              <Option value="comment">
                <CommentOutlined /> 评论
              </Option>
            </Select>
          </Form.Item>

          <Form.Item
            name="title"
            label="标题"
            rules={[
              { required: true, message: '请输入笔记标题' },
              { max: 200, message: '标题长度不能超过200个字符' }
            ]}
          >
            <Input placeholder="请输入笔记标题" />
          </Form.Item>

          <Form.Item
            name="content"
            label="内容"
            rules={[
              { required: true, message: '请输入笔记内容' },
              { max: 5000, message: '内容长度不能超过5000个字符' }
            ]}
          >
            <TextArea
              rows={6}
              placeholder="请输入笔记内容"
              showCount
              maxLength={5000}
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
                添加笔记
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default NotesComponent;
