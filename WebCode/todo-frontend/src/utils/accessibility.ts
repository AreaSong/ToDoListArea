/**
 * 无障碍访问性工具类
 * 提供键盘导航、屏幕阅读器支持、焦点管理等功能
 */

/**
 * 键盘事件处理工具
 */
export class KeyboardNavigation {
  /**
   * 检查是否为回车键或空格键
   */
  static isActivationKey(event: KeyboardEvent): boolean {
    return event.key === 'Enter' || event.key === ' ';
  }

  /**
   * 检查是否为方向键
   */
  static isArrowKey(event: KeyboardEvent): boolean {
    return ['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight'].includes(event.key);
  }

  /**
   * 检查是否为Tab键
   */
  static isTabKey(event: KeyboardEvent): boolean {
    return event.key === 'Tab';
  }

  /**
   * 检查是否为Escape键
   */
  static isEscapeKey(event: KeyboardEvent): boolean {
    return event.key === 'Escape';
  }

  /**
   * 处理可点击元素的键盘事件
   */
  static handleClickableKeydown(
    event: KeyboardEvent, 
    callback: () => void,
    preventDefault = true
  ) {
    if (this.isActivationKey(event)) {
      if (preventDefault) {
        event.preventDefault();
      }
      callback();
    }
  }

  /**
   * 获取可聚焦的元素
   */
  static getFocusableElements(container: Element): Element[] {
    const focusableSelectors = [
      'a[href]',
      'button:not([disabled])',
      'input:not([disabled])',
      'select:not([disabled])',
      'textarea:not([disabled])',
      '[tabindex]:not([tabindex="-1"])',
      '[contenteditable="true"]'
    ].join(', ');

    return Array.from(container.querySelectorAll(focusableSelectors));
  }

  /**
   * 循环焦点导航
   */
  static trapFocus(container: Element, event: KeyboardEvent) {
    const focusableElements = this.getFocusableElements(container);
    const firstElement = focusableElements[0] as HTMLElement;
    const lastElement = focusableElements[focusableElements.length - 1] as HTMLElement;

    if (event.key === 'Tab') {
      if (event.shiftKey) {
        // Shift + Tab
        if (document.activeElement === firstElement) {
          event.preventDefault();
          lastElement?.focus();
        }
      } else {
        // Tab
        if (document.activeElement === lastElement) {
          event.preventDefault();
          firstElement?.focus();
        }
      }
    }
  }
}

/**
 * 焦点管理工具
 */
export class FocusManager {
  private static focusStack: HTMLElement[] = [];

  /**
   * 保存当前焦点并设置新焦点
   */
  static saveFocusAndSet(newFocusElement?: HTMLElement) {
    const currentFocus = document.activeElement as HTMLElement;
    if (currentFocus) {
      this.focusStack.push(currentFocus);
    }

    if (newFocusElement) {
      // 延迟设置焦点，确保DOM更新完成
      setTimeout(() => {
        newFocusElement.focus();
      }, 0);
    }
  }

  /**
   * 恢复之前保存的焦点
   */
  static restoreFocus() {
    const previousFocus = this.focusStack.pop();
    if (previousFocus && document.contains(previousFocus)) {
      setTimeout(() => {
        previousFocus.focus();
      }, 0);
    }
  }

  /**
   * 清空焦点栈
   */
  static clearFocusStack() {
    this.focusStack = [];
  }

  /**
   * 设置焦点到第一个可聚焦元素
   */
  static focusFirstElement(container: Element) {
    const focusableElements = KeyboardNavigation.getFocusableElements(container);
    const firstElement = focusableElements[0] as HTMLElement;
    if (firstElement) {
      setTimeout(() => {
        firstElement.focus();
      }, 0);
    }
  }
}

/**
 * 屏幕阅读器支持工具
 */
