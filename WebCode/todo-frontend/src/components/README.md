# 用户体验组件使用指南

本文档介绍了新增的用户体验组件的使用方法和最佳实践。

## 📦 组件概览

### 1. ConfirmDialog - 确认对话框
用于需要用户确认的操作，特别是删除等危险操作。

```tsx
import ConfirmDialog from '../components/ConfirmDialog';

// 基础用法
<ConfirmDialog
  visible={confirmVisible}
  title="确认删除"
  content="确定要删除这个项目吗？"
  type="danger"
  onConfirm={handleConfirm}
  onCancel={handleCancel}
  consequences={[
    '数据将被永久删除',
    '相关记录将无法恢复'
  ]}
/>
```

**Props:**
- `type`: 'danger' | 'warning' | 'info' | 'success'
- `consequences`: 操作后果列表
- `additionalInfo`: 额外提示信息

### 2. EnhancedForm & EnhancedInput - 增强表单
提供实时验证、密码强度检测等功能。

```tsx
import { EnhancedForm, EnhancedInput } from '../components/EnhancedForm';

<EnhancedForm
  form={form}
  onFinish={handleSubmit}
  loading={loading}
  showProgress={true}
>
  <EnhancedInput
    name="email"
    label="邮箱"
    type="email"
    required
    realTimeValidation
    helpText="请输入有效的邮箱地址"
  />
  
  <EnhancedInput
    name="password"
    label="密码"
    type="password"
    required
    strengthMeter
    helpText="密码强度会实时显示"
  />
</EnhancedForm>
```

**特性:**
- 实时验证反馈
- 密码强度指示器
- 表单完成进度
- 友好的错误提示

### 3. SuccessFeedback - 成功反馈
用于操作成功后的用户反馈。

```tsx
import SuccessFeedback, { SuccessTypes } from '../components/SuccessFeedback';

// 使用预设类型
<SuccessFeedback
  visible={successVisible}
  {...SuccessTypes.taskCompleted}
  onClose={handleClose}
  stats={[
    { label: '完成任务', value: 5, suffix: '个' },
    { label: '节省时间', value: 30, suffix: '分钟' }
  ]}
/>

// 自定义配置
<SuccessFeedback
  visible={successVisible}
  title="操作成功！"
  description="您的更改已保存"
  type="celebration"
  celebrationLevel="high"
  autoClose={true}
  autoCloseDelay={3000}
  onClose={handleClose}
/>
```

**预设类型:**
- `SuccessTypes.taskCreated`
- `SuccessTypes.taskCompleted`
- `SuccessTypes.milestoneReached`
- `SuccessTypes.levelUp`

### 4. EnhancedEmpty - 增强空状态
提供更友好的空状态展示。

```tsx
import EnhancedEmpty, { EmptyPresets } from '../components/EnhancedEmpty';

// 使用预设
<EnhancedEmpty
  {...EmptyPresets.noTasks(handleCreateTask)}
  showTips={true}
/>

// 自定义配置
<EnhancedEmpty
  type="custom"
  title="暂无数据"
  description="开始创建您的第一个项目"
  actions={[
    {
      text: '创建项目',
      type: 'primary',
      icon: <PlusOutlined />,
      onClick: handleCreate
    }
  ]}
  tips={[
    '💡 小贴士1',
    '⚡ 小贴士2'
  ]}
  showTips={true}
/>
```

### 5. FeedbackManager - 反馈管理器
统一的用户反馈管理工具。

```tsx
import { feedback } from '../utils/feedbackManager';

// 操作反馈
feedback.taskCreated();
feedback.taskUpdated();
feedback.taskDeleted();

// 错误处理
feedback.networkError('保存数据');
feedback.permissionError('删除项目');
feedback.loadError('任务列表');

// 确认对话框
feedback.confirmDelete('任务名称', async () => {
  await deleteTask();
});

// 加载状态
const loadingKey = feedback.showLoading('正在处理...');
// ... 异步操作
feedback.hideLoading();
```

## 🎨 最佳实践

### 1. 一致性原则
- 使用统一的反馈组件，避免混用不同的提示方式
- 保持错误信息的语言风格一致
- 统一使用中文提示信息

### 2. 用户友好性
- 提供清晰的操作后果说明
- 使用适当的动画效果
- 给予用户足够的操作反馈

### 3. 性能考虑
- 合理使用自动关闭功能
- 避免同时显示多个模态框
- 及时清理不需要的状态

### 4. 错误处理
```tsx
// 推荐的错误处理模式
try {
  feedback.showLoading('正在保存...');
  const result = await saveData();
  feedback.hideLoading();
  feedback.operationSuccess('保存');
} catch (error) {
  feedback.hideLoading();
  feedback.operationError('保存', error);
}
```

### 5. 表单验证
```tsx
// 推荐的表单验证模式
<EnhancedInput
  name="email"
  label="邮箱"
  type="email"
  required
  realTimeValidation
  customValidator={(value) => {
    // 自定义验证逻辑
    if (!value.includes('@company.com')) {
      return {
        isValid: false,
        message: '请使用公司邮箱',
        type: 'error'
      };
    }
    return { isValid: true, message: '', type: 'success' };
  }}
/>
```

## 🔧 集成步骤

1. **导入组件**
```tsx
import ConfirmDialog from '../components/ConfirmDialog';
import { EnhancedForm, EnhancedInput } from '../components/EnhancedForm';
import SuccessFeedback from '../components/SuccessFeedback';
import EnhancedEmpty from '../components/EnhancedEmpty';
import { feedback } from '../utils/feedbackManager';
```

2. **添加状态管理**
```tsx
const [confirmVisible, setConfirmVisible] = useState(false);
const [successVisible, setSuccessVisible] = useState(false);
const [loading, setLoading] = useState(false);
```

3. **替换现有的反馈机制**
```tsx
// 替换 message.success
feedback.operationSuccess('操作');

// 替换 Modal.confirm
feedback.confirmDelete('项目名称', handleDelete);

// 替换 Empty 组件
<EnhancedEmpty {...EmptyPresets.noTasks(handleCreate)} />
```

## 📱 响应式支持

所有组件都支持响应式设计，会根据屏幕尺寸自动调整：
- 移动端：简化布局，增大触摸区域
- 平板端：适中的间距和字体大小
- 桌面端：完整的功能和视觉效果

## 🎯 注意事项

1. **避免过度使用动画**：在低性能设备上可能影响体验
2. **合理设置自动关闭时间**：重要信息应该给用户足够时间阅读
3. **提供取消选项**：所有确认操作都应该可以取消
4. **保持信息简洁**：避免冗长的提示文本
5. **测试各种场景**：包括网络错误、权限错误等边缘情况
