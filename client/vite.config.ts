import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true,
    // Same-origin API calls in development; the API runs on :5080.
    proxy: { '/api': 'http://localhost:5080' },
  },
})
