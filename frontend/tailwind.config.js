/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#1F5E4A',
          dark: '#163F33',
          light: '#EAF4EF'
        },
        background: {
          main: '#F7F9F7',
          card: '#FFFFFF'
        },
        text: {
          main: '#1F2937',
          muted: '#667085'
        },
        border: {
          main: '#D9E2DC'
        },
        status: {
          warning: '#B7791F',
          error: '#B54747'
        }
      }
    },
  },
  plugins: [],
}
