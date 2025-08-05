import React, { useState } from 'react';
import { Form, Input, Button, Space, Typography, Alert, Progress, Tooltip } from 'antd';
import { 
  CheckCircleOutlined, 
  ExclamationCircleOutlined, 
  InfoCircleOutlined,
  EyeInvisibleOutlined,
  EyeTwoTone
} from '@ant-design/icons';
import type { FormInstance, Rule } from 'antd/es/form';

const { Text } = Typography;

interface ValidationResult {
  isValid: boolean;
  message: string;
  type: 'success' | 'warning' | 'error' | 'info';
}

interface EnhancedInputProps {
  name: string;
  label: string;
  placeholder?: string;
  type?: 'text' | 'email' | 'password' | 'textarea';
  required?: boolean;
  rules?: Rule[];
  realTimeValidation?: boolean;
  strengthMeter?: boolean; // 密码强度指示器
  showCount?: boolean;
  maxLength?: number;
  rows?: number;
  tooltip?: string;
  helpText?: string;
  validateTrigger?: string | string[];
  dependencies?: string[];
  customValidator?: (value: string) => ValidationResult;
}

interface EnhancedFormProps {
  form: FormInstance;
  onFinish: (values: any) => void;
  loading?: boolean;
  submitText?: string;
  cancelText?: string;
  onCancel?: () => void;
  showProgress?: boolean;
  children?: React.ReactNode;
  layout?: 'horizontal' | 'vertical' | 'inline';
  size?: 'small' | 'middle' | 'large';
}

