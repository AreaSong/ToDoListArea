import React, { useState, useEffect } from 'react';
import { createRoot } from 'react-dom/client';
import { Modal, Result, Button, Space, Typography, Card, Progress } from 'antd';
import { 
  CheckCircleOutlined, 
  RocketOutlined, 
  TrophyOutlined,
  StarOutlined,
  HeartOutlined,
  ThunderboltOutlined
} from '@ant-design/icons';

const { Text, Title } = Typography;

interface SuccessFeedbackProps {
  visible: boolean;
  title: string;
  description?: string;
  type?: 'simple' | 'celebration' | 'achievement' | 'milestone';
  onClose: () => void;
  onNext?: () => void;
  nextText?: string;
  showProgress?: boolean;
  progress?: number;
  autoClose?: boolean;
  autoCloseDelay?: number;
  celebrationLevel?: 'low' | 'medium' | 'high';
  customIcon?: React.ReactNode;
  actions?: Array<{
    text: string;
    type?: 'primary' | 'default' | 'dashed' | 'link';
    onClick: () => void;
  }>;
  stats?: Array<{
    label: string;
    value: string | number;
    suffix?: string;
  }>;
}

const SuccessFeedback: React.FC<SuccessFeedbackProps> = ({
  visible,
  title,
  description,
  type = 'simple',
  onClose,
  onNext,
  nextText = '继续',
  showProgress = false,
  progress = 100,
  autoClose = false,
  autoCloseDelay = 3000,
  celebrationLevel = 'medium',
  customIcon,
  actions = [],
  stats = []
}) => {
  const [countdown, setCountdown] = useState(autoCloseDelay / 1000);
  const [showConfetti, setShowConfetti] = useState(false);

  useEffect(() => {
    if (visible && type === 'celebration') {
      setShowConfetti(true);
      const timer = setTimeout(() => setShowConfetti(false), 3000);
      return () => clearTimeout(timer);
    }
  }, [visible, type]);

  useEffect(() => {
    if (visible && autoClose) {
      const timer = setInterval(() => {
        setCountdown(prev => {
          if (prev <= 1) {
            onClose();
            return 0;
          }
          return prev - 1;
        });
      }, 1000);

      return () => clearInterval(timer);
    }
  }, [visible, autoClose, onClose]);

  const getIcon = () => {
    if (customIcon) return customIcon;

    switch (type) {
      case 'celebration':
        return <RocketOutlined style={{ color: '#52c41a', fontSize: 64 }} />;
      case 'achievement':
        return <TrophyOutlined style={{ color: '#faad14', fontSize: 64 }} />;
      case 'milestone':
        return <StarOutlined style={{ color: '#1890ff', fontSize: 64 }} />;
      default:
        return <CheckCircleOutlined style={{ color: '#52c41a', fontSize: 64 }} />;
    }
  };

  const getAnimationClass = () => {
    switch (celebrationLevel) {
      case 'high':
        return 'success-bounce-high';
      case 'medium':
        return 'success-bounce-medium';
      case 'low':
        return 'success-fade-in';
      default:
        return 'success-fade-in';
    }
  };

  const renderStats = () => {
    if (stats.length === 0) return null;

    return (
      <Card size="small" style={{ marginTop: 16 }}>
        <Space split={<div style={{ width: 1, height: 20, backgroundColor: '#f0f0f0' }} />}>
          {stats.map((stat, index) => (
            <div key={index} style={{ textAlign: 'center', minWidth: 80 }}>
              <div style={{ fontSize: 20, fontWeight: 'bold', color: '#52c41a' }}>
                {stat.value}{stat.suffix}
              </div>
              <Text type="secondary" style={{ fontSize: 12 }}>
                {stat.label}
              </Text>
            </div>
          ))}
        </Space>
      </Card>
    );
  };

  const renderActions = () => {
    const allActions = [
      ...actions,
      ...(onNext ? [{
        text: nextText,
        type: 'primary' as const,
        onClick: onNext
      }] : []),
      {
        text: autoClose ? `关闭 (${countdown}s)` : '关闭',
        type: 'default' as const,
        onClick: onClose
      }
    ];

    return (
      <Space>
        {allActions.map((action, index) => (
          <Button
            key={index}
            type={action.type}
            onClick={action.onClick}
          >
            {action.text}
          </Button>
        ))}
      </Space>
    );
  };

  return (
    <>
      <Modal
        open={visible}
        onCancel={onClose}
        footer={null}
        centered
        width={480}
        closable={!autoClose}
        maskClosable={!autoClose}
        className={getAnimationClass()}
      >
        <Result
          icon={getIcon()}
          title={<Title level={3}>{title}</Title>}
          subTitle={description}
          extra={
            <div>
              {showProgress && (
                <div style={{ marginBottom: 16 }}>
                  <Progress
                    percent={progress}
                    strokeColor={{
                      '0%': '#108ee9',
                      '100%': '#87d068',
                    }}
                    trailColor="#f0f0f0"
                  />
                </div>
              )}
              
              {renderStats()}
              
              <div style={{ marginTop: 24 }}>
                {renderActions()}
              </div>
            </div>
          }
        />
      </Modal>

      {/* 庆祝动画样式 */}
      <style>
        {`
          .success-fade-in {
            animation: successFadeIn 0.3s ease-out;
          }
          
          .success-bounce-medium {
            animation: successBounce 0.6s ease-out;
          }
          
          .success-bounce-high {
            animation: successBounceHigh 0.8s ease-out;
          }
          
          @keyframes successFadeIn {
            from {
              opacity: 0;
              transform: translateY(-20px);
            }
            to {
              opacity: 1;
              transform: translateY(0);
            }
          }
          
          @keyframes successBounce {
            0% {
              opacity: 0;
              transform: scale(0.3) translateY(-50px);
            }
            50% {
              opacity: 1;
              transform: scale(1.05) translateY(-10px);
            }
            70% {
              transform: scale(0.95) translateY(0);
            }
            100% {
              transform: scale(1) translateY(0);
            }
          }
          
          @keyframes successBounceHigh {
            0% {
              opacity: 0;
              transform: scale(0.1) translateY(-100px) rotate(-180deg);
            }
            60% {
              opacity: 1;
              transform: scale(1.1) translateY(-20px) rotate(10deg);
            }
            80% {
              transform: scale(0.9) translateY(5px) rotate(-5deg);
            }
            100% {
              transform: scale(1) translateY(0) rotate(0deg);
            }
          }
        `}
      </style>
    </>
  );
};

