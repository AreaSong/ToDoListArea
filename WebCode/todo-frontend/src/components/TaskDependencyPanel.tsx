import React, { useState, useEffect } from 'react';
import { Card, Button, Empty, message, Modal, Form, Select, InputNumber, List, Popconfirm, Tooltip, Alert, Space, Tag } from 'antd';
import { PlusOutlined, LinkOutlined, DeleteOutlined, ExclamationCircleOutlined, ClockCircleOutlined } from '@ant-design/icons';
import { taskDependencyApi } from '../api/taskDependencyApi';
import { taskApi } from '../services/api';

interface TaskDependencyPanelProps {
  taskId: string;
  onDependencyChange?: () => void;
}

const TaskDependencyPanel: React.FC<TaskDependencyPanelProps> = ({ taskId, onDependencyChange }) => {
  const [dependencies, setDependencies] = useState<any[]>([]);
  const [dependents, setDependents] = useState<any[]>([]);
  const [conflicts, setConflicts] = useState<any>(null);
  const [availableTasks, setAvailableTasks] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [form] = Form.useForm();

  // 加载数据
  const loadData = async () => {
    setLoading(true);
    try {
      console.log('开始加载依赖关系数据，taskId:', taskId);

      // 获取当前用户ID
      const user = JSON.parse(localStorage.getItem('user') || '{}');
      const userId = user.id;
      
      if (!userId) {
        throw new Error('用户未登录');
      }

      // 并行加载所有数据
      const [dependenciesData, dependentsData, conflictsData, tasksData] = await Promise.all([
        taskDependencyApi.getTaskDependencies(taskId),
        taskDependencyApi.getTaskDependents(taskId),
        taskDependencyApi.checkTaskConflicts(taskId),
        taskApi.getTasks(userId, { pageNumber: 1, pageSize: 100 })
      ]);

      console.log('API调用结果:', {
        dependenciesData,
        dependentsData,
        conflictsData,
        tasksData
      });

      setDependencies(dependenciesData || []);
      setDependents(dependentsData || []);
      setConflicts(conflictsData);
      
      // 过滤掉当前任务和已经有依赖关系的任务
      const existingDependencyIds = (dependenciesData || []).map(d => d.dependsOnTaskId);
      const tasks = tasksData?.data?.items || [];
      const filteredTasks = tasks.filter(task => 
        task.id !== taskId && !existingDependencyIds.includes(task.id)
      );
      setAvailableTasks(filteredTasks);

      message.success('依赖关系数据加载成功');
      
    } catch (error) {
      console.error('加载依赖关系数据失败:', error);
      message.error('加载依赖关系失败');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (taskId) {
      loadData();
    }
  }, [taskId]);

  // 添加依赖关系
  const handleAddDependency = async (values: any) => {
    try {
      await taskDependencyApi.createTaskDependency(taskId, values);
      message.success('依赖关系添加成功');
      setModalVisible(false);
      form.resetFields();
      loadData();
      onDependencyChange?.();
    } catch (error: any) {
      console.error('添加依赖关系失败:', error);
      message.error(error.response?.data || '添加依赖关系失败');
    }
  };

  // 删除依赖关系
  const handleDeleteDependency = async (dependencyId: string) => {
    try {
      await taskDependencyApi.deleteTaskDependency(dependencyId);
      message.success('依赖关系删除成功');
      loadData();
      onDependencyChange?.();
    } catch (error) {
      console.error('删除依赖关系失败:', error);
      message.error('删除依赖关系失败');
    }
  };

  return (
    <div>
      {/* 冲突警告 */}
      {conflicts?.hasConflicts && (
        <Alert
          message="检测到依赖冲突"
          description={
            <ul style={{ margin: 0, paddingLeft: 20 }}>
              {conflicts.conflicts.map((conflict: string, index: number) => (
                <li key={index}>{conflict}</li>
              ))}
            </ul>
          }
          type="warning"
          icon={<ExclamationCircleOutlined />}
          showIcon
          style={{ marginBottom: 16 }}
        />
      )}

      {/* 任务依赖关系 */}
      <Card
        title={
          <Space>
            <LinkOutlined />
            任务依赖关系
            {dependencies.length > 0 && (
              <Tag color="blue">{dependencies.length} 个依赖</Tag>
            )}
          </Space>
        }
        extra={
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => setModalVisible(true)}
            disabled={availableTasks.length === 0}
          >
            添加依赖
          </Button>
        }
        size="small"
        style={{ marginBottom: 16 }}
      >
        <div style={{ minHeight: loading ? 100 : 'auto' }}>
          {loading ? (
            <div style={{ textAlign: 'center', padding: '40px 0' }}>
              加载中...
            </div>
          ) : dependencies.length > 0 ? (
            <List
              dataSource={dependencies}
              renderItem={(dependency: any) => (
                <List.Item
                  key={dependency.id}
                  actions={[
                    <Tooltip title="删除依赖关系">
                      <Popconfirm
                        title="确定要删除这个依赖关系吗？"
                        onConfirm={() => handleDeleteDependency(dependency.id)}
                        okText="确定"
                        cancelText="取消"
                      >
                        <Button type="text" danger icon={<DeleteOutlined />} size="small" />
                      </Popconfirm>
                    </Tooltip>
                  ]}
                >
                  <List.Item.Meta
                    avatar={<LinkOutlined style={{ color: '#1890ff' }} />}
                    title={
                      <Space>
                        <span>{dependency.dependsOnTaskTitle}</span>
                        <Tag color="blue">依赖</Tag>
                        {dependency.lagTime > 0 && (
                          <Tag icon={<ClockCircleOutlined />} color="orange">
                            延迟 {dependency.lagTime} 分钟
                          </Tag>
                        )}
                      </Space>
                    }
                    description={`此任务依赖于 "${dependency.dependsOnTaskTitle}" 完成后才能开始`}
                  />
                </List.Item>
              )}
              size="small"
            />
          ) : (
            <Empty
              image={Empty.PRESENTED_IMAGE_SIMPLE}
              description="暂无依赖关系"
              style={{ margin: '20px 0' }}
            />
          )}
        </div>
      </Card>

      {/* 被依赖关系 */}
      {dependents.length > 0 && (
        <Card
          title={
            <Space>
              <LinkOutlined />
              被依赖关系
              <Tag color="green">{dependents.length} 个任务依赖此任务</Tag>
            </Space>
          }
          size="small"
        >
          <List
            dataSource={dependents}
            renderItem={(dependent: any) => (
              <List.Item key={dependent.id}>
                <List.Item.Meta
                  avatar={<LinkOutlined style={{ color: '#52c41a' }} />}
                  title={
                    <Space>
                      <span>{dependent.taskTitle}</span>
                      <Tag color="green">被依赖</Tag>
                      {dependent.lagTime > 0 && (
                        <Tag icon={<ClockCircleOutlined />} color="orange">
                          延迟 {dependent.lagTime} 分钟
                        </Tag>
                      )}
                    </Space>
                  }
                  description={`"${dependent.taskTitle}" 依赖于此任务完成后才能开始`}
                />
              </List.Item>
            )}
            size="small"
          />
        </Card>
      )}

      {/* 添加依赖关系对话框 */}
      <Modal
        title="添加任务依赖关系"
        open={modalVisible}
        onCancel={() => {
          setModalVisible(false);
          form.resetFields();
        }}
        footer={null}
        width={500}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleAddDependency}
        >
          <Form.Item
            name="dependsOnTaskId"
            label="选择依赖任务"
            rules={[{ required: true, message: '请选择依赖任务' }]}
          >
            <Select
              placeholder="选择此任务依赖的任务"
              showSearch
              filterOption={(input, option) =>
                (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
              }
              options={(availableTasks || []).map(task => ({
                value: task.id,
                label: task.title,
                disabled: task.status === '已完成'
              }))}
            />
          </Form.Item>

          <Form.Item
            name="lagTime"
            label="延迟时间（分钟）"
            tooltip="依赖任务完成后，需要等待多长时间才能开始此任务"
          >
            <InputNumber
              min={0}
              placeholder="0"
              style={{ width: '100%' }}
              addonAfter="分钟"
            />
          </Form.Item>

          <Alert
            message="依赖关系说明"
            description="添加依赖关系后，此任务将在依赖任务完成后才能开始。系统会自动检测循环依赖并阻止创建。"
            type="info"
            showIcon
            style={{ marginBottom: 16 }}
          />

          <Form.Item style={{ marginBottom: 0, textAlign: 'right' }}>
            <Space>
              <Button onClick={() => {
                setModalVisible(false);
                form.resetFields();
              }}>
                取消
              </Button>
              <Button type="primary" htmlType="submit">
                添加依赖
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default TaskDependencyPanel;
