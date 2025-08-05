import React from 'react';
import { createRoot } from 'react-dom/client';
import { Modal, Button, Space, Typography, Alert } from 'antd';
import { 
  ExclamationCircleOutlined, 
  DeleteOutlined, 
  WarningOutlined,
  InfoCircleOutlined,
  CheckCircleOutlined 
} from '@ant-design/icons';

const { Text } = Typography;

export interface ConfirmDialogProps {
  visible: boolean;
  title: string;
  content: string;
  type?: 'danger' | 'warning' | 'info' | 'success';
  confirmText?: string;
  cancelText?: string;
  loading?: boolean;
  onConfirm: () => void | Promise<void>;
  onCancel: () => void;
  showIcon?: boolean;
  width?: number;
  centered?: boolean;
  maskClosable?: boolean;
  keyboard?: boolean;
  danger?: boolean;
  additionalInfo?: string;
  consequences?: string[];
}

const ConfirmDialog: React.FC<ConfirmDialogProps> = ({
  visible,
  title,
  content,
  type = 'warning',
  confirmText = '确认',
  cancelText = '取消',
  loading = false,
  onConfirm,
  onCancel,
  showIcon = true,
  width = 416,
  centered = true,
  maskClosable = false,
  keyboard = true,
  danger = false,
  additionalInfo,
  consequences = []
}) => {
  const getIcon = () => {
    switch (type) {
      case 'danger':
        return <DeleteOutlined style={{ color: '#ff4d4f', fontSize: 22 }} />;
      case 'warning':
        return <ExclamationCircleOutlined style={{ color: '#faad14', fontSize: 22 }} />;
      case 'info':
        return <InfoCircleOutlined style={{ color: '#1890ff', fontSize: 22 }} />;
      case 'success':
        return <CheckCircleOutlined style={{ color: '#52c41a', fontSize: 22 }} />;
      default:
        return <WarningOutlined style={{ color: '#faad14', fontSize: 22 }} />;
    }
  };

  const getAlertType = () => {
    switch (type) {
      case 'danger':
        return 'error';
      case 'warning':
        return 'warning';
      case 'info':
        return 'info';
      case 'success':
        return 'success';
      default:
        return 'warning';
    }
  };

  const handleConfirm = async () => {
    try {
      await onConfirm();
    } catch (error) {
      console.error('确认操作失败:', error);
    }
  };

  return (
    <Modal
      title={
        <Space>
          {showIcon && getIcon()}
          <span>{title}</span>
        </Space>
      }
      open={visible}
      onCancel={onCancel}
      width={width}
      centered={centered}
      maskClosable={maskClosable}
      keyboard={keyboard}
      footer={
        <Space>
          <Button onClick={onCancel} disabled={loading}>
            {cancelText}
          </Button>
          <Button
            type={danger || type === 'danger' ? 'primary' : 'primary'}
            danger={danger || type === 'danger'}
            loading={loading}
            onClick={handleConfirm}
          >
            {confirmText}
          </Button>
        </Space>
      }
    >
      <div style={{ paddingTop: 8 }}>
        <Text style={{ fontSize: 14, lineHeight: 1.6 }}>
          {content}
        </Text>

        {additionalInfo && (
          <Alert
            message={additionalInfo}
            type={getAlertType()}
            showIcon
            style={{ marginTop: 16, marginBottom: 8 }}
          />
        )}

        {consequences.length > 0 && (
          <div style={{ marginTop: 16 }}>
            <Text strong style={{ color: '#ff4d4f' }}>
              此操作将导致：
            </Text>
            <ul style={{ marginTop: 8, paddingLeft: 20, color: '#666' }}>
              {consequences.map((consequence, index) => (
                <li key={index} style={{ marginBottom: 4 }}>
                  {consequence}
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>
    </Modal>
  );
};

// 便捷方法
export const showConfirmDialog = (props: Omit<ConfirmDialogProps, 'visible'>) => {
  return new Promise<boolean>((resolve) => {
    const div = document.createElement('div');
    document.body.appendChild(div);
    const root = createRoot(div);

    const destroy = () => {
      root.unmount();
      if (div.parentNode) {
        div.parentNode.removeChild(div);
      }
    };

    const onConfirm = async () => {
      try {
        await props.onConfirm();
        resolve(true);
      } catch (error) {
        resolve(false);
      } finally {
        destroy();
      }
    };

    const onCancel = () => {
      props.onCancel?.();
      resolve(false);
      destroy();
    };

    root.render(
      <ConfirmDialog
        {...props}
        visible={true}
        onConfirm={onConfirm}
        onCancel={onCancel}
      />
    );
  });
};

export default ConfirmDialog;
