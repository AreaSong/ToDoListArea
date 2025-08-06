import React, { useState, useEffect } from 'react';
import {
  List,
  Button,
  Input,
  Checkbox,
  Space,
  message,
  Popconfirm,
  Empty,
  Typography,
  Tooltip,
  Modal,
} from 'antd';
import {
  PlusOutlined,
  DeleteOutlined,
  EditOutlined,
  CheckOutlined,
  CloseOutlined,
} from '@ant-design/icons';
import { taskDetailsApi } from '../services/api';
import type { ChecklistItem, CreateChecklistItem, UpdateChecklistItem } from '../types/api';

const { Text } = Typography;

interface ChecklistComponentProps {
  taskId: string;
  onStatsChange?: () => void;
}

const ChecklistComponent: React.FC<ChecklistComponentProps> = ({ taskId, onStatsChange }) => {
  const [items, setItems] = useState<ChecklistItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [addingItem, setAddingItem] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [newItemTitle, setNewItemTitle] = useState('');
  const [editTitle, setEditTitle] = useState('');
  const [batchModalVisible, setBatchModalVisible] = useState(false);

  // 获取检查清单
  const fetchChecklist = async () => {
    setLoading(true);
    try {
      const response = await taskDetailsApi.getTaskChecklist(taskId);
      if (response.success && response.data) {
        setItems(response.data);
      } else {
        message.error(response.message || '获取检查清单失败');
      }
    } catch (error) {
      console.error('获取检查清单失败:', error);
      message.error('获取检查清单失败，请重试');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchChecklist();
  }, [taskId]);

  // 添加检查项
  const handleAddItem = async () => {
    if (!newItemTitle.trim()) {
      message.warning('请输入检查项内容');
      return;
    }

    try {
      const createData: CreateChecklistItem = {
        title: newItemTitle.trim()
      };

      const response = await taskDetailsApi.addChecklistItem(taskId, createData);
      if (response.success && response.data) {
        setItems(prev => [...prev, response.data!]);
        setNewItemTitle('');
        setAddingItem(false);
        message.success('检查项添加成功');
        onStatsChange?.();
      } else {
        message.error(response.message || '添加检查项失败');
      }
    } catch (error) {
      console.error('添加检查项失败:', error);
      message.error('添加检查项失败，请重试');
    }
  };

  // 更新检查项状态
  const handleToggleItem = async (item: ChecklistItem) => {
    try {
      const updateData: UpdateChecklistItem = {
        isCompleted: !item.isCompleted
      };

      const response = await taskDetailsApi.updateChecklistItem(item.id, updateData);
      if (response.success && response.data) {
        setItems(prev => prev.map(i => i.id === item.id ? response.data! : i));
        onStatsChange?.();
      } else {
        message.error(response.message || '更新检查项失败');
      }
    } catch (error) {
      console.error('更新检查项失败:', error);
      message.error('更新检查项失败，请重试');
    }
  };

  // 编辑检查项标题
  const handleEditItem = async (item: ChecklistItem) => {
    if (!editTitle.trim()) {
      message.warning('请输入检查项内容');
      return;
    }

    try {
      const updateData: UpdateChecklistItem = {
        title: editTitle.trim()
      };

      const response = await taskDetailsApi.updateChecklistItem(item.id, updateData);
      if (response.success && response.data) {
        setItems(prev => prev.map(i => i.id === item.id ? response.data! : i));
        setEditingId(null);
        setEditTitle('');
        message.success('检查项更新成功');
      } else {
        message.error(response.message || '更新检查项失败');
      }
    } catch (error) {
      console.error('更新检查项失败:', error);
      message.error('更新检查项失败，请重试');
    }
  };

  // 删除检查项
  const handleDeleteItem = async (itemId: string) => {
    try {
      const response = await taskDetailsApi.deleteTaskDetail(itemId);
      if (response.success) {
        setItems(prev => prev.filter(i => i.id !== itemId));
        message.success('检查项删除成功');
        onStatsChange?.();
      } else {
        message.error(response.message || '删除检查项失败');
      }
    } catch (error) {
      console.error('删除检查项失败:', error);
      message.error('删除检查项失败，请重试');
    }
  };

  // 批量操作
  const handleBatchOperation = async (isCompleted: boolean) => {
    try {
      const response = await taskDetailsApi.batchUpdateChecklistStatus(taskId, isCompleted);
      if (response.success && response.data) {
        await fetchChecklist(); // 重新获取数据
        setBatchModalVisible(false);
        message.success(`批量${isCompleted ? '完成' : '重置'}成功`);
        onStatsChange?.();
      } else {
        message.error(response.message || '批量操作失败');
      }
    } catch (error) {
      console.error('批量操作失败:', error);
      message.error('批量操作失败，请重试');
    }
  };

  // 开始编辑
  const startEdit = (item: ChecklistItem) => {
    setEditingId(item.id);
    setEditTitle(item.title);
  };

  // 取消编辑
  const cancelEdit = () => {
    setEditingId(null);
    setEditTitle('');
  };

  return (
    <div>
      {/* 操作按钮 */}
      <div style={{ marginBottom: 16, display: 'flex', justifyContent: 'space-between' }}>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={() => setAddingItem(true)}
          disabled={addingItem}
        >
          添加检查项
        </Button>
        
        {items.length > 0 && (
          <Button
            onClick={() => setBatchModalVisible(true)}
          >
            批量操作
          </Button>
        )}
      </div>

      {/* 添加新项目 */}
      {addingItem && (
        <div style={{ marginBottom: 16, padding: 16, border: '1px dashed #d9d9d9', borderRadius: 6 }}>
          <Space.Compact style={{ width: '100%' }}>
            <Input
              placeholder="请输入检查项内容"
              value={newItemTitle}
              onChange={(e) => setNewItemTitle(e.target.value)}
              onPressEnter={handleAddItem}
              autoFocus
            />
            <Button type="primary" icon={<CheckOutlined />} onClick={handleAddItem} />
            <Button icon={<CloseOutlined />} onClick={() => {
              setAddingItem(false);
              setNewItemTitle('');
            }} />
          </Space.Compact>
        </div>
      )}

      {/* 检查清单 */}
      {items.length === 0 ? (
        <Empty
          description="暂无检查项，点击上方按钮添加第一个检查项"
          image={Empty.PRESENTED_IMAGE_SIMPLE}
        />
      ) : (
        <List
          loading={loading}
          dataSource={items}
          renderItem={(item) => (
            <List.Item
              style={{
                padding: '12px 0',
                opacity: item.isCompleted ? 0.6 : 1
              }}
              actions={[
                <Tooltip title="编辑">
                  <Button
                    type="text"
                    size="small"
                    icon={<EditOutlined />}
                    onClick={() => startEdit(item)}
                  />
                </Tooltip>,
                <Popconfirm
                  title="确定要删除这个检查项吗？"
                  onConfirm={() => handleDeleteItem(item.id)}
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
              <div style={{ display: 'flex', alignItems: 'center', width: '100%' }}>
                <Checkbox
                  checked={item.isCompleted}
                  onChange={() => handleToggleItem(item)}
                  style={{ marginRight: 12 }}
                />
                {editingId === item.id ? (
                  <Space.Compact style={{ flex: 1 }}>
                    <Input
                      value={editTitle}
                      onChange={(e) => setEditTitle(e.target.value)}
                      onPressEnter={() => handleEditItem(item)}
                      autoFocus
                    />
                    <Button
                      type="primary"
                      size="small"
                      icon={<CheckOutlined />}
                      onClick={() => handleEditItem(item)}
                    />
                    <Button
                      size="small"
                      icon={<CloseOutlined />}
                      onClick={cancelEdit}
                    />
                  </Space.Compact>
                ) : (
                  <Text
                    style={{
                      textDecoration: item.isCompleted ? 'line-through' : 'none',
                      flex: 1
                    }}
                  >
                    {item.title}
                  </Text>
                )}
              </div>
            </List.Item>
          )}
        />
      )}

      {/* 批量操作模态框 */}
      <Modal
        title="批量操作"
        open={batchModalVisible}
        onCancel={() => setBatchModalVisible(false)}
        footer={null}
      >
        <div style={{ textAlign: 'center' }}>
          <p>选择要执行的批量操作：</p>
          <Space>
            <Button
              type="primary"
              onClick={() => handleBatchOperation(true)}
            >
              全部标记为完成
            </Button>
            <Button
              onClick={() => handleBatchOperation(false)}
            >
              全部标记为未完成
            </Button>
          </Space>
        </div>
      </Modal>
    </div>
  );
};

export default ChecklistComponent;
