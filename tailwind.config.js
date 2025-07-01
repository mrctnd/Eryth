/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Views/**/*.cshtml",
    "./Pages/**/*.cshtml",
    "./wwwroot/js/**/*.js"
  ],
  theme: {
    extend: {
      colors: {
        // New music platform color palette
        'black': '#000000',
        'white': '#FFFFFF', 
        'muted': '#282828',
        'accent': '#1DB954',
        
        // Semantic aliases for better UX
        'primary': '#000000',        // Main background (black)
        'primary-light': '#282828',  // Subtle backgrounds (dark gray)
        'secondary': '#282828',      // Separators and cards (dark gray)
        'secondary-light': '#383838', // Lighter variant for hovers
        'highlight': '#1DB954',      // CTAs, active states (accent green)
        'accent-dark': '#1ed760',    // Darker green for hovers
        'accent-light': '#1ed760'    // Lighter green variant
      }
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
    require('@tailwindcss/typography'),
  ],
}