export class ScreenReaderSupport {
  /**
   * 创建屏幕阅读器公告
   */
  static announce(message: string, priority: 'polite' | 'assertive' = 'polite') {
    const announcement = document.createElement('div');
    announcement.setAttribute('aria-live', priority);
    announcement.setAttribute('aria-atomic', 'true');
    announcement.className = 'sr-only';
    announcement.textContent = message;

    document.body.appendChild(announcement);

    // 清理公告元素
    setTimeout(() => {
      document.body.removeChild(announcement);
    }, 1000);
  }

  /**
   * 设置元素的可访问性标签
   */
  static setAccessibleLabel(element: HTMLElement, label: string) {
    element.setAttribute('aria-label', label);
  }

  /**
   * 设置元素的描述
   */
  static setAccessibleDescription(element: HTMLElement, description: string) {
    const descId = `desc-${Math.random().toString(36).substr(2, 9)}`;
    
    // 创建描述元素
    const descElement = document.createElement('div');
    descElement.id = descId;
    descElement.className = 'sr-only';
    descElement.textContent = description;
    
    document.body.appendChild(descElement);
    element.setAttribute('aria-describedby', descId);

    return descId;
  }

  /**
   * 设置元素的展开/折叠状态
   */
  static setExpandedState(element: HTMLElement, expanded: boolean) {
    element.setAttribute('aria-expanded', expanded.toString());
  }

  /**
   * 设置元素的选中状态
   */
  static setSelectedState(element: HTMLElement, selected: boolean) {
    element.setAttribute('aria-selected', selected.toString());
  }

  /**
   * 设置元素的禁用状态
   */
  static setDisabledState(element: HTMLElement, disabled: boolean) {
    if (disabled) {
      element.setAttribute('aria-disabled', 'true');
    } else {
      element.removeAttribute('aria-disabled');
    }
  }
}

/**
 * 颜色对比度检查工具
 */
export class ColorContrastChecker {
  /**
   * 计算相对亮度
   */
  private static getRelativeLuminance(r: number, g: number, b: number): number {
    const [rs, gs, bs] = [r, g, b].map(c => {
      c = c / 255;
      return c <= 0.03928 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4);
    });
    return 0.2126 * rs + 0.7152 * gs + 0.0722 * bs;
  }

  /**
   * 计算对比度比率
   */
  static getContrastRatio(color1: string, color2: string): number {
    // 简化的颜色解析，实际应用中可能需要更复杂的解析
    const parseColor = (color: string) => {
      const hex = color.replace('#', '');
      const r = parseInt(hex.substr(0, 2), 16);
      const g = parseInt(hex.substr(2, 2), 16);
      const b = parseInt(hex.substr(4, 2), 16);
      return { r, g, b };
    };

    const c1 = parseColor(color1);
    const c2 = parseColor(color2);

    const l1 = this.getRelativeLuminance(c1.r, c1.g, c1.b);
    const l2 = this.getRelativeLuminance(c2.r, c2.g, c2.b);

    const lighter = Math.max(l1, l2);
    const darker = Math.min(l1, l2);

    return (lighter + 0.05) / (darker + 0.05);
  }

  /**
   * 检查是否符合WCAG AA标准
   */
  static meetsWCAGAA(foreground: string, background: string, isLargeText = false): boolean {
    const ratio = this.getContrastRatio(foreground, background);
    return isLargeText ? ratio >= 3 : ratio >= 4.5;
  }

  /**
   * 检查是否符合WCAG AAA标准
   */
  static meetsWCAGAAA(foreground: string, background: string, isLargeText = false): boolean {
    const ratio = this.getContrastRatio(foreground, background);
    return isLargeText ? ratio >= 4.5 : ratio >= 7;
  }
}

/**
 * 无障碍性检查工具
 */
