import React from 'react';
import { Spin, Card, Typography, Progress, Space } from 'antd';
import { LoadingOutlined, ClockCircleOutlined } from '@ant-design/icons';

const { Text } = Typography;

interface LoadingSpinnerProps {
  size?: 'small' | 'default' | 'large';
  tip?: string;
  spinning?: boolean;
  children?: React.ReactNode;
  delay?: number;
  style?: React.CSSProperties;
  overlay?: boolean;
  progress?: number;
  showProgress?: boolean;
  timeout?: number;
}

interface LoadingPageProps {
  title?: string;
  description?: string;
  progress?: number;
  showProgress?: boolean;
  timeout?: number;
}

// 自定义加载图标
const customIcon = <LoadingOutlined style={{ fontSize: 24, color: '#1890ff' }} spin />;

/**
 * 基础加载旋转器组件
 */
export const LoadingSpinner: React.FC<LoadingSpinnerProps> = ({
  size = 'default',
  tip = '加载中...',
  spinning = true,
  children,
  delay = 0,
  style,
  overlay = false,
  progress,
  showProgress = false,
  timeout
}) => {
  const [timeoutReached, setTimeoutReached] = React.useState(false);

  React.useEffect(() => {
    if (timeout && spinning) {
      const timer = setTimeout(() => {
        setTimeoutReached(true);
      }, timeout);

      return () => clearTimeout(timer);
    }
  }, [timeout, spinning]);

  if (timeoutReached) {
    return (
      <div style={{ textAlign: 'center', padding: '20px', ...style }}>
        <Space direction="vertical" size="middle">
          <ClockCircleOutlined style={{ fontSize: 48, color: '#faad14' }} />
          <div>
            <Text strong>加载时间较长</Text>
            <br />
            <Text type="secondary">请检查网络连接或稍后重试</Text>
          </div>
        </Space>
      </div>
    );
  }

  const spinContent = (
    <div style={{ textAlign: 'center' }}>
      {showProgress && typeof progress === 'number' && (
        <div style={{ marginBottom: 16 }}>
          <Progress 
            percent={progress} 
            size="small" 
            status={progress === 100 ? 'success' : 'active'}
            showInfo={true}
          />
        </div>
      )}
      <div>{tip}</div>
    </div>
  );

  if (overlay) {
    return (
      <div style={{ position: 'relative', ...style }}>
        <Spin 
          spinning={spinning} 
          size={size} 
          tip={spinContent}
          delay={delay}
          indicator={customIcon}
        >
          {children}
        </Spin>
      </div>
    );
  }

  return (
    <Spin 
      spinning={spinning} 
      size={size} 
      tip={spinContent}
      delay={delay}
      indicator={customIcon}
      style={style}
    >
      {children}
    </Spin>
  );
};

/**
 * 全页面加载组件
 */
