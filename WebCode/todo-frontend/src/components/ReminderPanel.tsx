import React, { useState } from 'react';
import { Card, Button, Empty, Modal, Form, Input, DatePicker, List, Popconfirm, Space, Tag, Spin } from 'antd';
import { PlusOutlined, BellOutlined, DeleteOutlined, CheckOutlined, ExclamationCircleOutlined } from '@ant-design/icons';
import { useReminders } from '../hooks/useReminders';
import dayjs from 'dayjs';

const { TextArea } = Input;

interface ReminderPanelProps {
  taskId?: string;
  onReminderChange?: () => void;
}

const ReminderPanel: React.FC<ReminderPanelProps> = ({ taskId, onReminderChange }) => {
  const [modalVisible, setModalVisible] = useState(false);
  const [form] = Form.useForm();
  
  const {
    reminders,
    stats,
    loading,
    error,
    createReminder,
    deleteReminder,
    completeReminder,
    refresh
  } = useReminders(taskId);

  // 处理添加提醒
  const handleAddReminder = async (values: any) => {
    try {
      await createReminder({
        taskId: taskId,
        title: values.title,
        message: values.message,
        reminderTime: values.reminderTime.toISOString(),
        channels: ['web']
      });
      
      setModalVisible(false);
      form.resetFields();
      onReminderChange?.();
    } catch (error) {
      // 错误已在hook中处理
    }
  };

  // 处理删除提醒
  const handleDeleteReminder = async (reminderId: string) => {
    try {
      await deleteReminder(reminderId);
      onReminderChange?.();
    } catch (error) {
      // 错误已在hook中处理
    }
  };

  // 处理完成提醒
  const handleCompleteReminder = async (reminderId: string) => {
    try {
      await completeReminder(reminderId);
      onReminderChange?.();
    } catch (error) {
      // 错误已在hook中处理
    }
  };

  // 获取状态标签
  const getStatusTag = (status: string) => {
    const statusConfig = {
      pending: { color: 'orange', text: '待处理' },
      completed: { color: 'green', text: '已完成' },
      snoozed: { color: 'blue', text: '已延迟' }
    };

    const config = statusConfig[status as keyof typeof statusConfig] || { color: 'default', text: status };
    return <Tag color={config.color}>{config.text}</Tag>;
  };

  // 格式化时间
  const formatTime = (timeString: string) => {
    return dayjs(timeString).format('YYYY-MM-DD HH:mm');
  };

  return (
    <div>
      {/* 错误提示 */}
      {error && (
        <Card size="small" style={{ marginBottom: 16, borderColor: '#ff4d4f' }}>
          <div style={{ color: '#ff4d4f', display: 'flex', alignItems: 'center' }}>
            <ExclamationCircleOutlined style={{ marginRight: 8 }} />
            {error}
            <Button 
              type="link" 
              size="small" 
              onClick={refresh}
              style={{ marginLeft: 'auto' }}
            >
              重试
            </Button>
          </div>
        </Card>
      )}

      {/* 统计信息 */}
      {!taskId && stats && (
        <Card size="small" style={{ marginBottom: 16 }}>
          <div style={{ display: 'flex', justifyContent: 'space-around', textAlign: 'center' }}>
            <div>
              <div style={{ fontSize: '20px', fontWeight: 'bold', color: '#1890ff' }}>{stats.totalReminders}</div>
              <div style={{ color: '#666' }}>总提醒</div>
            </div>
            <div>
              <div style={{ fontSize: '20px', fontWeight: 'bold', color: '#fa8c16' }}>{stats.pendingReminders}</div>
              <div style={{ color: '#666' }}>待处理</div>
            </div>
            <div>
              <div style={{ fontSize: '20px', fontWeight: 'bold', color: '#52c41a' }}>{stats.completedReminders}</div>
              <div style={{ color: '#666' }}>已完成</div>
            </div>
            <div>
              <div style={{ fontSize: '20px', fontWeight: 'bold', color: '#722ed1' }}>{stats.todayReminders}</div>
              <div style={{ color: '#666' }}>今日</div>
            </div>
          </div>
        </Card>
      )}

      {/* 提醒列表 */}
      <Card
        title={
          <Space>
            <BellOutlined />
            {taskId ? '任务提醒' : '我的提醒'}
            {reminders.length > 0 && (
              <Tag color="blue">{reminders.length}</Tag>
            )}
          </Space>
        }
        extra={
          <Space>
            <Button
              type="text"
              icon={<BellOutlined />}
              onClick={refresh}
              loading={loading}
              size="small"
            >
              刷新
            </Button>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={() => setModalVisible(true)}
              size="small"
            >
              添加提醒
            </Button>
          </Space>
        }
        size="small"
      >
        <Spin spinning={loading}>
          <div style={{ minHeight: 100 }}>
            {reminders.length > 0 ? (
              <List
                dataSource={reminders}
                renderItem={(reminder) => (
                  <List.Item
                    key={reminder.id}
                    actions={[
                      reminder.status === 'pending' && (
                        <Button 
                          type="text" 
                          icon={<CheckOutlined />} 
                          size="small"
                          onClick={() => handleCompleteReminder(reminder.id)}
                          title="标记为完成"
                        >
                          完成
                        </Button>
                      ),
                      <Popconfirm
                        title="确定要删除这个提醒吗？"
                        onConfirm={() => handleDeleteReminder(reminder.id)}
                        okText="确定"
                        cancelText="取消"
                      >
                        <Button 
                          type="text" 
                          danger 
                          icon={<DeleteOutlined />} 
                          size="small"
                          title="删除提醒"
                        >
                          删除
                        </Button>
                      </Popconfirm>
                    ].filter(Boolean)}
                  >
                    <List.Item.Meta
                      avatar={<BellOutlined style={{ color: '#1890ff' }} />}
                      title={
                        <Space>
                          <span>{reminder.title}</span>
                          {getStatusTag(reminder.status)}
                        </Space>
                      }
                      description={
                        <div>
                          {reminder.message && (
                            <div style={{ marginBottom: 4 }}>{reminder.message}</div>
                          )}
                          <div style={{ color: '#666', fontSize: '12px' }}>
                            提醒时间: {formatTime(reminder.reminderTime)}
                            {reminder.taskTitle && ` | 关联任务: ${reminder.taskTitle}`}
                          </div>
                        </div>
                      }
                    />
                  </List.Item>
                )}
                size="small"
              />
            ) : (
              <Empty
                image={Empty.PRESENTED_IMAGE_SIMPLE}
                description={taskId ? "暂无任务提醒" : "暂无提醒"}
                style={{ margin: '20px 0' }}
              >
                <Button 
                  type="primary" 
                  icon={<PlusOutlined />}
                  onClick={() => setModalVisible(true)}
                >
                  添加第一个提醒
                </Button>
              </Empty>
            )}
          </div>
        </Spin>
      </Card>

      {/* 添加提醒对话框 */}
      <Modal
        title={
          <Space>
            <BellOutlined />
            {taskId ? "添加任务提醒" : "添加提醒"}
          </Space>
        }
        open={modalVisible}
        onCancel={() => {
          setModalVisible(false);
          form.resetFields();
        }}
        footer={null}
        width={500}
        destroyOnClose
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleAddReminder}
          initialValues={{
            channels: ['web']
          }}
        >
          <Form.Item
            name="title"
            label="提醒标题"
            rules={[
              { required: true, message: '请输入提醒标题' },
              { max: 100, message: '标题长度不能超过100个字符' }
            ]}
          >
            <Input placeholder="输入提醒标题" />
          </Form.Item>

          <Form.Item
            name="message"
            label="提醒内容"
            rules={[
              { max: 500, message: '内容长度不能超过500个字符' }
            ]}
          >
            <TextArea 
              placeholder="输入提醒内容（可选）" 
              rows={3}
              showCount
              maxLength={500}
            />
          </Form.Item>

          <Form.Item
            name="reminderTime"
            label="提醒时间"
            rules={[{ required: true, message: '请选择提醒时间' }]}
          >
            <DatePicker 
              showTime 
              placeholder="选择提醒时间"
              style={{ width: '100%' }}
              disabledDate={(current) => current && current < dayjs().startOf('day')}
              format="YYYY-MM-DD HH:mm"
            />
          </Form.Item>

          <Form.Item style={{ marginBottom: 0, textAlign: 'right' }}>
            <Space>
              <Button onClick={() => {
                setModalVisible(false);
                form.resetFields();
              }}>
                取消
              </Button>
              <Button type="primary" htmlType="submit" loading={loading}>
                添加提醒
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default ReminderPanel;