export class AccessibilityChecker {
  /**
   * 检查图片是否有alt属性
   */
  static checkImageAltText(): string[] {
    const images = document.querySelectorAll('img');
    const issues: string[] = [];

    images.forEach((img, index) => {
      if (!img.hasAttribute('alt')) {
        issues.push(`图片 ${index + 1} 缺少 alt 属性`);
      } else if (img.getAttribute('alt')?.trim() === '') {
        issues.push(`图片 ${index + 1} 的 alt 属性为空`);
      }
    });

    return issues;
  }

  /**
   * 检查表单标签
   */
  static checkFormLabels(): string[] {
    const inputs = document.querySelectorAll('input, select, textarea');
    const issues: string[] = [];

    inputs.forEach((input, index) => {
      const id = input.getAttribute('id');
      const ariaLabel = input.getAttribute('aria-label');
      const ariaLabelledby = input.getAttribute('aria-labelledby');
      
      if (id) {
        const label = document.querySelector(`label[for="${id}"]`);
        if (!label && !ariaLabel && !ariaLabelledby) {
          issues.push(`表单控件 ${index + 1} 缺少标签`);
        }
      } else if (!ariaLabel && !ariaLabelledby) {
        issues.push(`表单控件 ${index + 1} 缺少标签或ID`);
      }
    });

    return issues;
  }

  /**
   * 检查标题层级
   */
  static checkHeadingHierarchy(): string[] {
    const headings = document.querySelectorAll('h1, h2, h3, h4, h5, h6');
    const issues: string[] = [];
    let previousLevel = 0;

    headings.forEach((heading, index) => {
      const currentLevel = parseInt(heading.tagName.charAt(1));
      
      if (index === 0 && currentLevel !== 1) {
        issues.push('页面应该以 h1 标题开始');
      }
      
      if (currentLevel > previousLevel + 1) {
        issues.push(`标题 ${index + 1} 跳过了层级（从 h${previousLevel} 跳到 h${currentLevel}）`);
      }
      
      previousLevel = currentLevel;
    });

    return issues;
  }

  /**
   * 运行完整的无障碍性检查
   */
  static runFullCheck(): { issues: string[]; score: number } {
    const allIssues = [
      ...this.checkImageAltText(),
      ...this.checkFormLabels(),
      ...this.checkHeadingHierarchy()
    ];

    // 简单的评分系统
    const totalChecks = 3;
    const issueCount = allIssues.length;
    const score = Math.max(0, Math.round((1 - issueCount / (totalChecks * 5)) * 100));

    return {
      issues: allIssues,
      score
    };
  }
}

// 添加全局CSS类用于屏幕阅读器
const addScreenReaderStyles = () => {
  if (!document.querySelector('#accessibility-styles')) {
    const style = document.createElement('style');
    style.id = 'accessibility-styles';
    style.textContent = `
      .sr-only {
        position: absolute !important;
        width: 1px !important;
        height: 1px !important;
        padding: 0 !important;
        margin: -1px !important;
        overflow: hidden !important;
        clip: rect(0, 0, 0, 0) !important;
        white-space: nowrap !important;
        border: 0 !important;
      }
      
      .sr-only-focusable:focus {
        position: static !important;
        width: auto !important;
        height: auto !important;
        padding: inherit !important;
        margin: inherit !important;
        overflow: visible !important;
        clip: auto !important;
        white-space: inherit !important;
      }
      
      /* 高对比度模式支持 */
      @media (prefers-contrast: high) {
        * {
          border-color: ButtonText !important;
        }
      }
      
      /* 减少动画模式支持 */
      @media (prefers-reduced-motion: reduce) {
        *, *::before, *::after {
          animation-duration: 0.01ms !important;
          animation-iteration-count: 1 !important;
          transition-duration: 0.01ms !important;
        }
      }
    `;
    document.head.appendChild(style);
  }
};

// 初始化无障碍性样式
if (typeof document !== 'undefined') {
  addScreenReaderStyles();
}

export {
  KeyboardNavigation,
  FocusManager,
  ScreenReaderSupport,
  ColorContrastChecker,
  AccessibilityChecker
};
