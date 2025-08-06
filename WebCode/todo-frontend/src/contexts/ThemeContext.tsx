import React, { createContext, useContext, useState, useEffect } from 'react';
import type { ReactNode } from 'react';
import { ConfigProvider, theme, message } from 'antd';
import type { ThemePreferences } from '../types/api';

interface ThemeContextType {
  themePreferences: ThemePreferences;
  updateTheme: (preferences: Partial<ThemePreferences>) => void;
  applyTheme: (preferences: ThemePreferences) => void;
  isDarkMode: boolean;
}

const defaultThemePreferences: ThemePreferences = {
  theme: 'light',
  primaryColor: '#1890ff',
  compactMode: false,
};

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

export const useTheme = () => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme must be used within a ThemeProvider');
  }
  return context;
};

interface ThemeProviderProps {
  children: ReactNode;
}

export const ThemeProvider: React.FC<ThemeProviderProps> = ({ children }) => {
  const [themePreferences, setThemePreferences] = useState<ThemePreferences>(defaultThemePreferences);
  const [isDarkMode, setIsDarkMode] = useState(false);

  // 检测系统主题
  const getSystemTheme = () => {
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  };

  // 应用主题到界面
  const applyThemeToDOM = (preferences: ThemePreferences) => {
    const root = document.documentElement;
    
    // 确定实际使用的主题
    let actualTheme = preferences.theme;
    if (preferences.theme === 'auto') {
      actualTheme = getSystemTheme();
    }
    
    setIsDarkMode(actualTheme === 'dark');
    
    // 设置CSS变量
    root.style.setProperty('--primary-color', preferences.primaryColor);
    root.setAttribute('data-theme', actualTheme);
    
    // 设置body类名用于样式控制
    document.body.className = document.body.className.replace(/theme-\w+/g, '');
    document.body.classList.add(`theme-${actualTheme}`);
    
    if (preferences.compactMode) {
      document.body.classList.add('compact-mode');
    } else {
      document.body.classList.remove('compact-mode');
    }
  };

  // 更新主题设置
  const updateTheme = (newPreferences: Partial<ThemePreferences>) => {
    const updatedPreferences = { ...themePreferences, ...newPreferences };
    setThemePreferences(updatedPreferences);
    applyThemeToDOM(updatedPreferences);
    
    // 保存到localStorage
    localStorage.setItem('themePreferences', JSON.stringify(updatedPreferences));
    
    message.success('主题设置已应用', 1);
  };

  // 应用完整主题设置（从服务器获取后调用）
  const applyTheme = (preferences: ThemePreferences) => {
    setThemePreferences(preferences);
    applyThemeToDOM(preferences);
    
    // 保存到localStorage
    localStorage.setItem('themePreferences', JSON.stringify(preferences));
  };

  // 初始化主题
  useEffect(() => {
    // 从localStorage读取主题设置
    const savedTheme = localStorage.getItem('themePreferences');
    if (savedTheme) {
      try {
        const parsed = JSON.parse(savedTheme);
        setThemePreferences(parsed);
        applyThemeToDOM(parsed);
      } catch (error) {
        console.error('Failed to parse saved theme preferences:', error);
        applyThemeToDOM(defaultThemePreferences);
      }
    } else {
      applyThemeToDOM(defaultThemePreferences);
    }

    // 监听系统主题变化
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    const handleSystemThemeChange = () => {
      if (themePreferences.theme === 'auto') {
        applyThemeToDOM(themePreferences);
      }
    };

    mediaQuery.addEventListener('change', handleSystemThemeChange);
    return () => mediaQuery.removeEventListener('change', handleSystemThemeChange);
  }, []);

  // Ant Design主题配置
  const antdTheme = {
    algorithm: isDarkMode ? theme.darkAlgorithm : theme.defaultAlgorithm,
    token: {
      colorPrimary: themePreferences.primaryColor,
      borderRadius: themePreferences.compactMode ? 4 : 6,
      fontSize: themePreferences.compactMode ? 13 : 14,
    },
    components: {
      Layout: {
        bodyBg: isDarkMode ? '#141414' : '#f5f5f5',
        headerBg: isDarkMode ? '#1f1f1f' : '#ffffff',
      },
      Card: {
        colorBgContainer: isDarkMode ? '#1f1f1f' : '#ffffff',
      },
    },
  };

  return (
    <ThemeContext.Provider value={{ themePreferences, updateTheme, applyTheme, isDarkMode }}>
      <ConfigProvider theme={antdTheme}>
        {children}
      </ConfigProvider>
    </ThemeContext.Provider>
  );
};
