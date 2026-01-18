/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Pages/**/*.cshtml',
    './Pages/**/*.cs',
    './wwwroot/js/**/*.js'
  ],
  theme: {
    extend: {
      colors: {
        // Railway-themed colors for the engineering library
        primary: {
          50: '#f0f7ff',
          100: '#e0effe',
          200: '#bae0fd',
          300: '#7cc7fc',
          400: '#36a9f8',
          500: '#0c8ee9',
          600: '#0070c7',
          700: '#0159a1',
          800: '#064b85',
          900: '#0b3f6e',
          950: '#072849',
        },
        // Steel gray accent
        steel: {
          50: '#f6f7f9',
          100: '#eceef2',
          200: '#d5d9e2',
          300: '#b0b8c9',
          400: '#8592ab',
          500: '#667491',
          600: '#515d78',
          700: '#434c62',
          800: '#3a4153',
          900: '#333947',
          950: '#22252f',
        },
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', '-apple-system', 'sans-serif'],
      },
    },
  },
  plugins: [],
}
