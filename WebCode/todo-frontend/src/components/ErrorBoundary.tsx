import { Component } from 'react';
import type { ErrorInfo, ReactNode } from 'react';
import { Result, Button, Typography, Card, Space, Collapse, message } from 'antd';
import { ExclamationCircleOutlined, ReloadOutlined, HomeOutlined, BugOutlined, CopyOutlined } from '@ant-design/icons';

const { Text, Paragraph } = Typography;
const { Panel } = Collapse;

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}

interface State {
  hasError: boolean;
  error?: Error;
  errorInfo?: ErrorInfo;
  errorId: string;
}

class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = {
      hasError: false,
      errorId: ''
    };
  }

  static getDerivedStateFromError(error: Error): Partial<State> {
    // 生成错误ID用于追踪
    const errorId = `ERR_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    return {
      hasError: true,
      error,
      errorId
    };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('ErrorBoundary caught an error:', error, errorInfo);
    this.setState({
      error,
      errorInfo
    });

    // 报告错误
    this.reportError(error, errorInfo);
  }

  reportError = (error: Error, errorInfo: ErrorInfo) => {
    // 在生产环境中，这里可以发送错误到监控服务
    const errorReport = {
      message: error.message,
      stack: error.stack,
      componentStack: errorInfo.componentStack,
      timestamp: new Date().toISOString(),
      userAgent: navigator.userAgent,
      url: window.location.href,
      errorId: this.state.errorId
    };

    console.log('Error Report:', errorReport);

    // 示例：发送到错误监控服务
    // fetch('/api/errors', {
    //   method: 'POST',
    //   headers: { 'Content-Type': 'application/json' },
    //   body: JSON.stringify(errorReport)
    // });
  };

  handleReload = () => {
    window.location.reload();
  };

  handleGoHome = () => {
    window.location.href = '/dashboard';
  };

  handleRetry = () => {
    this.setState({
      hasError: false,
      error: undefined,
      errorInfo: undefined,
      errorId: ''
    });
  };

  handleCopyError = () => {
    const { error, errorInfo, errorId } = this.state;
    const errorText = `错误ID: ${errorId}\n错误消息: ${error?.message}\n错误堆栈: ${error?.stack}\n组件堆栈: ${errorInfo?.componentStack}`;

    navigator.clipboard.writeText(errorText).then(() => {
      message.success('错误信息已复制到剪贴板');
    }).catch(() => {
      message.error('复制失败，请手动复制');
    });
  };

  render() {
    if (this.state.hasError) {
      // 如果提供了自定义fallback，使用它
      if (this.props.fallback) {
        return this.props.fallback;
      }

      const { error, errorInfo, errorId } = this.state;
      const isDevelopment = process.env.NODE_ENV === 'development';

      return (
        <div style={{ padding: '20px', minHeight: '100vh', backgroundColor: '#f5f5f5' }}>
          <Card style={{ maxWidth: 800, margin: '0 auto' }}>
            <Result
              status="error"
              icon={<ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />}
              title="页面出现了问题"
              subTitle={
                <Space direction="vertical" size="small">
                  <Text>很抱歉，页面遇到了意外错误。我们已经记录了这个问题。</Text>
                  <Text type="secondary">错误ID: {errorId}</Text>
                </Space>
              }
              extra={
                <Space wrap>
                  <Button type="primary" icon={<ReloadOutlined />} onClick={this.handleRetry}>
                    重试
                  </Button>
                  <Button icon={<ReloadOutlined />} onClick={this.handleReload}>
                    刷新页面
                  </Button>
                  <Button icon={<HomeOutlined />} onClick={this.handleGoHome}>
                    返回首页
                  </Button>
                  {isDevelopment && (
                    <Button icon={<CopyOutlined />} onClick={this.handleCopyError}>
                      复制错误信息
                    </Button>
                  )}
                </Space>
              }
            />
            {isDevelopment && error && (
              <Card
                title={
                  <Space>
                    <BugOutlined />
                    开发调试信息
                  </Space>
                }
                style={{ marginTop: 20 }}
                size="small"
              >
                <Collapse size="small">
                  <Panel header="错误详情" key="error">
                    <Paragraph>
                      <Text strong>错误消息:</Text>
                      <br />
                      <Text code>{error.message}</Text>
                    </Paragraph>

                    {error.stack && (
                      <Paragraph>
                        <Text strong>错误堆栈:</Text>
                        <br />
                        <Text code style={{ whiteSpace: 'pre-wrap', fontSize: '12px' }}>
                          {error.stack}
                        </Text>
                      </Paragraph>
                    )}
                  </Panel>

                  {errorInfo && (
                    <Panel header="组件堆栈" key="component">
                      <Text code style={{ whiteSpace: 'pre-wrap', fontSize: '12px' }}>
                        {errorInfo.componentStack}
                      </Text>
                    </Panel>
                  )}
                </Collapse>
              </Card>
            )}

            <Card
              title="常见解决方案"
              style={{ marginTop: 20 }}
              size="small"
            >
              <ul style={{ paddingLeft: 20 }}>
                <li>尝试刷新页面</li>
                <li>清除浏览器缓存和Cookie</li>
                <li>检查网络连接</li>
                <li>如果问题持续存在，请联系技术支持</li>
              </ul>
            </Card>
          </Card>
        </div>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary;
