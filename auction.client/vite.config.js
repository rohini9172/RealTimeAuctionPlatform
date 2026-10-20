import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5263',
      '/uploads': 'http://localhost:5263',
      '/auctionHub': {
        target: 'http://localhost:5263',
        ws: true,
      },
    },
  },
  build: {
    outDir: 'dist',
  },
})
