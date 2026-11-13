import path from 'node:path'
import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  // Dev: proxy /api và /hubs sang backend để không vướng CORS/chứng chỉ dev (launchSettings: https://localhost:31034)
  const target = env.DEV_API_TARGET || 'https://localhost:31034'

  return {
    plugins: [react()],
    resolve: { alias: { '@': path.resolve(__dirname, 'src') } },
    css: { preprocessorOptions: { scss: { quietDeps: true, silenceDeprecations: ['import', 'global-builtin', 'color-functions', 'mixed-decls'] } } },
    server: {
      port: 5173, // khớp Cors:AllowedOrigins trong appsettings.json
      proxy: {
        '/api': { target, changeOrigin: true, secure: false },
        '/hubs': { target, changeOrigin: true, secure: false, ws: true },
      },
    },
    build: { chunkSizeWarningLimit: 900 },
  }
})