// 密码强度检测
const checkPasswordStrength = (password: string): ValidationResult => {
  if (!password) {
    return { isValid: false, message: '请输入密码', type: 'error' };
  }

  let score = 0;
  const checks = [
    { regex: /.{8,}/, message: '至少8个字符' },
    { regex: /[a-z]/, message: '包含小写字母' },
    { regex: /[A-Z]/, message: '包含大写字母' },
    { regex: /\d/, message: '包含数字' },
    { regex: /[!@#$%^&*(),.?":{}|<>]/, message: '包含特殊字符' }
  ];

  checks.forEach(check => {
    if (check.regex.test(password)) score++;
  });

  if (score < 2) {
    return { isValid: false, message: '密码强度太弱', type: 'error' };
  } else if (score < 4) {
    return { isValid: true, message: '密码强度中等', type: 'warning' };
  } else {
    return { isValid: true, message: '密码强度很强', type: 'success' };
  }
};

// 邮箱验证
const validateEmail = (email: string): ValidationResult => {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  if (!emailRegex.test(email)) {
    return { isValid: false, message: '请输入有效的邮箱地址', type: 'error' };
  }
  return { isValid: true, message: '邮箱格式正确', type: 'success' };
};

// 增强输入组件
export const EnhancedInput: React.FC<EnhancedInputProps> = ({
  name,
  label,
  placeholder,
  type = 'text',
  required = false,
  rules = [],
  realTimeValidation = false,
  strengthMeter = false,
  showCount = false,
  maxLength,
  rows = 4,
  tooltip,
  helpText,
  validateTrigger = 'onChange',
  dependencies = [],
  customValidator
}) => {
  const [validationResult, setValidationResult] = useState<ValidationResult | null>(null);
  const [passwordStrength, setPasswordStrength] = useState(0);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const value = e.target.value;

    if (realTimeValidation) {
      let result: ValidationResult;

      if (customValidator) {
        result = customValidator(value);
      } else if (type === 'email') {
        result = validateEmail(value);
      } else if (type === 'password' && strengthMeter) {
        result = checkPasswordStrength(value);
        // 计算密码强度百分比
        const score = [
          /.{8,}/.test(value),
          /[a-z]/.test(value),
          /[A-Z]/.test(value),
          /\d/.test(value),
          /[!@#$%^&*(),.?":{}|<>]/.test(value)
        ].filter(Boolean).length;
        setPasswordStrength((score / 5) * 100);
      } else {
        result = { isValid: true, message: '', type: 'success' };
      }

      setValidationResult(result);
    }
  };

  const getValidationIcon = () => {
    if (!validationResult) return null;
    
    switch (validationResult.type) {
      case 'success':
        return <CheckCircleOutlined style={{ color: '#52c41a' }} />;
      case 'warning':
        return <ExclamationCircleOutlined style={{ color: '#faad14' }} />;
      case 'error':
        return <ExclamationCircleOutlined style={{ color: '#ff4d4f' }} />;
      default:
        return <InfoCircleOutlined style={{ color: '#1890ff' }} />;
    }
  };

  const renderInput = () => {
    const commonProps = {
      placeholder,
      onChange: handleChange,
      showCount,
      maxLength,
      suffix: realTimeValidation ? getValidationIcon() : undefined
    };

    switch (type) {
      case 'password':
        return (
          <div>
            <Input.Password
              {...commonProps}
              iconRender={(visible) => (visible ? <EyeTwoTone /> : <EyeInvisibleOutlined />)}
            />
            {strengthMeter && passwordStrength > 0 && (
              <div style={{ marginTop: 8 }}>
                <Progress
                  percent={passwordStrength}
                  size="small"
                  strokeColor={
                    passwordStrength < 40 ? '#ff4d4f' :
                    passwordStrength < 80 ? '#faad14' : '#52c41a'
                  }
                  showInfo={false}
                />
                <Text type="secondary" style={{ fontSize: 12 }}>
                  密码强度: {
                    passwordStrength < 40 ? '弱' :
                    passwordStrength < 80 ? '中' : '强'
                  }
                </Text>
              </div>
            )}
          </div>
        );
      case 'textarea':
        return <Input.TextArea {...commonProps} rows={rows} />;
      default:
        return <Input {...commonProps} />;
    }
  };

  const enhancedRules = [
    ...rules,
    ...(required ? [{ required: true, message: `请输入${label}` }] : [])
  ];

  return (
    <Form.Item
      name={name}
      label={
        <Space>
          {label}
          {tooltip && (
            <Tooltip title={tooltip}>
              <InfoCircleOutlined style={{ color: '#1890ff' }} />
            </Tooltip>
          )}
        </Space>
      }
      rules={enhancedRules}
      validateTrigger={validateTrigger}
      dependencies={dependencies}
      help={
        <div>
          {helpText && <Text type="secondary" style={{ fontSize: 12 }}>{helpText}</Text>}
          {realTimeValidation && validationResult && (
            <div style={{ marginTop: 4 }}>
              <Text type={validationResult.type === 'error' ? 'danger' : 'secondary'} style={{ fontSize: 12 }}>
                {validationResult.message}
              </Text>
            </div>
          )}
        </div>
      }
    >
      {renderInput()}
    </Form.Item>
  );
};

// 增强表单组件
export const EnhancedForm: React.FC<EnhancedFormProps> = ({
  form,
  onFinish,
  loading = false,
  submitText = '提交',
  cancelText = '取消',
  onCancel,
  showProgress = false,
  children,
  layout = 'vertical',
  size = 'middle'
}) => {
  const [completedFields, setCompletedFields] = useState(0);
  const [totalFields, setTotalFields] = useState(0);

  const handleValuesChange = () => {
    if (showProgress) {
      const fields = form.getFieldsValue();
      const completed = Object.values(fields).filter(value => 
        value !== undefined && value !== null && value !== ''
      ).length;
      const total = Object.keys(fields).length;
      
      setCompletedFields(completed);
      setTotalFields(total);
    }
  };

  return (
    <Form
      form={form}
      layout={layout}
      size={size}
      onFinish={onFinish}
      onValuesChange={handleValuesChange}
      scrollToFirstError
    >
      {showProgress && totalFields > 0 && (
        <div style={{ marginBottom: 24 }}>
          <Text strong>表单完成进度</Text>
          <Progress
            percent={Math.round((completedFields / totalFields) * 100)}
            size="small"
            style={{ marginTop: 8 }}
          />
        </div>
      )}

      {children}

      <Form.Item style={{ marginBottom: 0, textAlign: 'right', marginTop: 24 }}>
        <Space>
          {onCancel && (
            <Button onClick={onCancel} disabled={loading}>
              {cancelText}
            </Button>
          )}
          <Button type="primary" htmlType="submit" loading={loading}>
            {submitText}
          </Button>
        </Space>
      </Form.Item>
    </Form>
  );
};

export default EnhancedForm;
