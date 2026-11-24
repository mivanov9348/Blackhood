import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// The API runs on http://localhost:5125 (dotnet run / Visual Studio "http" profile).
// Proxying /api keeps everything on one origin, so the auth cookie just works.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5125',
    },
  },
})