export const LoadingPage: React.FC<LoadingPageProps> = ({
  title = '加载中',
  description = '正在为您准备内容...',
  progress,
  showProgress = false,
  timeout = 10000
}) => {
  const [timeoutReached, setTimeoutReached] = React.useState(false);

  React.useEffect(() => {
    const timer = setTimeout(() => {
      setTimeoutReached(true);
    }, timeout);

    return () => clearTimeout(timer);
  }, [timeout]);

  if (timeoutReached) {
    return (
      <div style={{ 
        height: '100vh', 
        display: 'flex', 
        alignItems: 'center', 
        justifyContent: 'center',
        backgroundColor: '#f5f5f5'
      }}>
        <Card style={{ textAlign: 'center', maxWidth: 400 }}>
          <Space direction="vertical" size="large">
            <ClockCircleOutlined style={{ fontSize: 64, color: '#faad14' }} />
            <div>
              <Text strong style={{ fontSize: 18 }}>加载时间较长</Text>
              <br />
              <Text type="secondary">
                页面加载时间超过预期，请检查网络连接或刷新页面重试
              </Text>
            </div>
            <Space>
              <button 
                onClick={() => window.location.reload()}
                style={{
                  padding: '8px 16px',
                  backgroundColor: '#1890ff',
                  color: 'white',
                  border: 'none',
                  borderRadius: '6px',
                  cursor: 'pointer'
                }}
              >
                刷新页面
              </button>
              <button 
                onClick={() => window.history.back()}
                style={{
                  padding: '8px 16px',
                  backgroundColor: '#f5f5f5',
                  color: '#666',
                  border: '1px solid #d9d9d9',
                  borderRadius: '6px',
                  cursor: 'pointer'
                }}
              >
                返回上页
              </button>
            </Space>
          </Space>
        </Card>
      </div>
    );
  }

  return (
    <div style={{ 
      height: '100vh', 
      display: 'flex', 
      alignItems: 'center', 
      justifyContent: 'center',
      backgroundColor: '#f5f5f5'
    }}>
      <Card style={{ textAlign: 'center', minWidth: 300 }}>
        <Space direction="vertical" size="large">
          <Spin size="large" indicator={customIcon} />
          <div>
            <Text strong style={{ fontSize: 18 }}>{title}</Text>
            <br />
            <Text type="secondary">{description}</Text>
          </div>
          {showProgress && typeof progress === 'number' && (
            <Progress 
              percent={progress} 
              status={progress === 100 ? 'success' : 'active'}
              strokeColor={{
                '0%': '#108ee9',
                '100%': '#87d068',
              }}
            />
          )}
        </Space>
      </Card>
    </div>
  );
};

/**
 * 内联加载组件
 */
export const InlineLoading: React.FC<{
  text?: string;
  size?: 'small' | 'default' | 'large';
}> = ({ text = '加载中...', size = 'small' }) => {
  return (
    <div style={{ 
      display: 'flex', 
      alignItems: 'center', 
      justifyContent: 'center',
      padding: '20px'
    }}>
      <Space>
        <Spin size={size} />
        <Text type="secondary">{text}</Text>
      </Space>
    </div>
  );
};

/**
 * 骨架屏加载组件
 */
export const SkeletonLoading: React.FC<{
  rows?: number;
  avatar?: boolean;
  title?: boolean;
  active?: boolean;
}> = ({ rows = 3, avatar = false, title = true, active = true }) => {
  return (
    <div style={{ padding: '20px' }}>
      {Array.from({ length: rows }).map((_, index) => (
        <div key={index} style={{ marginBottom: '16px' }}>
          <div style={{ display: 'flex', alignItems: 'center', marginBottom: '8px' }}>
            {avatar && (
              <div style={{
                width: '40px',
                height: '40px',
                borderRadius: '50%',
                backgroundColor: '#f0f0f0',
                marginRight: '12px',
                animation: active ? 'skeleton-loading 1.4s ease-in-out infinite' : 'none'
              }} />
            )}
            {title && (
              <div style={{
                width: '200px',
                height: '16px',
                backgroundColor: '#f0f0f0',
                borderRadius: '4px',
                animation: active ? 'skeleton-loading 1.4s ease-in-out infinite' : 'none'
              }} />
            )}
          </div>
          <div style={{
            width: '100%',
            height: '12px',
            backgroundColor: '#f0f0f0',
            borderRadius: '4px',
            marginBottom: '8px',
            animation: active ? 'skeleton-loading 1.4s ease-in-out infinite' : 'none'
          }} />
          <div style={{
            width: '80%',
            height: '12px',
            backgroundColor: '#f0f0f0',
            borderRadius: '4px',
            animation: active ? 'skeleton-loading 1.4s ease-in-out infinite' : 'none'
          }} />
        </div>
      ))}
      <style>
        {`
          @keyframes skeleton-loading {
            0% { opacity: 1; }
            50% { opacity: 0.4; }
            100% { opacity: 1; }
          }
        `}
      </style>
    </div>
  );
};

export default LoadingSpinner;
