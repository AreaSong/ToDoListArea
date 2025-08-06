import React from 'react';
import { Empty, Button, Space, Typography, Card } from 'antd';
import { 
  PlusOutlined, 
  FileTextOutlined, 
  SearchOutlined,
  InboxOutlined,
  SmileOutlined,
  RocketOutlined,
  BulbOutlined
} from '@ant-design/icons';

const { Text, Title } = Typography;

interface EmptyAction {
  text: string;
  type?: 'primary' | 'default' | 'dashed' | 'link';
  icon?: React.ReactNode;
  onClick: () => void;
}

interface EnhancedEmptyProps {
  type?: 'no-data' | 'no-search-results' | 'no-tasks' | 'no-templates' | 'no-activities' | 'custom';
  title?: string;
  description?: string;
  image?: React.ReactNode;
  actions?: EmptyAction[];
  showTips?: boolean;
  tips?: string[];
  style?: React.CSSProperties;
  size?: 'small' | 'default' | 'large';
  animated?: boolean;
}

const EnhancedEmpty: React.FC<EnhancedEmptyProps> = ({
  type = 'no-data',
  title,
  description,
  image,
  actions = [],
  showTips = false,
  tips = [],
  style,
  size = 'default',
  animated = true
}) => {
  const getPresetConfig = () => {
    switch (type) {
      case 'no-tasks':
        return {
          title: '还没有任务',
          description: '创建您的第一个任务，开始高效的工作管理',
          image: <InboxOutlined style={{ fontSize: 64, color: '#d9d9d9' }} />,
          actions: [
            {
              text: '创建任务',
              type: 'primary' as const,
              icon: <PlusOutlined />,
              onClick: () => {}
            }
          ],
          tips: [
            '💡 使用模板可以快速创建常用任务',
            '⏰ 设置提醒时间，不错过重要任务',
            '🏷️ 使用分类标签，让任务更有条理'
          ]
        };
      
      case 'no-templates':
        return {
          title: '暂无模板',
          description: '创建任务模板，提高工作效率',
          image: <FileTextOutlined style={{ fontSize: 64, color: '#d9d9d9' }} />,
          actions: [
            {
              text: '创建模板',
              type: 'primary' as const,
              icon: <PlusOutlined />,
              onClick: () => {}
            }
          ],
          tips: [
            '📝 将常用任务保存为模板',
            '🔄 一键复用，节省时间',
            '📊 跟踪模板使用统计'
          ]
        };
      
      case 'no-search-results':
        return {
          title: '没有找到相关内容',
          description: '尝试调整搜索条件或清除筛选器',
          image: <SearchOutlined style={{ fontSize: 64, color: '#d9d9d9' }} />,
          actions: [
            {
              text: '清除筛选',
              type: 'default' as const,
              onClick: () => {}
            },
            {
              text: '重新搜索',
              type: 'primary' as const,
              icon: <SearchOutlined />,
              onClick: () => {}
            }
          ],
          tips: [
            '🔍 尝试使用更简单的关键词',
            '📅 检查时间范围设置',
            '🏷️ 尝试不同的分类筛选'
          ]
        };
      
      case 'no-activities':
        return {
          title: '暂无活动记录',
          description: '开始使用应用，活动记录会自动生成',
          image: <SmileOutlined style={{ fontSize: 64, color: '#d9d9d9' }} />,
          actions: [
            {
              text: '去创建任务',
              type: 'primary' as const,
              icon: <RocketOutlined />,
              onClick: () => {}
            }
          ],
          tips: [
            '📈 活动记录帮助您了解工作习惯',
            '⏱️ 查看时间分配和效率统计',
            '🎯 设定目标，追踪进度'
          ]
        };
      
      default:
        return {
          title: '暂无数据',
          description: '这里还没有任何内容',
          image: <InboxOutlined style={{ fontSize: 64, color: '#d9d9d9' }} />,
          actions: [],
          tips: []
        };
    }
  };

  const config = getPresetConfig();
  const finalTitle = title || config.title;
  const finalDescription = description || config.description;
  const finalImage = image || config.image;
  const finalActions = actions.length > 0 ? actions : config.actions;
  const finalTips = tips.length > 0 ? tips : config.tips;

  const getSizeStyle = () => {
    switch (size) {
      case 'small':
        return { padding: '20px' };
      case 'large':
        return { padding: '60px 20px' };
      default:
        return { padding: '40px 20px' };
    }
  };

  const renderTips = () => {
    if (!showTips || finalTips.length === 0) return null;

    return (
      <Card 
        size="small" 
        style={{ 
          marginTop: 24, 
          backgroundColor: '#fafafa',
          border: '1px dashed #d9d9d9'
        }}
      >
        <div style={{ textAlign: 'left' }}>
          <Text strong style={{ color: '#1890ff', marginBottom: 8, display: 'block' }}>
            <BulbOutlined /> 小贴士
          </Text>
          <ul style={{ margin: 0, paddingLeft: 20 }}>
            {finalTips.map((tip, index) => (
              <li key={index} style={{ marginBottom: 4, color: '#666' }}>
                {tip}
              </li>
            ))}
          </ul>
        </div>
      </Card>
    );
  };

  const renderActions = () => {
    if (finalActions.length === 0) return null;

    return (
      <Space wrap style={{ marginTop: 16 }}>
        {finalActions.map((action, index) => (
          <Button
            key={index}
            type={action.type}
            icon={action.icon}
            onClick={action.onClick}
          >
            {action.text}
          </Button>
        ))}
      </Space>
    );
  };

  return (
    <div 
      style={{ 
        textAlign: 'center', 
        ...getSizeStyle(),
        ...style 
      }}
      className={animated ? 'enhanced-empty-animated' : ''}
    >
      <Empty
        image={finalImage}
        imageStyle={{ 
          height: size === 'small' ? 40 : size === 'large' ? 80 : 60,
          marginBottom: 16
        }}
        description={
          <div>
            <Title level={4} style={{ color: '#666', marginBottom: 8 }}>
              {finalTitle}
            </Title>
            <Text type="secondary" style={{ fontSize: 14 }}>
              {finalDescription}
            </Text>
          </div>
        }
      />
      
      {renderActions()}
      {renderTips()}

      {/* 动画样式 */}
      <style>
        {`
          .enhanced-empty-animated {
            animation: emptyFadeIn 0.5s ease-out;
          }
          
          @keyframes emptyFadeIn {
            from {
              opacity: 0;
              transform: translateY(20px);
            }
            to {
              opacity: 1;
              transform: translateY(0);
            }
          }
          
          .enhanced-empty-animated .ant-empty-image {
            animation: emptyFloat 3s ease-in-out infinite;
          }
          
          @keyframes emptyFloat {
            0%, 100% {
              transform: translateY(0px);
            }
            50% {
              transform: translateY(-10px);
            }
          }
        `}
      </style>
    </div>
  );
};

// 预设的空状态配置
export const EmptyPresets = {
  noTasks: (onCreateTask: () => void) => ({
    type: 'no-tasks' as const,
    actions: [
      {
        text: '创建任务',
        type: 'primary' as const,
        icon: <PlusOutlined />,
        onClick: onCreateTask
      }
    ],
    showTips: true
  }),
  
  noTemplates: (onCreateTemplate: () => void) => ({
    type: 'no-templates' as const,
    actions: [
      {
        text: '创建模板',
        type: 'primary' as const,
        icon: <PlusOutlined />,
        onClick: onCreateTemplate
      }
    ],
    showTips: true
  }),
  
  noSearchResults: (onClearFilter: () => void, onNewSearch: () => void) => ({
    type: 'no-search-results' as const,
    actions: [
      {
        text: '清除筛选',
        type: 'default' as const,
        onClick: onClearFilter
      },
      {
        text: '重新搜索',
        type: 'primary' as const,
        icon: <SearchOutlined />,
        onClick: onNewSearch
      }
    ],
    showTips: true
  })
};

export default EnhancedEmpty;