// 便捷方法
export const showSuccessFeedback = (props: Omit<SuccessFeedbackProps, 'visible'>) => {
  return new Promise<void>((resolve) => {
    const div = document.createElement('div');
    document.body.appendChild(div);
    const root = createRoot(div);

    const destroy = () => {
      root.unmount();
      if (div.parentNode) {
        div.parentNode.removeChild(div);
      }
    };

    const onClose = () => {
      props.onClose?.();
      resolve();
      destroy();
    };

    root.render(
      <SuccessFeedback
        {...props}
        visible={true}
        onClose={onClose}
      />
    );
  });
};

// 预设的成功反馈类型
export const SuccessTypes = {
  taskCreated: {
    title: '任务创建成功！',
    description: '您的新任务已经添加到列表中',
    type: 'simple' as const,
    celebrationLevel: 'low' as const
  },
  taskCompleted: {
    title: '任务完成！',
    description: '恭喜您完成了这个任务',
    type: 'celebration' as const,
    celebrationLevel: 'medium' as const,
    customIcon: <HeartOutlined style={{ color: '#ff4d4f', fontSize: 64 }} />
  },
  milestoneReached: {
    title: '里程碑达成！',
    description: '您已经完成了一个重要的里程碑',
    type: 'milestone' as const,
    celebrationLevel: 'high' as const
  },
  levelUp: {
    title: '等级提升！',
    description: '您的效率等级得到了提升',
    type: 'achievement' as const,
    celebrationLevel: 'high' as const,
    customIcon: <ThunderboltOutlined style={{ color: '#faad14', fontSize: 64 }} />
  }
};

export default SuccessFeedback;
