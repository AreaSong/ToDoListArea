import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5006',
        changeOrigin: true,
        secure: false,
      }
    }
  },
  build: {
    rollupOptions: {
      output: {
        manualChunks: {
          // 将React相关库分离到单独的chunk
          'react-vendor': ['react', 'react-dom', 'react-router-dom'],
          // 将Ant Design分离到单独的chunk
          'antd-vendor': ['antd'],
          // 将甘特图组件分离到单独的chunk
          'gantt-vendor': ['gantt-task-react'],
          // 将日期处理库分离
          'date-vendor': ['dayjs']
        }
      }
    },
    // 设置chunk大小警告限制
    chunkSizeWarningLimit: 1000,
    // 启用压缩
    minify: 'terser'
  }
})
